using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class MainForm : Form
    {
      
        // for get dark theme
        public bool isDarkMode { get; set; }

        public DataTable userlogin { get; set; }
        public DataSet dtSet { get; set; }
        

        SqlConnection conn;
        public MainForm()
        {
            InitializeComponent();
        }

        //InventoryLoginForm inventoryLoginForm;

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Empty Field", "non"}
    };
            return data;
        }

        private void invenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            //if(inventoryLoginForm == null)
            //{
            //    inventoryLoginForm = new InventoryLoginForm();
            //    inventoryLoginForm.MdiParent = this;
            //    inventoryLoginForm.FormClosed += InventoryLoginForm_FormClosed;
            //    inventoryLoginForm.Show();
            //}
            //else
            //{

            //    inventoryLoginForm.Activate();
            //    inventoryLoginForm.Show();

            //    inventoryLoginForm.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            //}

        }


        //inventoryLogin form closed function
        //private void InventoryLoginForm_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    inventoryLoginForm = null;
        //    //throw new NotImplementedException();
        //}
        // CHANGE 1: Make these public so child forms can access them
        public InventoryForm inventoryForm;
        public InventoryLoginForm inventoryLoginForm;
        private void itemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());

            // LOGIC FIX: Check if the final destination (InventoryForm) is already open first
            if (inventoryForm != null)
            {
                inventoryForm.Activate();
                if (inventoryForm.WindowState == FormWindowState.Minimized)
                    inventoryForm.WindowState = FormWindowState.Maximized;
                return;
            }

            // If Inventory is not open, check if Login form is already open
            if (inventoryLoginForm != null)
            {
                inventoryLoginForm.Activate();
                if (inventoryLoginForm.WindowState == FormWindowState.Minimized)
                    inventoryLoginForm.WindowState = FormWindowState.Maximized;
                return;
            }

            // If neither is open, open the Login form
            inventoryLoginForm = new InventoryLoginForm();
            inventoryLoginForm.MdiParent = this;
            inventoryLoginForm.FormClosed += InventoryLoginForm_FormClosed;
            inventoryLoginForm.Show();
            inventoryLoginForm.WindowState = FormWindowState.Maximized;
        }

        // Existing method - keep this
        private void InventoryLoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            inventoryLoginForm = null;
        }

        // CHANGE 2: Add this new method to handle when the actual Inventory Form closes
        public void InventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            inventoryForm = null;
        }

        //user form
        UserForm userForm;

        private void userToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (userForm == null)
            {
                userForm = new UserForm();
                userForm.MdiParent = this;
                userForm.FormClosed += UserForm_FormClosed;
                userForm.Show();
                userForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                userForm.Activate();
                userForm.WindowState = FormWindowState.Maximized;
            }
        }

        //user form closed function
        private void UserForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            userForm = null;
            //throw new NotImplementedException();
        }

        //settings form
        SettingsForm settingsForm;

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (settingsForm == null)
            {
                settingsForm = new SettingsForm();
                settingsForm.MdiParent = this;
                settingsForm.FormClosed += SettingsForm_FormClosed;
                settingsForm.Show();
            }
            else
            {
                settingsForm.Activate();
            }
        }

        //settings form closed function
        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            settingsForm = null;
            //throw new NotImplementedException();
        }

        // to check whether form opened or not since "if (cashierForm == null)" since cashierForm has no relation to existing cashierforms.I think this occur when cashier window opened from bill window while pressing view.then cashierForm has no connection
        bool IsCashierOpen()
        {
            return Application.OpenForms.Cast<Form>().Any(f => f is CashierForm);
        }

        //cashier form
        CashierForm cashierForm;

        private void cashierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            try
            {
                if (!IsCashierOpen())
                {
                    cashierForm = new CashierForm();
                    cashierForm.MdiParent = this;
                    cashierForm.userlogin = this.userlogin;
                    cashierForm.dtSet = this.dtSet;
                    cashierForm.isDarkMode = this.isDarkMode;
                    // add items through this dtset
                    int count = dtSet.Tables[0].Rows.Count;
                    int i = 0;
                    while (i < count)
                    {
                        cashierForm.ItemsComboBox.Items.Add(dtSet.Tables[0].Rows[i]["ItemName"].ToString());
                        i++;
                    }
                    cashierForm.FormClosed += CashierForm_FormClosed;
                    cashierForm.Show();

                }
                else
                {
                    // bring existing forms upfront
                    Application.OpenForms["CashierForm"].BringToFront(); ;
                }

            ((CashierForm)Application.OpenForms["CashierForm"]).Iswholesaleform = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening CashierForm: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(ex, "MainForm", nameof(cashierToolStripMenuItem_Click), GetCurrentFormData());
            }

        }

        //cashier form closed function
        private void CashierForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            cashierForm = null;
            //throw new NotImplementedException();
        }

        //stock form
        StockForm stockForm;

        private void stockToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (stockForm == null)
            {
                stockForm = new StockForm();
                stockForm.MdiParent = this;
                stockForm.FormClosed += StockForm_FormClosed;
                stockForm.Show();
            }
            else
            {
                stockForm.Activate();
            }
        }

        //stock form closed function
        private void StockForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            stockForm = null;
            //throw new NotImplementedException();
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            toolStripMenuItem3.Text = DateTime.Now.ToLongTimeString();
        }

        //bills form
        BillsForm billsForm;
        private void billsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (billsForm == null)
            {
                billsForm = new BillsForm();
                billsForm.MdiParent = this;
                billsForm.userlogin = this.userlogin;
                billsForm.dtSet = this.dtSet;
                BillsForm.isDarkMode = isDarkMode;

                billsForm.FormClosed += BillsForm_FormClosed;
                billsForm.Show();
            }
            else
            {
                billsForm.Activate();
            }
        }
        //bills form closed function
        private void BillsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            billsForm = null;
            //throw new NotImplementedException();
        }

        SaleReportForm saleReportForm;

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (saleReportForm == null)
            {
                saleReportForm = new SaleReportForm();
                saleReportForm.MdiParent = this;
                saleReportForm.userlogin = this.userlogin;
                saleReportForm.FormClosed += SaleReportForm_FormClosed;
                saleReportForm.Show();
            }
            else
            {
                saleReportForm.Activate();
            }
        }

        private void SaleReportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            saleReportForm = null;
            //throw new NotImplementedException();
        }
        /*
         
         
         //
private void MainForm_Load(object sender, EventArgs e)
{
    // ... existing code ...
    loginToolStripMenuItem.Text = "Login As " + userlogin.Rows[0][1].ToString();

    // Check Username
    string currentUser = userlogin.Rows[0][1].ToString().ToLower();

    if (currentUser == "pradeep" || currentUser == "amro")
    {
        // Create the button dynamically
        ToolStripMenuItem auditBtn = new ToolStripMenuItem("⚠️ AUDIT LOGS");
        auditBtn.ForeColor = Color.Red;
        auditBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        
        // Add click event
        auditBtn.Click += (s, args) => {
            new AuditForm().ShowDialog();
        };
        
        // Add to menu
        menuStrip1.Items.Add(auditBtn);
    }
}
         
         
         
         
         */
        private void MainForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            // initialize.so no null errors will come
            dtSet = new DataSet();
            isDarkMode = false;
            this.WindowState = FormWindowState.Maximized;
            loginToolStripMenuItem.Text = "Login As " + userlogin.Rows[0][1].ToString();

            // suspicious bills window for pradeep
            // Check Username
            string currentUser = userlogin.Rows[0][1].ToString().ToLower();

            if (currentUser == "pradeep" || currentUser == "amro")
            {
                // Create the button dynamically
                ToolStripMenuItem auditBtn = new ToolStripMenuItem("⚠️ AUDIT LOGS");
                auditBtn.ForeColor = Color.Red;
                auditBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                // Add click event
                auditBtn.Click += (s, args) => {
                    new AuditForm().ShowDialog();
                };

                // Add to menu
                // Insert at index 3 (the fourth place)
                MenuStrip.Items.Insert(3, auditBtn);

                XmlDocument doc1 = new XmlDocument();

                doc1.Load("requiredInfo.xml");
                XmlNodeList nodes1 = doc1.GetElementsByTagName("sqlString");
                string conString1 = nodes1[0].InnerText;

                using (SqlConnection con = new SqlConnection(conString1))
                {
                    con.Open();
                    string query = @"
            SELECT 
                ReceiptId
            FROM SuspiciousBills 
            WHERE CAST(DateCreated AS DATE) = CAST(GETDATE() AS DATE) -- This filters for today only
ORDER BY DateCreated DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DialogResult result = MessageBox.Show("" +
                            $"There are {dt.Rows.Count} suspicious bills.To view those click yes.\n" +
                            $"අවධානය යොමු කල යුතු බිල් {dt.Rows.Count} ක් ඇත.\nඒවා බැලීමට yes ඔබන්න.\n\n" +
                            $"එහි වගුවේ 'රතු' පාටින් පාඩුව වැඩි හා මුල් අවස්තාවේ ඇතුලත් කර බොහෝ වේලාවකට පසු වෙනස් කල බිල් පෙන්වයි .\n'තැබිලි' පාටින් පාඩුව වැඩි බිල් පෙන්වයි.\n 'කහ' පාටින් බිල මුල් අවස්තාවේ ඇතුලත් කර බොහෝ වේලාවකට පසු වෙනස් කල බිල් පෙන්වයි.\n'කළු' පාටින් පාඩුව 1000 ට අඩු බිල් පෙන්වයි.\n" +
                            $"\nThe table shows bills with high losses and also that have been changed after a long time since the bill was originally entered in 'red' color.\n Bills with high losses in 'orange' color.\nBills that have been changed after a long time since the bill was originally entered in 'yellow' color.\nBills that have loss less than 1000 in 'black' color.", "View audit window", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            auditBtn.PerformClick();
                        }
                    }
                }

            }

            //adding itemnames to  combobox
            XmlDocument doc = new XmlDocument();

            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            string conString = nodes[0].InnerText;

            conn = new SqlConnection(conString);
            string sqlcommand2 = "SELECT ItemName FROM iteminformation";
            conn.Open();
            SqlDataAdapter dtAdapter = new SqlDataAdapter(sqlcommand2, conn);

            dtAdapter.Fill(dtSet);

        }


        ShortcutsForm shortcutsForm;

        private void shortcutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (shortcutsForm == null)
            {
                shortcutsForm = new ShortcutsForm();
                shortcutsForm.FormClosed += ShortcutsForm_FormClosed;
                shortcutsForm.Show();
            }
            else
            {
                shortcutsForm.Activate();
            }
        }


        private void ShortcutsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            shortcutsForm = null;
            //throw new NotImplementedException();
        }

        UpdatesForm updatesForm;

        private void updatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (updatesForm == null)
            {
                updatesForm = new UpdatesForm();
                updatesForm.FormClosed += UpdatesForm_FormClosed;
                updatesForm.Show();
            }
            else
            {
                updatesForm.Activate();
            }
        }

        private void UpdatesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            updatesForm = null;
            //   throw new NotImplementedException();
        }

        CustomersForm customersForm;
        private void customersToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (customersForm == null)
            {
                customersForm = new CustomersForm();
                customersForm.MdiParent = this;
                customersForm.FormClosed += CustomersForm_FormClosed;
                customersForm.Show();
            }
            else
            {
                customersForm.Activate();
            }
        }

        private void CustomersForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            customersForm = null;
            //throw new NotImplementedException();
        }

        DebtForm debtForm;

        private void debtToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (debtForm == null)
            {
                debtForm = new DebtForm();
                debtForm.MdiParent = this;
                debtForm.FormClosed += DebtForm_FormClosed;
                debtForm.Show();
            }
            else
            {
                debtForm.Activate();
            }
        }


        private void DebtForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            debtForm = null;
            //throw new NotImplementedException();
        }

        BuisnessInfoForm buisnessInfoForm;
        private void buisnessInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (buisnessInfoForm == null)
            {
                buisnessInfoForm = new BuisnessInfoForm();
                buisnessInfoForm.MdiParent = this;
                buisnessInfoForm.FormClosed += BuisnessInfoForm_FormClosed;
                buisnessInfoForm.Show();
            }
            else
            {
                buisnessInfoForm.Activate();
            }
        }

        private void BuisnessInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            buisnessInfoForm = null;
        }

        ItemInfoHistoryForm itemInfoHistoryForm;
        private void itemInfoHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (itemInfoHistoryForm == null)
            {
                itemInfoHistoryForm = new ItemInfoHistoryForm();
                itemInfoHistoryForm.MdiParent = this;
                itemInfoHistoryForm.FormClosed += ItemInfoHistoryForm_FormClosed;
                itemInfoHistoryForm.Show();
            }
            else
            {
                itemInfoHistoryForm.Activate();
            }
        }

        private void ItemInfoHistoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            itemInfoHistoryForm = null;
        }

        LanguageForm languageForm;
        private void languageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (languageForm == null)
            {
                languageForm = new LanguageForm();
                languageForm.MdiParent = this;
                languageForm.FormClosed += LanguageForm_FormClosed;
                languageForm.Show();
            }
            else
            {
                languageForm.Activate();
            }
        }

        private void LanguageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            //throw new NotImplementedException();
            languageForm = null;
        }

        DailyTransactionsForm dailyTransactionsForm;
        private void dailyTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (dailyTransactionsForm == null)
            {
                dailyTransactionsForm = new DailyTransactionsForm();
                dailyTransactionsForm.MdiParent = this;
                dailyTransactionsForm.FormClosed += DailyTransactionsForm_FormClosed;
                dailyTransactionsForm.Show();
            }
            else
            {
                dailyTransactionsForm.Activate();
                dailyTransactionsForm.WindowState = FormWindowState.Maximized;
            }
        }

        private void DailyTransactionsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            //throw new NotImplementedException();
            dailyTransactionsForm = null;
        }

        InfoPdfForm infoPdfForm;
        private void pdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (infoPdfForm == null)
            {
                infoPdfForm = new InfoPdfForm();
                infoPdfForm.FormClosed += InfoPdfForm_FormClosed;
                infoPdfForm.Show();
            }
            else
            {
                infoPdfForm.Activate();
            }
        }

        private void InfoPdfForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            infoPdfForm = null;
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            var result = MessageBox.Show(
                "Do you really want to close this window ?",
                "Ask",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {


                    // EXPLICIT exit ONLY on user confirmation
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "Error when closing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Optionally log error here
                    ErrorLogger.Log(error, "MainForm", nameof(MainForm_FormClosing), GetCurrentFormData());

                }
            }
            else
            {
                // Cancel the close
                e.Cancel = true;
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());
            if (MessageBox.Show("Do you really want to close this window ?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {

                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "Error when closing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "MainForm", nameof(logoutToolStripMenuItem_Click), GetCurrentFormData());
                }

            }
        }

        AnalyticsDashboardForm analyticsForm;
        private void analyticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Log that we are opening the form
            ErrorLogger.UpdateFormData("MainForm", GetCurrentFormData());

            // Check if it's aleardy open to prevent opening duplicates
            if (analyticsForm == null)
            {
                analyticsForm = new AnalyticsDashboardForm();
                analyticsForm.MdiParent = this; // This makes analytics form is a child of main form
                analyticsForm.FormClosed += AnalyticsForm_FormClosed;
                analyticsForm.Show();

                analyticsForm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                analyticsForm.Activate();
                analyticsForm.WindowState = FormWindowState.Maximized;
            }
        }

        private void AnalyticsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            analyticsForm = null;
        }
    }
}
