using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySQL_Backup
{
    public class DatabasePassword
    {
        public string PasswordKey { get; set; }
        public string PasswordValue { get; set; }

        public override string ToString()
        {
            return string.Format("{0}|delimit|{1}", PasswordKey, PasswordValue);
        }
    }
}
