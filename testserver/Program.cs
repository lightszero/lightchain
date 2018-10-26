using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace testserver
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new lightchain.httpserver.httpserver();
            server.SetHttpAction("/test1", onTest01);
            server.SetHttpAction("/test2", onTest02);
            server.SetHttpAction("/test3", onTest03);
            server.SetWebsocketAction("/ws", onTestWs01);

            System.Threading.ThreadPool.SetMaxThreads(1000, 1000);
            server.Start(80);


            Console.WriteLine("http server open on http://*:80");

            //main loop
            while (true)
            {
                Console.Write("-->");
                Console.ReadLine();
            }
        }
        static async Task onTest01(HttpContext context)
        {
            await context.Response.WriteAsync("hello world.");
        }

        static async Task onTest02(HttpContext context)
        {
            await Task.Delay(1000);
            await context.Response.WriteAsync("slow.");
        }

        static async Task onTest03(HttpContext context)
        {
            System.Threading.Thread.Sleep(1000);
            await context.Response.WriteAsync("dead.");
        }

        static async Task onTestWs01(lightchain.httpserver.httpserver.WebsocketEventType type, System.Net.WebSockets.WebSocket socket, byte[] message)
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
