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

        private bool _running;
        private SocketIO _socketIo;

        protected abstract string Event { get; }
        protected abstract string Name { get; }
        protected abstract string Url { get; }

        protected SocketData(BaseDataHandler baseDataHandler)
        {
            BaseDataHandler = baseDataHandler;
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
            _socketIo.OnConnected += () => BaseDataHandler.Logger.WriteLine($"Connected to {Name}");
            _socketIo.OnClosed += OnSocketClosed;
            _socketIo.OnError += (args) =>
            {
                BaseDataHandler.Logger.WriteLine(args.Text);
                _socketIo.CloseAsync();
            };
            _socketIo.On(Event, args =>
            {
                try
                {
                    BaseDataHandler.Logger.WriteLine("Message: " + args.Text);
                    OnSocketMessage(args.Text);
                }
                catch (Exception e)
                {
                    BaseDataHandler.Logger.WriteLine(e);
                }
            });
            _running = true;
            Connect();
        }

        protected abstract void OnSocketMessage(string text);

        private async void OnSocketClosed(ServerCloseReason reason)
        {
            BaseDataHandler.Logger.WriteLine($"Closed {Name}: {reason}");
            if (reason != ServerCloseReason.ClosedByClient)
            {
                await Connect();
            }

            _running = false;
        }

        private async Task Connect()
        {
            while (_running)
            {
                BaseDataHandler.Logger.WriteLine($"Reconnecting {Name}");
                for (var i = 0; i < 3; i++)
                    try
                    {
                        await _socketIo.ConnectAsync();
                        return;
                    }
                    catch (WebSocketException ex)
                    {
                        BaseDataHandler.Logger.WriteLine(ex.Message);
                        await Task.Delay(2000);
                    }
                    catch (AggregateException ex)
                    {
                        BaseDataHandler.Logger.WriteLine(ex.Message);
                        await Task.Delay(2000);
                    }
                    catch (TimeoutException ex)
                    {
                        BaseDataHandler.Logger.WriteLine(ex.Message);
                        await Task.Delay(2000);
                    }

                BaseDataHandler.Logger.WriteLine($"{Name}: Tried to reconnect 3 times, unable to connect to the server");
                await Task.Delay(10000);
            }
        }
    }
}