
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using LightDB;
namespace lightdb.server
{
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

            httpserver.Start(config.server_port);//一个参数，只开80端口
            Console.WriteLine("http on port " + config.server_port);

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
            AddMenu("db.state", "show cur dbstate.", ShowDBState);
            AddMenu("db.block", "show cur dbblock ,use db.block [n].", ShowDBBlock);

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
                try
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
                catch (Exception err)
                {
                    Console.WriteLine("err:" + err.Message);
                }
            }
        }

        static async Task OnHttp404(HttpContext context)
        {
            await context.Response.WriteAsync("this server must be connect with websocket.");
        }
        static void ShowDBState(string[] words)
        {
            Console.WriteLine("db open state:" + storage.state_DBOpen);
            if (storage.state_DBOpen)
            {
                using (var snap = storage.maindb.UseSnapShot())
                {
                    Console.WriteLine("dataheight=" + snap.DataHeight);
                    var value = snap.GetValue(LightDB.LightDB.systemtable_info, "_magic_".ToBytes_UTF8Encode());
                    if (value == null)
                        Console.WriteLine("no magic.");
                    else
                        Console.WriteLine("magic is:" + value.AsString());

                    Console.WriteLine("==list writer==");
                    var keys = snap.CreateKeyFinder(StorageService.tableID_Writer);
                    foreach (byte[] keybin in keys)
                    {
                        var key = keybin.ToString_UTF8Decode();
                        var iswriter = snap.GetValue(StorageService.tableID_Writer, keybin).AsBool();
                        if (iswriter)
                        {
                            Console.WriteLine("writer:" + key);
                        }
                    }
                }
            }

        }

        static void ShowDBBlock(string[] words)
        {
            uint blockid = uint.Parse(words[1]);
            using (var snap = storage.maindb.UseSnapShot())
            {
                var value = snap.GetValue(LightDB.LightDB.systemtable_block, BitConverter.GetBytes((UInt64)blockid));
                if (value != null && value.type != DBValue.Type.Deleted)
                {
                    var task = LightDB.WriteTask.FromRaw(value.value);

                    Console.WriteLine("block:" + blockid + " len=" + value.value.Length);
                    Console.WriteLine("==blockitems==");
                    foreach (var i in task.items)
                    {
                        Console.WriteLine(i.ToString());
                    }
                    if (task.extData != null)
                    {
                        Console.WriteLine("==blockext==");
                        foreach (var i in task.extData)
                        {
                            Console.WriteLine(i.Key + "=" + i.Value?.ToString_Hex());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("block:" + blockid + " not exist.");
                }
            }
        }
    }
}
