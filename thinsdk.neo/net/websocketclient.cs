using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace lightdb.sdk
{
    public interface IWebSocketPeer
    {
        Task OnConnect(System.Net.WebSockets.WebSocket websocket);
        Task OnRecv(NetMessage msg);
        Task OnDisConnect();
    }
    public static class Helper
    {
        private async static Task Send(this System.Net.WebSockets.WebSocket websocket, sdk.NetMessage msg)
        {
            byte[] data = msg.ToBytes();
            await websocket.SendAsync(new ArraySegment<byte>(data), System.Net.WebSockets.WebSocketMessageType.Binary, true, System.Threading.CancellationToken.None);
        }
    }
    public class Client
    {
        public static async Task Start(Uri uri, IWebSocketPeer peer)
        {
            System.Net.WebSockets.ClientWebSocket websocket = new System.Net.WebSockets.ClientWebSocket();
            //connect
            try
            {
                await websocket.ConnectAsync(uri, System.Threading.CancellationToken.None);
                peer.OnConnect(websocket);
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;
                Console.WriteLine("error on connect." + err.Message);
            }
            //recv
            try
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(1024 * 1024))
                {
                    while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        ArraySegment<byte> buffer = System.Net.WebSockets.WebSocket.CreateServerBuffer(1024);
                        var recv = await websocket.ReceiveAsync(buffer, System.Threading.CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, recv.Count);
                        if (recv.EndOfMessage)
                        {
                            var count = ms.Position;
                            //var bytes = new byte[count];
                            ms.Position = 0;
                            //ms.Read(bytes, 0, (int)count);

                            //ms.Position = 0;

                            var msg = NetMessage.Unpack(ms);
                            var posend = ms.Position;
                            if (posend != count)
                                throw new Exception("error msg.");
                            await peer.OnRecv(msg);// .onEvent(httpserver.WebsocketEventType.Recieve, websocket, bytes);
                        }
                        //Console.WriteLine("recv=" + recv.Count + " end=" + recv.EndOfMessage);
                    }
                }
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;

                Console.WriteLine("error on recv.");
            }
            //disconnect
            try
            {
                //await context.Response.WriteAsync("");
                await peer.OnDisConnect();// onEvent(httpserver.WebsocketEventType.Disconnect, websocket);
            }
            catch (Exception err)
            {
                Console.CursorLeft = 0;

                Console.WriteLine("error on disconnect.");
            }


        }
    }
}
