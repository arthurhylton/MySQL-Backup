using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySQL_Backup
{
    public class BackupItem
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string PasswordKey { get; set; }
        public string PasswordValue { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string SaveTo { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", Host, Username, PasswordKey, Port, Database, SaveTo);
        }
        public string ConnectionString()
        {
            return @"server=" + Host + ";userid=" + Username + ";password=" + PasswordValue + ";database=" + Database + "";
        }
    }
}
