using System;

namespace lightchain.db.test
{
    class Program
    {
        static void InitMenu()
        {
            AddMenu("exit", "exit application", (words) => { Environment.Exit(0); });
            AddMenu("help", "show help", ShowMenu);
            AddMenu("test.db.open", "open/create a db on a path", test_db_open);
            AddMenu("test.db.close", "close db", test_db_close);
            AddMenu("test.db.tablecreate", "create a table", test_db_tablecreate);
            AddMenu("test.db.tabledelete", "delete a table", test_db_tabledelete);
            AddMenu("test.db.tablewrite", "write a table", test_db_tablewrite);
            AddMenu("test.db.tableinfo", "get a table info", test_db_tableinfo);
        }
        static lightchain.db.LightDB db = null;
        static void test_db_open(string[] words)
        {
            try
            {
                Console.WriteLine("open db");
                db = new LightDB();
                //打开一个数据库，打开时如果不存在会创建一个
                db.Open("d:\\db001");
            }
            catch (Exception err)
            {
                Console.WriteLine("error:" + err.Message);
            }
        }
        static void test_db_close(string[] words)
        {
            try
            {
                Console.WriteLine("close db");
                db.Close();
                db = null;
            }
            catch (Exception err)
            {
                Console.WriteLine("error:" + err.Message);
            }
        }
        static void test_db_tablecreate(string[] words)
        {
            try
            {

                Console.WriteLine("test db table");

                using (var snap = db.CreateSnapInfo())
                {
                    var writebatch = db.CreateWriteBatch(snap);
                    var info = new lightchain.db.TableInfo(
                        new byte[] { 0x01, 0x02, 0x03 },//tablehead 是区分表格的数据，至少长度2，太短的不允许
                                                        //下面三个参数都是提供表的信息，无所谓什么
                        "mytable",//tablename 
                        "testtable0001",//tabledesc
                        DBValue.Type.String//tablekeytype
                        );
                    writebatch.CreateTable(info);

                    for (var i = 0; i < 100; i++)
                    {
                        var key = ("key" + i).ToBytes_UTF8Decode();
                        writebatch.Put(new byte[] { 0x01, 0x02, 0x03 }, key, DBValue.FromValue(DBValue.Type.UINT32, (UInt32)i));
                        db.Write(writebatch);
                    }
                }
                Console.WriteLine("create table and write 100 item.");
            }
            catch (Exception err)
            {
                Console.WriteLine("error:" + err.Message);
            }
        }
        static void test_db_tablewrite(string[] words)
        {
            try
            {

                Console.WriteLine("test db table");

                using (var snap = db.CreateSnapInfo())
                {
                    var writebatch = db.CreateWriteBatch(snap);
                    for (var i = 0; i < 100; i++)
                    {
                        var key = ("key" + i).ToBytes_UTF8Decode();
                        writebatch.Put(new byte[] { 0x01, 0x02, 0x03 }, key, DBValue.FromValue(DBValue.Type.UINT32, (UInt32)i));
                        db.Write(writebatch);
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("error:" + err.Message);
            }
        }
        static void test_db_tableinfo(string[] words)
        {
            try
            {
                Console.WriteLine("test db table");
                using (var snap = db.CreateSnapInfo())
                {
                    var info = snap.GetTableInfo(new byte[] { 0x01, 0x02, 0x03 });
                    var count = snap.GetTableCount(new byte[] { 0x01, 0x02, 0x03 });
                    Console.WriteLine("get table info: name=" + info.tablename);
                    Console.WriteLine("get table count =" + count);

                }
            }
            catch (Exception err)
            {
                Console.WriteLine("error:" + err.Message);
            }
        }
        static void test_db_tabledelete(string[] words)
        {

        }
        public static System.Collections.Generic.Dictionary<string, Action<string[]>> menuItem = new System.Collections.Generic.Dictionary<string, Action<string[]>>();
        public static System.Collections.Generic.Dictionary<string, string> menuDesc = new System.Collections.Generic.Dictionary<string, string>();
        static void AddMenu(string cmd, string desc, Action<string[]> onMenu)
        {
            menuItem[cmd.ToLower()] = onMenu;
            menuDesc[cmd.ToLower()] = desc;
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
        static void Main(string[] args)
        {
            Console.WriteLine("lightchain.db test.");
            InitMenu();
            MenuLoop();
        }
    }
}
