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
        private readonly MyConcurrentQueue<Action> _meteorQueue = new MyConcurrentQueue<Action>(32);
        
        private MySandboxGame _game;
        private int _mUpdateCounter;

        private bool _registeredReplication;
        private bool _sessionLoaded;
        private DataHandler _dataHandler;
        private StreamlabsData _streamlabsData;

        public static Plugin Static { get; private set; }

        public MySandboxGame Game => _game;

        public void Dispose()
        {
            _streamlabsData.Dispose();
        }

        public void Init(object gameInstance)
        {
            Static = this;
            _game = (MySandboxGame) gameInstance;
            Configuration.Init(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(Plugin).Assembly.CodeBase).Path)));

            _dataHandler = new DataHandler();
            _streamlabsData = new StreamlabsData(_dataHandler);
            var token = Configuration.Config.Get(c => c.Streamlabs.Token).Trim();
            if (token.Length > 0)
            {
                _streamlabsData.Init(token);
            }
            
            
            MyScreenManager.ScreenAdded += ScreenAdded;
        }

        public void Update()
        {
            if (MyMultiplayer.ReplicationLayer != null && !_registeredReplication)
            {
                _registeredReplication = true;
                MyMultiplayer.ReplicationLayer.RegisterFromAssembly(typeof(Plugin).Assembly);
            }

            if (!MySandboxGame.IsGameReady) return;

            if (MySession.Static != null && _sessionLoaded == false)
            {
                _sessionLoaded = true;
                OnSessionLoaded();
            }
            else if (MySession.Static == null && _sessionLoaded)
            {
                _sessionLoaded = false;
                OnSessionUnloaded();
            }

            if (_sessionLoaded)
            {
                _game.Invoke(OnSessionUpdate, "OnSessionUpdate");
            }
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

        public void OnSessionUnloaded()
        {
            _meteorQueue.Clear();
        }

        public void OnSessionLoaded()
        {
        }

        public void OnSessionUpdate()
        {
            ++_mUpdateCounter;
            if (_meteorQueue.Count > 0 && _mUpdateCounter % 30 == 0)
            {
                _mUpdateCounter = 0;
                if (_meteorQueue.TryDequeue(out var action)) action.Invoke();
            }
        }

        public void QueueMeteors()
        {
            _meteorQueue.Enqueue(() => MeteorShower.MeteorWave());
        }
    }
}