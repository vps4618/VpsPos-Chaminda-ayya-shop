using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Xml;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VpsPos
{
    public partial class InstallmentForm : Form
    {
        public InstallmentForm()
        {
            InitializeComponent();
        }

        // logging helper
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Customer Name", CustomerComboBox.Text},
        { "Total amount", TotalAmountTextBox.Text},
        { "Down payment", DownPaymentTextBox.Text},
        { "Interest Rate", InterestRateTextBox.Text },
        { "Month", MonthsTextBox.Text }
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in ScheduleGridView.Rows)
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


        private void InstallmentForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InstallmentForm", GetCurrentFormData());

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load("requiredInfo.xml");
                String conString = xmlDocument.GetElementsByTagName("sqlString")[0].InnerText;
                this.WindowState = FormWindowState.Maximized;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    using (SqlCommand selectCommand = new SqlCommand("SELECT name FROM vpsPosCustomers", connection))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                        DataTable dataTable = new DataTable();
                        sqlDataAdapter.Fill(dataTable);
                        for (int index = 0; index < dataTable.Rows.Count; ++index)
                        {
                            this.CustomerComboBox.Items.Add(dataTable.Rows[index][0]);
                        }
                    }
                }

                CustomerComboBox.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "InstallmentForm", "InstallmentForm_Load", GetCurrentFormData());
                MessageBox.Show("An error occurred while loading the form. Please check the logs for more details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
