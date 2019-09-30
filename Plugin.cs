using System;
using System.IO;
using System.Text;
using GoryMoon.StreamEngineer.Config;
using GoryMoon.StreamEngineer.Data;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage.Collections;
using VRage.Plugins;

namespace GoryMoon.StreamEngineer
{
    public class Plugin : IPlugin
    {
        private DataHandler _dataHandler;
        private StreamlabsData _streamlabsData;

        public static Plugin Static { get; private set; }

        public void Dispose()
        {
            
        }

        public void Init(object gameInstance)
        {
            Static = this;
            Configuration.Init(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(Plugin).Assembly.CodeBase).Path)));

            _dataHandler = new DataHandler();
            _streamlabsData = new StreamlabsData(_dataHandler);

            //MyScreenManager.ScreenAdded += ScreenAdded;
        }

        public static void StartService()
        {
            var token = Configuration.Config.Get(c => c.Streamlabs.Token).Trim();
            if (token.Length > 0)
            {
                Static._streamlabsData.Init(token);
            }
        }
        
        public static void StopService()
        {
            Static._streamlabsData.Dispose();
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