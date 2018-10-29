using Microsoft.AspNetCore.Http;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace lightchain.db
{
    class Program
    {
        public static LightChainDB db;
        public static lightchain.httpserver.httpserver server;
        public static System.Collections.Generic.Dictionary<string, Action<string[]>> menuItem = new System.Collections.Generic.Dictionary<string, Action<string[]>>();
        public static System.Collections.Generic.Dictionary<string, string> menuDesc = new System.Collections.Generic.Dictionary<string, string>();

        static void Main(string[] args)
        {
            db = new LightChainDB();
            Console.WriteLine("lightchain " + db.Version);

            db.Init("./testdb");

            server = new lightchain.httpserver.httpserver();
            server.SetWebsocketAction("/ws", (socket) => new websockerPeer(socket));
            server.SetFailAction(OnHttp404);

            server.Start(80);//一个参数，只开80端口
            Console.WriteLine("http on port 80");

            InitMenu();
            MenuLoop();
        }
        static void AddMenu(string cmd, string desc, Action<string[]> onMenu)
        {
            menuItem[cmd.ToLower()] = onMenu;
            menuDesc[cmd.ToLower()] = desc;
        }
        static void InitMenu()
        {
            AddMenu("help", "show help", ShowMenu);
            AddMenu("db.test", "db [num] dbtest program", DBTest);
        }
        static void ShowMenu(string[] words = null)
        {
            Console.WriteLine("==Menu==");
            foreach (var key in menuItem.Keys)
            {
                var line = "  " + key + " - ";
                if (menuDesc.ContainsKey(key))
                    line += menuDesc[key];
                Console.WriteLine(line);
            }
        }
        static void MenuLoop()
        {
            while (true)
            {
                Console.Write("-->");
                var line = Console.ReadLine();
                var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    var cmd = words[0].ToLower();
                    if (cmd == "?")
                    {
                        ShowMenu();
                    }
                    else if (menuItem.ContainsKey(cmd))
                    {
                        menuItem[cmd](words);
                    }
                }
            }
        }

        static async Task OnHttp404(HttpContext context)
        {
            await context.Response.WriteAsync("this server must be connect with websocket.");
        }
        static void DBTest(string[] words)
        {
            Console.WriteLine("do db test.");
            if(words.Length<2)
            {
                Console.WriteLine("need a number. type \"db.test 1\"");
                return;
            }
            var n = int.Parse(words[1]);
            if(n==1)
            {
                WriteBlock wblock = new WriteBlock();
                //wblock.ops.Add(new WriteOp_CreateTable());
                //for (var i=0;i<1000000;i++)
                //{
                //    var rb = db.CreateWriteBatch();
                //   rb.Put()
                //}
            }
        }

    }
}
