using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GoryMoon.StreamEngineer.Actions;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using HarmonyLib;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

namespace GoryMoon.StreamEngineer
{
    using ConfigGetter = Func<string>;

    public class Plugin : IPlugin, IDataPlugin
    {
        public static Plugin Static;
        public ILogger Logger { get; set; }

        public DataHandler DataHandler { get; private set; }
        private List<(DataSource data, ConfigGetter token)> _connections;

        private static readonly ConcurrentQueue<Action> DeferredActions = new ConcurrentQueue<Action>();
        private static readonly ConcurrentQueue<(string text, bool chat)> Messages = new ConcurrentQueue<(string text, bool chat)>();
        public static bool Started { get; private set; }

        private void AddConnection(Type connectionType, ConfigGetter configGetter)
        {
            if (!typeof(DataSource).IsAssignableFrom(connectionType))
            {
                Logger.WriteError($"{connectionType.Name} is not a DataSource");
                return;
            }

            var data = Activator.CreateInstance(connectionType, DataHandler, this);
            if (data is DataSource socketData)
                _connections.Add((socketData, configGetter));
        }

        public void Dispose()
        {
            DataHandler.Dispose();
            _connections.ForEach(tuple => tuple.data.Dispose());
        }

        public void Init(object gameInstance)
        {
            Static = this;
            Logger = new Logger();
            
            var path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().Location).Path)) + Path.DirectorySeparatorChar + "settings";
            Configuration.TokenConfig.Init(path);
            Configuration.PluginConfig.Init(path);

            
            DataHandler = new DataHandler(path, this);
            _connections = new List<(DataSource data, ConfigGetter token)>();
            AddConnection(typeof(StreamlabsDataSource), () => Configuration.Token.Get(config => config.StreamlabsToken));
            //AddConnection(typeof(StreamElementsDataSource), () => Configuration.Token.Get(config => config.StreamElementsToken));
            AddConnection(typeof(TwitchExtensionDataSource), () => Configuration.Token.Get(config => config.TwitchExtensionToken));
            AddConnection(typeof(IntegrationAppDataSource), () => Configuration.Token.Get(config => config.IntegrationAppEnabled) ? "dummy": "");

            var harmony = new Harmony("se.gorymoon.streamengineer");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Started = true;
            DeferredActions.ForEach(a => a.Invoke());
            MyScreenManager.ScreenAdded += ScreenAdded;
        }

        public void Update()
        {
        }

        public static void StartService()
        {
            if (!Sync.IsServer) return;

            Static._connections.ForEach(tuple =>
            {
                var token = tuple.token()?.Trim();
                if (!string.IsNullOrEmpty(token))
                    tuple.data.Init(token);
            });
        }

        public static void StopService()
        {
            if (Sync.IsServer)
                Static._connections.ForEach(tuple => tuple.data.Dispose());
        }

        public void ConnectionError(string name, string msg)
        {
            if (!Sync.IsDedicated)
                EnqueueMessage($"Unable to connect to '{name}' with message:\n{msg}.", false);
        }

        public static void EnqueueMessage(string msg, bool chat) => Messages.Enqueue((msg, chat));

        public void ScreenAdded(MyGuiScreenBase screenBase)
        {
            if (screenBase.GetType() == MyPerGameSettings.GUI.MainMenu &&
                Configuration.Plugin.Get(c => c.ShowMenuPopup))
            {
                Configuration.Plugin.Set(c => c.ShowMenuPopup, false);
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.OK,
                    new StringBuilder(
                        "Welcome to StreamEngineer\nTo get started you need to do some changes to the 'settings.toml' in the plugin folder.\nYou need to restart after changing any service settings,\nyou don't need to restart for settings related to events."),
                    new StringBuilder("StreamEngineer")));
            }
            else if (screenBase.GetType() == MyPerGameSettings.GUI.HUDScreen && !Messages.IsEmpty)
            {
                while (Messages.TryDequeue(out var msg))
                {
                    if (msg.chat)
                        Utils.SendChat(msg.text, true);
                    else
                    {
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error,
                            MyMessageBoxButtonsType.OK,
                            new StringBuilder(msg.text),
                            new StringBuilder("StreamEngineer")));
                    }
                }
            }
        }

        public static void RunOrDefer(Action action)
        {
            if (Started)
                action.Invoke();
            else
                DeferredActions.Enqueue(action);
        }
    }
}