using System;
using System.IO;
using System.Threading.Tasks;

namespace lightdb.testclient
{
    class Program
    {

        static lightdb.sdk.Client client = new lightdb.sdk.Client();

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
                if (line == "p")
                    await ping();
            }
        }
        static async Task ping()
        {
            var pingms = await client.Ping();
            Console.WriteLine("ping=" + pingms);
        }
        static async void StartClient()
        {
            client.OnDisconnect += async () =>
            {
                Console.WriteLine("OnDisConnect.");
            };
            client.OnRecv_Unknown += async (msg) =>
              {
                  Console.WriteLine("got unknown msg:" + msg.Cmd);
              };
            await client.Connect(new Uri("ws://127.0.0.1:80/ws"));
            Console.WriteLine("connected.");

            for (var i = 0; i < 1; i++)
            {
                await ping();
            }
        }


    }


}
