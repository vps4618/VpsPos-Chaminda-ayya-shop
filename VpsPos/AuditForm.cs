using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class AuditForm : Form
    {
        public AuditForm()
        {
            InitializeComponent();
        }

        // log helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string> {
     { "From date", dtpStart.Text},
                { "To date",dtpEnd.Text }
            };


            return data;
        }

        private void AuditForm_Load(object sender, EventArgs e)
        {

            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            // 1. Setup the Feed Container
            flpFeed.FlowDirection = FlowDirection.TopDown;
            flpFeed.WrapContents = false;
            flpFeed.AutoScroll = true;
            flpFeed.BackColor = Color.LightGray;
            this.DoubleBuffered = true;
            // --- NEW FIXES START HERE ---

            // 1. Fix Layout: Force panel1 to dock first so flpFeed respects it
            panel1.SendToBack();

            // 2. Fix Time: Enable Time selection in the pickers
            dtpStart.Format = DateTimePickerFormat.Custom;
            dtpStart.CustomFormat = "yyyy-MM-dd HH:mm"; // Shows Date + Time

            dtpEnd.Format = DateTimePickerFormat.Custom;
            dtpEnd.CustomFormat = "yyyy-MM-dd HH:mm";

            // 3. Set Defaults: Start = Today 00:00, End = Now
            dtpStart.Value = DateTime.Now.Date;
            dtpEnd.Value = DateTime.Now;
        }

        protected override void OnShown(EventArgs e)
        {
            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            base.OnShown(e);

            // Load default (Today)
            LoadAuditFeed(DateTime.Now.Date, DateTime.Now.Date.AddDays(1).AddTicks(-1));
            UpdateCardWidths();
        }

        private void LoadAuditFeed(DateTime? start = null, DateTime? end = null)
        {
            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            flpFeed.Controls.Clear();
            flpFeed.SuspendLayout();

            // Default to today if no dates passed (for initial load)
            DateTime fromDate = start ?? DateTime.Now.Date;
            DateTime toDate = end ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("requiredInfo.xml");
                string conString = doc.GetElementsByTagName("sqlString")[0].InnerText;

                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();
                    string query = @"
                    SELECT 
                        ReceiptId,
                        CashierName as [SIGNED BY (වෙනස් කල තැනැත්තා)], 
                        OriginalDate as [Bill Created (බිල ඇතුල් කල මුල්ම වේලාව)],
                        DateCreated as [Bill Edited (බිල වෙනස් කල වේලාව)],
                        PastItems as [Items BEFORE (මුල් බිලේ තිබූ භාණ්ඩ)],
                        NewItems as [Items AFTER (වෙනස් කල පසු භාණ්ඩ)],
                        CustomerName as [Customer (පාරිභෝගිකයා)],
                        OldTotal as [OldTotal (පරණ එකතුව)], NewTotal as [NewTotal (අලුත් එකතුව)],
                        (OldTotal - NewTotal) as [LOSS (පාඩුව)],
                        Reason as [Reason (හේතුව)]
                    FROM SuspiciousBills 
                    WHERE DateCreated >= @fromDate AND DateCreated <= @toDate 
                    ORDER BY DateCreated DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);

                    // Add Parameters
                    da.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate);
                    da.SelectCommand.Parameters.AddWithValue("@toDate", toDate);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        Label lbl = new Label();
                        lbl.Text = "No Suspicious Bills Found Today.";
                        lbl.AutoSize = true;
                        lbl.Font = new Font("Segoe UI", 14);
                        lbl.Padding = new Padding(20);
                        flpFeed.Controls.Add(lbl);
                    }

                    int initialWidth = Math.Max(flpFeed.ClientSize.Width - 30, 600);

                    foreach (DataRow row in dt.Rows)
                    {
                        Control card = CreateAuditCard(row, initialWidth);
                        flpFeed.Controls.Add(card);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
                ErrorLogger.Log(ex, "AuditForm", nameof(LoadAuditFeed), GetCurrentFormData());
            }
            finally
            {
                flpFeed.ResumeLayout();
            }
        }

        private Control CreateAuditCard(DataRow row, int fixedWidth)
        {
            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            // --- DATA SETUP ---
            string cashier = row["SIGNED BY (වෙනස් කල තැනැත්තා)"]?.ToString() ?? "Unknown";
            string reason = row["Reason (හේතුව)"]?.ToString() ?? "";
            string itemsBefore = row["Items BEFORE (මුල් බිලේ තිබූ භාණ්ඩ)"]?.ToString() ?? "";
            string itemsAfter = row["Items AFTER (වෙනස් කල පසු භාණ්ඩ)"]?.ToString() ?? "";
            string customer = row["Customer (පාරිභෝගිකයා)"]?.ToString() ?? "Unknown";
            string receiptId = row["ReceiptId"].ToString();
            DateTime.TryParse(row["Bill Created (බිල ඇතුල් කල මුල්ම වේලාව)"]?.ToString(), out DateTime created);
            DateTime.TryParse(row["Bill Edited (බිල වෙනස් කල වේලාව)"]?.ToString(), out DateTime edited);
            double.TryParse(row["LOSS (පාඩුව)"]?.ToString(), out double loss);

            Color headerColor;
            Color detailColor;

            // Time Diff Logic
            TimeSpan diff = edited - created;
            string timeDiffText = $"{Math.Floor(diff.TotalHours)} Hours {diff.Minutes} Minutes <- start (බිල දැමූ මුල් වේලාව) : {created} - end (බිල වෙනස් කල වේලාව) : {edited}";
            bool isGhostReturn = diff.TotalHours > 4;


            if (loss > 1000 && isGhostReturn)
            {
                headerColor = Color.Red;
                detailColor = Color.Yellow;
            }
            else if (isGhostReturn)
            {
                headerColor = Color.Yellow;
                detailColor = Color.Yellow;
            }
            else if(loss > 1000)
            {
                headerColor = Color.Orange;
                detailColor = Color.Transparent;
            }
            else
            {
                headerColor = Color.FromArgb(64, 64, 64);
                detailColor = Color.Transparent;
            }

                // --- CARD PANEL ---
                Panel pnlCard = new Panel();
            pnlCard.AutoSize = true;
            pnlCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlCard.BackColor = Color.White;
            pnlCard.Padding = new Padding(0);
            pnlCard.Margin = new Padding(10, 10, 10, 20);
            pnlCard.MinimumSize = new Size(fixedWidth, 0);
            pnlCard.MaximumSize = new Size(fixedWidth, 0);

            // --- 1. HEADER (Top) ---
            Label lblHeader = new Label();
            lblHeader.Text = $"| Receipt Id : {receiptId} | CASHIER (වෙනස් කල තැනැත්තා): {cashier}   |   CUSTOMER(පාරිභෝගිකයා): {customer}   |   LOSS(පාඩුව): {loss:N2}";
            lblHeader.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            Color foreColor;
            if(headerColor == Color.Yellow)
            {
                foreColor = Color.Black;
            }
            else
            {
                foreColor = Color.White;
            }
            lblHeader.ForeColor = foreColor;
            lblHeader.BackColor = headerColor;
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblHeader.Height = 40;
            lblHeader.Dock = DockStyle.Top;

            // --- 2. MAIN DETAILS (Middle) ---
            TableLayoutPanel tlpDetails = new TableLayoutPanel();
            tlpDetails.ColumnCount = 2;
            tlpDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tlpDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDetails.AutoSize = true;
            tlpDetails.Dock = DockStyle.Top;
            tlpDetails.Padding = new Padding(10);

            AddRow(tlpDetails, "Time Diff (වේලාවල් අතර වෙනස)", timeDiffText, fixedWidth, detailColor);
            AddRow(tlpDetails, "Reason (හේතුව)", reason, fixedWidth, null);

            // --- 3. ITEMS COMPARISON (Bottom - Left/Right) ---
            TableLayoutPanel tlpItems = new TableLayoutPanel();
            tlpItems.ColumnCount = 2;
            // Split 50% / 50%
            tlpItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpItems.AutoSize = true;
            tlpItems.Dock = DockStyle.Top;
            tlpItems.Padding = new Padding(10);
            tlpItems.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single; // Line between columns

            // Add Headers for Items
            Label lblOldHeader = new Label { Text = "Items BEFORE (මුල් බිලේ භාණ්ඩ)", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, ForeColor = Color.DarkBlue };
            Label lblNewHeader = new Label { Text = "Items AFTER (වෙනස් කල පසු බිලේ භාණ්ඩ)", Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, ForeColor = Color.DarkGreen };

            // Add Values for Items
            Label lblOldVal = new Label { Text = itemsBefore, AutoSize = true, Font = new Font("Segoe UI", 10) };
            Label lblNewVal = new Label { Text = itemsAfter, AutoSize = true, Font = new Font("Segoe UI", 10) };

            // Set wrapping for Item lists (half width - padding)
            int halfWidth = (fixedWidth / 2) - 30;
            lblOldVal.MaximumSize = new Size(halfWidth, 0);
            lblNewVal.MaximumSize = new Size(halfWidth, 0);

            // Add to Table (Row 0 = Headers, Row 1 = Values)
            tlpItems.Controls.Add(lblOldHeader, 0, 0);
            tlpItems.Controls.Add(lblNewHeader, 1, 0);
            tlpItems.Controls.Add(lblOldVal, 0, 1);
            tlpItems.Controls.Add(lblNewVal, 1, 1);


            // --- ADD ORDER (REVERSE OF VISUAL STACK) ---
            // Visual: Header -> Details -> Items
            // Code Add Order for Dock=Top: Items -> Details -> Header

            pnlCard.Controls.Add(tlpItems);   // Bottom
            pnlCard.Controls.Add(tlpDetails); // Middle
            pnlCard.Controls.Add(lblHeader);  // Top

            return pnlCard;
        }

        private void AddRow(TableLayoutPanel tlp, string title, string value, int cardWidth, Color? bg)
        {

            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Margin = new Padding(0, 5, 0, 5);

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblValue.AutoSize = true;
            if (bg.HasValue) lblValue.BackColor = bg.Value;

            int maxTextWidth = cardWidth - 160;
            lblValue.MaximumSize = new Size(maxTextWidth, 0);
            lblValue.Margin = new Padding(0, 5, 0, 15);

            tlp.RowCount++;
            tlp.Controls.Add(lblTitle, 0, tlp.RowCount - 1);
            tlp.Controls.Add(lblValue, 1, tlp.RowCount - 1);
        }

        protected override void OnResize(EventArgs e)
        {
            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            base.OnResize(e);
            UpdateCardWidths();
        }

        private void UpdateCardWidths()
        {
            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            if (flpFeed.Controls.Count == 0) return;

            flpFeed.SuspendLayout();
            int newWidth = Math.Max(flpFeed.ClientSize.Width - 30, 500);

            foreach (Control c in flpFeed.Controls)
            {
                if (c is Panel pnl)
                {
                    pnl.MinimumSize = new Size(newWidth, 0);
                    pnl.MaximumSize = new Size(newWidth, 0);

                    // Update inner controls
                    foreach (Control child in pnl.Controls)
                    {
                        // 1. Update Main Details (Full Width)
                        if (child is TableLayoutPanel tlp && tlp.ColumnCount == 2 && tlp.ColumnStyles[0].SizeType == SizeType.Absolute)
                        {
                            foreach (Control cell in tlp.Controls)
                            {
                                if (cell is Label lbl && tlp.GetColumn(lbl) == 1) // Value column
                                {
                                    lbl.MaximumSize = new Size(newWidth - 160, 0);
                                }
                            }
                        }
                        // 2. Update Items Comparison (Half Width)
                        else if (child is TableLayoutPanel tlpSplit && tlpSplit.ColumnCount == 2 && tlpSplit.ColumnStyles[0].SizeType == SizeType.Percent)
                        {
                            int halfWidth = (newWidth / 2) - 30;
                            foreach (Control cell in tlpSplit.Controls)
                            {
                                if (cell is Label lbl && tlpSplit.GetRow(lbl) == 1) // Value Row
                                {
                                    lbl.MaximumSize = new Size(halfWidth, 0);
                                }
                            }
                        }
                    }
                }
            }
            flpFeed.ResumeLayout();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("AuditForm", GetCurrentFormData());
            // Pass the exact values from the pickers (including time)
            LoadAuditFeed(dtpStart.Value, dtpEnd.Value);

            // Scroll back to top
            flpFeed.VerticalScroll.Value = 0;
        }

        
    }
}