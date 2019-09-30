using System;
using System.IO;
using System.Text;
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
        private DataHandler _dataHandler;
        private StreamlabsData _streamlabsData;

        private static Plugin _static;

        public void Dispose()
        {
            
        }

        public void Init(object gameInstance)
        {
            _static = this;
            Configuration.Init(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(Plugin).Assembly.CodeBase).Path)));

            _dataHandler = new DataHandler();
            _streamlabsData = new StreamlabsData(_dataHandler);

            //MyScreenManager.ScreenAdded += ScreenAdded;
        }

        public static void StartService()
        {
            if (!Sync.IsServer) return;
            
            var token = Configuration.Config.Get(c => c.Streamlabs.Token).Trim();
            if (token.Length > 0)
            {
                _static._streamlabsData.Init(token);
            }
        }
        
        public static void StopService()
        {
            if (Sync.IsServer) _static._streamlabsData.Dispose();
        }

        public void Update()
        {

        }

        public void ScreenAdded(MyGuiScreenBase screenBase)
        {
            if (screenBase.GetType() == MyPerGameSettings.GUI.MainMenu && Configuration.Config.Get(c => c.ShowMenuPopup))
            {
                //Configuration.Config.Set(c => c.ShowMenuPopup, false);
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info,
                    MyMessageBoxButtonsType.OK,
                    new StringBuilder(
                        "Welcome to StreamEngineer\nTo get started you need to do some changes to the 'settings.toml' in the plugin folder.\nYou need to restart after changing any service settings,\nyou don't need to restart for settings related to events."),
                    new StringBuilder("StreamEngineer")));
                
            }
        }

    }
}