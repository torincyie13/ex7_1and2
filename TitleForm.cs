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
    
    public partial class frmTitles : Form
    {

        string myState;
        int myBookmark;
        ComboBox[] authorsCombo = new ComboBox[4];
        SqlCommand authorsCommand;
        SqlDataAdapter authorsAdapter;
        DataTable[] authorsTable = new DataTable[4];
        public frmTitles()
        {
            InitializeComponent();
        }

        SqlConnection booksConnection;
        SqlCommand titlesCommand;
        SqlDataAdapter titlesAdapter;
        DataTable titlesTable;
        CurrencyManager titlesManager;

        SqlCommand publishersCommand;
        SqlDataAdapter publishersAdapter;
        DataTable publishersTable;  

        private void frmTitles_Load(object sender, EventArgs e)
        {
            try
            {
                // point to help file
                hlpPublishers.HelpNamespace = Application.StartupPath + "\\Publishers.chm";
                string path = Path.GetFullPath("SQLBooksDB.mdf");
                // connect to books database
                booksConnection = new
                    SqlConnection("Data Source=.\\SQLEXPRESS; AttachDBFilename=" + path + ";" +
                    "Integrated Security=True; Connect Timeout=30; User Instance=True");
                booksConnection.Open();
                // establish command object
                titlesCommand = new SqlCommand("Select * From Titles ORDER BY Title", booksConnection);
                // establish data adapter/data table
                titlesAdapter = new SqlDataAdapter();
                titlesAdapter.SelectCommand = titlesCommand;
                titlesTable = new DataTable();
                titlesAdapter.Fill(titlesTable);
                // bind controls to data table
                txtTitle.DataBindings.Add("Text", titlesTable, "Title");
                txtYear.DataBindings.Add("Text", titlesTable, "Year_Published");
                txtISBN.DataBindings.Add("Text", titlesTable, "ISBN");
                txtDescription.DataBindings.Add("Text", titlesTable, "Description");
                txtNotes.DataBindings.Add("Text", titlesTable, "Notes");
                txtSubject.DataBindings.Add("Text", titlesTable, "Subject");
                txtComments.DataBindings.Add("Text", titlesTable, "Comments");
                // estalish currency manager
                titlesManager = (CurrencyManager)
                    this.BindingContext[titlesTable];
                // establish publisher table/combo box to pick publisher
                publishersCommand = new SqlCommand("Select * from Publishers ORDER BY Name", booksConnection);
                publishersAdapter = new SqlDataAdapter();
                publishersAdapter.SelectCommand = publishersCommand;
                publishersTable = new DataTable();
                publishersAdapter.Fill(publishersTable);
                cboPublisher.DataSource = publishersTable;
                cboPublisher.DisplayMember = "Name";
                cboPublisher.ValueMember = "PubID";
                cboPublisher.DataBindings.Add("SelectedValue", titlesTable, "PubID");
                // set up combo box array
                authorsCombo[0] = cboAuthor1;
                authorsCombo[1] = cboAuthor2;
                authorsCombo[2] = cboAuthor3;
                authorsCombo[3] = cboAuthor4;
                authorsCommand = new SqlCommand("Select * from Authors ORDER BY Author", booksConnection);
                authorsAdapter = new SqlDataAdapter();
                authorsAdapter.SelectCommand = authorsCommand;
                for (int i = 0; i < 4; i++)
                {
                    // establish auhtor table/combo boxes to pick author
                    authorsTable[i] = new DataTable();
                    authorsAdapter.Fill(authorsTable[i]);
                    authorsCombo[i].DataSource = authorsTable[i]; ;
                    authorsCombo[i].DisplayMember = "Author";
                    authorsCombo[i].ValueMember = "Au_ID"
;
                    // set all to no selection
                    authorsCombo[i].SelectedIndex = -1;
                }
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

        private void frmTitles_FormClosing(object sender, FormClosingEventArgs e)
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
                    SqlCommandBuilder titlesAdapterCommands = new SqlCommandBuilder(titlesAdapter);
                    titlesAdapter.Update(titlesTable);
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
                titlesCommand.Dispose();
                titlesAdapter.Dispose();
                titlesTable.Dispose();

                publishersCommand.Dispose();
                publishersAdapter.Dispose();
                publishersTable.Dispose();

                authorsCommand.Dispose();
                authorsAdapter.Dispose();
                authorsTable[0].Dispose();
                authorsTable[1].Dispose();
                authorsTable[2].Dispose();
                authorsTable[3].Dispose();
            }
        }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (titlesManager.Position == 0)
            {
                Console.Beep();
            }
            titlesManager.Position--;
            SetText();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (titlesManager.Position == titlesManager.Count - 1)
            {
                Console.Beep();
            }
            titlesManager.Position++;
            SetText();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData())
            {
                return;
            }
            string savedName = txtYear.Text;
            int savedRow;
            try
            {
                titlesManager.EndCurrentEdit();
                titlesTable.DefaultView.Sort = "Name";
                savedRow = titlesTable.DefaultView.Find(savedName);
                titlesManager.Position = savedRow;
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
                titlesManager.RemoveAt(titlesManager.Position);
            }
            catch (Exception)
            {
                MessageBox.Show("Error deleting record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void SetText()
        {
            this.Text = "Titles - Record " + (titlesManager.Position + 1).ToString() +
                " of " + titlesManager.Count.ToString() + " Records";
        }
        private void SetState(string appState)
        {
            myState = appState;

            switch (appState)
            {
                case "View":
                    txtTitle.ReadOnly = true;
                    txtYear.ReadOnly = true;
                    txtISBN.ReadOnly = true;
                    txtISBN.BackColor = Color.White;
                    txtISBN.ForeColor = Color.Black;
                    txtDescription.ReadOnly = true;
                    txtNotes.ReadOnly = true;
                    txtSubject.ReadOnly = true;
                    txtComments.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnAddNew.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    btnFirst.Enabled = true;
                    btnLast.Enabled = true;
                    grpFindTitle.Enabled = true;
                    cboPublisher.Enabled = false;
                    cboAuthor1.Enabled = true;
                    cboAuthor2.Enabled = true;
                    cboAuthor3.Enabled = true;
                    cboAuthor4.Enabled = true;
                    btnXAuthor1.Enabled = false;
                    btnXAuthors2.Enabled = false;
                    btnXAuthor3.Enabled = false;
                    btnXAuthor4.Enabled = false;
                    txtTitle.Focus();
                    break;
                default: // Add or Edit if not View
                    txtTitle.ReadOnly = false;
                    txtYear.ReadOnly = false;
                    txtISBN.ReadOnly = false;
                    if (myState.Equals("Edit"))
                    {
                        txtISBN.BackColor = Color.Red;
                        txtYear.ForeColor = Color.White;
                        txtISBN.ReadOnly = true;
                        txtISBN.TabStop = false;
                    }
                    else
                    {
                        txtISBN.TabStop = true;
                    }
                    txtDescription.ReadOnly = false;
                    txtNotes.ReadOnly = false;
                    txtSubject.ReadOnly = false;
                    txtComments.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    btnFirst.Enabled = false;
                    btnLast.Enabled = false;
                    grpFindTitle.Enabled = false;
                    cboPublisher.Enabled = false;
                    cboAuthor1.Enabled = false;
                    cboAuthor2.Enabled = false;
                    cboAuthor3.Enabled = false;
                    cboAuthor4.Enabled = false;
                    btnXAuthor1.Enabled = true;
                    btnXAuthors2.Enabled = true;
                    btnXAuthor3.Enabled = true;
                    btnXAuthor4.Enabled = true;
                    txtTitle.Focus();
                    break;
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                myBookmark = titlesManager.Position;
                titlesManager.AddNew();
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
            titlesManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                titlesManager.Position = myBookmark;
            }
            SetState("View");
        }

        private bool ValidateData()
        {
            string message = "";
            bool allOK = true;
            return (allOK);
        }

        private void txtAuthorName_KeyPress(object sender, KeyPressEventArgs e)
        {
            /* TextBox whichBox = (TextBox)sender;
            if ((int)e.KeyChar == 13)
            {
                switch (whichBox.Name)
                {
                    case "txtPubName":
                        txtISBN.Focus();
                        break;
                    case "txtCompanyName":
                        txtDescription.Focus();
                        break;
                    case "txtPubAddress":
                        txtNotes.Focus();
                        break;
                    case "txtPubCity":
                        txtSubject.Focus();
                        break;
                    case "txtPubState":
                        txtComments.Focus();
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
                        txtYear.Focus();
                        break;
                }
            }*/
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
            titlesManager.Position = 0;
            SetText();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            titlesManager.Position = titlesManager.Count - 1;
            SetText();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtFind.Text.Equals(""))
            {
                return;
            }
            int savedRow = titlesManager.Position;
            DataRow[] foundRows;
            titlesTable.DefaultView.Sort = "Title";
            foundRows = titlesTable.Select("Title LIKE '" + txtFind.Text + "*'");
            if (foundRows.Length == 0)
            {
                titlesManager.Position = savedRow;
            }
            else
            {
                titlesManager.Position = titlesTable.DefaultView.Find(foundRows[0]["Title"]);
            }
            SetText();
        }

        private void btnPublishers_Click(object sender, EventArgs e)
        {
            frmPublishers pubForm = new frmPublishers();
            string pubSave = cboPublisher.Text;
            pubForm.ShowDialog();
            pubForm.Close();
            // need to regenerate publisher data
            booksConnection.Close();
            string path = Path.GetFullPath("SQLBooksDB.mdf");
            booksConnection = new SqlConnection("Data Source=.\\SQLEXPRESS; AttachDBFilename=" + path + ";" +
                    "Integrated Security=True; Connect Timeout=30; User Instance=True");
            booksConnection.Open();
            publishersAdapter.SelectCommand = publishersCommand;
            publishersTable = new DataTable();
            publishersAdapter.Fill(publishersTable);
            cboPublisher.DataSource = publishersTable;
            cboPublisher.Text = pubSave;
        }

        private void btnXAuthor_Click(object sender, EventArgs e)
        {
            Button whichButton = (Button) sender;
            switch (whichButton.Name)
            {
                case "btnXAuthor1":
                    cboAuthor1.SelectedIndex = -1;
                    break;
                case "btnXAuthor2":
                    cboAuthor2.SelectedIndex = -1;
                    break;
                case "btnXAuthor3":
                    cboAuthor3.SelectedIndex = -1;
                    break;
                case "btnXAuthor4":
                    cboAuthor4.SelectedIndex = -1;
                    break;
            }
        }
    }
}

