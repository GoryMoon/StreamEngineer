using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                _socketIo?.DisconnectAsync();
                _running = false;
            }
        }

        protected void Init(Dictionary<string, string> parameters)
        {
            _socketIo = new SocketIO(Url, new SocketIOOptions
            {
                Query = parameters,
                EnabledSslProtocols = (SslProtocols)12288 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
            });
            _socketIo.OnConnected += (sender, e) => _plugin.Logger.WriteLine($"Connected to {Name}");
            _socketIo.OnDisconnected += OnSocketClosed;
            _socketIo.OnError += (sender, args) =>
            {
                _plugin.ConnectionError(Name, args);
                _plugin.Logger.WriteLine(args);
                _socketIo.DisconnectAsync();
            };
            foreach (var s in Event)
            {
                _socketIo.On(s, args =>
                {
                    try
                    {
                        var jObject = args.GetValue<JObject>();
                        _plugin.Logger.WriteLine("Event: " + s + ", Message: " + jObject.ToString(Formatting.None));
                        OnSocketMessage(s, jObject);
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

        protected abstract void OnSocketMessage(string s, JObject obj);

        private async void OnSocketClosed(object sender, string s)
        {
            _plugin.Logger.WriteLine($"Closed {Name}: {s}");
            if (s != null)
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
                    catch (AggregateException err){
                        foreach (var errInner in err.InnerExceptions) {
                            _plugin.Logger.WriteLine(errInner.Message);
                            _plugin.Logger.WriteLine(errInner.StackTrace);
                        }
                        await Task.Delay(2000); 
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