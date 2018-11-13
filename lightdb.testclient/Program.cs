using System;
using System.IO;
using System.Threading.Tasks;

namespace lightdb.testclient
{
    class Program
    {
        static void Main(string[] args)
        {
            StartClient();
            Loops();
        }
        static async void Loops()
        {


            Console.WriteLine("Hello World!");
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    Environment.Exit(0);
                    return;

                }
            }
        }
        static async void StartClient()
        {
            await lightdb.sdk.Client.Start(new Uri("ws://127.0.0.1:80/ws"), new Peer());
        }


    }
    class Peer : lightdb.sdk.IWebSocketPeer
    {
        System.Net.WebSockets.WebSocket websocket;
        public async Task OnConnect(System.Net.WebSockets.WebSocket websocket)
        {
            this.websocket = websocket;
            Console.WriteLine("connected.");
            var msg = sdk.NetMessage.Create("_ping");
            msg.Params["_id"] = new byte[4];
            await Send(msg);
        }
        private async Task Send(sdk.NetMessage msg)
        {
            await websocket.SendAsync(msg.ToBytes(), System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
        }
        public async Task OnDisConnect()
        {
            Console.WriteLine("OnDisConnect.");
            this.websocket = null;
        }

        public async Task OnRecv(lightdb.sdk.NetMessage msg)
        {
            Console.WriteLine("msg got=" + msg.Cmd);
        }
    }

}
