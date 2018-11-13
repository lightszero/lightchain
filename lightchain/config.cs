using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

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

}
