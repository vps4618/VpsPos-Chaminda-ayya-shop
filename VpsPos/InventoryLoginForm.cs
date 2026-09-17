using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class InventoryLoginForm : Form
    {
        private DatabaseClass myDatabase = new DatabaseClass();

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Username", UsernameTextBox2.Text},
                { "Password",PasswordTextBox2.Text}
    };
            return data;
        }
        public InventoryLoginForm()
        {
            InitializeComponent();
            UsernameTextBox2.Text = "";
            PasswordTextBox2.Text = "";
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryLoginForm", GetCurrentFormData());
            if (UsernameTextBox2.Text == "" && PasswordTextBox2.Text == "")
            {
                MessageBox.Show("Please enter username and password.", "Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
            else if (UsernameTextBox2.Text == "")
            {
                MessageBox.Show("Please enter username.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (PasswordTextBox2.Text == "")
            {
                MessageBox.Show("Please enter password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                /*
                  ErrorLogger.UpdateFormData("LoginForm", GetCurrentFormData());
            if (UsernameTextBox.Text == "" && PasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter username and password.", "Warning");
            }
            else if (UsernameTextBox.Text == "")
            {
                MessageBox.Show("Please enter username.", "Warning");
            }
            else if (PasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter password.", "Warning");
            }
            else
            {

                string username = UsernameTextBox.Text.Trim();
                string password = PasswordTextBox.Text.Trim();
                DataTable loginTable = new DataTable();

                byte[] salt;
                byte[] hash;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosUsers WHERE userName=@username";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = username;
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            salt = (byte[])reader["salt"];
                            hash = (byte[])reader["hash"];

                            reader.Close();
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(loginTable);
                        }
                        else
                        {
                            MessageBox.Show("Username or Password is incorrect !", "warning");
                            return;
                        }

                    }
                }

                byte[] enteredHash;
                using (var sha256 = SHA256.Create())
                    enteredHash = sha256.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray());

                if (enteredHash.SequenceEqual(hash))
                {
                    MessageBox.Show("Login Successfull !", "info");
                    MainForm mainForm = new MainForm();
                    mainForm.userlogin = loginTable;
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Username or Password is incorrect !", "warning");
                }*/

                XmlDocument doc1 = new XmlDocument();

                doc1.Load("requiredInfo.xml");
                XmlNodeList nodes1 = doc1.GetElementsByTagName("sqlString");
                string conString1 = nodes1[0].InnerText;


                string username = UsernameTextBox2.Text.Trim();
                string password = PasswordTextBox2.Text.Trim();
                DataTable loginTable = new DataTable();

                byte[] salt;
                byte[] hash;

                using (SqlConnection connection = new SqlConnection(conString1))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosUsers WHERE userName=@username";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = username;
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            salt = (byte[])reader["salt"];
                            hash = (byte[])reader["hash"];

                            reader.Close();
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(loginTable);
                        }
                        else
                        {
                            MessageBox.Show("Username or Password is incorrect !", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                    }
                }

                byte[] enteredHash;
                using (var sha256 = SHA256.Create())
                    enteredHash = sha256.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray());

                if (enteredHash.SequenceEqual(hash))
                {
                    MessageBox.Show("Login Successfull !", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // CHANGE 3: Logic to open InventoryForm and update MainForm

                    // 1. Get reference to the Parent (MainForm)
                    MainForm mainForm = (MainForm)this.MdiParent;

                    // 2. Check if it's already open (safety check)
                    if (mainForm.inventoryForm == null)
                    {
                        // 3. Create the new form
                        InventoryForm openInventory = new InventoryForm();

                        // 4. Set MDI Parent
                        openInventory.MdiParent = mainForm;

                        // 5. Connect the Close Event to MainForm's handler
                        openInventory.FormClosed += mainForm.InventoryForm_FormClosed;

                        // 6. UPDATE MAINFORM'S REFERENCE
                        mainForm.inventoryForm = openInventory;

                        // 7. Show the form
                        openInventory.Show();
                    }
                    else
                    {
                        mainForm.inventoryForm.Activate();
                    }

                    // 8. Close this Login form
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Username or Password is incorrect !", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
              
               
            }

        }

        private void InventoryLoginForm_Load(object sender, EventArgs e)
        {

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
