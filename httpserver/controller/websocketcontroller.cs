using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace lightchain.httpserver
{
    public class WebSocketController : IController
    {

        httpserver.onProcessWebsocket onEvent;
        public WebSocketController(httpserver.onProcessWebsocket onEvent)
        {
            this.onEvent = onEvent;
        }
        public async Task ProcessAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket websocket = null;
                try
                {
                    websocket = await context.WebSockets.AcceptWebSocketAsync();
                    await onEvent(httpserver.WebsocketEventType.Connect, websocket);
                }
                catch
                {
                    Console.WriteLine("error on connect.");
                }
                try
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(1024 * 1024);
                    while (websocket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        ArraySegment<byte> buffer = System.Net.WebSockets.WebSocket.CreateServerBuffer(1024);
                        var recv = await websocket.ReceiveAsync(buffer, System.Threading.CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, recv.Count);
                        if (recv.EndOfMessage)
                        {
                            var count = ms.Position;
                            var bytes = new byte[count];
                            ms.Position = 0;
                            ms.Read(bytes, 0, (int)count);

                            ms.Position = 0;
                            await onEvent(httpserver.WebsocketEventType.Recieve, websocket, bytes);
                        }
                        //Console.WriteLine("recv=" + recv.Count + " end=" + recv.EndOfMessage);
                    }
                }
                catch(Exception err)
                {
                    Console.WriteLine("error on recv.");
                }
                try
                {
                    //await context.Response.WriteAsync("");
                    await onEvent(httpserver.WebsocketEventType.Disconnect, websocket);
                }
                catch(Exception err)
                {
                    Console.WriteLine("error on disconnect.");
                }

            }
            else
            {

            }

        }
    }
}
