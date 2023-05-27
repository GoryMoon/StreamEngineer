using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using SocketIOClient.Transport;

namespace GoryMoon.StreamEngineer.Data
{
    public abstract class SocketDataSource : DataSource
    {
        private bool _running;
        private SocketIO _socketIo;

        protected SocketDataSource(BaseDataHandler dataHandler, IDataPlugin plugin) : base(dataHandler, plugin)
        {
        }

        protected abstract string[] Event { get; }
        protected abstract string Name { get; }
        protected abstract string Url { get; }

        public override void Dispose()
        {
            if (_running)
            {
                _running = false;
                _socketIo?.DisconnectAsync();
            }
        }

        protected void Init(Dictionary<string, string> parameters, EngineIO engine = EngineIO.V3,
            TransportProtocol transport = TransportProtocol.Polling)
        {
            _socketIo = new SocketIO(Url, new SocketIOOptions
            {
                Query = parameters,
                EIO = engine,
                Transport = transport
            });
            var jsonSerializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            });
            _socketIo.JsonSerializer = jsonSerializer;
            _socketIo.OnConnected += (sender, e) => OnConnected(_socketIo);
            _socketIo.OnDisconnected += OnSocketClosed;
            _socketIo.OnError += (sender, args) =>
            {
                Plugin.ConnectionError(Name, args);
                Plugin.Logger.WriteError($"[{Name}] {args}");
                _socketIo.DisconnectAsync();
            };
            foreach (var s in Event)
                _socketIo.On(s, args =>
                {
                    try
                    {
                        var jObject = args.GetValue<JObject>();
                        Plugin.Logger.WriteLine($"[{Name}] Event: {s}, Message: {jObject.ToString(Formatting.None)}");
                        OnSocketMessage(s, jObject);
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.WriteError(e);
                    }
                });
            _running = true;
            Connect();
        }

        protected abstract void OnSocketMessage(string s, JObject obj);

        protected virtual void OnConnected(SocketIO socket)
        {
            Plugin.Logger.WriteAndChat($"[{Name}] Connected Socket");
        }

        private async void OnSocketClosed(object sender, string s)
        {
            Plugin.Logger.WriteLine($"[{Name}] Closed Socket: {s}");
            if (s != null)
                await Connect();
            else
                _running = false;
        }

        private async Task Connect()
        {
            while (_running)
            {
                Plugin.Logger.WriteLine($"[{Name}] Reconnecting");
                for (var i = 0; i < 3; i++)
                    try
                    {
                        await _socketIo.ConnectAsync();
                        return;
                    }
                    catch (AggregateException err)
                    {
                        foreach (var errInner in err.InnerExceptions)
                        {
                            Plugin.Logger.WriteError(errInner.Message);
                            Plugin.Logger.WriteError(errInner.StackTrace);
                        }

                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.WriteError(ex.Message);
                        Plugin.Logger.WriteError(ex.StackTrace);
                        await Task.Delay(2000);
                    }

                Plugin.Logger.WriteLine($"[{Name}] Tried to reconnect 3 times, unable to connect to the server");
                await Task.Delay(10000);
            }
        }
    }
}