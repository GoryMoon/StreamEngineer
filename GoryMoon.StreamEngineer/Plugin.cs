using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GoryMoon.StreamEngineer.Actions;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Harmony;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

namespace GoryMoon.StreamEngineer
{
    public class Plugin : IPlugin
    {
        public static Plugin Static;
        public const bool Dev = true;
        public Logger Logger { get; private set; }
        public DataHandler DataHandler { get; private set; }
        private StreamlabsData _streamlabsData;
        private TwitchExtensionData _twitchExtensionData;

        private static readonly ConcurrentQueue<Action> DeferredActions = new ConcurrentQueue<Action>();
        public static bool Started { get; private set; }

        public void Dispose()
        {
            DataHandler.Dispose();
            _streamlabsData.Dispose();
            _twitchExtensionData.Dispose();
        }

        public void Init(object gameInstance)
        {
            Static = this;
            var path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().Location).Path));
            Configuration.TokenConfig.Init(path);
            Configuration.PluginConfig.Init(path);
            
            Logger = new Logger();
            Logger.WriteLine(Assembly.GetExecutingAssembly().Location);
            Logger.WriteLine(path);
            DataHandler = new DataHandler(path);
            _streamlabsData = new StreamlabsData(DataHandler);
            _twitchExtensionData = new TwitchExtensionData(DataHandler);
            
            var harmony = HarmonyInstance.Create("se.gorymoon.streamengineer");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Started = true;
            DeferredActions.ForEach(a => a.Invoke());
            //MyScreenManager.ScreenAdded += ScreenAdded;
        }

        public void Update()
        {
        }

        public static void StartService()
        {
            if (!Sync.IsServer) return;

            var token = Configuration.Token.Get(c => c.StreamlabsToken)?.Trim();
            if (!string.IsNullOrEmpty(token)) Static._streamlabsData.Init(token);
            
            var twitchToken = Configuration.Token.Get(c => c.TwitchExtensionToken)?.Trim();
            if (!string.IsNullOrEmpty(twitchToken)) Static._twitchExtensionData.Init(twitchToken);
        }

        public static void StopService()
        {
            if (Sync.IsServer)
            {
                Static._streamlabsData.Dispose();
                Static._twitchExtensionData.Dispose();
            }
        }

        public void ScreenAdded(MyGuiScreenBase screenBase)
        {
            if (screenBase.GetType() == MyPerGameSettings.GUI.MainMenu && Configuration.Plugin.Get(c => c.ShowMenuPopup))
                //Configuration.Config.Set(c => c.ShowMenuPopup, false);
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.OK,
                    new StringBuilder(
                        "Welcome to StreamEngineer\nTo get started you need to do some changes to the 'settings.toml' in the plugin folder.\nYou need to restart after changing any service settings,\nyou don't need to restart for settings related to events."),
                    new StringBuilder("StreamEngineer")));
        }

        public static void RunOrDefer(Action action)
        {
            if (Started)
            {
                action.Invoke();
            }
            else
            {
                DeferredActions.Enqueue(action);
            }
        }
    }
}