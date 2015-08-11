using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MySQL_Backup
{
    public partial class FrmAddEditBackupItem : Form
    {
        public bool newItem;
        public BackupItem bkpitm;
        List<DatabasePassword> lstDbPasswords = new List<DatabasePassword>();
        public FrmAddEditBackupItem()
        {
            InitializeComponent();
            bkpitm = new BackupItem();
            newItem = true;
            loadPasswords();
        }

        public FrmAddEditBackupItem(BackupItem itm)
        {
            InitializeComponent();
            bkpitm = itm;
            newItem = false;
            this.Text = lblHeader.Text = "Edit Backup Item";
            loadPasswords();
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
        private void FrmAddEditBackupItem_Load(object sender, EventArgs e)
        {
            txtHost.DataBindings.Add("Text", bkpitm, "Host",false, DataSourceUpdateMode.OnValidation);
            txtUsername.DataBindings.Add("Text", bkpitm, "Username", false, DataSourceUpdateMode.OnValidation);
            txtKey.DataBindings.Add("Text", bkpitm, "PasswordKey", false, DataSourceUpdateMode.Never);
            txtPort.DataBindings.Add("Text", bkpitm, "Port", false, DataSourceUpdateMode.OnValidation);
            txtDatabase.DataBindings.Add("Text", bkpitm, "Database", false, DataSourceUpdateMode.OnValidation);
            txtSaveTo.DataBindings.Add("Text", bkpitm, "SaveTo", false, DataSourceUpdateMode.OnValidation);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void llblPwdManager_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FrmPasswordManager frmPwd = new FrmPasswordManager();
            if (frmPwd.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                txtKey.Text = frmPwd.currentPassword.PasswordKey;
                //txtPassword.Text = frmPwd.currentPassword.PasswordValue;
                this.ActiveControl = txtKey;
            }
            if (frmPwd.passwordsAddedOrModified)
                loadPasswords();
        }

        private void txtKey_Validating(object sender, CancelEventArgs e)
        {
            string error_message = "";
            //begin validations
            if (!lstDbPasswords.Any(k => k.PasswordKey == txtKey.Text))
            {
                error_message += "Key doesn't exist. Please create with the password manager." + Environment.NewLine;
                MessageBox.Show(error_message);
                e.Cancel = true;
            }
            else
            {
                txtKey.DataBindings["Text"].WriteValue();
                bkpitm.PasswordValue = lstDbPasswords.First(v => v.PasswordKey == bkpitm.PasswordKey).PasswordValue;
                //txtPassword.DataBindings["Text"].WriteValue();
            }
            //end validations
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if(ValidateChildren())
            {
                //MessageBox.Show(bkpitm.PasswordValue);
                if (Helpers.TestConnection(bkpitm))
                {
                    MessageBox.Show("Connection Successful");
                }
                else
                {
                    MessageBox.Show("Connection Failed");
                }
            }
        }
    }
}
