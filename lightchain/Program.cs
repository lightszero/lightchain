
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace lightdb.server
{
    public class Config
    {
        private Config()
        {

        }
        public enum Config_Server_Type
        {
            Master,//主服务器
        }
        public int server_port
        {
            get; private set;
        }
        public string server_storage_path
        {
            get; private set;
        }
        public Config_Server_Type server_type
        {
            get; private set;
        }
        public string storage_maindb_magic
        {
            get; private set;
        }
        public string storage_maindb_firstwriter_address
        {
            get; private set;
        }
        public static Config Parse(string txt)
        {
            Config config = new Config();
            var jobj = JObject.Parse(txt);
            config.server_port = (int)jobj["server_port"];
            config.server_storage_path = (string)jobj["server_storage_path"];
            config.server_type = Enum.Parse<Config_Server_Type>((string)jobj["server_type"]);
            config.storage_maindb_magic = (string)jobj["storage_maindb_magic"];
            config.storage_maindb_firstwriter_address = (string)jobj["storage_maindb_firstwriter_address"];
            return config;
        }
    }
    class Program
    {
        public static Config config
        {
            get;
            private set;
        }
        public static StorageService storage
        {
            get;
            private set;
        }
        public static lightchain.httpserver.httpserver httpserver
        {
            get;
            private set;
        }
        static System.Collections.Generic.Dictionary<string, Action<string[]>> menuItem = new System.Collections.Generic.Dictionary<string, Action<string[]>>();
        static System.Collections.Generic.Dictionary<string, string> menuDesc = new System.Collections.Generic.Dictionary<string, string>();

        static void Main(string[] args)
        {
            //把当前目录搞对，怎么启动都能找到dll了
            var lastpath = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location); ;
            Console.WriteLine("exepath=" + lastpath);
            Environment.CurrentDirectory = lastpath;

            config = Config.Parse(System.IO.File.ReadAllText("config.json"));

            storage = new StorageService();
            storage.Init();

            httpserver = new lightchain.httpserver.httpserver();
            httpserver.SetWebsocketAction("/ws", (socket) => new websockerPeer(socket));
            httpserver.SetFailAction(OnHttp404);

            httpserver.Start(80);//一个参数，只开80端口
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
            AddMenu("state", "show cur dbstate.", ShowState);
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
        static void ShowState(string[] words)
        {
            Console.WriteLine("do db test.");
            if (words.Length < 2)
            {
                Console.WriteLine("need a number. type \"db.test 1\"");
                return;
            }
            var n = int.Parse(words[1]);
            //if (n == 1)
            //{
            //    WriteBlock wblock = new WriteBlock();
            //    for (var i = 0; i < 10; i++)
            //    {
            //        var tableid = new byte[] { 1, (byte)i };
            //        wblock.ops.Add(new WriteOp_CreateTable(tableid, "testtable" + i, "testtable", DBValue.Type.String));
            //        db.WriteBlock(wblock);
            //    }

            //    //wblock.ops.Add(new WriteOp_CreateTable());
            //    //for (var i=0;i<1000000;i++)
            //    //{
            //    //    var rb = db.CreateWriteBatch();
            //    //   rb.Put()
            //    //}
            //}
        }
        static void Other1(string[] words)
        {
            Console.WriteLine("other1");
            byte[] data = new byte[] { 1, 2, 3 };
            byte[] data2 = new byte[] { 1, 2, 3, 4, 5 };
            var num1 = new System.Numerics.BigInteger(data);
            var num2 = new System.Numerics.BigInteger(data2);
            var num3 = new System.Numerics.BigInteger(data);
            var num4 = new System.Numerics.BigInteger(data2);
            Console.WriteLine("num1=" + num1.ToString("X") + " hash=" + num1.GetHashCode().ToString("X"));
            Console.WriteLine("num2=" + num2.ToString("X") + " hash=" + num2.GetHashCode().ToString("X"));
            Console.WriteLine("num3=" + num3.ToString("X") + " hash=" + num3.GetHashCode().ToString("X"));
            Console.WriteLine("num4=" + num4.ToString("X") + " hash=" + num4.GetHashCode().ToString("X"));

        }

    }
}
