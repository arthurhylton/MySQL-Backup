using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Text;
namespace MySQL_Backup
{
    public partial class FrmUpdate : Form
    {
        WebClient client = new System.Net.WebClient();
        public FrmUpdate()
        {
            InitializeComponent();
        }

        private void frmUpdate_Load(object sender, EventArgs e)
        {
            string lastMod = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("MMMM dd, yyyy");
            lblCopyright.Text = AssemblyCopyright;
            lblVersion.Text = String.Format("Version {0} - {1}", AssemblyVersion, lastMod);
            lblDescription.Text = AssemblyDescription;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += client_DownloadFileCompleted;
        }

        #region Assembly Attribute Accessors
        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        #region File Properties
        public static string MsiFileName { get { return "MySqlBackupSetup.msi"; } }
        public static string XMLFileName { get { return "MySqlBackupVersion.xml"; } }
        public static string FilePath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                char separator = System.IO.Path.DirectorySeparatorChar;
                if (path.Last() == separator)
                    return path + MsiFileName;
                else
                    return path + separator + MsiFileName;
            }
        }
        #endregion

        private void btnCheck_Click(object sender, EventArgs e)
        {
            btnCheck.Enabled = false;
            try
            {
                //get the url to check for updates
                string updateUrl = Properties.Settings.Default.UpdatePath;
                if (string.IsNullOrEmpty(updateUrl) || string.IsNullOrEmpty(updateUrl.Trim()))
                    throw new Exception("The update url path was not set");
                UriBuilder builder = new UriBuilder(updateUrl);
                string url = builder.Uri.AbsoluteUri;
                if (!url.EndsWith("/"))
                    url += '/';

                //check the remote xml for the version
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.Load(url + XMLFileName);
                string latestVersion = xmlDoc.SelectSingleNode(@"/app/version").InnerText;

                if (AssemblyVersion.CompareTo(latestVersion) < 0)
                {
                    //ask the user if they would like to download the new version
                    string msg = "Version " + latestVersion + " available" + Environment.NewLine + "Click Yes to download and install the new version";
                    if (DialogResult.Yes == MessageBox.Show(this, msg, "New version available", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                    {
                        tssLblNotification.Text = "Downloading update...";
                        Uri uri = new Uri(url + MsiFileName);
                        client.DownloadFileAsync(uri, FilePath);
                    }
                    else
                        btnCheck.Enabled = true;
                }
                else
                {
                    MessageBox.Show("You are already running the latest version", "Application Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnCheck.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnCheck.Enabled = true;
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            tsProgress.Visible = false;
            try
            {
                if (e.Error != null)
                    throw e.Error;
                Process p = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "msiexec";
                info.Verb = "runas";

                info.ErrorDialog = true;
                info.ErrorDialogParentHandle = this.Handle;

                info.Arguments = @"/i " + "\"" + FilePath + "\"" + @" /norestart /passive";
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();

                StringBuilder msg = new StringBuilder("The update won't take effect until the application restarts.").AppendLine();
                msg.Append("Would you like to restart now?");
                p.Close();
                if (DialogResult.Yes == MessageBox.Show(msg.ToString(), "Update successful", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                {
                    Application.Restart();
                }
                btnCheck.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
                btnCheck.Enabled = true;
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = Convert.ToDouble(e.BytesReceived);
            double totalBytes = Convert.ToDouble(e.TotalBytesToReceive);
            double percentage = bytesIn / totalBytes * 100;
            tsProgress.Value = Convert.ToInt32(percentage);
        }
        
        private void lnkLblWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://www.arthurhylton.com/");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
