using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using CommandLine;

namespace SkipTTS
{
    class Program
    {
        private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);
        private static string token = null;
        private static string reqResp;

        public class Options
        {
            [Option('p', "pause", Required = false, HelpText = "Set alerts to pause.")]
            public bool Pause { get; set; }
            [Option('s', "skip", Required = false, HelpText = "Skip alert.")]
            public bool Skip { get; set; }
            [Option('r', "resume", Required = false, HelpText = "Set alerts to resume.")]
            public bool Resume { get; set; }
            [Option('m', "Mute", Required = false, HelpText = "Set alerts to mute.")]
            public bool Mute { get; set; }
            [Option('u', "unmute", Required = false, HelpText = "Set alerts to unmute.")]
            public bool Unmute { get; set; }
        }


        static void Main(string[] args)
        {
            ReadToken();

            Func<ResponseMessage, string> method = null;

            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Pause)
                       {
                           method = Pause;
                           reqResp = "togglequeue";
                       }
                       if (o.Skip)
                       {
                           method = Skip;
                           reqResp = "skip";
                       }
                       if (o.Resume)
                       {
                           method = Resume;
                           reqResp = "togglequeue";
                       }
                       if (o.Mute)
                       {
                           method = Mute;
                           reqResp = "overlay:mute";
                       }
                       if (o.Unmute)
                       {
                           method = Unmute;
                           reqResp = "overlay:mute";
                       }
                   });

            if (method == null || token == null)
            {
                Environment.Exit(-1);
            }

            var url = new Uri("wss://realtime.streamelements.com/socket.io/?cluster=main&EIO=3&transport=websocket");
            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);

                client.MessageReceived.Subscribe(msg => {
                    string resp = method(msg);
                    if(resp != null)
                    {
                        Task.Run(() => {
                            client.Send(resp);
                            if (resp.Contains(reqResp))
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

        private static string Skip(ResponseMessage message)
        {
            string msg = message.Text;
            if (msg == "40")
            {
                return $"42[\"authenticate\",{{ \"method\":\"apikey\",\"token\":\"{token}\" }}]";
            }
            else if (msg.Contains("authenticated"))
            {
                return "422[\"event:skip\",null]";
            }
            return null;
        }

        private static string Resume(ResponseMessage message)
        {
            string msg = message.Text;
            if (msg == "40")
            {
                return $"42[\"authenticate\",{{ \"method\":\"apikey\",\"token\":\"{token}\" }}]";
            } else if (msg.Contains("authenticated"))
            {
                return "422[\"overlay:togglequeue\",true]";
            }
            return null;
        }

        private static string Pause(ResponseMessage message)
        {
            string msg = message.Text;
            if (msg == "40")
            {
                return $"42[\"authenticate\",{{ \"method\":\"apikey\",\"token\":\"{token}\" }}]";
            }
            else if (msg.Contains("authenticated"))
            {
                return "422[\"overlay:togglequeue\",false]";
            }
            return null;
        }
                
        private static string Mute(ResponseMessage message)
        {
            string msg = message.Text;
            if (msg == "40")
            {
                return $"42[\"authenticate\",{{ \"method\":\"apikey\",\"token\":\"{token}\" }}]";
            }
            else if (msg.Contains("authenticated"))
            {
                return "422[\"overlay:mute\", {\"muted\":true}]";
            }
            return null;
        }
               
        private static string Unmute(ResponseMessage message)
        {
            string msg = message.Text;
            if (msg == "40")
            {
                return $"42[\"authenticate\",{{ \"method\":\"apikey\",\"token\":\"{token}\" }}]";
            }
            else if (msg.Contains("authenticated"))
            {
                return "422[\"overlay:mute\", {\"muted\":false}]";
            }
            return null;
        }



        private static void ReadToken()
        {
            token = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "token.txt"));
        }
    }
}
