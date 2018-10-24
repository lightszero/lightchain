using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace httpserver_testor
{
    class Program
    {
        static void ShowMenu()
        {
            Console.WriteLine("1)test http:*:80/test1 10000 times");
            Console.WriteLine("2)test http:*:80/test2 1000 times");
            Console.WriteLine("3)test http:*:80/test3 300 times");
            Console.WriteLine("type number to test.");
        }
        static void Main(string[] args)
        {
            ShowMenu();

            while (true)
            {
                Console.Write("-->");
                var line = Console.ReadLine();
                if (line.ToLower() == "help")
                {
                    ShowMenu();
                }
                if (line.ToLower() == "1")
                {
                    Task.WaitAll(Test1("http://127.0.0.1:80/test1",100));
                }
                if (line.ToLower() == "2")
                {
                    Task.WaitAll(Test1("http://127.0.0.1:80/test2",10));
                }
                if (line.ToLower() == "3")
                {
                    Task.WaitAll(Test1("http://127.0.0.1:80/test3",3));
                }
            }
        }

        static int finishCount = 0;
        static DateTime timer = DateTime.Now;
        static async Task Test1(string url,int linecount)
        {
            ThreadPool.SetMaxThreads(1000, 1000);

            finishCount = 0;
            DateTime begintime = DateTime.Now;

            timer = DateTime.Now;
            Task[] tasks = new Task[100];
            for (var line = 0; line < 100; line++)
            {
                tasks[line]=TestLine(url,linecount);
            }
            Task.WaitAll(tasks);
            Console.WriteLine("http succ=" + finishCount);
            var speed = ((double)finishCount) / (DateTime.Now - begintime).TotalSeconds;
            Console.WriteLine("http speed=" + speed + "tps");
        }
        static async Task TestLine(string url,int testcount)
        {
            //await Task.Delay(500);
            HttpClient http = new HttpClient();

            for (var count = 0; count < testcount; count++)
            {
                var text = await http.GetStringAsync(url);
                //if (text == "hello world.")
                {
                    finishCount++;
                    var now = DateTime.Now;
                    if ((now - timer).TotalSeconds > 1.0)
                    {
                        timer = now;
                        Console.WriteLine("http succ=" + finishCount);
                    }
                }
                //else
                {

                }
            }
        }
    }
}
