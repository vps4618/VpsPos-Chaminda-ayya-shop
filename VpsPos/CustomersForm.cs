using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class CustomersForm : Form
    {
        public CustomersForm()
        {
            InitializeComponent();
        }

        string conString;

        // logging helper
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Id", IdTextBox.Text},
        { "Name", NameTextBox.Text},
        { "Phone Number", PhoneNumberTextBox.Text},
        { "Email", EmailTextBox.Text },
        { "Address", AddressTextBox.Text },
        {"Age", AgeTextBox.Text}
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in CustomersGridView.Rows)
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

        private void CustomersForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            XmlDocument doc = new XmlDocument();
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;
            this.WindowState = FormWindowState.Maximized;
            SearchComboBox.SelectedIndex = 1;
        }

        private void AddButton_Click(object sender, EventArgs e)

        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            if (NameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Name is not filled !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string name = NameTextBox.Text.Trim();
                string phoneNumber = PhoneNumberTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string address = AddressTextBox.Text.Trim();
                string age = AgeTextBox.Text.Trim();

                try
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT * FROM vpsPosCustomers WHERE name=@name";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = name;
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable customerDataTable = new DataTable();
                            adapter.Fill(customerDataTable);

                            if (customerDataTable.Rows.Count > 0)
                            {
                                MessageBox.Show("A customer with the name you provided is already in the database !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {

                                using (SqlConnection connection1 = new SqlConnection(conString))
                                {
                                    connection1.Open();
                                    string queryString1 = "INSERT INTO vpsPosCustomers (name,phoneNumber,email,address,age,debt) VALUES(@name,@phoneNumber,@email,@address,@age,0);";
                                    using (SqlCommand command1 = new SqlCommand(queryString1, connection1))
                                    {
                                        command1.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = name;
                                        command1.Parameters.AddWithValue("@phoneNumber", SqlDbType.VarChar).Value = phoneNumber;
                                        command1.Parameters.AddWithValue("@email", SqlDbType.VarChar).Value = email;
                                        command1.Parameters.AddWithValue("@address", SqlDbType.VarChar).Value = address;
                                        command1.Parameters.AddWithValue("@age", SqlDbType.VarChar).Value = age;
                                        command1.ExecuteNonQuery();

                                    }
                                }

                                MessageBox.Show("Customer added to the database successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                ClearAllTextBoxes();
                            }
                        }
                    }


                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "CustomerForm", nameof(AddButton_Click), GetCurrentFormData());
                }
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            if (IdTextBox.Text == "")
            {
                MessageBox.Show("Please select a customer from grid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else if (NameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Name is not filled.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string id = IdTextBox.Text;
                string name = NameTextBox.Text.Trim();
                string phoneNumber = PhoneNumberTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string address = AddressTextBox.Text.Trim();
                string age = AgeTextBox.Text.Trim();

                try
                {
                    using (SqlConnection connection2 = new SqlConnection(conString))
                    {
                        connection2.Open();
                        string queryString2 = "SELECT * FROM vpsPosCustomers WHERE name=@name";
                        using (SqlCommand command2 = new SqlCommand(queryString2, connection2))
                        {
                            command2.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = name;
                            SqlDataAdapter adapter = new SqlDataAdapter(command2);
                            DataTable customerDataTable = new DataTable();
                            adapter.Fill(customerDataTable);

                            if (customerDataTable.Rows.Count > 0 && customerDataTable.Rows[0][0].ToString() != id)
                            {
                                MessageBox.Show("A user with the name you provided already exist in the database !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                using (SqlConnection connection1 = new SqlConnection(conString))
                                {
                                    connection1.Open();
                                    string queryString1 = "UPDATE vpsPosCustomers SET name=@name,phoneNumber=@phoneNumber,email=@email,address=@address,age=@age WHERE id=@id;";
                                    using (SqlCommand command1 = new SqlCommand(queryString1, connection1))
                                    {
                                        command1.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = name;
                                        command1.Parameters.AddWithValue("@phoneNumber", SqlDbType.VarChar).Value = phoneNumber;
                                        command1.Parameters.AddWithValue("@email", SqlDbType.VarChar).Value = email;
                                        command1.Parameters.AddWithValue("@address", SqlDbType.VarChar).Value = address;
                                        command1.Parameters.AddWithValue("@age", SqlDbType.VarChar).Value = age;
                                        command1.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;
                                        command1.ExecuteNonQuery();
                                    }
                                }

                                MessageBox.Show("Customer updated successfully.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                ClearAllTextBoxes();
                            }
                        }
                    }



                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "CustomerForm", nameof(UpdateButton_Click), GetCurrentFormData());
                }
            }
        }

        private void ClearAllTextBoxes()
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            IdTextBox.Clear();
            NameTextBox.Clear();
            PhoneNumberTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
            AgeTextBox.Clear();

            AddButton.Enabled = true;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ClearAllTextBoxes();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InsertDataToGrid()
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            if (SearchTextBox.Text.Trim() == "")
            {
                CustomersGridView.DataSource = null;

            }
            else
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = $"SELECT id,name,phoneNumber,email,address,age FROM vpsPosCustomers  WHERE {SearchComboBox.SelectedItem} like @phrase;";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@phrase", SqlDbType.NVarChar).Value = "%" + SearchTextBox.Text.Trim() + "%";
                        SqlDataAdapter adapter = new SqlDataAdapter(command);

                        DataTable customerDataTable = new DataTable();
                        adapter.Fill(customerDataTable);
                        CustomersGridView.DataSource = customerDataTable;
                    }
                }

            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (IdTextBox.Text == "")
            {
                MessageBox.Show("Please select a customer from the grid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    string id = IdTextBox.Text;
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();
                        try
                        {
                            string queryString = @"
                                               DELETE FROM vpsPosDebtTransactions WHERE customer_id=@id;
                                                DELETE FROM vpsPosCustomers WHERE id=@id;
                                              ";
                            SqlCommand command = new SqlCommand(queryString, connection, transaction);

                            command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;
                            command.ExecuteNonQuery();

                            transaction.Commit();

                            MessageBox.Show("Customer deleted from the database successfully.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearAllTextBoxes();
                        }
                        catch (Exception error)
                        {
                            transaction.Rollback();
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "CustomerForm", nameof(DeleteButton_Click), GetCurrentFormData());
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }

                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "CustomerForm", nameof(DeleteButton_Click), GetCurrentFormData());
                }
            }
        }

        private void CustomersGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string id = CustomersGridView.CurrentRow.Cells[0].Value.ToString();

            DataTable customerDataTable = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosCustomers WHERE id=@id;";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(customerDataTable);
                    }
                }
                IdTextBox.Text = customerDataTable.Rows[0][0].ToString();
                NameTextBox.Text = customerDataTable.Rows[0][1].ToString();
                PhoneNumberTextBox.Text = customerDataTable.Rows[0][2].ToString();
                EmailTextBox.Text = customerDataTable.Rows[0][3].ToString();
                AddressTextBox.Text = customerDataTable.Rows[0][4].ToString();
                AgeTextBox.Text = customerDataTable.Rows[0][5].ToString();

                AddButton.Enabled = false;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "CustomerForm", nameof(CustomersGridView_CellClick), GetCurrentFormData());
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            if (SearchCheckBox.Checked == true)
            {
                InsertDataToGrid();
            }
            else
            {
                MessageBox.Show("Check box not checked !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            if (SearchCheckBox.Checked == false)
            {
                InsertDataToGrid();

            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            InsertDataToGrid();
        }

        private void GetAllButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("CustomersForm", GetCurrentFormData());
            InfoForm infoForm = new InfoForm();
            infoForm.Show();
            using (SqlConnection connection = new SqlConnection(conString))
            {
                connection.Open();
                string queryString = "SELECT * FROM vpsPosCustomers";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable customerDatatable = new DataTable();
                    adapter.Fill(customerDatatable);
                    CustomersGridView.DataSource = customerDatatable;
                }
            }
            infoForm.Close();
        }


    }
}
