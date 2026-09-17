using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Xml;

namespace VpsPos
{

    public partial class UserForm : Form


    {

        //making conString  and conn global by using public static keywords
        public static string conString;

        //create databaseclass object to connect to database
        private DatabaseClass databaseClass = new DatabaseClass();

        public UserForm()
        {
            InitializeComponent();
            SearchUserComboBox.SelectedIndex = 1;
        }

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "User Id", UserIdTextBox.Text},
        { "Full Name", UserFullNameTextBox.Text},
        { "User Name", UserUserNameTextBox.Text},
        { "Previous Password", PreviousPasswordTextBox.Text },
        { "New Password", NewPasswordTextBox.Text },
        { "Confirm New Password", ConfirmNewPasswordTextBox.Text },
        { "Search Type", SearchUserComboBox.Text },
        { "Search Text", SearchUserTextBox.Text}
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in UsersGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    gridDump += $"Row {row.Index}: ";
                    foreach (DataGridViewCell cell in row.Cells)
                        gridDump += $"{cell.Value?.ToString()} | ";
                    gridDump += "\n";
                }
            }
            data.Add("GridContent", gridDump);

            return data;
        }

        //create gridview load function
        private void loadGridView()
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            //adding data source to UsersGridView
            UsersGridView.DataSource = this.databaseClass.executeSqlCommand("SELECT userId,userName,fullName,regDate FROM vpsPosUsers");
        }

        //clear text boxes
        private void clearTextBoxes()
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            UserIdTextBox.Clear();
            UserFullNameTextBox.Clear();
            UserUserNameTextBox.Clear();
            PreviousPasswordTextBox.Clear();
            NewPasswordTextBox.Clear();
            ConfirmNewPasswordTextBox.Clear();
            ConvertFormToAddNewUser();
        }

        private void ConvertFormToAddNewUser()
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            PreviousPasswordLabel.Text = "Password : ";
            NewPasswordLabel.Text = "Confirm Password : ";
            NewPasswordLabel.Left = 180;
            PreviousPasswordLabel.Left = 240;
            ConfirmNewPasswordLabel.Visible = false;
            ConfirmNewPasswordTextBox.Visible = false;
        }

        private void ConvertFormToUpdateUserInfo()
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            PreviousPasswordLabel.Text = "Previous Password : ";
            NewPasswordLabel.Text = "New Password : ";
            NewPasswordLabel.Left = 202;
            PreviousPasswordLabel.Left = 174;
            ConfirmNewPasswordLabel.Visible = true;
            ConfirmNewPasswordTextBox.Visible = true;
        }
        //loads grid view when opening form
        private void UserForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            XmlDocument doc = new XmlDocument();
            
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;

            this.WindowState = FormWindowState.Maximized;
            //call gridview function
            loadGridView();
            ConvertFormToAddNewUser();
        }

        //load information into text boxes when click a cell of gridview
        private void UsersGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            clearTextBoxes();
            //loading values to text boxes
            UserIdTextBox.Text = UsersGridView.CurrentRow.Cells[0].Value.ToString();
            UserFullNameTextBox.Text = UsersGridView.CurrentRow.Cells[2].Value.ToString();
            UserUserNameTextBox.Text = UsersGridView.CurrentRow.Cells[1].Value.ToString();

            //disable button
            SaveUserButton.Enabled = false;
            ConvertFormToUpdateUserInfo();
        }

        //add user
        private void SaveUserButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            //conditions
            if (UserFullNameTextBox.Text == "" && UserUserNameTextBox.Text == "" && PreviousPasswordTextBox.Text == "" && NewPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter all information.", "Warning");
            }
            else if (UserFullNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter fullname.", "Warning");
            }
            else if (UserUserNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter username.", "Warning");
            }
            else if (PreviousPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter password.", "Warning");
            }
            else if (NewPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter confirm password.", "Warning");
            }
            else
            {
                string userName = UserUserNameTextBox.Text.Trim();

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosUsers WHERE userName=@username";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", userName);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            MessageBox.Show("An user with username you provided already registered.", "error");
                            return;
                        }
                    }
                }

                string fullname = UserFullNameTextBox.Text.Trim();
                string password = PreviousPasswordTextBox.Text.Trim();
                string confirmedPassword = NewPasswordTextBox.Text.Trim();

                if (password != confirmedPassword)
                {
                    MessageBox.Show("Passwords don't match !", "error");
                    return;
                }

                byte[] salt;
                byte[] hash;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
                using (var sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray());
                }

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "INSERT INTO vpsPosUsers (userName,fullName,regDate,salt,hash) VALUES (@username,@fullname,GETDATE(),@salt,@hash)";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = userName;
                        command.Parameters.AddWithValue("@fullname", SqlDbType.VarChar).Value = fullname;
                        command.Parameters.AddWithValue("@salt", SqlDbType.VarBinary).Value = salt;
                        command.Parameters.AddWithValue("@hash", SqlDbType.VarBinary).Value = hash;

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User registered successfully !", "info");
                loadGridView();
                clearTextBoxes();
            }
        }

        //update user information
        private void UpdateUserButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            //conditions
            if (UserIdTextBox.Text == "" && UserFullNameTextBox.Text == "" && UserUserNameTextBox.Text == "" && PreviousPasswordTextBox.Text == "" && NewPasswordTextBox.Text == "" && ConfirmNewPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please select an user.", "Warning");
            }
            else if (UserIdTextBox.Text == "")
            {
                MessageBox.Show("Please select an user.", "Warning");
            }
            else if (UserFullNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter fullname.", "Warning");
            }
            else if (UserUserNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter username.", "Warning");
            }
            else if (PreviousPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter previous password.", "Warning");
            }
            else if (NewPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter new password.", "Warning");
            }
            else if (ConfirmNewPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter new confirm password.", "Warning");
            }
            else
            {
                // check username duplication
                string userName = UserUserNameTextBox.Text.Trim();

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosUsers WHERE userName=@username";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", userName);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            // checking that username is clicked row username
                            if (UsersGridView.CurrentRow.Cells[1].Value.ToString() != userName)
                            {
                                MessageBox.Show("An user with username you provided already registered.", "error");
                                return;
                            }
                        }
                    }
                }

                // checking hash match
                string previousPassword = PreviousPasswordTextBox.Text.Trim();
                string newPassword = NewPasswordTextBox.Text.Trim();
                string confirmedNewPassword = ConfirmNewPasswordTextBox.Text.Trim();
                string fullName = UserFullNameTextBox.Text.Trim();
                string id = UserIdTextBox.Text;

                byte[] oldHash;
                byte[] oldSalt;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT hash,salt FROM vpsPosUsers WHERE userName=@username";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = UsersGridView.CurrentRow.Cells[1].Value;
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            oldSalt = (byte[])reader["salt"];
                            oldHash = (byte[])reader["hash"];

                            byte[] enteredHash;
                            using (var sha256 = SHA256.Create())
                                enteredHash = sha256.ComputeHash(oldSalt.Concat(Encoding.UTF8.GetBytes(previousPassword)).ToArray());

                            if (enteredHash.SequenceEqual(oldHash))
                            {
                                if (newPassword != confirmedNewPassword)
                                {
                                    MessageBox.Show("New Passwords don't match !", "error");
                                    return;
                                }
                                byte[] newHash;
                                byte[] newSalt;

                                new RNGCryptoServiceProvider().GetBytes(newSalt = new byte[16]);

                                using (var sha256 = SHA256.Create())
                                {
                                    newHash = sha256.ComputeHash(newSalt.Concat(Encoding.UTF8.GetBytes(newPassword)).ToArray());
                                }

                                using (SqlConnection connection1 = new SqlConnection(conString))
                                {
                                    connection1.Open();
                                    string queryString1 = "UPDATE vpsPosUsers SET userName=@username,fullName=@fullname,regDate=GETDATE(),salt=@salt,hash=@hash WHERE userId=@id";
                                    using (SqlCommand command1 = new SqlCommand(queryString1, connection1))
                                    {
                                        command1.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = userName;
                                        command1.Parameters.AddWithValue("@fullname", SqlDbType.VarChar).Value = fullName;
                                        command1.Parameters.AddWithValue("@salt", SqlDbType.VarBinary).Value = newSalt;
                                        command1.Parameters.AddWithValue("@hash", SqlDbType.VarBinary).Value = newHash;
                                        command1.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;

                                        command1.ExecuteNonQuery();

                                        MessageBox.Show("User updated successfully ! ", "info");

                                        clearTextBoxes();
                                        loadGridView();
                                        SaveUserButton.Enabled = true;
                                    }
                                }

                            }
                            else
                            {
                                MessageBox.Show("Previous Password is incorrect !", "warning");
                                return;
                            }
                        }
                    }
                }


            }
        }

        //clear text boxes
        private void ClearUserButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            clearTextBoxes();
            SaveUserButton.Enabled = true;
        }

        //delete user
        private void DeleteUserButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            if (UserIdTextBox.Text == "" && UserFullNameTextBox.Text == "" && UserUserNameTextBox.Text == "" && PreviousPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please select an user.", "Warning");
            }
            else if (UserIdTextBox.Text == "")
            {
                MessageBox.Show("Please select an user.", "Warning");
            }
            else if (PreviousPasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter previous password.", "Warning");
            }
            else
            {
                //remove user
                if (MessageBox.Show("Do you want to delete this user?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string id = UserIdTextBox.Text;
                    string previousPassword = PreviousPasswordTextBox.Text.Trim();

                    byte[] hash;
                    byte[] salt;

                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT salt,hash FROM vpsPosUsers WHERE userId=@userid";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@userid", SqlDbType.Int).Value = id;

                            SqlDataReader reader = command.ExecuteReader();

                            if (reader.Read())
                            {
                                salt = (byte[])reader["salt"];
                                hash = (byte[])reader["hash"];

                                byte[] enteredHash;

                                using (var sha256 = SHA256.Create())
                                {
                                    enteredHash = sha256.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(previousPassword)).ToArray());

                                    if (enteredHash.SequenceEqual(hash))
                                    {
                                        using (SqlConnection connection1 = new SqlConnection(conString))
                                        {
                                            connection1.Open();
                                            string queryString1 = "DELETE FROM vpsPosUsers WHERE userId=@id";
                                            using (SqlCommand command1 = new SqlCommand(queryString1, connection1))
                                            {
                                                command1.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;
                                                command1.ExecuteNonQuery();

                                                MessageBox.Show("User deleted !", "Info");
                                                loadGridView();
                                                clearTextBoxes();
                                                SaveUserButton.Enabled = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Previous password is incorrect !", "error");
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    MessageBox.Show("User not deleted !", "Info");
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            this.Close();
        }

        private void SearchUserTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("UserForm", GetCurrentFormData());
            InfoForm infoForm = new InfoForm();
            if (SearchUserTextBox.Text.Count() < 3)
            {
                infoForm.Show();

            }

            DataTable searchedUsersDatatable = new DataTable();
            string searchColumn = SearchUserComboBox.Text;
            using (SqlConnection connection = new SqlConnection(conString))
            {
                connection.Open();
                string queryString = $"SELECT userId,userName,fullName,regDate FROM vpsPosUsers WHERE {searchColumn} like @phrase";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@phrase", "%" + SearchUserTextBox.Text + "%");
                    command.ExecuteNonQuery();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);

                    adapter.Fill(searchedUsersDatatable);
                    UsersGridView.DataSource = searchedUsersDatatable;
                }
            }

            if (SearchUserTextBox.Text.Count() < 3)
            {
                infoForm.Close();

            }
        }

    }
}


