using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lightchain.db
{
    public class websockerPeer : lightchain.httpserver.httpserver.IWebSocketPeer
    {
        System.Net.WebSockets.WebSocket websocket;
        public websockerPeer(System.Net.WebSockets.WebSocket websocket)
        {
            this.websocket = websocket;
        }
        public async Task OnConnect()
        {
            Console.WriteLine("websocket in:" + websocket.SubProtocol);
        }

        public async Task OnDisConnect()
        {
            Console.WriteLine("websocket gone:" + websocket.CloseStatus + "." + websocket.CloseStatusDescription);

        }

        public async Task OnRecv(byte[] message)
        {
            var txt = System.Text.Encoding.UTF8.GetString(message);
            Console.WriteLine("websocket read:" + txt);
        }
    }
}
