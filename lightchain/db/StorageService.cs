using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using LightDB;
namespace lightdb.server
{

    public class StorageService
    {

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
            try
            {
                maindb.Open(Program.config.server_storage_path);
                state_DBOpen = true;
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
                    var tableWriter = new byte[] { 0x07 };
                    createop.FirstTask.CreateTable(new LightDB.TableInfo(tableWriter, "_writeraddress_", "", LightDB.DBValue.Type.String));
                    createop.FirstTask.Put(tableWriter, Program.config.storage_maindb_firstwriter_address.ToBytes_UTF8Encode(), LightDB.DBValue.FromValue(LightDB.DBValue.Type.BOOL, true));

                    maindb.Open(Program.config.server_storage_path, createop);
                    Console.WriteLine("db created.");
                    state_DBOpen = true;
                }
                catch (Exception err)
                {
                    Console.WriteLine("error msg:" + err.Message);

                }
            }
        }
        public void CreateDB(string magicstr, byte[] adminPublicKey)
        {

        }
    }
}
