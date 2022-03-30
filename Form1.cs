using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace ex5_9
{
    
    public partial class frmPublishers : Form
    {
        string myState;
        int myBookmark;
        public frmPublishers()
        {
            InitializeComponent();
        }

        SqlConnection booksConnection;
        SqlCommand publishersCommand;
        SqlDataAdapter publishersAdapter;
        DataTable publishersTable;
        CurrencyManager publishersManager;

        private void frmPublishers_Load(object sender, EventArgs e)
        {
            try
            {
                // point to help file
                hlpPublishers.HelpNamespace = Application.StartupPath + "\\Publishers.chm";
                string path = Path.GetFullPath("SQLBooksDB.mdf");
                // connect to books database
                booksConnection = new
                    SqlConnection("Data Source=.\\SQLEXPRESS; AttachDBFilename=" + Application.StartupPath + "\\SQLBooksDB.mdf;" +
                "Integrated Security=True; Connect Timeout=30; User Instance=True");
                //SqlConnection("Data Source=.\\SQLEXPRESS; AttachDBFilename=" + path + ";" +
                //"Integrated Security=True; Connect Timeout=30; User Instance=True");
                booksConnection.Open();
                // establish command object
                publishersCommand = new SqlCommand("Select * From Publishers ORDER BY Name", booksConnection);
                // establish data adapter/data table
                publishersAdapter = new SqlDataAdapter();
                publishersAdapter.SelectCommand = publishersCommand;
                publishersTable = new DataTable();
                publishersAdapter.Fill(publishersTable);
                // bind controls to data table
                txtPubID.DataBindings.Add("Text", publishersTable, "PubID");
                txtPubName.DataBindings.Add("Text", publishersTable, "Name");
                txtCompanyName.DataBindings.Add("Text", publishersTable, "Company_Name");
                txtPubAddress.DataBindings.Add("Text", publishersTable, "Address");
                txtPubCity.DataBindings.Add("Text", publishersTable, "City");
                txtPubState.DataBindings.Add("Text", publishersTable, "State");
                txtPubZip.DataBindings.Add("Text", publishersTable, "Zip");
                txtPubTelephone.DataBindings.Add("Text", publishersTable, "Telephone");
                txtPubFax.DataBindings.Add("Text", publishersTable, "FAX");
                txtPubComments.DataBindings.Add("Text", publishersTable, "Fax");
                // estalish currency manager
                publishersManager = (CurrencyManager)
                    this.BindingContext[publishersTable];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error establishing Publishers table.", MessageBoxButtons.OK);
                return;
            }
                this.Show();
                SetState("View");
                SetText();
        }

        private void frmPublishers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before stopping the application.", "",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else
            {
                try
                {
                    // save changes to the database
                    SqlCommandBuilder publishersAdapterCommands = new SqlCommandBuilder(publishersAdapter);
                    publishersAdapter.Update(publishersTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving database to file: \r\n"
                        + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }



                // close the connection
                booksConnection.Close();
                // dispose of the objects
                booksConnection.Dispose();
                publishersCommand.Dispose();
                publishersAdapter.Dispose();
                publishersTable.Dispose();
            }
        }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (publishersManager.Position == 0)
            {
                Console.Beep();
            }
            publishersManager.Position--;
            SetText();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (publishersManager.Position == publishersManager.Count - 1)
            {
                Console.Beep();
            }
            publishersManager.Position++;
            SetText();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData())
            {
                return;
            }
            string savedName = txtPubName.Text;
            int savedRow;
            try
            {
                publishersManager.EndCurrentEdit();
                publishersTable.DefaultView.Sort = "Name";
                savedRow = publishersTable.DefaultView.Find(savedName);
                publishersManager.Position = savedRow;
                MessageBox.Show("Record saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error saving record.", MessageBoxButtons.OK);
                return;
            }
            SetText();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (response == DialogResult.No)
            {
                return;
            }
            try
            {
                publishersManager.RemoveAt(publishersManager.Position);
            }
            catch (Exception)
            {
                MessageBox.Show("Error deleting record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void SetState(string appState)
        {
            myState = appState;

            switch (appState)
            {
                case "View":
                    txtPubID.BackColor = Color.White;
                    txtPubID.ForeColor = Color.Black;
                    txtPubName.ReadOnly = true;
                    txtCompanyName.ReadOnly = true;
                    txtPubAddress.ReadOnly = true;
                    txtPubComments.ReadOnly = true;
                    txtPubState.ReadOnly = true;
                    txtPubZip.ReadOnly = true;
                    txtPubTelephone.ReadOnly = true;
                    txtPubFax.ReadOnly = true;
                    txtPubComments.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnAddNew.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDelete.Enabled = true;
                    btnFirst.Enabled = true;
                    btnLast.Enabled = true;
                    grpFindPublisher.Enabled = true;
                    txtPubName.Focus();
                    break;
                default: // Add or Edit if not View
                    txtPubID.BackColor = Color.Red;
                    txtPubID.ForeColor = Color.White;
                    txtPubName.ReadOnly = false;
                    txtCompanyName.ReadOnly = false;
                    txtPubAddress.ReadOnly = false;
                    txtPubComments.ReadOnly = false;
                    txtPubState.ReadOnly = false;
                    txtPubZip.ReadOnly = false;
                    txtPubTelephone.ReadOnly = false;
                    txtPubFax.ReadOnly = false;
                    txtPubComments.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDelete.Enabled = false;
                    btnFirst.Enabled = false;
                    btnLast.Enabled = false;
                    grpFindPublisher.Enabled = false;
                    txtPubName.Focus();
                    break;
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                myBookmark = publishersManager.Position;
                publishersManager.AddNew();
                SetState("Add");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error adding new record.", MessageBoxButtons.OK);
                return;
            }
            SetText();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            publishersManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                publishersManager.Position = myBookmark;
            }
            SetState("View");
        }

        private bool ValidateData()
        {
            string message = "";
            bool allOK = true;

            // Check for name

            if (txtPubName.Text.Trim().Equals(""))
            {
                message = "You must enter an Publisher Name." + "\r\n";
                txtPubName.Focus();
                allOK = false;
            }
            if (!allOK)
            {
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return (allOK);
        }

        private void txtAuthorName_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox whichBox = (TextBox)sender;
            if ((int)e.KeyChar == 13)
            {
                switch (whichBox.Name)
                {
                    case "txtPubName":
                        txtCompanyName.Focus();
                        break;
                    case "txtCompanyName":
                        txtPubAddress.Focus();
                        break;
                    case "txtPubAddress":
                        txtPubCity.Focus();
                        break;
                    case "txtPubCity":
                        txtPubState.Focus();
                        break;
                    case "txtPubState":
                        txtPubZip.Focus();
                        break;
                    case "txtPubZip":
                        txtPubFax.Focus();
                        break;
                    case "txtPubFax":
                        txtPubTelephone.Focus();
                        break;
                    case "txtPubTelephone":
                        txtPubComments.Focus();
                        break;
                    case "txtPubComments":
                        txtPubName.Focus();
                        break;
                }
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, hlpPublishers.HelpNamespace);
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            publishersManager.Position = 0;
            SetText();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            publishersManager.Position = publishersManager.Count - 1;
            SetText();
        }

        private void SetText()
        {
            this.Text = "Publishers - Record " + (publishersManager.Position + 1).ToString() +
                " of " + publishersManager.Count.ToString() + " Records";
        }
        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtFind.Text.Equals(""))
            {
                return;
            }
            int savedRow = publishersManager.Position;
            DataRow[] foundRows;
            publishersTable.DefaultView.Sort = "Name";
            foundRows = publishersTable.Select("Name LIKE '" + txtFind.Text + "*'");
            if (foundRows.Length == 0)
            {
                publishersManager.Position = savedRow;
            }
            else
            {
                publishersManager.Position = publishersTable.DefaultView.Find(foundRows[0]["Name"]);
            }
            SetText();
        }
    }
}

