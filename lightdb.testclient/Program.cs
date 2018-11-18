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
            TaskScheduler.UnobservedTaskException += (s, e) =>
              {
                  Console.WriteLine("error on ============>" + e.ToString());
              };
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
                {
                    try
                    {
                        await ping();
                        //Task.WaitAll(ping());
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("error on ping.");
                    }
                    continue;
                }
            }
        }
        static async Task ping()
        {

            try
            {
                var pingms = await client.Ping();
                Console.WriteLine("ping=" + pingms);

            }
            catch (Exception err)
            {
                Console.WriteLine("error client. ping.");
            }
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

            for (var i = 0; i < 100; i++)
            {
                await ping();
            }
        }


    }


}
