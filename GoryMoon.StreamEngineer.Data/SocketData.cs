using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SocketIOClient;

namespace GoryMoon.StreamEngineer.Data
{
    public abstract class SocketData: IDisposable
    {
        protected readonly BaseDataHandler BaseDataHandler;
        private readonly IDataPlugin _plugin;

        private bool _running;
        private SocketIO _socketIo;

        protected abstract string[] Event { get; }
        protected abstract string Name { get; }
        protected abstract string Url { get; }

        protected SocketData(BaseDataHandler baseDataHandler, IDataPlugin plugin)
        {
            BaseDataHandler = baseDataHandler;
            _plugin = plugin;
        }

        public void Dispose()
        {
            if (_running)
            {
                _socketIo?.CloseAsync();
                _running = false;
            }
        }

        protected void Init(Dictionary<string, string> parameters)
        {
            _socketIo = new SocketIO(Url)
            {
                Parameters = parameters
            };
            _socketIo.OnConnected += () => _plugin.Logger.WriteLine($"Connected to {Name}");
            _socketIo.OnClosed += OnSocketClosed;
            _socketIo.OnError += (args) =>
            {
                _plugin.ConnectionError(Name, args.Text);
                _plugin.Logger.WriteLine(args.Text);
                _socketIo.CloseAsync();
            };
            foreach (var s in Event)
            {
                _socketIo.On(s, args =>
                {
                    try
                    {
                        _plugin.Logger.WriteLine("Event: " + s + ", Message: " + args.Text);
                        OnSocketMessage(s, args.Text);
                    }
                    catch (Exception e)
                    {
                        _plugin.Logger.WriteLine(e);
                    }
                });
            }
            _running = true;
            Connect();
        }

        protected abstract void OnSocketMessage(string s, string text);

        private async void OnSocketClosed(ServerCloseReason reason)
        {
            _plugin.Logger.WriteLine($"Closed {Name}: {reason}");
            if (reason != ServerCloseReason.ClosedByClient)
            {
                await Connect();
            }
            else
            {
                _running = false;
            }
        }

        private async Task Connect()
        {
            while (_running)
            {
                _plugin.Logger.WriteLine($"Reconnecting {Name}");
                for (var i = 0; i < 3; i++)
                    try
                    {
                        await _socketIo.ConnectAsync();
                        return;
                    }
                    catch (Exception ex)
                    {
                        _plugin.Logger.WriteLine(ex.Message);
                        _plugin.Logger.WriteLine(ex.StackTrace);
                        await Task.Delay(2000);
                    }

                _plugin.Logger.WriteLine($"{Name}: Tried to reconnect 3 times, unable to connect to the server");
                await Task.Delay(10000);
            }
        }
    }
}