using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace SkipTTS
{
    class Program
    {
        private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);
        private static string token;


        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                token = args[0];
            } else
            {
                ReadToken();
            }
            
            var url = new Uri("wss://realtime.streamelements.com/socket.io/?cluster=main&EIO=3&transport=websocket");
            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);

                client.MessageReceived.Subscribe(msg => {
                    string resp = Response(msg);
                    if(resp != null)
                    {
                        Task.Run(() => {
                            client.Send(resp);
                            if (resp.Contains("skip"))
                            {
                                ExitEvent.Set();
                            }
                         });
                    }
                  });

                client.Start().Wait();

                
                ExitEvent.WaitOne();
            }
        }

        private static string Response(ResponseMessage message)
        {
            string msg = message.Text;
            if (msg == "40")
            {
                return $"42[\"authenticate\",{{ \"method\":\"apikey\",\"token\":\"{token}\" }}]";
            } else if (msg.Contains("authenticated"))
            {
                return "422[\"event:skip\",null]";
            }
            return null;
        }

        private static void ReadToken()
        {
            token = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "token.txt"));
        }
    }
}
