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
    public partial class FrmPasswordManager : Form
    {
        public bool passwordsAddedOrModified = false;
        bool editing = false;
        List<DatabasePassword> lstDbPasswords;
        public DatabasePassword currentPassword;
        public FrmPasswordManager()
        {
            InitializeComponent();
            lstDbPasswords = new List<DatabasePassword>();
            currentPassword = new DatabasePassword();
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
            bsPasswords.ResetBindings(false);
        }

        private void setupBindings()
        {
            txtKey.DataBindings.Clear();
            txtPassword.DataBindings.Clear();

            txtKey.DataBindings.Add("Text", currentPassword, "PasswordKey");
            txtPassword.DataBindings.Add("Text", currentPassword, "PasswordValue");

            if (editing)
            {
                lblAddPasswordHeader.Text = "Edit Password";
            }
            else
            {
                lblAddPasswordHeader.Text = "Add New Password";
            }
        }
        private void FrmPasswordManager_Load(object sender, EventArgs e)
        {
            bsPasswords.DataSource = lstDbPasswords;

            setupBindings();
        }

        private void dgvPasswords_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit_test = dgvPasswords.HitTest(e.X, e.Y);
                if (hit_test.RowIndex > -1)
                {
                    dgvPasswords.Rows[hit_test.RowIndex].Selected = true;
                }
            }
        }

        private void dgvPasswords_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            editing = false;
            currentPassword = dgvPasswords.SelectedRows[0].DataBoundItem as DatabasePassword;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void savePasswordList()
        {
            //overwrite everything in the collection in settings
            System.Collections.Specialized.StringCollection newCollection = new System.Collections.Specialized.StringCollection();
            foreach (DatabasePassword pwd in lstDbPasswords)
            {
                newCollection.Add(pwd.ToString());
            }
            Properties.Settings.Default.DatabasePasswords = newCollection;
            Properties.Settings.Default.Save();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            bool everything_ok = true;
            string error_message = "";

            //validations
            if (txtKey.Text.Trim() == "" || txtPassword.Text.Trim() == "")
            {
                everything_ok = false;
                error_message += "Both key and password fields must not be empty." + Environment.NewLine;
            }
            if (!editing)
            {
                if (lstDbPasswords.Any(k => k.PasswordKey == currentPassword.PasswordKey))
                {
                    everything_ok = false;
                    error_message += "Key already exists." + Environment.NewLine;
                }
            }

            //end validations
            if (everything_ok)
            {
                if (!editing)
                {
                    lstDbPasswords.Add(currentPassword);
                }
                savePasswordList();
                //txtKey.Text = "";
                //txtPassword.Text = "";
                currentPassword = new DatabasePassword();
                editing = false;
                setupBindings();
                loadPasswords();
                passwordsAddedOrModified = true;
            }
            else
            {
                MessageBox.Show("Error:"+Environment.NewLine+error_message);
            }
        }

        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            if (txtKey.Text.Length > 0 && txtPassword.Text.Length > 0)
            {
                btnSave.Enabled = true;
            }
            else
            {
                btnSave.Enabled = false;
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            if (txtKey.Text.Length > 0 && txtPassword.Text.Length > 0)
            {
                btnSave.Enabled = true;
            }
            else
            {
                btnSave.Enabled = false;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lstDbPasswords.Remove(dgvPasswords.SelectedRows[0].DataBoundItem as DatabasePassword);
            savePasswordList();
            bsPasswords.ResetBindings(false);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPassword = dgvPasswords.SelectedRows[0].DataBoundItem as DatabasePassword;
            editing = true;
            setupBindings();
        }
    }
}
