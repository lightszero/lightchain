using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using LightDB;
namespace lightdb.server
{

    public class StorageService
    {
        public static readonly byte[] tableID_Writer = new byte[] { 0x07 };
        public LightDB.LightDB maindb;
        public bool state_DBOpen
        {
            get;
            private set;
        }

        public void Init()
        {
            Console.CursorLeft = 0;
            Console.WriteLine(" == Open DB ==");


            maindb = new LightDB.LightDB();

            state_DBOpen = false;
            string fullpath = System.IO.Path.GetFullPath(Program.config.server_storage_path);
            if (System.IO.Directory.Exists(fullpath) == false)
                System.IO.Directory.CreateDirectory(fullpath);
            string pathDB = System.IO.Path.Combine(fullpath, "maindb");
            try
            {
                maindb.Open(pathDB);
                state_DBOpen = true;
                Console.WriteLine("db opened in:" + pathDB);
            }
            catch (Exception err)
            {
                Console.WriteLine("error msg:" + err.Message);
            }
            if (state_DBOpen == false)
            {
                Console.WriteLine("open database fail. try to create it.");
                try
                {
                    LightDB.DBCreateOption createop = new LightDB.DBCreateOption();
                    createop.MagicStr = Program.config.storage_maindb_magic;
                    createop.FirstTask = new LightDB.WriteTask();

                    createop.FirstTask.CreateTable(new LightDB.TableInfo(tableID_Writer, "_writeraddress_", "", LightDB.DBValue.Type.String));
                    createop.FirstTask.Put(tableID_Writer, Program.config.storage_maindb_firstwriter_address.ToBytes_UTF8Encode(), LightDB.DBValue.FromValue(LightDB.DBValue.Type.BOOL, true));

                    maindb.Open(pathDB, createop);
                    Console.WriteLine("db created in:" + pathDB);
                    state_DBOpen = true;
                }
                catch (Exception err)
                {
                    Console.WriteLine("error msg:" + err.Message);

                }
            }
        }

    }
}
