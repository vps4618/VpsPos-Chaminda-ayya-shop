using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class BillsForm : Form
    {
        public static bool isDarkMode { get; set; }
        // log helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string> {
     { "SearchFromWhat", SearchCheckBox.Text},
                { "SearchText",SearchTextBox.Text },
                { "Date-Start",DateStart.Text },
                { "Date-End",DateEnd.Text },
            };
            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in BillsGridView.Rows)
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

        // 6th column appear and dissapear 
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            // show total
            if (keyData == (Keys.F6))
            {

                // hide only for pradeep
                XmlDocument doc = new XmlDocument();
                doc.Load("requiredInfo.xml");
                XmlNodeList nodes = doc.GetElementsByTagName("store");
                string storeName = nodes[0].InnerText;
                if (storeName == "pradeep")
                {
                    if (userlogin.Rows[0][1].ToString() != "pradeep")
                    {
                        BillsGridView.Columns[6].Visible = true;

                        NoOfBillsTextBox.Visible = true;
                        noOfBillsLabel.Visible = true;
                        BillsGridView.Columns[3].Visible = true;
                        DateStart.Visible = true;
                        DateEnd.Visible = true;
                        SearchButton.Visible = true;

                        columnHideTimer.Start();
                    }
                }

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public DataSet dtSet { get; set; }
        public DataTable userlogin { get; set; }
        //making conString  and conn global by using public static keywords
        public static string conString;
        XmlDocument doc = new XmlDocument();

        //create databaseclass object to connect to database
        private DatabaseClass databaseClass = new DatabaseClass();

        public BillsForm()
        {
            InitializeComponent();
        }

        public void refreshBillsGridView()
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            DataTable dataTable = new DataTable();
            // Get the current date
            DateTime currentDate = DateTime.Today;


            DateTime startDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);

            DateTime endDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);

            using (SqlConnection connection = new SqlConnection(conString))
            {
                connection.Open();
                string queryString = "SELECT receiptId AS Id,receiptDate AS Date,cashierFormId AS Form_Id,customerName AS Customer_Name,totalAmount AS Total FROM vpsPosReceipts WHERE receiptDate BETWEEN @startDateTime AND @endDateTime ORDER BY receiptId DESC";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@startDateTime", SqlDbType.DateTime).Value = startDateTime;

                    command.Parameters.AddWithValue("@endDateTime", SqlDbType.DateTime).Value = endDateTime;

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                    BillsGridView.DataSource = dataTable;
                }
            }

            // hide only for pradeep
            XmlDocument doc = new XmlDocument();
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("store");
            string storeName = nodes[0].InnerText;
            if (storeName == "pradeep")
            {
                if (userlogin.Rows[0][1].ToString() != "pradeep")
                {
                    BillsGridView.Columns[6].Visible = false;
                    BillsGridView.Columns[3].Visible = false;
                }
            }

            NoOfBillsTextBox.Text = dataTable.Rows.Count.ToString();
            
        }

        private void BillsForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;

            this.WindowState = FormWindowState.Maximized;
            SearchComboBox.SelectedIndex = 0;

            refreshBillsGridView();

            // hide only for pradeep traders
            XmlNodeList nodes1 = doc.GetElementsByTagName("store");
            string storeName = nodes1[0].InnerText;
            if (storeName == "pradeep")
            {
                if (userlogin.Rows[0][1].ToString() != "pradeep")
                {
                    BillsGridView.Columns[6].Visible = false;
                    NoOfBillsTextBox.Visible = false;
                    noOfBillsLabel.Visible = false;
                    BillsGridView.Columns[3].Visible = false;
                    DateStart.Visible = false;
                    DateEnd.Visible = false;
                    SearchButton.Visible = false;
                }
            }

        }

        //cashier form
        CashierForm cashierForm;

        private DataTable sortBillItemsForPrinting(string language, DataTable dataTable, string fieldName, string messageLan)
        {
            // asking for reorder (only for bills which have more than 20 items)
            if (dataTable.Rows.Count > 15)
            {
                DialogResult result = MessageBox.Show($"Do you want to sort bill items for easy checking ?\nඔබට බිල චෙක් කිරීමේ පහසුව සදහා භාණ්ඩ {messageLan} අකුරු අනුපිලිවෙලට ප්‍රින්ට් කිරීම අවශ්‍යද ?", "sorting", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // first method
                    /*DataView dv = itemsInReceiptDataTable.DefaultView;
                    dv.Sort = "sinhalaName ASC";

                    itemsInReceiptDataTable = dv.ToTable();*/

                    // second method
                    CultureInfo si = new CultureInfo(language);

                    var sortedRows = dataTable.AsEnumerable()
                        .OrderBy(r => r.Field<string>(fieldName), StringComparer.Create(si, true));

                    dataTable = sortedRows.CopyToDataTable();

                }
            }

            return dataTable;
        }

        private void BillsGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            if (BillsGridView.CurrentCell.ColumnIndex == 0)
            {
                try
                {
                    string billType = "";

                    DialogResult result = MessageBox.Show("Do you want a wholesale bill ?\nඔබට අවශ්‍ය තොග බිලක්ද ?", "bill type", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        billType = "wholesale";
                    }
                    else if (result == DialogResult.No) { billType = "retail"; } else if (result == DialogResult.Cancel) { billType = "null"; };

                    if (billType != "null")
                    {
                        InfoForm infoForm = new InfoForm();
                        infoForm.Show();
                        infoForm.Text = "Printing... Please wait.";

                        int receiptId = Convert.ToInt32(BillsGridView.CurrentRow.Cells[2].Value);

                        // getting receipt data
                        DataTable initialReceiptDataDataTable = new DataTable();

                        string sqlCommandForGettingInitialData = @"
SELECT * FROM vpsPosReceipts WHERE receiptId=" + receiptId.ToString();

                        initialReceiptDataDataTable = this.databaseClass.executeSqlCommand(sqlCommandForGettingInitialData);

                        // items
                        DataTable itemsInReceiptDataTable = new DataTable();

                        string sqlCommandForGettingReceiptItems = @"
SELECT * FROM vpsPosReceiptItems WHERE receiptId=" + receiptId.ToString() + " ORDER BY orderOfItems ASC";

                        itemsInReceiptDataTable = this.databaseClass.executeSqlCommand(sqlCommandForGettingReceiptItems);


                        // buisness info 
                        DataTable buisnessInfoDatatable = new DataTable();

                        using (SqlConnection connection = new SqlConnection(conString))
                        {
                            connection.Open();
                            string queryString = "SELECT * FROM vpsPosBuisnessInfo";
                            using (SqlCommand command = new SqlCommand(queryString, connection))
                            {
                                SqlDataAdapter adapter = new SqlDataAdapter(command);
                                adapter.Fill(buisnessInfoDatatable);
                            }
                        }

                        // get language info
                        DataTable languageDatatable = new DataTable();
                        using (SqlConnection connection = new SqlConnection(conString))
                        {
                            connection.Open();
                            string queryString = "SELECT * FROM vpsPosLanguage";
                            using (SqlCommand command = new SqlCommand(queryString, connection))
                            {
                                SqlDataAdapter adapter = new SqlDataAdapter(command);
                                adapter.Fill(languageDatatable);
                            }
                        }

                        // get user type (Sub or Main)
                        XmlDocument doc = new XmlDocument();

                        doc.Load("requiredInfo.xml");
                        XmlNodeList nodes = doc.GetElementsByTagName("user");
                        string userType = nodes[0].InnerText;


                        // getting required data for checking need of small bill
                        XmlNodeList nodeSmallBill = doc.GetElementsByTagName("smallBillForPc1");
                        string wantSmallBill = nodeSmallBill[0].InnerText;

                        XmlNodeList phraseNode = doc.GetElementsByTagName("phrase");
                        string phrase = phraseNode[0].InnerText;


                        if (userType == "Main")
                        {
                            itemsInReceiptDataTable = sortBillItemsForPrinting("si-LK", itemsInReceiptDataTable, "sinhalaName", "සිංහල");

                            if (billType == "retail")
                            {


                                //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                                if (phrase == "B" && wantSmallBill == "yes")
                                {
                                    CrystalReports.retailReceiptMain_pc1 retailReceipt = new CrystalReports.retailReceiptMain_pc1();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }
                                else
                                {
                                    CrystalReports.retailReceiptMain retailReceipt = new CrystalReports.retailReceiptMain();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }

                            }
                            else
                            {
                                CrystalReports.wholesaleReceiptMain wholesaleReceipt = new CrystalReports.wholesaleReceiptMain();
                                wholesaleReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                wholesaleReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                wholesaleReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                wholesaleReceipt.PrintToPrinter(1, true, 0, 0);
                                // for prevent maximum print job done error 
                                wholesaleReceipt.Close();
                                wholesaleReceipt.Dispose();
                            }
                        }
                        else if (userType == "Sub")
                        {
                            itemsInReceiptDataTable = sortBillItemsForPrinting("en-US", itemsInReceiptDataTable, "itemName", "ඉංග්‍රීසි");

                            if (billType == "retail")
                            {


                                //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                                if (phrase == "B" && wantSmallBill == "yes")
                                {
                                    CrystalReports.retailReceiptSub_pc1 retailReceipt = new CrystalReports.retailReceiptSub_pc1();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }
                                else
                                {
                                    CrystalReports.retailReceiptSub retailReceipt = new CrystalReports.retailReceiptSub();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }

                            }
                            else
                            {
                                CrystalReports.wholesaleReceiptSub wholesaleReceipt = new CrystalReports.wholesaleReceiptSub();
                                wholesaleReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                wholesaleReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                wholesaleReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                wholesaleReceipt.PrintToPrinter(1, true, 0, 0);
                                // for prevent maximum print job done error 
                                wholesaleReceipt.Close();
                                wholesaleReceipt.Dispose();
                            }
                        }
                        else if (languageDatatable.Rows[0][0].ToString() == "Sinhala")
                        {

                            itemsInReceiptDataTable = sortBillItemsForPrinting("si-LK", itemsInReceiptDataTable, "sinhalaName", "සිංහල");

                            if (billType == "retail")
                            {


                                //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                                if (phrase == "B" && wantSmallBill == "yes")
                                {
                                    CrystalReports.retailReceiptMain_pc1 retailReceipt = new CrystalReports.retailReceiptMain_pc1();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }
                                else
                                {
                                    CrystalReports.retailReceiptMain retailReceipt = new CrystalReports.retailReceiptMain();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }

                            }
                            else
                            {
                                CrystalReports.wholesaleReceiptMain wholesaleReceipt = new CrystalReports.wholesaleReceiptMain();
                                wholesaleReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                wholesaleReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                wholesaleReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                wholesaleReceipt.PrintToPrinter(1, true, 0, 0);
                                // for prevent maximum print job done error 
                                wholesaleReceipt.Close();
                                wholesaleReceipt.Dispose();
                            }
                        }
                        else if (languageDatatable.Rows[0][0].ToString() == "English")
                        {
                            itemsInReceiptDataTable = sortBillItemsForPrinting("en-US", itemsInReceiptDataTable, "itemName", "ඉංග්‍රීසි");

                            if (billType == "retail")
                            {


                                //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                                if (phrase == "B" && wantSmallBill == "yes")
                                {
                                    CrystalReports.retailReceiptSub_pc1 retailReceipt = new CrystalReports.retailReceiptSub_pc1();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }
                                else
                                {
                                    CrystalReports.retailReceiptSub retailReceipt = new CrystalReports.retailReceiptSub();
                                    retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                    retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                    retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                    retailReceipt.PrintToPrinter(1, true, 0, 0);

                                    // for prevent maximum print job done error 
                                    retailReceipt.Close();
                                    retailReceipt.Dispose();
                                }
                            }
                            else
                            {
                                CrystalReports.wholesaleReceiptSub wholesaleReceipt = new CrystalReports.wholesaleReceiptSub();
                                wholesaleReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                                wholesaleReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                                wholesaleReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                                wholesaleReceipt.PrintToPrinter(1, true, 0, 0);
                                // for prevent maximum print job done error 
                                wholesaleReceipt.Close();
                                wholesaleReceipt.Dispose();
                            }
                        }

                        infoForm.Close();
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "BillForm", nameof(BillsGridView_CellClick), GetCurrentFormData());
                }
            }
            else if (BillsGridView.CurrentCell.ColumnIndex == 1)
            {

                //close already opened cashier forms.
                CashierForm cashierForm1 = Application.OpenForms.OfType<CashierForm>().FirstOrDefault();
                if (cashierForm1 != null)
                {
                    cashierForm1.Close();
                }

                cashierForm = new CashierForm();
                cashierForm.MdiParent = this.MdiParent;
                cashierForm.userlogin = this.userlogin;
                cashierForm.isDarkMode = isDarkMode;

                // add  items through main form dtset
                int count = dtSet.Tables[0].Rows.Count;
                int i = 0;
                while (i < count)
                {
                    cashierForm.ItemsComboBox.Items.Add(dtSet.Tables[0].Rows[i]["ItemName"].ToString());
                    i++;
                }

                // cashier form dtset will main form dt set to prevent errors of nullexception
                cashierForm.dtSet = this.dtSet;


                cashierForm.FormClosed += CashierForm_FormClosed;
                cashierForm.Show();

                string saleId = BillsGridView.CurrentRow.Cells[2].Value.ToString();

                string sqlcommand1 = "SELECT * FROM vpsPosReceipts WHERE receiptId = '" + saleId + "'";

                DataTable receiptInfoDataTable = new DataTable();
                receiptInfoDataTable = databaseClass.executeSqlCommand(sqlcommand1);

                string FormId = receiptInfoDataTable.Rows[0][2].ToString();
                string Total = receiptInfoDataTable.Rows[0][5].ToString();
                string discount = receiptInfoDataTable.Rows[0][8].ToString();
                string Charge = receiptInfoDataTable.Rows[0][6].ToString();
                string Change = receiptInfoDataTable.Rows[0][7].ToString();

                string customerName = receiptInfoDataTable.Rows[0][3].ToString();
                string employeeName = receiptInfoDataTable.Rows[0][4].ToString();

                string sqlcommand = "SELECT * FROM vpsPosReceiptItems WHERE receiptId = '" + saleId + "' ORDER BY orderOfItems ASC";

                DataTable itemsDataTable = new DataTable();
                itemsDataTable = databaseClass.executeSqlCommand(sqlcommand);


                //insert items
                cashierForm.ItemsInsert(Total, discount, Charge, Change, FormId, customerName, itemsDataTable, employeeName);
                cashierForm.isFromBillsGrid = true;
                cashierForm.receiptIdFromBillsGrid = saleId;

            }
         
        }
        //cashier form closed function
        private void CashierForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cashierForm = null;
            //throw new NotImplementedException();
        }

        //refresh
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            if (SearchTextBox.Text.Trim() == "")
            {
                BillsGridView.DataSource = null;
            }
            else
            {

                InfoForm infoForm = new InfoForm();

                infoForm.Show();

                string searchColumn = SearchComboBox.Text;
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = $"SELECT receiptId AS Id,receiptDate AS Date,cashierFormId AS Form_Id,customerName AS Customer_Name,totalAmount AS Total FROM vpsPosReceipts WHERE {searchColumn} like @phrase";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@phrase", SqlDbType.VarChar).Value = SearchTextBox.Text.Trim() + "%";
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable BillDataTable = new DataTable();
                        adapter.Fill(BillDataTable);
                        BillsGridView.DataSource = BillDataTable;

                    }
                }

                if (userlogin.Rows[0][1].ToString() != "pradeep")
                {
                    BillsGridView.Columns[6].Visible = false;
                    BillsGridView.Columns[3].Visible = false;
                }
                NoOfBillsTextBox.Text = BillsGridView.RowCount.ToString();


                infoForm.Close();


            }

        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            if (SearchCheckBox.Checked == false)
            {
                if (SearchTextBox.Text.Trim() == "")
                {
                    BillsGridView.DataSource = null;
                }

                else
                {
                    InfoForm infoForm = new InfoForm();

                    if (SearchTextBox.Text.Trim().Count() < 3)
                    {
                        infoForm.Show();

                    }

                    string searchColumn = SearchComboBox.Text;
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT receiptId AS Id,receiptDate AS Date,cashierFormId AS Form_Id,customerName AS Customer_Name,totalAmount AS Total  FROM vpsPosReceipts WHERE {searchColumn} like @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@phrase", SqlDbType.VarChar).Value = SearchTextBox.Text.Trim() + "%";
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable BillDataTable = new DataTable();
                            adapter.Fill(BillDataTable);
                            BillsGridView.DataSource = BillDataTable;

                        }
                    }

                    if (userlogin.Rows[0][1].ToString() != "pradeep")
                    {
                        BillsGridView.Columns[6].Visible = false;
                        BillsGridView.Columns[3].Visible = false;
                    }
                    NoOfBillsTextBox.Text = BillsGridView.RowCount.ToString();

                    if (SearchTextBox.Text.Trim().Count() < 3)
                    {
                        infoForm.Close();

                    }
                }
            }

        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            InfoForm infoForm = new InfoForm();

            infoForm.Show();
            string EndDate = DateEnd.Value.ToString("yyyy-MM-dd HH:mm");

            string StartDate = DateStart.Value.ToString("yyyy-MM-dd HH:mm");
            string sqlcommanddate = "SELECT receiptId AS Id,receiptDate AS Date,cashierFormId AS Form_Id,customerName AS Customer_Name,totalAmount AS Total  FROM vpsPosReceipts WHERE receiptDate BETWEEN '" + StartDate + "' AND '" + EndDate + "'";
            BillsGridView.DataSource = this.databaseClass.executeSqlCommand(sqlcommanddate);

            NoOfBillsTextBox.Text = BillsGridView.RowCount.ToString();
            if (userlogin.Rows[0][1].ToString() != "pradeep")
            {
                BillsGridView.Columns[6].Visible = false;
                BillsGridView.Columns[3].Visible = false;
            }
            infoForm.Close();
          
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BillsGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void SearchTextBoxButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("BillForm", GetCurrentFormData());
            if (SearchCheckBox.Checked == true)
            {
                if (SearchTextBox.Text.Trim() == "")
                {
                    BillsGridView.DataSource = null;
                }
                else
                {
                    InfoForm infoForm = new InfoForm();
                    infoForm.Show();

                    string searchColumn = SearchComboBox.Text;
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT receiptId AS Id,receiptDate AS Date,cashierFormId AS Form_Id,customerName AS Customer_Name,totalAmount AS Total  FROM vpsPosReceipts WHERE {searchColumn} like @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@phrase", SqlDbType.VarChar).Value = SearchTextBox.Text.Trim() + "%";
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable BillDataTable = new DataTable();
                            adapter.Fill(BillDataTable);
                            BillsGridView.DataSource = BillDataTable;

                        }
                    }

                    NoOfBillsTextBox.Text = BillsGridView.RowCount.ToString();

                    infoForm.Close();
                    if (userlogin.Rows[0][1].ToString() != "pradeep")
                    {
                        BillsGridView.Columns[6].Visible = false;
                        BillsGridView.Columns[3].Visible = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Checkbox not checked !", "Warning");
            }
            
        }

        //hide column after 5 minutes
        private void columnHideTimer_Tick(object sender, EventArgs e)
        {
            BillsGridView.Columns[6].Visible = false;

            NoOfBillsTextBox.Visible = false;
            noOfBillsLabel.Visible = false;
            BillsGridView.Columns[3].Visible = false;
            DateStart.Visible = false;
            DateEnd.Visible = false;
            SearchButton.Visible = false;

            columnHideTimer.Stop();
        }
    }
}
