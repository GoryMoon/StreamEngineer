using System;
using System.IO;
using System.Text;
using GoryMoon.StreamEngineer.Actions;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Sandbox.Game;
using Sandbox.Game.Multiplayer;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

namespace GoryMoon.StreamEngineer
{
    public class Plugin : IPlugin
    {
        public static Plugin Static;
        public Logger Logger { get; private set; }
        private DataHandler _dataHandler;
        private StreamlabsData _streamlabsData;

        public void Dispose()
        {
            _dataHandler.Dispose();
            _streamlabsData.Dispose();
        }

        public void Init(object gameInstance)
        {
            Static = this;
            var path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(Plugin).Assembly.CodeBase).Path));
            Configuration.TokenConfig.Init(path);
            Configuration.PluginConfig.Init(path);
            
            Logger = new Logger();
            _dataHandler = new DataHandler(path);
            _streamlabsData = new StreamlabsData(_dataHandler);

            //MyScreenManager.ScreenAdded += ScreenAdded;
        }

        public void Update()
        {
        }

        public static void StartService()
        {
            if (!Sync.IsServer) return;

            var token = Configuration.Token.Get(c => c.StreamlabsToken).Trim();
            if (token.Length > 0) Static._streamlabsData.Init(token);
        }

        public static void StopService()
        {
            if (Sync.IsServer) Static._streamlabsData.Dispose();
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
    }
}