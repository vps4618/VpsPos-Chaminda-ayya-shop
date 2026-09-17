using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class AnalyticsDashboardForm : Form
    {
        string conString;
        public bool isDarkMode { get; set; }

        public AnalyticsDashboardForm()
        {
            InitializeComponent();
            
            //********

            //
            //
            //
            //
            //
            //-- REMOVE THESE LINES AFTER FINISHING CREATING ANALYTICS WINDOW (Also edit Program.cs file)


            this.ControlBox = true;
            this.WindowState = FormWindowState.Maximized;
            //--
            

        }

        // adding logging helper method
        private Dictionary<string , string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {
        { "Active Tab", mainTabControl != null ? mainTabControl.SelectedTab.Text : "None" },
        { "Search Item", txtSearchItem != null ? txtSearchItem.Text : "" },
        { "Start Date", dtpStartDate != null ? dtpStartDate.Text : "" },
        { "End Date", dtpEndDate != null ? dtpEndDate.Text : "" }
    };
            return data;
        }

        private void AnalyticsDashboardForm_Load(object sender, EventArgs e)
        {
            // logging the form load
            ErrorLogger.UpdateFormData("AnalyticsDashboardForm", GetCurrentFormData());

            try
            {
                // Load connection string from requiredInfo.xml
                XmlDocument doc = new XmlDocument();
                doc.Load("requiredInfo.xml");
                XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
                conString = nodes[0].InnerText;
                // Apply Dark Mode if it's enabled in MainForm
                //ApplyTheme();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading Analytics", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(ex, "AnalyticsDashboardForm", nameof(AnalyticsDashboardForm_Load), GetCurrentFormData());
            }
        }

        /*private void ApplyTheme()
        {
            if (isDarkMode)
            {
                this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30); // Dark Background
                this.ForeColor = System.Drawing.Color.White; // Light Text

                // If i add datagridviews , style them here too
                // foreach (Control c in this.Controls) { ... }
            }
            else
            {
                this.BackColor = System.Drawing.SystemColors.Control;
                this.ForeColor = System.Drawing.SystemColors.ControlText;
            }
        }*/

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // change theme
            if (keyData == (Keys.Control | Keys.T))
            {
                isDarkMode = !isDarkMode;
                SwitchMode.ApplyTheme(this, isDarkMode);

                // --- SYNC WITH MAIN FORM ---
                if (this.MdiParent is MainForm mainForm)
                {
                    mainForm.isDarkMode = this.isDarkMode;
                    BillsForm.isDarkMode = this.isDarkMode;
                }

                return false;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnSearchItem_Click(object sender, EventArgs e)
        {
            // logging the action
            ErrorLogger.UpdateFormData("AnalyticsDashboardForm", GetCurrentFormData());

            if (string.IsNullOrWhiteSpace(txtSearchItem.Text))
            {
                MessageBox.Show("Please enter an item name to search.");
                return;
            }

            // Showing loading screen
            InfoForm infoForm = new InfoForm();
            infoForm.Show();
            Application.DoEvents(); // Keeps the UI from freezing while the query runs

            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();

                    // SQL Query: Joins ReceiptItems with Receipts to filter by date
                    // Calculates Total Quantity, Revenue, and Profit for the searched item
                    string query = @"
                SELECT 
                    i.itemName AS 'Item Name',
                    SUM(i.quantity) AS 'Total Quantity Sold',
                    SUM(i.salesPriceTotal) AS 'Total Revenue (Rs.)',
                    SUM(i.salesPriceTotal - i.costPriceTotal) AS 'Total Profit (Rs.)'
                FROM vpsPosReceiptItems i
                INNER JOIN vpsPosReceipts r ON i.receiptId = r.receiptId
                WHERE i.itemName LIKE @itemName 
                  AND r.receiptDate >= @startDate 
                  AND r.receiptDate <= @endDate
                GROUP BY i.itemName";

                using (SqlCommand cmd = new SqlCommand(query , connection))
                    {
                        // Using LIKE allows partial searching (e.g., typing "Dhal" finds "Mysoor Dhal")
                        cmd.Parameters.AddWithValue("@itemName", "%" + txtSearchItem.Text.Trim() + "%");

                        // Set start date to the very beginning of the day (00:00:00)
                        cmd.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);

                        // Set end date to the very end of the day (23:59:59)
                        cmd.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the results to the grid
                        dgvItemInsights.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error fetching item insights", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(ex, "AnalyticsDashboardForm", nameof(btnSearchItem_Click), GetCurrentFormData());
            }
            finally
            {
                // ensuring the loading screen closes even if an error occurs
                infoForm.Close();
            }

        }

        private void dgvItemInsights_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // We add a parameter to the method: string exactItemName
        private void LoadChartData(string exactItemName)
        {
            string viewBy = cmbViewBy.Text;
            string selectDateClause = "";
            string groupByClause = "";

            if (viewBy == "Day")
            { /* Same logic as before */
                selectDateClause = "CONVERT(date, r.receiptDate) AS DateLabel";
                groupByClause = "CONVERT(date, r.receiptDate)";
            }
            else if (viewBy == "Month")
            { /* Same logic as before */
                selectDateClause = "CONVERT(varchar(7), r.receiptDate, 120) AS DateLabel";
                groupByClause = "CONVERT(varchar(7), r.receiptDate, 120)";
            }
            else
            { /* Same logic as before */
                selectDateClause = "DATENAME(year, r.receiptDate) AS DateLabel";
                groupByClause = "DATENAME(year, r.receiptDate)";
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();

                    // ADDED: SUM(i.quantity) AS TotalQuantity
                    string query = $@"
                SELECT 
                    {selectDateClause},
                    SUM(i.quantity) AS TotalQuantity, 
                    SUM(i.salesPriceTotal) AS TotalRevenue,
                    SUM(i.salesPriceTotal - i.costPriceTotal) AS TotalProfit
                FROM vpsPosReceiptItems i
                INNER JOIN vpsPosReceipts r ON i.receiptId = r.receiptId
                WHERE i.itemName = @exactItemName
                  AND r.receiptDate >= @startDate 
                  AND r.receiptDate <= @endDate
                GROUP BY {groupByClause}
                ORDER BY {groupByClause}";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@exactItemName", exactItemName);
                        cmd.Parameters.AddWithValue("@startDate", dtpStartDate.Value.Date);
                        cmd.Parameters.AddWithValue("@endDate", dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1));

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable chartData = new DataTable();
                        adapter.Fill(chartData);

                        ConfigureChart(chartData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Graph Error");
                ErrorLogger.Log(ex, "AnalyticsDashboardForm", nameof(LoadChartData), GetCurrentFormData());
            }
        }

        private void ConfigureChart(DataTable data)
        {
            // Clear old data
            chartItemSales.Series.Clear();

            // 1. Create Revenue Series (Left Axis - Column)
            var revenueSeries = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Revenue",
                Color = System.Drawing.Color.MediumSeaGreen,
                IsValueShownAsLabel = true,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column
            };

            // 2. Create Profit Series (Left Axis - Line)
            var profitSeries = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Profit",
                Color = System.Drawing.Color.DodgerBlue,
                IsValueShownAsLabel = true,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 3
            };

            // 3. Create NEW Quantity Series (Right Axis - Line)
            var quantitySeries = new System.Windows.Forms.DataVisualization.Charting.Series
            {
                Name = "Quantity",
                Color = System.Drawing.Color.DarkOrange, // Distinct color so it stands out
                IsValueShownAsLabel = true,
                ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line,
                BorderWidth = 3,
                // CRITICAL: Tells the chart to use the right-side scale for this line
                YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary
            };

            chartItemSales.Series.Add(revenueSeries);
            chartItemSales.Series.Add(profitSeries);
            chartItemSales.Series.Add(quantitySeries); // Add the new series

            // Bind the SQL data to the axes
            foreach (DataRow row in data.Rows)
            {
                string xDateLabel = row["DateLabel"].ToString();
                double yRevenue = Convert.ToDouble(row["TotalRevenue"]);
                double yProfit = Convert.ToDouble(row["TotalProfit"]);
                double yQuantity = Convert.ToDouble(row["TotalQuantity"]); // Extract the new column

                revenueSeries.Points.AddXY(xDateLabel, yRevenue);
                profitSeries.Points.AddXY(xDateLabel, yProfit);
                quantitySeries.Points.AddXY(xDateLabel, yQuantity); // Plot the quantity
            }

            // 4. Configure Axis Visuals (Titles to prevent confusion)
            var chartArea = chartItemSales.ChartAreas[0];

            // Format X Axis
            chartArea.AxisX.LabelStyle.Angle = -45;
            chartArea.AxisX.Interval = 1;

            // Format Primary Y Axis (Left Side - Money)
            chartArea.AxisY.Title = "Amount (Rs.)";
            chartArea.AxisY.TitleFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);

            // Format Secondary Y Axis (Right Side - Quantity)
            chartArea.AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True; // Turn it on
            chartArea.AxisY2.Title = "Quantity Sold";
            chartArea.AxisY2.TitleFont = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);

            // Optional: Add a grid line color so the dark theme looks clean
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY2.MajorGrid.Enabled = false; // Turn off right-side grid lines to prevent visual clutter
        }

        private void cmbViewBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if there is a row selected in the grid
            if (dgvItemInsights.CurrentRow != null)
            {
                string selectedItem = dgvItemInsights.CurrentRow.Cells["Item Name"].Value.ToString();
                LoadChartData(selectedItem);
            }
        }

        private void dgvItemInsights_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Check if the user clicked a valid row (not the header row, which is index -1)
            if (e.RowIndex >= 0)
            {
                try
                {
                    // 2. Get the exact item name from the first column ("Item Name") of the clicked row
                    string clickedItemName = dgvItemInsights.Rows[e.RowIndex].Cells["Item Name"].Value.ToString();

                    // 3. Send that exact name to our graphing method
                    LoadChartData(clickedItemName);
                }
                catch (Exception ex)
                {
                    // Failsafe in case a column is missing or empty
                    ErrorLogger.Log(ex, "AnalyticsDashboardForm", nameof(dgvItemInsights_CellClick), GetCurrentFormData());
                }
            }
        }
    }
}
