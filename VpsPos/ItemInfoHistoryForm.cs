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
    public partial class ItemInfoHistoryForm : Form
    {
        public ItemInfoHistoryForm()
        {
            InitializeComponent();
        }

        public static string conString;
        XmlDocument doc = new XmlDocument();

        // logging helper
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "From Date", FromDateTimePicker.Text},
        { "To Date",ToDateTimePicker.Text},
        { "Search with Button", SearchWithButtonCheckBox.Checked.ToString()},
        { "Search between Dates", SearchBetweenDatesCheckBox.Checked.ToString() },
        { "Search type", SearchComboBox.Text },
        { "Search Text", SearchTextBox.Text },
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in ItemInfoHistoryGridView.Rows)
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

        private void ItemInfoHistoryForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            try
            {
                // Get the current date
                DateTime currentDate = DateTime.Today;


                DateTime startDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);

                DateTime endDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);

                doc.Load("requiredInfo.xml");
                XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
                conString = nodes[0].InnerText;
                this.WindowState = FormWindowState.Maximized;
                SearchComboBox.SelectedIndex = 0;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT id as Id,date as Date,itemId as Item_Id,itemName as Item_Name,barcode as Barcode,costPrice as Cost_Price,wholesalePrice as Wholesale_Price,salesPrice AS Sale_Price,billPrice as Bill_Price,quantity as Quantity FROM vpsPosInfoHistory WHERE date between @fromDate AND @toDate";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@fromDate", SqlDbType.DateTime).Value = startDateTime;
                        command.Parameters.AddWithValue("@toDate", SqlDbType.DateTime).Value = endDateTime;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable historyDatatable = new DataTable();
                        adapter.Fill(historyDatatable);
                        ItemInfoHistoryGridView.DataSource = historyDatatable;
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "ItemInfoHistoryForm", nameof(ItemInfoHistoryForm_Load), GetCurrentFormData());
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            this.Close();
        }

        private void DateSearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            try
            {

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT id as Id,date as Date,itemId as Item_Id,itemName as Item_Name,barcode as Barcode,costPrice as Cost_Price,wholesalePrice as Wholesale_Price,salesPrice AS Sale_Price,billPrice as Bill_Price,quantity as Quantity FROM vpsPosInfoHistory WHERE date between @fromDate AND @toDate";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@fromDate", SqlDbType.DateTime).Value = FromDateTimePicker.Value;
                        command.Parameters.AddWithValue("@toDate", SqlDbType.DateTime).Value = ToDateTimePicker.Value;
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable historyDatatable = new DataTable();
                        adapter.Fill(historyDatatable);
                        ItemInfoHistoryGridView.DataSource = historyDatatable;
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "ItemInfoHistoryForm", nameof(DateSearchButton_Click), GetCurrentFormData());
            }
        }

        private void getSearchResult()
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            try
            {
                if (SearchBetweenDatesCheckBox.Checked == true)
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT id as Id,date as Date,itemId as Item_Id,itemName as Item_Name,barcode as Barcode,costPrice as Cost_Price,wholesalePrice as Wholesale_Price,salesPrice AS Sale_Price,billPrice as Bill_Price,quantity as Quantity FROM vpsPosInfoHistory WHERE date between @fromDate AND @toDate AND {SearchComboBox.SelectedItem.ToString()} like @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@fromDate", SqlDbType.DateTime).Value = FromDateTimePicker.Value;
                            command.Parameters.AddWithValue("@toDate", SqlDbType.DateTime).Value = ToDateTimePicker.Value;
                            command.Parameters.AddWithValue("@phrase", SqlDbType.NVarChar).Value = "%" + SearchTextBox.Text.Trim() + "%";
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable historyDatatable = new DataTable();
                            adapter.Fill(historyDatatable);
                            ItemInfoHistoryGridView.DataSource = historyDatatable;
                        }
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT id as Id,date as Date,itemId as Item_Id,itemName as Item_Name,barcode as Barcode,costPrice as Cost_Price,wholesalePrice as Wholesale_Price,salesPrice AS Sale_Price,billPrice as Bill_Price,quantity as Quantity FROM vpsPosInfoHistory WHERE {SearchComboBox.SelectedItem.ToString()} like @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@phrase", SqlDbType.NVarChar).Value = "%" + SearchTextBox.Text.Trim() + "%";
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable historyDatatable = new DataTable();
                            adapter.Fill(historyDatatable);
                            ItemInfoHistoryGridView.DataSource = historyDatatable;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "ItemInfoHistoryForm", nameof(getSearchResult), GetCurrentFormData());
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            if (SearchTextBox.Text.Trim().Count() == 0)
            {
                ItemInfoHistoryGridView.DataSource = null;
            }
            else if (SearchWithButtonCheckBox.Checked == false && SearchComboBox.SelectedItem.ToString() != "barcode")
            {

                InfoForm infoForm = new InfoForm();
                if (SearchTextBox.Text.Count() <= 3)
                {
                    infoForm.Show();
                }

                getSearchResult();

                if (SearchTextBox.Text.Count() <= 3)
                {
                    infoForm.Close();
                }

            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            if (SearchTextBox.Text.Trim().Count() == 0)
            {
                ItemInfoHistoryGridView.DataSource = null;
            }
            else if (SearchWithButtonCheckBox.Checked == true)
            {
                getSearchResult();
            }
            else
            {
                MessageBox.Show("Search with button check box not checked !!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("ItemInfoHistoryForm", GetCurrentFormData());
            if (e.KeyCode == Keys.Enter)
            {
                if (SearchComboBox.SelectedItem.ToString() == "barcode" && SearchWithButtonCheckBox.Checked == false)
                {
                    getSearchResult();
                }
            }
        }

        private void FromDateTimePicker_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
