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
            AddMenu("exit", "exit application", (words) => { Environment.Exit(0); });
            AddMenu("help", "show help", ShowMenu);
            AddMenu("test.db", "db [num] dbtest program", DBTest);
            AddMenu("test.other.1", "test num", Other1);

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
            if (words.Length < 2)
            {
                Console.WriteLine("need a number. type \"db.test 1\"");
                return;
            }
            var n = int.Parse(words[1]);
            if (n == 1)
            {
                WriteBlock wblock = new WriteBlock();
                for (var i = 0; i < 10; i++)
                {
                    var tablehead = new byte[] { 1, (byte)i };
                    wblock.ops.Add(new WriteOp_CreateTable(tablehead, "testtable" + i, "testtable", DBValue.Type.String));
                    db.WriteBlock(wblock);
                }
               
                //wblock.ops.Add(new WriteOp_CreateTable());
                //for (var i=0;i<1000000;i++)
                //{
                //    var rb = db.CreateWriteBatch();
                //   rb.Put()
                //}
            }
        }
        static void Other1(string[] words)
        {
            Console.WriteLine("other1");
            byte[] data = new byte[] { 1, 2, 3 };
            byte[] data2 = new byte[] { 1, 2, 3, 4, 5 };
            var num1 = new System.Numerics.BigInteger(data);
            var num2 = new System.Numerics.BigInteger(data2);
            var num3 = new System.Numerics.BigInteger(data);
            var num4= new System.Numerics.BigInteger(data2);
            Console.WriteLine("num1=" + num1.ToString("X") + " hash=" + num1.GetHashCode().ToString("X"));
            Console.WriteLine("num2=" + num2.ToString("X") + " hash=" + num2.GetHashCode().ToString("X"));
            Console.WriteLine("num3=" + num3.ToString("X") + " hash=" + num3.GetHashCode().ToString("X"));
            Console.WriteLine("num4=" + num4.ToString("X") + " hash=" + num4.GetHashCode().ToString("X"));

        }

    }
}
