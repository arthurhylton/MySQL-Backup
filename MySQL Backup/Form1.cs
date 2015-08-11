extern alias oldVer;
extern alias newVer;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MysqlAlias = newVer::MySql.Data.MySqlClient;
using OldMysqlAlias = oldVer::MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;

namespace MySQL_Backup
{
    public partial class FrmMain : Form
    {
        private string configFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\config.txt";
        public static int mode = 1;

        private List<BackupItem> lstBkpItems;
        List<DatabasePassword> lstDbPasswords = new List<DatabasePassword>();

        public FrmMain(string init = null)
        {
            InitializeComponent();
            lstBkpItems = new List<BackupItem>();
            loadPasswords();
            //bsBkpItems.ResetBindings(false);

            if (init != null && init.Equals("autoStart9"))
            {
                btnBeginBackup_Click(null, null);
            }
        }

        private void loadPasswords()
        {
            lstDbPasswords.Clear();
            if (Properties.Settings.Default.DatabasePasswords != null)
            {
                System.Collections.Specialized.StringCollection pwdCollection = Properties.Settings.Default.DatabasePasswords;
                foreach (string str in pwdCollection)
                {
                    string[] delimited = str.Split(new string[] { "|delimit|" }, StringSplitOptions.None);
                    lstDbPasswords.Add(new DatabasePassword { PasswordKey = delimited[0], PasswordValue = delimited[1] });
                }
            }
        }

        bool backupDatabase(BackupItem bk_itm)
        {
            //This uses the newer version of mysql dll 6.6 ot enable connection to servers that use new style password authemtication (4.1 style)

            string result_folder = bk_itm.SaveTo + "\\" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString("D2") + "-" + DateTime.Now.Day.ToString("D2") + "\\" + bk_itm.Database + "\\";

            MysqlAlias.MySqlConnection conn = null;
            MysqlAlias.MySqlDataReader rdr = null;

            try
            {
                conn = new MysqlAlias.MySqlConnection(bk_itm.ConnectionString());
                conn.Open();
                //get every table in this db
                string stm = "SHOW TABLES";
                MysqlAlias.MySqlCommand cmd = new MysqlAlias.MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                //deal with directory that .sql files should be saved in
                if (!Directory.Exists(result_folder))
                {
                    Directory.CreateDirectory(result_folder);
                }

                string current_table = "";
                while (rdr.Read())
                {
                    current_table = rdr.GetString(0);
                    // Use ProcessStartInfo class
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\mysqldump.exe";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    //::@C:\Users\User\Downloads\MySQLWinBackup\MySQLWinBackup\mysqldump.exe
                    startInfo.Arguments = "--host=\"" + bk_itm.Host + "\" --port=\"" + bk_itm.Port + "\" --user=\"" + bk_itm.Username + "\" --password=\"" + bk_itm.PasswordValue + "\" -Q --result-file=\"" + result_folder + "\\" + current_table + ".sql\" \"" + bk_itm.Database + "\" \"" + current_table + "\"";

                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                            txtOutput.Text += "Database: " + bk_itm.Database + "   -  table: " + current_table + " done " + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog(ex.Message);
                        return false;
                    }
                }
                //progressBar2.PerformStep();
                //if (progressBar2.Value >= progressBar2.Maximum)
                //{
                //    progressBar2.Style = ProgressBarStyle.Marquee;
                //}

            }
            catch (MysqlAlias.MySqlException ex)
            {
                ErrorLog(ex.Message);
                return false;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return true;
        }

        bool backupDatabaseOldStyle(BackupItem bk_itm)
        {
            //This uses the version 2.0 of mysql dll so that servers that still use old style password authentication (pre 4.1) will
            //still be able to connect.

            string result_folder = bk_itm.SaveTo + "\\" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString("D2") + "-" + DateTime.Now.Day.ToString("D2") + "\\" + bk_itm.Database + "\\";


            OldMysqlAlias.MySqlConnection conn = null;
            OldMysqlAlias.MySqlDataReader rdr = null;

            try
            {
                conn = new OldMysqlAlias.MySqlConnection(bk_itm.ConnectionString());
                conn.Open();
                //get every table in this db
                string stm = "SHOW TABLES";
                OldMysqlAlias.MySqlCommand cmd = new OldMysqlAlias.MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                //deal with directory that .sql files should be saved in
                if (!Directory.Exists(result_folder))
                {
                    Directory.CreateDirectory(result_folder);
                }

                string current_table = "";
                while (rdr.Read())
                {
                    //Console.WriteLine(rdr.GetInt32(0) + ": " + rdr.GetString(1));
                    current_table = rdr.GetString(0);
                    // Use ProcessStartInfo class
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\mysqldump.exe";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    //::@C:\Users\User\Downloads\MySQLWinBackup\MySQLWinBackup\mysqldump.exe
                    startInfo.Arguments = "--host=\"" + bk_itm.Host + "\" --port=\"" + bk_itm.Port + "\" --user=\"" + bk_itm.Username + "\" --password=\"" + bk_itm.PasswordValue + "\" -Q --result-file=\"" + result_folder + "\\" + current_table + ".sql\" \"" + bk_itm.Database + "\" \"" + current_table + "\"";

                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                            txtOutput.Text += "Database: " + bk_itm.Database + "   -  table: " + current_table + " done " + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog(ex.Message + " Using Old-Style Password Authentication");
                        return false;
                    }
                }

            }
            catch (MysqlAlias.MySqlException ex)
            {
                ErrorLog(ex.Message + " Using Old-Style Password Authentication");
                return false;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return true;
        }

        private void btnBeginBackup_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        // Handle the BindingComplete event to ensure the two forms 
        // remain synchronized. 
        private void bsBkpItems_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            if (e.BindingCompleteContext == BindingCompleteContext.DataSourceUpdate
                && e.Exception == null)
                e.Binding.BindingManagerBase.EndCurrentEdit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bsBkpItems.DataSource = lstBkpItems;
            loadBackupItems();
            //if (mode == 0)
            //{
            //    txtOutput.Text = "";
            //    btnBeginBackup.Visible = true;
            //}
            //else
            //{
            //    txtOutput.Text = "";

            //    Application.DoEvents();

            //    btnBeginBackup.PerformClick();

            //    Application.Exit();
            //}
        }

        private void loadBackupItems()
        {
            lstBkpItems.Clear();
            //loop through file and read lines
            List<string> lines = new List<string>();

            // 2
            // Use using StreamReader for disposing.
            using (StreamReader r = new StreamReader(configFile))
            {
                // 3
                // Use while != null pattern for loop
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    if (line.Split(new char[] { ' ' })[0].StartsWith("::"))
                    {
                        //ignore line as this is a comment
                    }
                    else
                    {
                        lines.Add(line);
                    }
                }
            }

            //now loop through list and backup dbs accordingly
            for (int indx = 0; indx < lines.Count; indx++)
            {
                string line = lines[indx];
                string[] split_line = line.Split(new char[] { ' ' });
                BackupItem bkpitm = new BackupItem();
                bkpitm.Host = split_line[0];
                bkpitm.Username = split_line[1];
                bkpitm.PasswordKey = split_line[2];
                if (lstDbPasswords.Any(pk => pk.PasswordKey == bkpitm.PasswordKey))
                {
                    bkpitm.PasswordValue = lstDbPasswords.First(p => p.PasswordKey == bkpitm.PasswordKey).PasswordValue;
                }else
                {
                    ErrorLog("No password found for: "+bkpitm.ToString());
                }
                bkpitm.Port = split_line[3];
                bkpitm.Database = split_line[4];
                bkpitm.SaveTo = split_line[5];

                lstBkpItems.Add(bkpitm);
            }

            bsBkpItems.ResetBindings(false);
        }

        #region Error Log
        public void ErrorLog(string Message)
        {
            StreamWriter sw = null;

            try
            {
                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                string sPathName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\" ;

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;

                //sw = new StreamWriter(sPathName + "DB_Backup_ErrorLog_" + sErrorTime + ".txt", true);
                sw = new StreamWriter(sPathName + "DB_Backup_ErrorLog.txt", true);

                sw.WriteLine(sLogFormat + Message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                ErrorLog(ex.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }


        }
        #endregion

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate()
            {
                btnBeginBackup.Enabled = false;
                foreach (BackupItem itm in lstBkpItems)
                {
                    if (backupDatabase(itm))
                    {
                    }
                    else
                    {
                        backupDatabaseOldStyle(itm);
                    }
                }
            });
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar1.Value = e.ProgressPercentage;
        }

        private void dgvBackupItems_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit_test = dgvBackupItems.HitTest(e.X, e.Y);
                if (hit_test.RowIndex > -1)
                {
                    dgvBackupItems.Rows[hit_test.RowIndex].Selected = true;
                }
            }
        }

        private void saveDataToConfigFile()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(configFile,false))
            {
                foreach (BackupItem item in lstBkpItems)
                {
                    file.WriteLine(item.ToString());
                }
            }
        }
        private void btnAddItem_Click(object sender, EventArgs e)
        {
            FrmAddEditBackupItem frmAdd = new FrmAddEditBackupItem();
            if (frmAdd.ShowDialog() == DialogResult.OK)
            {
                if (frmAdd.newItem)
                {
                    lstBkpItems.Add(frmAdd.bkpitm);
                    bsBkpItems.ResetBindings(false);
                }
                saveDataToConfigFile();
            }
            else
            {
                loadBackupItems();
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmAddEditBackupItem frmEdit = new FrmAddEditBackupItem(dgvBackupItems.SelectedRows[0].DataBoundItem as BackupItem);
            if (frmEdit.ShowDialog() == DialogResult.OK)
            {
                saveDataToConfigFile();
            }
            else
            {
                loadBackupItems();
            }
        }

    }
}
