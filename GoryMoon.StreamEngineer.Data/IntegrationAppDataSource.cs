using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public class IntegrationAppDataSource : DataSource
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }
        private Socket _integrationSocket;

        public IntegrationAppDataSource(BaseDataHandler dataHandler, IDataPlugin plugin) : base(dataHandler, plugin)
        {
        }

        public override void Dispose()
        {
            try
            {
                _integrationSocket?.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Plugin.Logger.WriteError(e, "Error closing integration socket");
            }
            
            try
            {
                _integrationSocket?.Close();
            }
            catch (Exception e)
            {
                Plugin.Logger.WriteError(e, "Error closing integration socket");
            }

            CancellationTokenSource?.Cancel();
        }

        public override void Init(string token)
        {
            CancellationTokenSource?.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            
            ThreadPool.QueueUserWorkItem(Run);
        }

        private void Run(object state)
        {
            var cancellationToken = CancellationTokenSource.Token;
            IntegrationRunning = true;
            const string id = "646de02049c82f57ac27df05";
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var numArray = new byte[1024];
                    var address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                    var remoteEp = new IPEndPoint(address, 23491);
                    
                    Plugin.Logger.WriteAndChat("[IntegrationApp] Connecting");
                    using (_integrationSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                    {
                        try
                        {
                            _integrationSocket.Connect(remoteEp);
                            IntegrationConnected = true;
                            Plugin.Logger.WriteAndChat("[IntegrationApp] Connected");

                            _integrationSocket.Send(Encoding.UTF8.GetBytes(new JObject
                            {
                                ["type"] = "INIT",
                                ["id"] = id
                            }.ToString()));

                            var num = 0;
                            while (!cancellationToken.IsCancellationRequested && _integrationSocket.Connected)
                            {
                                num += _integrationSocket.Receive(numArray, num, numArray.Length - num, SocketFlags.None);
                                if (num == 0)
                                    break;
                                try
                                {
                                    var str = Encoding.UTF8.GetString(numArray, 0, num).Trim();
                                    Plugin.Logger.WriteLine($"[IntegrationApp] Received: {str}");
                                    num = 0;
                                    var obj = JObject.Parse(str);
                                    var type = obj.Value<string>("type");
                                    if ("ACTION".Equals(type))
                                        DataHandler.OnIntegrationApp(obj.Value<JObject>("data"));
                                    else if ("SHUTDOWN".Equals(type))
                                        break;
                                }
                                catch (Exception e)
                                {
                                    Plugin.Logger.WriteError(e, "[IntegrationApp] Error parsing msg");
                                }
                            }
                        }
                        catch (SocketException e)
                        {
                            if (!e.Message.StartsWith("No connection could be made because the target machine actively refused it")
                                && !e.Message.StartsWith("A blocking operation was interrupted by a call to WSACancelBlockingCall"))
                                Plugin.Logger.WriteError(e, "[IntegrationApp] Unexpected exception");
                        }
                        catch (Exception e)
                        {
                            Plugin.Logger.WriteError(e, "[IntegrationApp] Unexpected exception");
                        }
                    }

                    if (IntegrationConnected) 
                        Plugin.Logger.WriteLine("[IntegrationApp] Socket Disconnect");

                    IntegrationConnected = false;
                }
                catch (Exception e)
                {
                    Plugin.Logger.WriteError(e, "[IntegrationApp] Error from running integration");
                }

                if (!cancellationToken.IsCancellationRequested)
                    cancellationToken.WaitHandle.WaitOne(5000);
            }

            IntegrationConnected = false;
            IntegrationRunning = false;
        }

        public bool IntegrationConnected { get; set; }

        public bool IntegrationRunning { get; set; }
    }
}