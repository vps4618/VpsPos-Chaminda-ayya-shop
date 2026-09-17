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
    public partial class DailyTransactionsForm : Form
    {
        public DailyTransactionsForm()
        {
            InitializeComponent();
        }

        XmlDocument doc = new XmlDocument();
        string conString = "";

        // logger helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Id", IdTextBox.Text},
        { "Customer/Supplier Name", NameTextBox.Text},
        { "Type", TypeComboBox.Text},
        { "Amount/Items/Notes", AmountRichTextBox.Text },
        { "IsTransactionEnded", IsTransactionEndedCheckBox.Checked.ToString() },
        { "From Date", FromDateTimePicker.Text },
        { "To Date", ToDateTimePicker.Text },
        { "Is-Search-With-Button-Checked", SearchWithButtonCheckBox.Checked.ToString()},
        { "Is-Search-Between-dates-checked", SearchWithDateCheckBox.Checked.ToString() },
        { "Search type", SearchComboBox.Text },
                {"Search Text",SearchTextBox.Text }
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in TransactionsGridView.Rows)
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

        private void DailyTransactionsForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;

            TypeComboBox.SelectedIndex = 0;
            SearchComboBox.SelectedIndex = 0;
            this.WindowState = FormWindowState.Maximized;
            UpdateButton.Enabled = false;
            DeleteButton.Enabled = false;

            try
            {
                // Get the current date
                DateTime currentDate = DateTime.Today;


                DateTime startDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);

                DateTime endDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = $"SELECT * FROM vpsPosDailyTransactions WHERE Date BETWEEN @startDate AND @endDate";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", SqlDbType.DateTime).Value = startDateTime;
                        command.Parameters.AddWithValue("@endDate", SqlDbType.DateTime).Value = endDateTime;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable resultDataTable = new DataTable();
                        adapter.Fill(resultDataTable);
                        TransactionsGridView.DataSource = resultDataTable;

                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DailyTransactionForm", nameof(DailyTransactionsForm_Load), GetCurrentFormData());

            }
        }

        private void clearAll()
        {
            IsTransactionEndedCheckBox.Checked = false;
            IdTextBox.Clear();
            NameTextBox.Clear();
            TypeComboBox.SelectedIndex = 0;
            AmountRichTextBox.Clear();
            AddButton.Enabled = true;
            UpdateButton.Enabled = false;
            DeleteButton.Enabled = false;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            if (NameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter Customer/Supplier Name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (AmountRichTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter Amount/Items/Note.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "INSERT INTO vpsPosDailyTransactions (Date,[Customer/Supplier_Name],Type,[Amount/Items/Note],Is_Transaction_Ended) VALUES (GETDATE(),@name,@type,@amount,@ended)";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = NameTextBox.Text.Trim();
                            command.Parameters.AddWithValue("@type", SqlDbType.VarChar).Value = TypeComboBox.Text;
                            command.Parameters.AddWithValue("@amount", SqlDbType.NVarChar).Value = AmountRichTextBox.Text.Trim();
                            command.Parameters.AddWithValue("@ended", SqlDbType.VarChar).Value = IsTransactionEndedCheckBox.Checked.ToString();
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Transaction added successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    clearAll();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "DailyTransactionForm", nameof(AddButton_Click), GetCurrentFormData());
                }
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            clearAll();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            if (NameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter Customer/Supplier Name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (AmountRichTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter Amount/Items/Note.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "UPDATE vpsPosDailyTransactions SET Date=GETDATE(),[Customer/Supplier_Name]=@name,Type=@type,[Amount/Items/Note]=@amount,Is_Transaction_Ended=@ended WHERE ID=@id";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = NameTextBox.Text.Trim();
                            command.Parameters.AddWithValue("@type", SqlDbType.VarChar).Value = TypeComboBox.Text;
                            command.Parameters.AddWithValue("@amount", SqlDbType.NVarChar).Value = AmountRichTextBox.Text.Trim();
                            command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = IdTextBox.Text.Trim();
                            command.Parameters.AddWithValue("@ended", SqlDbType.VarChar).Value = IsTransactionEndedCheckBox.Checked.ToString();
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Transaction updated successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    clearAll();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "DailyTransactionForm", nameof(UpdateButton_Click), GetCurrentFormData());

                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "DELETE FROM vpsPosDailyTransactions WHERE ID=@id";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = IdTextBox.Text.Trim();
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Transaction deleted successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                clearAll();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DailyTransactionForm", nameof(DeleteButton_Click), GetCurrentFormData());

            }
        }

        private void GetResultToGridView()
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            try
            {
                if (SearchWithDateCheckBox.Checked == true)
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT * FROM vpsPosDailyTransactions WHERE Date BETWEEN @startDate AND @endDate AND [{SearchComboBox.Text}] LIKE @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@startDate", SqlDbType.DateTime).Value = FromDateTimePicker.Value;
                            command.Parameters.AddWithValue("@endDate", SqlDbType.DateTime).Value = ToDateTimePicker.Value;
                            command.Parameters.AddWithValue("@phrase", SqlDbType.NVarChar).Value = "%" + SearchTextBox.Text.Trim() + "%";
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable resultDataTable = new DataTable();
                            adapter.Fill(resultDataTable);
                            TransactionsGridView.DataSource = resultDataTable;

                        }
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT * FROM vpsPosDailyTransactions WHERE [{SearchComboBox.Text}] LIKE @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@phrase", SqlDbType.NVarChar).Value = "%" + SearchTextBox.Text.Trim() + "%";
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable resultDataTable = new DataTable();
                            adapter.Fill(resultDataTable);
                            TransactionsGridView.DataSource = resultDataTable;

                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DailyTransactionForm", nameof(GetResultToGridView), GetCurrentFormData());

            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            if (SearchWithButtonCheckBox.Checked == true)
            {
                if (SearchTextBox.Text.Trim() == "")
                {
                    TransactionsGridView.DataSource = null;
                }
                else
                {
                    GetResultToGridView();
                }
            }
            else
            {
                MessageBox.Show("Search with button checked ");
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            string searchText = SearchTextBox.Text.Trim();
            if (SearchWithButtonCheckBox.Checked == false)
            {
                if (searchText == "")
                {
                    TransactionsGridView.DataSource = null;
                }
                else
                {
                    InfoForm infoForm = new InfoForm();
                    if (searchText.Count() <= 3)
                    {
                        infoForm.Show();
                    }

                    GetResultToGridView();

                    if (searchText.Count() <= 3)
                    {
                        infoForm.Close();
                    }

                }
            }
        }

        private void DateSearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = $"SELECT * FROM vpsPosDailyTransactions WHERE Date BETWEEN @startDate AND @endDate";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", SqlDbType.DateTime).Value = FromDateTimePicker.Value;
                        command.Parameters.AddWithValue("@endDate", SqlDbType.DateTime).Value = ToDateTimePicker.Value;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable resultDataTable = new DataTable();
                        adapter.Fill(resultDataTable);
                        TransactionsGridView.DataSource = resultDataTable;

                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DailyTransactionForm", nameof(DateSearchButton_Click), GetCurrentFormData());

            }
        }

        private void TransactionsGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("DailyTransactionsForm", GetCurrentFormData());
            if (TransactionsGridView.RowCount > 0)
            {
                try
                {
                    string id = TransactionsGridView.CurrentRow.Cells[0].Value.ToString();
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT * FROM vpsPosDailyTransactions WHERE Id=@id";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = id;
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable recordDatatable = new DataTable();
                            adapter.Fill(recordDatatable);

                            clearAll();

                            IdTextBox.Text = recordDatatable.Rows[0][0].ToString();
                            NameTextBox.Text = recordDatatable.Rows[0][2].ToString();
                            AmountRichTextBox.Text = recordDatatable.Rows[0][4].ToString();
                            TypeComboBox.Text = recordDatatable.Rows[0][3].ToString();
                            IsTransactionEndedCheckBox.Checked = Convert.ToBoolean(recordDatatable.Rows[0][5].ToString());

                            AddButton.Enabled = false;
                            UpdateButton.Enabled = true;
                            DeleteButton.Enabled = true;
                        }
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "DailyTransactionForm", nameof(TransactionsGridView_CellClick), GetCurrentFormData());

                }
            }
        }

    }
}
