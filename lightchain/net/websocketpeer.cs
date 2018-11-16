using lightdb.sdk;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lightdb.server
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
            Console.CursorLeft = 0;

            Console.WriteLine("websocket in:" + websocket.SubProtocol);
        }

        public async Task OnDisConnect()
        {
            Console.CursorLeft = 0;

            Console.WriteLine("websocket gone:" + websocket.CloseStatus + "." + websocket.CloseStatusDescription);

        }
        private async Task SendToClient(NetMessage msg)
        {
            await this.websocket.SendAsync(msg.ToBytes(), System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
        }
        public async Task OnRecv(System.IO.MemoryStream stream, int recvcount)
        {
            var p = stream.Position;
            var msg = NetMessage.Unpack(stream);
            var pend = stream.Position;
            if (pend - p > recvcount)
                throw new Exception("error net message.");

            var iddata = msg.Params["_id"];
            if (iddata.Length != 8)
                throw new Exception("error net message _id");

            switch (msg.Cmd)
            {
                case "_ping":
                    await OnPing(msg, iddata);
                    break;
                default:
                    throw new Exception("unknown msg cmd:" + msg.Cmd);
            }
        }
        public async Task OnPing(NetMessage msgRecv,byte[] id)
        {
            var msg = NetMessage.Create("_pingback");
            msg.Params["_id"] = id;

            await SendToClient(msg);

        }
    }
}
