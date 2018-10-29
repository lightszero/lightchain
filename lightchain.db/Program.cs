using Microsoft.AspNetCore.Http;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace lightchain.db
{
    class Program
    {

        static void Main(string[] args)
        {
            LightChainDB db = new LightChainDB();
            db.Init("./testdb");
            var server = new lightchain.httpserver.httpserver();
            server.SetWebsocketAction("/ws", OnWebSocket);
            server.SetFailAction(OnHttp404);

            server.Start(80);//一个参数，只开80端口
            var version = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine("lightchain " + version);
            while (true)
            {
                Console.Write("-->");
                Console.ReadLine();
            }
        }

        static async Task OnHttp404(HttpContext context)
        {
            await context.Response.WriteAsync("this server must be connect with websocket.");
        }
        static async Task OnWebSocket(lightchain.httpserver.httpserver.WebsocketEventType type, System.Net.WebSockets.WebSocket socket, byte[] message)
        {
            if (type == lightchain.httpserver.httpserver.WebsocketEventType.Connect)
            {
                Console.WriteLine("websocket in:" + socket.SubProtocol);
            }
            if (type == lightchain.httpserver.httpserver.WebsocketEventType.Recieve)
            {
                var txt = System.Text.Encoding.UTF8.GetString(message);
                Console.WriteLine("websocket read:" + txt);
            }
            if (type == lightchain.httpserver.httpserver.WebsocketEventType.Disconnect)
            {
                Console.WriteLine("websocket gone:" + socket.CloseStatus + "." + socket.CloseStatusDescription);
            }
        }

    }
}
