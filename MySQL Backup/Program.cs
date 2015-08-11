using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MySQL_Backup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //expecting args to be in the format: host username password port dbname path
            if (args.Length > 0 && args[0].Equals("autoStart9"))
            {
                Application.Run(new FrmMain(args[0]));
            }
            else
            {
                Application.Run(new FrmMain());
            }
        }
    }
}
