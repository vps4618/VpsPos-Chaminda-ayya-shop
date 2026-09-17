using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;

namespace VpsPos
{
    // test
    public partial class CashierForm : Form
    {
        // helper method for get all data for suspicious bills
        //
        private string GetOldItemsFromDB(int receiptId)
        {
            string items = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    // Fetch items currently in the DB (before the edit overwrites them)
                    string query = "SELECT itemName, salesPrice,quantity FROM vpsPosReceiptItems WHERE receiptId = @rid ORDER BY orderOfItems ASC";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@rid", receiptId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                double total = Convert.ToDouble(reader["quantity"]) * Convert.ToDouble(reader["salesPrice"]);
                                items += $"{reader["itemName"]} -> {reader["salesPrice"]} x {reader["quantity"]} = {total}\n\n";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            { return "Error fetching old items !"; }
            return items.Trim();
        }

        // advanced helper for suspicious bills
        //
        private void LogSuspiciousActivity(int receiptId, double oldTotal, double newTotal, string manualCashier, string custName, string reason, string oldItems, DateTime originalTime)
        {
            try
            {
                // 1. Get New Items from the Grid
                string newItems = "";
                foreach (DataGridViewRow row in CashierGridView.Rows)
                {
                    if (row.Cells[1].Value != null)
                        newItems += $"{row.Cells[1].Value} -> {row.Cells[4].Value} x {row.Cells[3].Value} = {row.Cells[5].Value}\n\n";
                }

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string query = @"INSERT INTO SuspiciousBills 
                (ReceiptId, CashierName, Reason, CustomerName, DateCreated, SuspiciousType, OldTotal, NewTotal, PastItems, NewItems, OriginalDate) 
                VALUES (@rid, @cashier, @reason, @cust, GETDATE(), 'Value_Reduction', @old, @new, @past, @future, @orig)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@rid", receiptId);
                        cmd.Parameters.AddWithValue("@cashier", manualCashier);
                        cmd.Parameters.AddWithValue("@reason", reason);
                        cmd.Parameters.AddWithValue("@cust", custName);
                        cmd.Parameters.AddWithValue("@old", oldTotal);
                        cmd.Parameters.AddWithValue("@new", newTotal);
                        cmd.Parameters.AddWithValue("@past", oldItems);
                        cmd.Parameters.AddWithValue("@future", newItems);
                        cmd.Parameters.AddWithValue("@orig", originalTime);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CashierForm", "LogSuspiciousActivity", GetCurrentFormData());
            }
        }

        // error logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "ItemName", ItemsComboBox.Text},
        { "Barcode", BarcodeTextBox.Text},
        { "IsRetail", RetailCheckBox.Checked.ToString()},
        { "CustomerName", customerNameTextBox.Text },
        { "AmountTotal", AmountTotalTextBox.Text },
        { "Charge", ChargeTextBox.Text },
        { "Change", ChangeTextBox.Text },
        { "Id", ReceiptIdTextBox.Text},
        { "NoOfItems", noOfItemsTextBox.Text },
        { "CurrentGridRowCount", CashierGridView.Rows.Count.ToString() }
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in CashierGridView.Rows)
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
        // to stop async function when AddTotal() call from Load function
        public bool isCallAddTotalFromLoad = false;

        public DataTable userlogin { get; set; }
        public bool isFromBillsGrid = false;
        public string receiptIdFromBillsGrid = "";
        public DataSet dtSet { get; set; }

        public bool isDarkMode { get; set; }
        // unique phrase to each computer using this app


        string clientPhrase;

        //making conString  and conn global by using public static keywords
        public static string conString;

        SqlConnection conn;

        //create databaseclass object to connect to database
        private DatabaseClass databaseClass = new DatabaseClass();

        public CashierForm()
        {
            InitializeComponent();

        }

        public bool Iswholesaleform { get; set; }

        // item add function to gridview
        public void addAnItemToGridView(DataTable scannedItem)
        {
            id = scannedItem.Rows[0][0].ToString();
            string itemName1 = scannedItem.Rows[0][1].ToString();

            //checking whether quantity is not zero
            DataTable quantityCheckDataTable = new DataTable();
            quantityCheckDataTable = this.databaseClass.executeSqlCommand("SELECT * FROM stock WHERE ITEM_ID='" + id + "' AND Quantity>0");
            if (quantityCheckDataTable.Rows.Count > 0)
            {
                if (RetailCheckBox.Checked == false)
                {
                    WholeSalePrice = Convert.ToDouble(scannedItem.Rows[0][15]);

                    // for checking whether wholesale price updated for cureent item
                    double chkSalePrice = Convert.ToDouble(scannedItem.Rows[0][7].ToString());
                    double chkWholesalePrice = Convert.ToDouble(scannedItem.Rows[0][15].ToString());
                    double chkBillPrice = Convert.ToDouble(scannedItem.Rows[0][14].ToString());
                    string chkEnglishName = scannedItem.Rows[0][1].ToString();
                    if ((chkWholesalePrice == chkSalePrice) || (chkWholesalePrice == chkBillPrice) || (chkWholesalePrice == 0))
                    {
                        MessageBox.Show($"Wholesale price is not set for {chkEnglishName}!\n\nමෙම භාණ්ඩයේ තොග මිල සාදා නැත! ", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }


                    itemName = scannedItem.Rows[0][1].ToString();
                    SinhalaName = scannedItem.Rows[0][13].ToString();
                    //QuantityTextBox.Focus();
                    double quantity1 = 1;
                    double totalSalesPrice = WholeSalePrice * quantity1;

                    double billPrice = Convert.ToDouble(scannedItem.Rows[0][14]);
                    double totalBillPrice = billPrice * quantity1;

                    double costPrice = Convert.ToDouble(scannedItem.Rows[0][6]);
                    double totalCostPrice = costPrice * quantity1;

                    double discount = 0;

                    BarcodeTextBox.Clear();
                    //QuantityTextBox.Clear();

                    //this condition is for while loop failing when checking  item duplication
                    if (CashierGridView.Rows.Count > 0)
                    {
                        bool check = false;
                        int i = 0;
                        int row = 0;
                        int count = CashierGridView.Rows.Count;

                        //loop for check item duplication
                        while (i < count)
                        {
                            if (CashierGridView.Rows[i].Cells[0].Value.ToString() == id)
                            {
                                check = true;
                                row = i;
                                break;

                            }
                            else
                            {
                                check = false;
                            }
                            i = i + 1;
                        }

                        //add item when false
                        if (check == false)

                        {
                            rowNumber = CashierGridView.Rows.Add(id, itemName, SinhalaName, quantity1, WholeSalePrice, totalSalesPrice, discount, false, billPrice, totalBillPrice, costPrice, totalCostPrice);
                            //select cell
                            CashierGridView.Rows[rowNumber].Cells[3].Selected = true;
                            CashierGridView.CurrentCell = CashierGridView.Rows[rowNumber].Cells[3];
                            CashierGridView.BeginEdit(true);
                        }

                        //increase quantity when item already exists
                        else
                        {
                            //increase
                            CashierGridView.Rows[row].Cells[3].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) + quantity1;
                            //refresh
                            CashierGridView.Rows[row].Cells[5].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[row].Cells[4].Value);

                            CashierGridView.Rows[row].Cells[9].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[row].Cells[8].Value);

                            CashierGridView.Rows[row].Cells[11].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[row].Cells[10].Value);

                            //select cell
                            CashierGridView.Rows[row].Cells[3].Selected = true;
                            CashierGridView.CurrentCell = CashierGridView.Rows[row].Cells[3];
                            CashierGridView.BeginEdit(true);
                        }
                    }
                    else
                    {
                        //add first row
                        rowNumber = CashierGridView.Rows.Add(id, itemName, SinhalaName, quantity1, WholeSalePrice, totalSalesPrice, discount, false, billPrice, totalBillPrice, costPrice, totalCostPrice);
                        //select
                        CashierGridView.Rows[rowNumber].Cells[3].Selected = true;
                        CashierGridView.CurrentCell = CashierGridView.Rows[rowNumber].Cells[3];
                        CashierGridView.BeginEdit(true);
                    }
                    AddTotal();
                    calculateChange();

                }
                // 2022/12/31 last update from barcode scan to here
                else
                {
                    SalePrice = Convert.ToDouble(scannedItem.Rows[0][7]);
                    itemName2 = scannedItem.Rows[0][1].ToString();
                    SinhalaName2 = scannedItem.Rows[0][13].ToString();
                    //QuantityTextBox.Focus();

                    double quantity1 = 1;
                    double salesPricetotal = SalePrice * quantity1;

                    double billPrice = Convert.ToDouble(scannedItem.Rows[0][14].ToString());
                    double billPriceTotal = billPrice * quantity1;

                    double costPrice = Convert.ToDouble(scannedItem.Rows[0][6].ToString());
                    double costPriceTotal = costPrice * quantity1;

                    double discount = billPriceTotal - salesPricetotal;

                    BarcodeTextBox.Clear();

                    //this condition is for while loop failing when checking  item duplication
                    if (CashierGridView.Rows.Count > 0)
                    {
                        bool check = false;
                        int i = 0;
                        int row = 0;
                        int count = CashierGridView.Rows.Count;

                        //loop for check item duplication
                        while (i < count)
                        {
                            if (CashierGridView.Rows[i].Cells[0].Value.ToString() == id)
                            {
                                check = true;
                                row = i;
                                //break is very important
                                break;
                            }
                            else
                            {
                                check = false;
                            }
                            i++;
                        }

                        //add item when false
                        if (check == false)

                        {
                            //add
                            rowNumber = CashierGridView.Rows.Add(id, itemName2, SinhalaName2, quantity1, SalePrice, salesPricetotal, discount, false, billPrice, billPriceTotal, costPrice, costPriceTotal);
                            //select
                            CashierGridView.Rows[rowNumber].Cells[3].Selected = true;
                            CashierGridView.CurrentCell = CashierGridView.Rows[rowNumber].Cells[3];
                            CashierGridView.BeginEdit(true);
                        }

                        //increase quantity when item already exists
                        else
                        {
                            //increase quantity
                            CashierGridView.Rows[row].Cells[3].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) + quantity1;
                            //refresh
                            CashierGridView.Rows[row].Cells[5].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[row].Cells[4].Value);

                            CashierGridView.Rows[row].Cells[9].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[row].Cells[8].Value);

                            CashierGridView.Rows[row].Cells[11].Value = Convert.ToDouble(CashierGridView.Rows[row].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[row].Cells[10].Value);

                            //select
                            CashierGridView.Rows[row].Cells[3].Selected = true;
                            CashierGridView.CurrentCell = CashierGridView.Rows[row].Cells[3];
                            CashierGridView.BeginEdit(true);
                        }
                    }
                    else
                    {
                        //add
                        rowNumber = CashierGridView.Rows.Add(id, itemName2, SinhalaName2, quantity1, SalePrice, salesPricetotal, discount, false, billPrice, billPriceTotal, costPrice, costPriceTotal);
                        //select
                        CashierGridView.Rows[rowNumber].Cells[3].Selected = true;

                        CashierGridView.CurrentCell = CashierGridView.Rows[rowNumber].Cells[3];

                        CashierGridView.BeginEdit(true);
                    }

                    AddTotal();
                    calculateChange();
                }
            }
            else
            {
                MessageBox.Show("There is no stock in this item !\nමෙම භාණ්ඩයේ තොග අවසන් වී ඇත!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void ItemsInsert(string TotalAmout, string totalDiscount, string Charge, string Change, string FormId, string customerName, DataTable itemsDataTable, string employee)
        {
            try
            {
                SaleId = Convert.ToInt32(itemsDataTable.Rows[0][0]);
                UserNameLogin.Text = employee;
                int i = 0;
                int count = itemsDataTable.Rows.Count;
                while (i < count)
                {
                    string itemId = itemsDataTable.Rows[i][1].ToString();
                    string itemName = itemsDataTable.Rows[i][2].ToString();
                    string sinhalaName = itemsDataTable.Rows[i][3].ToString();
                    string quantity = itemsDataTable.Rows[i][4].ToString();
                    string salesPrice = itemsDataTable.Rows[i][5].ToString();
                    string salesPriceTotal = itemsDataTable.Rows[i][6].ToString();
                    string discount = itemsDataTable.Rows[i][7].ToString();
                    bool space = false;
                    string billPrice = itemsDataTable.Rows[i][8].ToString();
                    string billPriceTotal = itemsDataTable.Rows[i][9].ToString();
                    string costPrice = itemsDataTable.Rows[i][10].ToString();
                    string costPriceTotal = itemsDataTable.Rows[i][11].ToString();

                    CashierGridView.Rows.Add(itemId, itemName, sinhalaName, quantity, salesPrice, salesPriceTotal, discount, space, billPrice, billPriceTotal, costPrice, costPriceTotal);
                    i++;



                }

                // update checked or not 
                foreach (DataGridViewRow row in CashierGridView.Rows)
                {
                    string sinhalaName = row.Cells[2].Value.ToString();
                    if (sinhalaName.Contains("✅   "))
                    {
                        row.Cells[7].Value = true;
                    }
                    else
                    {
                        row.Cells[7].Value = false;
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CashierForm", nameof(ItemsInsert), GetCurrentFormData());
                MessageBox.Show("No Items !\nභාණ්ඩ නැත!");
            }
            //input data from tbl_sales to text boxes
            ReceiptIdTextBox.Text = FormId;

            AmountTotalTextBox.Text = TotalAmout;
            ChargeTextBox.Text = Charge;
            ChangeTextBox.Text = Change;
            customerNameTextBox.Text = customerName;
            discountTextBox.Text = totalDiscount;
            Iswholesaleform = true;
            AddingNoOfItemsToTextBox();
            BarcodeTextBox.Focus();
        }


        private CancellationTokenSource saveToken;

        private async void AddTotal()
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            AddingNoOfItemsToTextBox();
            if (CashierGridView.Rows.Count > 0)
            {
                double total = 0;
                double EndTotal = 0;
                double discount = 0;
                int i;
                for (i = 0; i < CashierGridView.Rows.Count; i++)
                {
                    discount = discount + Convert.ToDouble(CashierGridView.Rows[i].Cells[6].Value);
                    total = total + Convert.ToDouble(CashierGridView.Rows[i].Cells[5].Value);
                    EndTotal = Math.Round(total, MidpointRounding.AwayFromZero);
                }
                AmountTotalTextBox.Text = EndTotal.ToString();
                discountTextBox.Text = discount.ToString();
            }
            else
            {
                discountTextBox.Text = "0";
                AmountTotalTextBox.Text = "0";
            }

            try
            {
                // to stop async function when AddTotal() call from Load function
                if (isCallAddTotalFromLoad == false && CashierGridView.Rows.Count != 0)
                {
                    XmlDocument doc1 = new XmlDocument();

                    doc1.Load("requiredInfo.xml");
                    XmlNodeList nodes = doc1.GetElementsByTagName("phrase");
                    string phrase = nodes[0].InnerText;
                    string pc = "";
                    if (phrase == "A")
                    {
                        pc = "Pc2";
                    }
                    else if (phrase == "B")
                    {
                        pc = "Pc1";
                    }

                    // 🔹 Cancel any previous save in progress
                    saveToken?.Cancel();
                    saveToken = new CancellationTokenSource();
                    var token = saveToken.Token;

                    using (SqlConnection conn = new SqlConnection(conString))
                    {
                        await conn.OpenAsync(token);

                        using (SqlTransaction tran = conn.BeginTransaction())
                        {
                            try
                            {
                                // 1. Clear table
                                string clearSql = "DELETE FROM cacheLastBillDetails" + pc + ";" + "DELETE FROM cacheLastBillItems" + pc + ";";

                                using (SqlCommand clearCmd = new SqlCommand(clearSql, conn, tran))
                                {
                                    await clearCmd.ExecuteNonQueryAsync(token);
                                }

                                // 2. Insert all items
                                foreach (DataGridViewRow row in CashierGridView.Rows)
                                {
                                    int orderNo = 0;
                                    if (row.Cells[12].Value == null)
                                    {
                                        orderNo = 0;
                                    }
                                    else
                                    {
                                        orderNo = Convert.ToInt32(row.Cells[12].Value);
                                    }

                                    token.ThrowIfCancellationRequested(); // check cancel

                                    string sql = "INSERT INTO cacheLastBillItems" + pc + " (ItemId, ItemName, SinhalaName, Quantity, SalesPrice,Total,discount,Mark,BillPrice,TotalBillPrice,CostPrice,TotalCostPrice,OrderNo) " +
                                                 "VALUES (@ItemId, @ItemName, @SinhalaName, @Quantity, @SalesPrice,@Total,@discount,@Mark,@BillPrice,@TotalBillPrice,@CostPrice,@TotalCostPrice,@OrderNo)";
                                    using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                                    {
                                        cmd.Parameters.AddWithValue("@ItemId", row.Cells[0].Value);
                                        cmd.Parameters.AddWithValue("@ItemName", row.Cells[1].Value);
                                        cmd.Parameters.AddWithValue("@SinhalaName", row.Cells[2].Value);
                                        cmd.Parameters.AddWithValue("@Quantity", row.Cells[3].Value);
                                        cmd.Parameters.AddWithValue("@SalesPrice", row.Cells[4].Value);
                                        cmd.Parameters.AddWithValue("@Total", row.Cells[5].Value);
                                        cmd.Parameters.AddWithValue("@discount", row.Cells[6].Value);
                                        cmd.Parameters.AddWithValue("@Mark", row.Cells[7].Value);
                                        cmd.Parameters.AddWithValue("@BillPrice", row.Cells[8].Value);
                                        cmd.Parameters.AddWithValue("@TotalBillPrice", row.Cells[9].Value);
                                        cmd.Parameters.AddWithValue("@CostPrice", row.Cells[10].Value);
                                        cmd.Parameters.AddWithValue("@TotalCostPrice", row.Cells[11].Value);
                                        cmd.Parameters.AddWithValue("@OrderNo", orderNo);

                                        await cmd.ExecuteNonQueryAsync(token);
                                    }
                                }

                                // 3. save bill id 

                                string sql1 = "INSERT INTO cacheLastBillDetails" + pc + " (billId) VALUES (@billId)";
                                using (SqlCommand cmd = new SqlCommand(sql1, conn, tran))
                                {
                                    cmd.Parameters.AddWithValue("@billId", ReceiptIdTextBox.Text);
                                    await cmd.ExecuteNonQueryAsync(token);
                                }

                                tran.Commit(); // ✅ all-or-nothing
                            }
                            catch (OperationCanceledException ex)
                            {
                                // normal: previous save canceled → just ignore
                            }
                            catch (SqlException ex) when (ex.Message.Contains("Operation cancelled by user"))
                            {
                                // ignore the cancellation error from SQL Server
                            }
                            catch (Exception ex)
                            {
                                // real errors: show or log
                                MessageBox.Show("Save failed: " + ex.Message + "\nක්‍රියාව අසාර් ථක වී ඇත.");
                                ErrorLogger.Log(ex, "CashierForm", nameof(AddTotal), GetCurrentFormData());
                            }
                        }
                    }

                }
                else
                {
                    isCallAddTotalFromLoad = false;
                }
            }
            catch (Exception error)
            {

            }

        }

        private void CashierForm_Load(object sender, EventArgs e)
        {
            try
            {
                // apply theme
                SwitchMode.ApplyTheme(this, isDarkMode);

                XmlDocument doc = new XmlDocument();

                doc.Load("requiredInfo.xml");
                XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
                conString = nodes[0].InnerText;

                conn = new SqlConnection(conString);

                XmlNodeList nodes1 = doc.GetElementsByTagName("phrase");
                clientPhrase = nodes1[0].InnerText;


                this.WindowState = FormWindowState.Maximized;
                AddingNoOfItemsToTextBox();
                RetailCheckBox.Checked = true;
                printCopiesTextBox.Text = "1";


                try
                {
                    UserNameLogin.Text = userlogin.Rows[0][2].ToString();


                }
                catch (Exception)
                {
                    UserNameLogin.Text = "nu";
                }
                //load id to id text
                DataTable id = new DataTable();
                id = this.databaseClass.executeSqlCommand("SELECT * FROM vpsPosReceipts");
                if (id.Rows.Count > 0)
                {

                    ReceiptIdTextBox.Text = clientPhrase + (id.Rows.Count + 1).ToString();


                }
                else
                {
                    ReceiptIdTextBox.Text = clientPhrase + 1;
                }

                isCallAddTotalFromLoad = true;

                AddTotal();
                calculateChange();

                // add message to messagerichtextbox
                string sqlcommand3 = "SELECT message FROM vpsPosCashierFormMessage";
                conn.Open();
                SqlDataAdapter dtAdapter1 = new SqlDataAdapter(sqlcommand3, conn);
                DataSet dtSet1 = new DataSet();
                dtAdapter1.Fill(dtSet1);
                messageRichTextBox.Text = dtSet1.Tables[0].Rows[0]["message"].ToString();

                conn.Close();

                messageRichTextBox.ReadOnly = true;
                /*
                //adding bills to  combobox
                string sqlcommandToInsertBills = "SELECT * FROM tbl_sales";
                conn.Open();
                SqlDataAdapter dtAdapterToInsertBills = new SqlDataAdapter(sqlcommandToInsertBills, conn);
                DataSet dtSetToInsertBills = new DataSet();
                dtAdapterToInsertBills.Fill(dtSetToInsertBills);
                int countToInsertBills = dtSetToInsertBills.Tables[0].Rows.Count;
                int iToInsertBills = 0;
                while (iToInsertBills < countToInsertBills)
                {
                    BillsComboBox.Items.Add(dtSetToInsertBills.Tables[0].Rows[iToInsertBills]["saleDate"].ToString()+" - "+ dtSetToInsertBills.Tables[0].Rows[iToInsertBills]["cashierFormId"].ToString());
                    iToInsertBills++;
                }

                conn.Close();
                */

                //how to get id from billcombo box
                /*
                            string bill = "20/07/2022 03:15:31 - 2231223";
                            string cut =bill.Remove(0,21);
                            Console.WriteLine(cut);
                 */
            }
            catch (Exception ex)
            {
                MessageBox.Show("CashierForm Load Error: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close(); // or Application.Exit();
            }

        }


        //making usable in another function
        string itemName { get; set; }
        string itemName2 { get; set; }
        string SinhalaName { get; set; }
        string SinhalaName2 { get; set; }
        string id { get; set; }
        double WholeSalePrice { get; set; }
        double SalePrice { get; set; }

        //insert item to gridview when barcode text box's text changed
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        int rowNumber { get; set; }


        //refresh when Enter key pressed  in gridview

        //change cell values(refresh) when cell clicked  by mouse
        private void CashierGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (CashierGridView.Rows.Count > 0)
            {
                int i = 0;
                int count = CashierGridView.Rows.Count;
                while (i < count)
                {
                    try
                    {
                        // fix unknownly entering '.' when adding quantities
                        if (CashierGridView.Rows[i].Cells[3].Value.ToString() == ".")
                        {
                            if (MessageBox.Show($"Row number{i}'s quantity has a value of '.' .Would you like to change it to 1 ?  (වැරදිමකින් ඇතුලත් වූ තිත වෙනිවට 1 ඉලක්කම එකතු කරන්නද?)", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            { CashierGridView.Rows[i].Cells[3].Value = 1; }
                        }
                    }
                    catch (Exception error)
                    {
                        ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_CellMouseClick), GetCurrentFormData());
                        MessageBox.Show(error.Message);
                    }


                    //cHange cell
                    // totalSalePriceUpdate
                    CashierGridView.Rows[i].Cells[3].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value);

                    CashierGridView.Rows[i].Cells[5].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                    //totalBillPriceUpdate
                    CashierGridView.Rows[i].Cells[9].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);
                    //totalCostPriceUpdate
                    CashierGridView.Rows[i].Cells[11].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);

                    try
                    {
                        // logic to alert saleprice is hugely changed
                        // saleprice = 4
                        // billprice = 8
                        double diff = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        double percentageChange = (diff / Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)) * 100;

                        // Alert if price drop > 50% OR absolute drop > 500
                        if ((percentageChange > 50 || diff > 500) && RetailCheckBox.Checked == true && RetailCheckBox.Checked)
                        {
                            MessageBox.Show(
                                $"⚠️ Price drop is unusually high!\n\n" +
                                $"Bill Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)}\n" +
                                $"New Sale Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value)}\n\n" +
                                $"Please double-check. (විකුනුම් මිලේ ලොකු වෙනසක් සිදුවී ඇත.එය නැවත බලන්න.)",
                                "Abnormal Price Change",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                        }

                        // check cost price is less than sale price
                        // cost = 10
                        // sale = 4
                        double costPrice = Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);
                        double salePrice = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        string idItem = Convert.ToString(CashierGridView.Rows[i].Cells[0].Value);

                        DataTable wholesalePriceDt = new DataTable();
                        wholesalePriceDt = this.databaseClass.executeSqlCommand("SELECT Wholesale FROM iteminformation WHERE ITEM_ID=" + idItem);
                        double wholesalePrice = Convert.ToDouble(wholesalePriceDt.Rows[0][0].ToString());

                        if (salePrice < costPrice)
                        {
                            MessageBox.Show(
                               $"Cost Price is higher than sale price.\n" +

                               $"So wholesale Price was added for that field.\nවිකුණන මිල කඩයට භාණ්ඩ ලැබෙන මිලට වඩා අඩුවී ඇත. එමනිසා එම ස්ථානයට තොග මිල ඇතුලත් කරන ලදි.",
                               "Abnormal Price Change",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                            CashierGridView.Rows[i].Cells[4].Value = wholesalePrice;
                        }

                        // check whether bill price < sale price
                        double salePrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        double billPrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);

                        if (billPrice3 < salePrice3)
                        {
                            MessageBox.Show($"Sale Price is higher than bill price.\n" +

                                                           $"So bill Price was added for that field.\nවිකුණන මිල භාණ්ඩයේ සදහන් මිලට වඩා වැඩිවී ඇත. එමනිසා එම ස්ථානයට භාණ්ඩයේ සදහන් මිල ඇතුලත් කරන ලදි.",
                                                           "Abnormal Price Change",
                                                           MessageBoxButtons.OK,
                                                           MessageBoxIcon.Warning);
                            CashierGridView.Rows[i].Cells[4].Value = billPrice3;
                        }

                    }
                    catch (Exception error)
                    {
                        ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_CellMouseClick), GetCurrentFormData());
                        MessageBox.Show(error.Message);
                    }

                    if (RetailCheckBox.Checked == true)
                    {
                        CashierGridView.Rows[i].Cells[6].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[9].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[5].Value);

                    }
                    else
                    {
                        CashierGridView.Rows[i].Cells[6].Value = 0;
                    }
                    i++;
                }

                //select
                CashierGridView.CurrentCell.Selected = true;
                CashierGridView.CurrentCell = CashierGridView.CurrentCell;
                CashierGridView.BeginEdit(true);

            }
            AddTotal();
            calculateChange();

            // 2022/12/31 this function also updated


        }

        private void RemoveItemButton_Click(object sender, EventArgs e)
        {

            if (CashierGridView.Rows.Count > 0)
            {
                // physichological barrier
                double itemVal = Convert.ToDouble(CashierGridView.CurrentRow.Cells[5].Value);

                // Check if editing a saved bill
                if (isFromBillsGrid && itemVal > 200)
                {
                    if (MessageBox.Show(
                        $"Removing this saved item of ({itemVal}) will be informed to owner.\nDo you want to continue ?\nපෙරදි ඇතුලත් කර තිබූ මෙම එකතුව රු.{itemVal} වන භාණ්ඩය ඉවත් කල බව ප්‍රධානියාට දැනුම් දෙනු ඇත.\nඔබට ඉදිරියට යාමට අවශ්‍යද ? ",
                        "Audit Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        CashierGridView.Rows.Remove(CashierGridView.CurrentRow);
                        BarcodeTextBox.Focus();
                    }

                }
                else if (!isFromBillsGrid && itemVal > 500)
                {
                    if (MessageBox.Show(
                       $"Removing this item of ({itemVal}) will be informed to owner.\nDo you want to continue ?\nමෙම එකතුව රු.{itemVal} වන භාණ්ඩය ඉවත් කල බව ප්‍රධානියාට දැනුම් දෙනු ඇත.\nඔබට ඉදිරියට යාමට අවශ්‍යද ? ",
                       "Audit Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        CashierGridView.Rows.Remove(CashierGridView.CurrentRow);
                        BarcodeTextBox.Focus();
                    }
                }
                else
                {
                    if (MessageBox.Show("Do you really want to delete this item ?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        CashierGridView.Rows.Remove(CashierGridView.CurrentRow);
                        BarcodeTextBox.Focus();

                    }
                }
            }
            else
            {
                MessageBox.Show("There is no item in grid.", "Warning");
            }

            AddTotal();
            calculateChange();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //barcode focus
            if (keyData == (Keys.F1))
            {
                BarcodeTextBox.Focus();
                return true;
            }
            //itemscombobox focus
            if (keyData == (Keys.F2))
            {
                ItemsComboBox.Focus();
                return true;
            }
            //wholesale checkk
            if (keyData == (Keys.F3))
            {
                RetailCheckBox.Checked = true;

                MessageBox.Show("Retail mode activated !\nසිල්ලර බිල් තත්වය සක්‍රිය කර ඇත!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return true;
            }
            //Wholesale uncheck
            if (keyData == (Keys.F4))
            {
                RetailCheckBox.Checked = false;

                MessageBox.Show("Wholesale mode activated !\nතොග බිල් තත්වය සක්‍රිය කර ඇත!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return true;
            }
            //chargetextbox focus
            if (keyData == (Keys.F5))
            {
                ChargeTextBox.Focus();
                return true;
            }

            // save receipt when f6 pressed
            if (keyData == (Keys.F6))
            {
                if (saveButton.Enabled == true)
                {
                    saveButton.PerformClick();
                }
                return true;
            }

            // save button focus
            if (keyData == (Keys.F7))
            {
                saveButton.Focus();
                return false;
            }

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

            //check out focus
            if (keyData == (Keys.F8))
            {
                printButton.Focus();
                return true;
            }
            //close button focus
            if (keyData == (Keys.F9))
            {
                CloseButton.Focus();
                return true;
            }
            //refresh button focus
            if (keyData == (Keys.F10))
            {
                RefreshButton.Focus();
                return true;
            }

            // convert full bill to wholesale or retail

            // to do - alert when wholesale price not updated when converting to wholesale
            if (keyData == (Keys.W | Keys.Control))
            {

                try
                {
                    DialogResult result = MessageBox.Show("Do you want to convert this bill to wholesale bill ?\nඔබට මේ බිල තොග බිලක් බවට පත් කිරීමට අවශ්‍යද ?\nYes - තොග , No - සිල්ලර", "convertion type", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);


                    // collect distinct item ids from grid
                    var itemIds = new List<int>();
                    foreach (DataGridViewRow row in CashierGridView.Rows)
                    {
                        if (row.IsNewRow) continue;
                        if (row.Cells[0].Value == null) continue;
                        if (int.TryParse(row.Cells[0].Value.ToString(), out int itemId))
                        {
                            if (!itemIds.Contains(itemId)) itemIds.Add(itemId);
                        }
                    }

                    if (itemIds.Count == 0)
                    {
                        return true;
                    }

                    // Build parameterized IN-list: @id0,@id1,.....
                    var paramNames = itemIds.Select((id, idx) => "@id" + idx).ToArray();
                    string sql = $"SELECT ITEM_ID,Wholesale,Price AS SalePrice FROM iteminformation WHERE ITEM_ID IN ({string.Join(",", paramNames)})";

                    var priceMap = new Dictionary<int, (double Wholesale, double Sale)>();

                    using (SqlConnection conn = new SqlConnection(conString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            for (int i = 0; i < itemIds.Count; i++)
                            {
                                cmd.Parameters.AddWithValue(paramNames[i], itemIds[i]);
                            }

                            DataTable dt = new DataTable();
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dt);
                            }

                            foreach (DataRow r in dt.Rows)
                            {
                                int id = Convert.ToInt32(r["ITEM_ID"].ToString());
                                double wholesale = Convert.ToDouble(r["Wholesale"].ToString());
                                double sale = Convert.ToDouble(r["SalePrice"].ToString());

                                priceMap[id] = (Wholesale: wholesale, Sale: sale);
                            }
                        }
                    }

                    // Apply to rows
                    foreach (DataGridViewRow row in CashierGridView.Rows)
                    {
                        if (row.IsNewRow) continue;
                        if (row.Cells[0].Value == null) continue;

                        int itemId = Convert.ToInt32(row.Cells[0].Value);
                        double qty = 1;
                        double.TryParse(Convert.ToString(row.Cells[3].Value ?? "1"), out qty);

                        var p = priceMap[itemId];

                        if (result == DialogResult.Yes)
                        {
                            row.Cells[4].Value = p.Wholesale;
                            row.Cells[5].Value = qty * p.Wholesale;

                            // for checking whether wholesale price updated for cureent item
                            double chkSalePrice = p.Sale;
                            double chkWholesalePrice = Convert.ToDouble(row.Cells[4].Value);
                            double chkBillPrice = Convert.ToDouble(row.Cells[8].Value);
                            string chkEnglishName = Convert.ToString(row.Cells[1].Value);
                            if ((chkWholesalePrice == chkSalePrice) || (chkWholesalePrice == chkBillPrice) || (chkWholesalePrice == 0))
                            {
                                MessageBox.Show($"Wholesale price is not set for {chkEnglishName}!\n\nමෙම භාණ්ඩයේ තොග මිල සාදා නැත! ", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }

                        }
                        else if (result == DialogResult.No)
                        {
                            row.Cells[4].Value = p.Sale;
                            row.Cells[5].Value = qty * p.Sale;
                        }
                    }

                    // Update the RetailCheckBox state
                    if (result == DialogResult.Yes)
                    {
                        RetailCheckBox.Checked = false;
                        MessageBox.Show("This bill was successfully converted to wholesale bill.\nමෙම බිල තොග බිලක් බවට සාර් ථකව පත් කරන ලදි.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else if (result == DialogResult.No)
                    {
                        RetailCheckBox.Checked = true;
                        MessageBox.Show("This bill was successfully converted to retail bill.\nමෙම බිල සිල්ලර බිලක් බවට සාර් ථකව පත් කරන ලදි.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Recalculate totals and change
                    AddTotal();
                    calculateChange();
                }
                catch (Exception e)
                {
                    ErrorLogger.Log(e, "CashierForm", nameof(ProcessCmdKey), GetCurrentFormData());
                    MessageBox.Show("Error Applying bill type : " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return true;
            }

            //restore cached grid
            if (keyData == (Keys.E | Keys.Control))
            {
                try
                {
                    XmlDocument doc1 = new XmlDocument();

                    doc1.Load("requiredInfo.xml");
                    XmlNodeList nodes = doc1.GetElementsByTagName("phrase");
                    string phrase = nodes[0].InnerText;
                    string pc = "";
                    if (phrase == "A")
                    {
                        pc = "Pc2";
                    }
                    else if (phrase == "B")
                    {
                        pc = "Pc1";
                    }

                    using (SqlConnection conn = new SqlConnection(conString))
                    {
                        conn.Open();

                        // Query for bill details
                        string sqlDetails = @"SELECT [billId], [op1], [op2] 
                              FROM [dbAxia].[dbo].[cacheLastBillDetails" + pc + "]";

                        SqlDataAdapter da2 = new SqlDataAdapter(sqlDetails, conn);
                        DataTable dtDetails = new DataTable();
                        da2.Fill(dtDetails);
                        string billId = "";


                        if (dtDetails.Rows.Count > 0)
                        {
                            billId = dtDetails.Rows[0]["billId"].ToString();

                            // if id doesnt match ,check cached id not saved in receipts. if so restoring can be done.This situation can happen when a user entering new receipt items to grid(in pc 2) and suddenly power drops and after reopening app , suddenly he forgot to restore previous bill and enter another new bill and save it(in pc 1). then he need to restore pc 2 previous bill. but he can't cause no of receipt increased so previous id is less than new id.This need to be fixed.

                            // Here also there is an condition. restoring that bill only possible when user put new bill in pc 1 and try to restore previous bill(before powercut) from pc 2. If he put new bill after powercut in pc 2 , since cached bill in pc 2 changed to new bill, then he cant restore previous bill before powercut.
                            // This also applied to pc 1 vice versa.
                            // * V 2 *
                            // logic to check previous id doesn't exist in receipts
                            string sqlForCheck = @"SELECT * 
                              FROM [dbAxia].[dbo].[vpsPosReceipts] WHERE cashierFormId='" + billId + "'";

                            SqlDataAdapter dCheck = new SqlDataAdapter(sqlForCheck, conn);
                            DataTable dtCheck = new DataTable();
                            dCheck.Fill(dtCheck);

                            if (billId == ReceiptIdTextBox.Text || dtCheck.Rows.Count == 0)
                            {

                                if (MessageBox.Show("Do you want restore last bill details ?\nබිලේ තොරතුරු නැවත ස්තාපනය කිරීමට අවශ්‍යද?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    // for * v2 * This added cause incremented bill id should be removed and replace that text box id with billId
                                    if (dtCheck.Rows.Count == 0)
                                    {
                                        ReceiptIdTextBox.Text = billId;
                                    }

                                    // Query for bill items
                                    string sqlItems = @"SELECT [ItemId], [ItemName], [SinhalaName], [Quantity], 
                                   [SalesPrice], [Total], [discount], [Mark], 
                                   [BillPrice], [TotalBillPrice], [CostPrice], 
                                   [TotalCostPrice], [OrderNo]
                            FROM [dbAxia].[dbo].[cacheLastBillItems" + pc + "]";

                                    SqlDataAdapter da = new SqlDataAdapter(sqlItems, conn);
                                    DataTable dtItems = new DataTable();
                                    da.Fill(dtItems);

                                    // Clear grid before adding
                                    CashierGridView.Rows.Clear();

                                    // Add each row one by one
                                    foreach (DataRow row in dtItems.Rows)
                                    {
                                        CashierGridView.Rows.Add(
                                            row["ItemId"],
                                            row["ItemName"],
                                            row["SinhalaName"],
                                            row["Quantity"],
                                            row["SalesPrice"],
                                            row["Total"],
                                            row["discount"],
                                            row["Mark"],
                                            row["BillPrice"],
                                            row["TotalBillPrice"],
                                            row["CostPrice"],
                                            row["TotalCostPrice"],
                                            row["OrderNo"]
                                        );
                                    }

                                    AddTotal();
                                    calculateChange();
                                    AddingNoOfItemsToTextBox();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Cashier form id of last bill doesn't match with current window id !\nමෙම පිටුවේ id එක හා අන්තිම බිලේ id නොගැලපේ !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No cached bills.\nනැවත ගැනීමට බිල් නැත.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }


                    }
                }
                catch (Exception error)
                {
                    ErrorLogger.Log(error, "CashierForm", nameof(ProcessCmdKey), GetCurrentFormData());
                    MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return true;
            }

            //close window
            if (keyData == (Keys.X | Keys.Control | Keys.Shift))
            {

                if (CashierGridView.Rows.Count != 0 && isFromBillsGrid == false)
                {
                    if (MessageBox.Show($"Grid has {CashierGridView.Rows.Count} items.\nIf you close this window , owner will be informed.\nDo you need to continue ?\n" +
                        $"වගුවේ භාණ්ඩ {CashierGridView.Rows.Count} ක් ඇත.\n" +
                        $"\nඔබ මේ පිටුව වැසුවොත් ඒ බව ප්‍රධානියාට දැනුම්දෙනු ඇත !\nඉදිරියට යාමට අවශ්‍යද ? ", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        this.Close();
                    }
                }
                else
                {
                    if (MessageBox.Show("Do you really want to close this window ?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this.Close();
                    }
                }
                return true;
            }



            // delete a row
            if (keyData == (Keys.Shift | Keys.Control | Keys.Delete))
            {
                if (CashierGridView.Rows.Count > 0)
                {
                    // physichological barrier
                    double itemVal = Convert.ToDouble(CashierGridView.CurrentRow.Cells[5].Value);

                    // Check if editing a saved bill
                    if (isFromBillsGrid && itemVal > 200)
                    {
                        if (MessageBox.Show(
                            $"Removing this saved item of ({itemVal}) will be informed to owner.\nDo you want to continue ?\nපෙරදි ඇතුලත් කර තිබූ මෙම එකතුව රු.{itemVal} වන භාණ්ඩය ඉවත් කල බව ප්‍රධානියාට දැනුම් දෙනු ඇත.\nඔබට ඉදිරියට යාමට අවශ්‍යද ? ",
                            "Audit Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            CashierGridView.Rows.Remove(CashierGridView.CurrentRow);
                            BarcodeTextBox.Focus();
                        }

                    }
                    else if (!isFromBillsGrid && itemVal > 500)
                    {
                        if (MessageBox.Show(
                           $"Removing this item of ({itemVal}) will be informed to owner.\nDo you want to continue ?\nමෙම එකතුව රු.{itemVal} වන භාණ්ඩය ඉවත් කල බව ප්‍රධානියාට දැනුම් දෙනු ඇත.\nඔබට ඉදිරියට යාමට අවශ්‍යද ? ",
                           "Audit Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            CashierGridView.Rows.Remove(CashierGridView.CurrentRow);
                            BarcodeTextBox.Focus();
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("Do you really want to delete this item ?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            CashierGridView.Rows.Remove(CashierGridView.CurrentRow);
                            BarcodeTextBox.Focus();

                        }
                    }
                }
                else
                {
                    MessageBox.Show("There is no item in grid.", "Warning");
                }

                AddTotal();
                calculateChange();
            }


            // save message
            if (keyData == (Keys.L | Keys.Control))
            {
                try
                {
                    string message = messageRichTextBox.Text;
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "UPDATE vpsPosCashierFormMessage SET message=@message";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@message", SqlDbType.VarChar).Value = message;
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Message updated successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception error)
                {
                    ErrorLogger.Log(error, "CashierForm", nameof(ProcessCmdKey), GetCurrentFormData());
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                messageRichTextBox.ReadOnly = true;

                return true;
            }

            //quick save
            if (keyData == (Keys.F11))
            {
                if (CashierGridView.RowCount > 0)
                {
                    BarcodeTextBox.Focus();
                    ChargeTextBox.Text = AmountTotalTextBox.Text;

                    double charge = Convert.ToDouble(ChargeTextBox.Text);
                    double total = Convert.ToDouble(AmountTotalTextBox.Text);
                    double change = charge - total;
                    ChangeTextBox.Text = change.ToString();

                    saveButton.PerformClick();
                }
                return true;
            }

            //Disable readonly mode in messagerichtextbox
            if (keyData == (Keys.Alt | Keys.L))
            {
                messageRichTextBox.ReadOnly = false;
                MessageBox.Show("Readonly mode Disabled !\nකියවීමට පමනක් ඇති හැකියාව ඉවත්කර ඇත.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }

            //quick save with print
            if (keyData == (Keys.F12))
            {
                if (CashierGridView.RowCount > 0)
                {
                    BarcodeTextBox.Focus();
                    ChargeTextBox.Text = AmountTotalTextBox.Text;

                    double charge = Convert.ToDouble(ChargeTextBox.Text);
                    double total = Convert.ToDouble(AmountTotalTextBox.Text);
                    double change = charge - total;
                    ChangeTextBox.Text = change.ToString();

                    printButton.PerformClick();
                }
                return true;
            }

            //quick change cost price and retail price for prevent profit error
            if (keyData == (Keys.Control | Keys.M))
            {
                if (CashierGridView.RowCount > 0)
                {
                    int id = CashierGridView.CurrentRow.Index;
                    double salePrice = Convert.ToDouble(CashierGridView.Rows[id].Cells[4].Value);
                    double costPrice = salePrice - salePrice * 0.1;
                    double billPrice = salePrice;
                    CashierGridView.Rows[id].Cells[10].Value = costPrice;
                    CashierGridView.Rows[id].Cells[8].Value = salePrice;

                    int i = 0;
                    int count = CashierGridView.Rows.Count;
                    while (i < count)
                    {
                        try
                        {
                            // fix unknownly entering '.' when adding quantities
                            if (CashierGridView.Rows[i].Cells[3].Value.ToString() == ".")
                            {
                                if (MessageBox.Show($"Row number{i}'s quantity has a value of '.' .Would you like to change it to 1 ?  (වැරදිමකින් ඇතුලත් වූ තිත වෙනිවට 1 ඉලක්කම එකතු කරන්නද?)", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                { CashierGridView.Rows[i].Cells[3].Value = 1; }
                            }
                        }
                        catch (Exception error)
                        {
                            ErrorLogger.Log(error, "CashierForm", nameof(ProcessCmdKey), GetCurrentFormData());
                            MessageBox.Show(error.Message);
                        }

                        //cHange cell
                        // totalSalePriceUpdate
                        CashierGridView.Rows[i].Cells[3].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value);

                        CashierGridView.Rows[i].Cells[5].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        //totalBillPriceUpdate
                        CashierGridView.Rows[i].Cells[9].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);
                        //totalCostPriceUpdate
                        CashierGridView.Rows[i].Cells[11].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);

                        try
                        {
                            // logic to alert saleprice is hugely changed
                            // saleprice = 4
                            // billprice = 8
                            double diff = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                            double percentageChange = (diff / Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)) * 100;

                            // Alert if price drop > 50% OR absolute drop > 500
                            if ((percentageChange > 50 || diff > 500) && RetailCheckBox.Checked == true && RetailCheckBox.Checked)
                            {
                                MessageBox.Show(
                                    $"⚠️ Price drop is unusually high!\n\n" +
                                    $"Bill Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)}\n" +
                                    $"New Sale Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value)}\n\n" +
                                    $"Please double-check. (විකුනුම් මිලේ ලොකු වෙනසක් සිදුවී ඇත.එය නැවත බලන්න.)",
                                    "Abnormal Price Change",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }

                            // check cost price is less than sale price
                            // cost = 10
                            // sale = 4
                            double costPrice1 = Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);
                            double salePrice1 = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                            string idItem = Convert.ToString(CashierGridView.Rows[i].Cells[0].Value);

                            DataTable wholesalePriceDt = new DataTable();
                            wholesalePriceDt = this.databaseClass.executeSqlCommand("SELECT Wholesale FROM iteminformation WHERE ITEM_ID=" + idItem);
                            double wholesalePrice = Convert.ToDouble(wholesalePriceDt.Rows[0][0].ToString());

                            if (salePrice1 < costPrice1)
                            {
                                MessageBox.Show(
                                   $"Cost Price is higher than sale price.\n" +

                                   $"So wholesale Price was added for that field.\nවිකුණන මිල කඩයට භාණ්ඩ ලැබෙන මිලට වඩා අඩුවී ඇත. එමනිසා එම ස්ථානයට තොග මිල ඇතුලත් කරන ලදි.",
                                   "Abnormal Price Change",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                                CashierGridView.Rows[i].Cells[4].Value = wholesalePrice;
                            }

                            // check whether bill price < sale price
                            double salePrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                            double billPrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);

                            if (billPrice3 < salePrice3)
                            {
                                MessageBox.Show($"Sale Price is higher than bill price.\n" +

                                                               $"So bill Price was added for that field.\nවිකුණන මිල භාණ්ඩයේ සදහන් මිලට වඩා වැඩිවී ඇත. එමනිසා එම ස්ථානයට භාණ්ඩයේ සදහන් මිල ඇතුලත් කරන ලදි.",
                                                               "Abnormal Price Change",
                                                               MessageBoxButtons.OK,
                                                               MessageBoxIcon.Warning);
                                CashierGridView.Rows[i].Cells[4].Value = billPrice3;
                            }
                        }
                        catch (Exception error)
                        {
                            ErrorLogger.Log(error, "CashierForm", nameof(ProcessCmdKey), GetCurrentFormData());
                            MessageBox.Show(error.Message);
                        }

                        if (RetailCheckBox.Checked == true)
                        {
                            CashierGridView.Rows[i].Cells[6].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[9].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[5].Value);

                        }
                        else
                        {
                            CashierGridView.Rows[i].Cells[6].Value = 0;
                        }

                        i++;

                    }

                }

                AddTotal();
                calculateChange();

                return true;

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void calculateChange()
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (ChargeTextBox.Text == "")
            {
                ChangeTextBox.Text = "";

            }
            else
            {
                if (CashierGridView.Rows.Count > 0)
                {
                    double charge = Convert.ToDouble(ChargeTextBox.Text);
                    double total = Convert.ToDouble(AmountTotalTextBox.Text);
                    double change = charge - total;

                    ChangeTextBox.Text = change.ToString();
                    if (charge >= total)
                    {
                        printButton.Enabled = true;
                        saveButton.Enabled = true;

                    }
                    else
                    {
                        printButton.Enabled = false;
                        saveButton.Enabled = false;
                    }
                }
                else
                {
                    ChangeTextBox.Clear();
                    ChargeTextBox.Clear();
                }
            }

        }

        private void ChargeTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (ChargeTextBox.Text == "")
            {
                ChangeTextBox.Text = "";

            }
            else
            {
                if (CashierGridView.Rows.Count > 0)
                {
                    try
                    {
                        double charge = Convert.ToDouble(ChargeTextBox.Text);
                        double total = Convert.ToDouble(AmountTotalTextBox.Text);
                        double change = charge - total;

                        ChangeTextBox.Text = change.ToString();
                        if (charge >= total)
                        {
                            printButton.Enabled = true;
                            saveButton.Enabled = true;

                        }
                        else
                        {
                            printButton.Enabled = false;
                            saveButton.Enabled = false;
                        }
                    }
                    catch (Exception error)
                    {
                        ErrorLogger.Log(error, "CashierForm", nameof(ChargeTextBox_TextChanged), GetCurrentFormData());
                        MessageBox.Show(error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select an item before entering charge\nදෙන මුදල දැමීමට පෙර භාණ්ඩ ඇතුලත් කර සිටින්න.");
                    ChargeTextBox.Clear();
                }
            }

        }

        private void ClearAllData()
        {
            BarcodeTextBox.Clear();
            discountTextBox.Clear();
            customerNameTextBox.Clear();
            ItemsComboBox.Text = "";
            // QuantityTextBox.Clear();
            CashierGridView.Rows.Clear();
            CashierGridView.Refresh();
            AmountTotalTextBox.Text = "0";
            discountTextBox.Text = "0";
            ChangeTextBox.Clear();
            ChargeTextBox.Clear();
            printButton.Enabled = false;
            saveButton.Enabled = false;
            //load id to id text
            DataTable idclear = new DataTable();
            idclear = this.databaseClass.executeSqlCommand("SELECT * FROM vpsPosReceipts");
            ReceiptIdTextBox.Text = clientPhrase + (idclear.Rows.Count + 1);
            BarcodeTextBox.Select();
            AddingNoOfItemsToTextBox();
            UserNameLogin.Text = userlogin.Rows[0][2].ToString();
        }

        int SaleId { get; set; }

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

        private void CheckOutButton_Click(object sender, EventArgs e)
        {
            try
            {
                InfoForm infoForm = new InfoForm();
                infoForm.Show();
                infoForm.Text = "Printing... Please wait.";

                makingOrderOfItems();
                bool success = InsertBillDetailToDatabase();
                if (!success)
                {
                    // Transaction failed, stop further execution
                    return;
                }
                int receiptId = SaleId;

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


                    if (RetailCheckBox.Checked == true)
                    {


                        //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                        // for small print , if pc 1 need it, cause x printer cant change size y itself
                        if (phrase == "B" && wantSmallBill == "yes")
                        {
                            CrystalReports.retailReceiptMain_pc1 retailReceipt = new CrystalReports.retailReceiptMain_pc1();
                            retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                            retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                            retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                        wholesaleReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

                        // for prevent maximum print job done error 
                        wholesaleReceipt.Close();
                        wholesaleReceipt.Dispose();
                    }
                }
                else if (userType == "Sub")
                {
                    itemsInReceiptDataTable = sortBillItemsForPrinting("en-US", itemsInReceiptDataTable, "itemName", "ඉංග්‍රීසි");

                    if (RetailCheckBox.Checked == true)
                    {


                        //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                        if (phrase == "B" && wantSmallBill == "yes")
                        {
                            CrystalReports.retailReceiptSub_pc1 retailReceipt = new CrystalReports.retailReceiptSub_pc1();
                            retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                            retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                            retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                        wholesaleReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

                        // for prevent maximum print job done error 
                        wholesaleReceipt.Close();
                        wholesaleReceipt.Dispose();
                    }
                }
                else if (languageDatatable.Rows[0][0].ToString() == "Sinhala")
                {
                    itemsInReceiptDataTable = sortBillItemsForPrinting("si-LK", itemsInReceiptDataTable, "sinhalaName", "සිංහල");

                    if (RetailCheckBox.Checked == true)
                    {


                        //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                        if (phrase == "B" && wantSmallBill == "yes")
                        {

                            CrystalReports.retailReceiptMain_pc1 retailReceipt = new CrystalReports.retailReceiptMain_pc1();
                            retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                            retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                            retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                        wholesaleReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);
                        // for prevent maximum print job done error 
                        wholesaleReceipt.Close();
                        wholesaleReceipt.Dispose();
                    }
                }
                else if (languageDatatable.Rows[0][0].ToString() == "English")
                {

                    itemsInReceiptDataTable = sortBillItemsForPrinting("en-US", itemsInReceiptDataTable, "itemName", "ඉංග්‍රීසි");

                    if (RetailCheckBox.Checked == true)
                    {


                        //F:\\documents\\c#\\VpsPosNewTest\\VpsPos\\bin\\x64\\Debug\\CrystalReports\\retailReceipt.rpt
                        if (phrase == "B" && wantSmallBill == "yes")
                        {
                            CrystalReports.retailReceiptSub_pc1 retailReceipt = new CrystalReports.retailReceiptSub_pc1();
                            retailReceipt.Database.Tables["initialData"].SetDataSource(initialReceiptDataDataTable);
                            retailReceipt.Database.Tables["items"].SetDataSource(itemsInReceiptDataTable);
                            retailReceipt.Database.Tables["buisnessInfo"].SetDataSource(buisnessInfoDatatable);
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                            retailReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);

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
                        wholesaleReceipt.PrintToPrinter(Convert.ToInt32(printCopiesTextBox.Text), true, 0, 0);
                        // for prevent maximum print job done error 
                        wholesaleReceipt.Close();
                        wholesaleReceipt.Dispose();
                    }
                }

                infoForm.Close();
                printCopiesTextBox.Text = "1";
            }
            catch (Exception error)
            {
                ErrorLogger.Log(error, "CashierForm", nameof(CheckOutButton_Click), GetCurrentFormData());
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // old function up tp 2025/08/26

        //private void InsertBillDetailToDatabase()
        //{

        //    try
        //    {
        //        int noOfItems = CashierGridView.RowCount;

        //        // calculating cost of Bill
        //        double costOfItems = 0;
        //        for (int inc = 0; inc < CashierGridView.RowCount; inc++)
        //        {
        //            costOfItems += Convert.ToDouble(CashierGridView.Rows[inc].Cells[11].Value);
        //        }

        //        //update quantity of items
        //        if (isFromBillsGrid)
        //        {
        //            string receiptId = receiptIdFromBillsGrid;

        //            for (int i = 0; i < CashierGridView.RowCount; i++)
        //            {
        //                string itemId = CashierGridView.Rows[i].Cells[0].Value.ToString();
        //                string quantity = CashierGridView.Rows[i].Cells[3].Value.ToString();

        //                DataTable itemDatatable = new DataTable();

        //                // checking this item is already taken in this receipt
        //                using (SqlConnection connection = new SqlConnection(conString))
        //                {
        //                    connection.Open();
        //                    string queryString = "SELECT quantity FROM vpsPosReceiptItems WHERE receiptId=@receiptId AND ITEM_ID=@itemId";
        //                    using (SqlCommand command = new SqlCommand(queryString, connection))
        //                    {
        //                        command.Parameters.AddWithValue("@receiptId", SqlDbType.Int).Value = receiptId;
        //                        command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
        //                        SqlDataAdapter adapter = new SqlDataAdapter(command);
        //                        adapter.Fill(itemDatatable);

        //                    }
        //                }

        //                // this item already included in receipt
        //                if (itemDatatable.Rows.Count > 0)
        //                {
        //                    float prevQuantity = float.Parse(itemDatatable.Rows[0][0].ToString());
        //                    float newQuantity = float.Parse(quantity);

        //                    float remainingQuantity = 0;
        //                    string queryString = "";

        //                    if (prevQuantity > newQuantity)
        //                    {
        //                        remainingQuantity = prevQuantity - newQuantity;
        //                        queryString = "UPDATE stock SET Quantity=Quantity + @remainingQuantity WHERE ITEM_ID=@itemId;";
        //                    }
        //                    else if (prevQuantity < newQuantity)
        //                    {
        //                        remainingQuantity = newQuantity - prevQuantity;
        //                        queryString = "UPDATE stock SET Quantity=Quantity - @remainingQuantity WHERE ITEM_ID=@itemId;";
        //                    }
        //                    else if (prevQuantity == newQuantity)
        //                    {
        //                        continue;
        //                    }

        //                    using (SqlConnection connection = new SqlConnection(conString))
        //                    {
        //                        connection.Open();
        //                        using (SqlCommand command = new SqlCommand(queryString, connection))
        //                        {
        //                            command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
        //                            command.Parameters.AddWithValue("@remainingQuantity", SqlDbType.Float).Value = remainingQuantity;
        //                            command.ExecuteNonQuery();
        //                        }
        //                    }

        //                }
        //                else
        //                {
        //                    // this item not included in receipt
        //                    using (SqlConnection connection = new SqlConnection(conString))
        //                    {
        //                        connection.Open();
        //                        string queryString = "UPDATE stock SET Quantity=Quantity-@quantity WHERE ITEM_ID=@itemId";
        //                        using (SqlCommand command = new SqlCommand(queryString, connection))
        //                        {
        //                            command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
        //                            command.Parameters.AddWithValue("@quantity", SqlDbType.Float).Value = quantity;
        //                            command.ExecuteNonQuery();
        //                        }
        //                    }
        //                }
        //            }

        //            // update stock if an item deleted
        //            using (SqlConnection connection = new SqlConnection(conString))
        //            {
        //                connection.Open();
        //                string queryString = "SELECT * FROM vpsPosReceiptItems WHERE receiptId=@receiptId";
        //                using (SqlCommand command = new SqlCommand(queryString, connection))
        //                {
        //                    command.Parameters.AddWithValue("@receiptId", SqlDbType.Int).Value = receiptId;
        //                    SqlDataAdapter adapter = new SqlDataAdapter(command);
        //                    DataTable itemsDatatable = new DataTable();
        //                    adapter.Fill(itemsDatatable);

        //                    bool founded = false;

        //                    for (int i = 0; i < itemsDatatable.Rows.Count; i++)
        //                    {
        //                        for (int y = 0; y < CashierGridView.Rows.Count; y++)
        //                        {
        //                            if (CashierGridView.Rows[y].Cells[0].Value.ToString() == itemsDatatable.Rows[i][1].ToString())
        //                            {
        //                                founded = true;
        //                                break;
        //                            }

        //                        }
        //                        if (founded)
        //                        {
        //                            founded = false;
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            using (SqlConnection connection1 = new SqlConnection(conString))
        //                            {
        //                                connection1.Open();
        //                                string queryString1 = "UPDATE stock SET Quantity=Quantity+@amount WHERE ITEM_ID =@itemId";
        //                                using (SqlCommand command1 = new SqlCommand(queryString1, connection1))
        //                                {
        //                                    command1.Parameters.AddWithValue("@amount", SqlDbType.Float).Value = itemsDatatable.Rows[i][4].ToString();
        //                                    command1.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemsDatatable.Rows[i][1].ToString();
        //                                    command1.ExecuteNonQuery();
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // new item stock change (new receipt)
        //            for (int i = 0; i < CashierGridView.RowCount; i++)
        //            {
        //                string itemId = CashierGridView.Rows[i].Cells[0].Value.ToString();
        //                string quantity = CashierGridView.Rows[i].Cells[3].Value.ToString();
        //                using (SqlConnection connection = new SqlConnection(conString))
        //                {
        //                    connection.Open();
        //                    string queryString = "UPDATE stock SET Quantity=Quantity-@quantity WHERE ITEM_ID=@itemId";
        //                    using (SqlCommand command = new SqlCommand(queryString, connection))
        //                    {
        //                        command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
        //                        command.Parameters.AddWithValue("@quantity", SqlDbType.Float).Value = quantity;
        //                        command.ExecuteNonQuery();
        //                    }
        //                }
        //            }
        //        }

        //        // reset variables required for change stock
        //        isFromBillsGrid = false;
        //        receiptIdFromBillsGrid = "";

        //        DataTable receiptIdDataTable = new DataTable();
        //        receiptIdDataTable = databaseClass.executeSqlCommand("SELECT * FROM vpsPosReceipts");
        //        DataTable receiptItemsDataTable = new DataTable();
        //        receiptItemsDataTable = databaseClass.executeSqlCommand("SELECT * FROM vpsPosReceiptItems");
        //        if (receiptIdDataTable.Rows.Count > 0)
        //        {
        //            bool check = false;
        //            int i = 0;
        //            SaleId = 0;
        //            int count = receiptIdDataTable.Rows.Count;
        //            while (i < count)
        //            {
        //                if (receiptIdDataTable.Rows[i][2].ToString() == ReceiptIdTextBox.Text)
        //                {
        //                    check = true;
        //                    SaleId = Convert.ToInt32(receiptIdDataTable.Rows[i][0].ToString());
        //                    break;
        //                }
        //                else
        //                {
        //                    check = false;
        //                    SaleId = 0;
        //                }
        //                i++;
        //            }

        //            //save sale detail when not  receipt box text already in database
        //            if (check == false)
        //            {
        //                //save sale information
        //                string sqlcommand = "INSERT INTO vpsPosReceipts (receiptDate,cashierFormId,customerName,employeeName,totalAmount,paidAmount,change,totalDiscount,noOfItems,totalCost) VALUES (GETDATE(),'" + ReceiptIdTextBox.Text + "','" + customerNameTextBox.Text + "','" + UserNameLogin.Text + "','" + AmountTotalTextBox.Text + "','" + ChargeTextBox.Text + "','" + ChangeTextBox.Text + "','" + discountTextBox.Text + "','" + noOfItems + "','" + costOfItems + "'); SELECT SCOPE_IDENTITY()";

        //                SaleId = Convert.ToInt32(this.databaseClass.executeSqlCommand(sqlcommand).Rows[0][0]);


        //            }
        //            else
        //            {
        //                //update sale detail when   receipt box text already in database
        //                //save sale information
        //                string sqlcommand = "UPDATE vpsPosReceipts SET receiptDate=GETDATE(),cashierFormId = '" + ReceiptIdTextBox.Text + "',customerName = '" + customerNameTextBox.Text + "',employeeName = '" + UserNameLogin.Text + "',totalAmount = '" + AmountTotalTextBox.Text + "',paidAmount ='" + ChargeTextBox.Text + "',change ='" + ChangeTextBox.Text + "',totalDiscount ='" + discountTextBox.Text + "',noOfItems='" + noOfItems + "',totalCost='" + costOfItems + "' WHERE receiptId = '" + SaleId + "'";

        //                this.databaseClass.executeSqlCommand(sqlcommand);


        //            }
        //            //save sale details
        //            bool checkDetail = false;
        //            int iDetail = 0;
        //            int countDetail = receiptItemsDataTable.Rows.Count;
        //            while (iDetail < countDetail)
        //            {
        //                if (receiptItemsDataTable.Rows[iDetail][0].ToString() == SaleId.ToString())
        //                {
        //                    checkDetail = true;

        //                    break;
        //                }
        //                else
        //                {
        //                    checkDetail = false;
        //                }
        //                iDetail++;
        //            }

        //            //save sale details when not  receipt box text already in database
        //            if (checkDetail == false)
        //            {

        //                //save sale details
        //                for (int iDetail1 = 0; iDetail1 < CashierGridView.Rows.Count; iDetail1++)
        //                {
        //                    string itemId = CashierGridView.Rows[iDetail1].Cells[0].Value.ToString();
        //                    string ItemNameDetail = CashierGridView.Rows[iDetail1].Cells[1].Value.ToString();
        //                    string SinhalaNameDetail = CashierGridView.Rows[iDetail1].Cells[2].Value.ToString();
        //                    string QuantityDetail = CashierGridView.Rows[iDetail1].Cells[3].Value.ToString();
        //                    string salesPriceDetail = CashierGridView.Rows[iDetail1].Cells[4].Value.ToString();
        //                    string salesPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[5].Value.ToString();
        //                    string discountDetail = CashierGridView.Rows[iDetail1].Cells[6].Value.ToString();

        //                    string billPriceDetail = CashierGridView.Rows[iDetail1].Cells[8].Value.ToString();
        //                    string billPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[9].Value.ToString();
        //                    string costPriceDetail = CashierGridView.Rows[iDetail1].Cells[10].Value.ToString();
        //                    string costPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[11].Value.ToString();
        //                    string orderOfitem = CashierGridView.Rows[iDetail1].Cells[12].Value.ToString();

        //                    string sqlcommandDetailDetail = "INSERT INTO vpsPosReceiptItems(receiptId,ITEM_ID,itemName,sinhalaName,quantity,salesPrice,salesPriceTotal,discount,billPrice,billPriceTotal,costPrice,costPriceTotal,orderOfItems) VALUES('" + SaleId + "','" + itemId + "','" + ItemNameDetail + "',N'" + SinhalaNameDetail + "','" + QuantityDetail + "','" + salesPriceDetail + "','" + salesPriceTotalDetail + "','" + discountDetail + "','" + billPriceDetail + "','" + billPriceTotalDetail + "','" + costPriceDetail + "','" + costPriceTotalDetail + "','" + orderOfitem + "')";

        //                    this.databaseClass.executeSqlCommand(sqlcommandDetailDetail);
        //                }

        //            }
        //            else
        //            {
        //                //update sale detail when   receipt box text already in database

        //                //delete rows
        //                string sqlcommandDelete = "DELETE FROM vpsPosReceiptItems WHERE receiptId = '" + SaleId + "'";

        //                this.databaseClass.executeSqlCommand(sqlcommandDelete);

        //                //save sale details

        //                for (int iDetail1 = 0; iDetail1 < CashierGridView.Rows.Count; iDetail1++)
        //                {
        //                    string itemId = CashierGridView.Rows[iDetail1].Cells[0].Value.ToString();
        //                    string ItemNameDetail = CashierGridView.Rows[iDetail1].Cells[1].Value.ToString();
        //                    string SinhalaNameDetail = CashierGridView.Rows[iDetail1].Cells[2].Value.ToString();
        //                    string QuantityDetail = CashierGridView.Rows[iDetail1].Cells[3].Value.ToString();
        //                    string salesPriceDetail = CashierGridView.Rows[iDetail1].Cells[4].Value.ToString();
        //                    string salesPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[5].Value.ToString();
        //                    string discountDetail = CashierGridView.Rows[iDetail1].Cells[6].Value.ToString();

        //                    string billPriceDetail = CashierGridView.Rows[iDetail1].Cells[8].Value.ToString();
        //                    string billPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[9].Value.ToString();
        //                    string costPriceDetail = CashierGridView.Rows[iDetail1].Cells[10].Value.ToString();
        //                    string costPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[11].Value.ToString();
        //                    string orderOfItem = CashierGridView.Rows[iDetail1].Cells[12].Value.ToString();

        //                    string sqlcommandDetailDetail = "INSERT INTO vpsPosReceiptItems(receiptId,ITEM_ID,itemName,sinhalaName,quantity,salesPrice,salesPriceTotal,discount,billPrice,billPriceTotal,costPrice,costPriceTotal,orderOfItems) VALUES('" + SaleId + "','" + itemId + "','" + ItemNameDetail + "',N'" + SinhalaNameDetail + "','" + QuantityDetail + "','" + salesPriceDetail + "','" + salesPriceTotalDetail + "','" + discountDetail + "','" + billPriceDetail + "','" + billPriceTotalDetail + "','" + costPriceDetail + "','" + costPriceTotalDetail + "','" + orderOfItem + "')";

        //                    this.databaseClass.executeSqlCommand(sqlcommandDetailDetail);
        //                }

        //            }
        //        }
        //        else
        //        {
        //            //save first sale information
        //            //save sale information
        //            string sqlcommand = "INSERT INTO vpsPosReceipts (receiptDate,cashierFormId,customerName,employeeName,totalAmount,paidAmount,change,totalDiscount,noOfItems,totalCost) VALUES (GETDATE(),'" + ReceiptIdTextBox.Text + "','" + customerNameTextBox.Text + "','" + UserNameLogin.Text + "','" + AmountTotalTextBox.Text + "','" + ChargeTextBox.Text + "','" + ChangeTextBox.Text + "','" + discountTextBox.Text + "','" + noOfItems + "','" + costOfItems + "'); SELECT SCOPE_IDENTITY()";

        //            SaleId = Convert.ToInt32(this.databaseClass.executeSqlCommand(sqlcommand).Rows[0][0]);

        //            //save sale details
        //            for (int iDetail1 = 0; iDetail1 < CashierGridView.Rows.Count; iDetail1++)
        //            {
        //                string itemId = CashierGridView.Rows[iDetail1].Cells[0].Value.ToString();
        //                string ItemNameDetail = CashierGridView.Rows[iDetail1].Cells[1].Value.ToString();
        //                string SinhalaNameDetail = CashierGridView.Rows[iDetail1].Cells[2].Value.ToString();
        //                string QuantityDetail = CashierGridView.Rows[iDetail1].Cells[3].Value.ToString();
        //                string salesPriceDetail = CashierGridView.Rows[iDetail1].Cells[4].Value.ToString();
        //                string salesPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[5].Value.ToString();
        //                string discountDetail = CashierGridView.Rows[iDetail1].Cells[6].Value.ToString();

        //                string billPriceDetail = CashierGridView.Rows[iDetail1].Cells[8].Value.ToString();
        //                string billPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[9].Value.ToString();
        //                string costPriceDetail = CashierGridView.Rows[iDetail1].Cells[10].Value.ToString();
        //                string costPriceTotalDetail = CashierGridView.Rows[iDetail1].Cells[11].Value.ToString();
        //                string orderOfItem = CashierGridView.Rows[iDetail1].Cells[12].Value.ToString();

        //                string sqlcommandDetailDetail = "INSERT INTO vpsPosReceiptItems (receiptId,ITEM_ID,itemName,sinhalaName,quantity,salesPrice,salesPriceTotal,discount,billPrice,billPriceTotal,costPrice,costPriceTotal,orderOfItems) VALUES('" + SaleId + "','" + itemId + "','" + ItemNameDetail + "',N'" + SinhalaNameDetail + "','" + QuantityDetail + "','" + salesPriceDetail + "','" + salesPriceTotalDetail + "','" + discountDetail + "','" + billPriceDetail + "','" + billPriceTotalDetail + "','" + costPriceDetail + "','" + costPriceTotalDetail + "','" + orderOfItem + "')";

        //                this.databaseClass.executeSqlCommand(sqlcommandDetailDetail);
        //            }

        //        }

        //        richTextBox1.Clear();
        //        richTextBox1.Text =
        //            $"*Last Bill Details*\n" +
        //            $"Total       : {AmountTotalTextBox.Text}\n" +
        //            $"Received : {ChargeTextBox.Text}\n" +
        //            $"Change   : {ChangeTextBox.Text}";
        //        ClearAllData();
        //    }
        //    catch (Exception error)
        //    {
        //        MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //}

        // new function
        private bool InsertBillDetailToDatabase()
        {
            using (SqlConnection connection = new SqlConnection(conString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    int noOfItems = CashierGridView.RowCount;

                    // Calculate total cost of items in the grid
                    double totalCost = 0;
                    foreach (DataGridViewRow row in CashierGridView.Rows)
                        totalCost += Convert.ToDouble(row.Cells[11].Value);

                    // ---------- STEP 1: Update stock ----------
                    if (isFromBillsGrid) // Editing existing receipt
                    {
                        string receiptId = receiptIdFromBillsGrid;

                        foreach (DataGridViewRow row in CashierGridView.Rows)
                        {
                            int itemId = Convert.ToInt32(row.Cells[0].Value);
                            float quantity = float.Parse(row.Cells[3].Value.ToString());

                            // Check previous quantity for this receipt
                            string queryCheckItem = "SELECT quantity FROM vpsPosReceiptItems WHERE receiptId=@receiptId AND ITEM_ID=@itemId";
                            var parametersCheck = new Dictionary<string, object>
                    {
                        {"@receiptId", receiptId},
                        {"@itemId", itemId}
                    };
                            DataTable dtItem = databaseClass.executeSqlCommand(queryCheckItem, connection, transaction, parametersCheck);

                            if (dtItem.Rows.Count > 0)
                            {
                                float prevQuantity = float.Parse(dtItem.Rows[0][0].ToString());
                                float diff = quantity - prevQuantity;

                                if (diff != 0) // Only adjust stock if quantity changed
                                {
                                    string queryUpdateStock = diff > 0
                                        ? "UPDATE stock SET Quantity=Quantity-@diff WHERE ITEM_ID=@itemId"
                                        : "UPDATE stock SET Quantity=Quantity+@diffAbs WHERE ITEM_ID=@itemId";

                                    var parametersUpdate = diff > 0
                                        ? new Dictionary<string, object> { { "@diff", diff }, { "@itemId", itemId } }
                                        : new Dictionary<string, object> { { "@diffAbs", Math.Abs(diff) }, { "@itemId", itemId } };

                                    databaseClass.executeSqlCommand(queryUpdateStock, connection, transaction, parametersUpdate);
                                }
                            }
                            else
                            {
                                // New item added to receipt, reduce stock
                                string queryUpdateStock = "UPDATE stock SET Quantity=Quantity-@quantity WHERE ITEM_ID=@itemId";
                                var parametersUpdate = new Dictionary<string, object> { { "@quantity", quantity }, { "@itemId", itemId } };
                                databaseClass.executeSqlCommand(queryUpdateStock, connection, transaction, parametersUpdate);
                            }
                        }

                        // Restore stock for items deleted from receipt
                        string queryOldItems = "SELECT ITEM_ID, quantity FROM vpsPosReceiptItems WHERE receiptId=@receiptId";
                        var parametersOld = new Dictionary<string, object> { { "@receiptId", receiptId } };
                        DataTable dtOldItems = databaseClass.executeSqlCommand(queryOldItems, connection, transaction, parametersOld);

                        foreach (DataRow oldRow in dtOldItems.Rows)
                        {
                            int oldItemId = Convert.ToInt32(oldRow["ITEM_ID"]);
                            float oldQty = float.Parse(oldRow["quantity"].ToString());
                            bool exists = false;

                            foreach (DataGridViewRow gvRow in CashierGridView.Rows)
                            {
                                if (Convert.ToInt32(gvRow.Cells[0].Value) == oldItemId)
                                {
                                    exists = true;
                                    break;
                                }
                            }

                            if (!exists) // Item was deleted, restore stock
                            {
                                string queryRestore = "UPDATE stock SET Quantity=Quantity+@qty WHERE ITEM_ID=@itemId";
                                var parametersRestore = new Dictionary<string, object> { { "@qty", oldQty }, { "@itemId", oldItemId } };
                                databaseClass.executeSqlCommand(queryRestore, connection, transaction, parametersRestore);
                            }
                        }
                    }
                    else // New receipt
                    {
                        foreach (DataGridViewRow row in CashierGridView.Rows)
                        {
                            int itemId = Convert.ToInt32(row.Cells[0].Value);
                            float quantity = float.Parse(row.Cells[3].Value.ToString());
                            string queryUpdateStock = "UPDATE stock SET Quantity=Quantity-@quantity WHERE ITEM_ID=@itemId";
                            var parametersUpdate = new Dictionary<string, object> { { "@quantity", quantity }, { "@itemId", itemId } };
                            databaseClass.executeSqlCommand(queryUpdateStock, connection, transaction, parametersUpdate);
                        }
                    }

                    // Reset temporary variables
                    isFromBillsGrid = false;
                    receiptIdFromBillsGrid = "";

                    // ---------- STEP 2: Save or Update receipt ----------
                    SaleId = 0;
                    string queryCheckReceipt = "SELECT receiptId,totalAmount,receiptDate FROM vpsPosReceipts WHERE cashierFormId=@cashierFormId";
                    var parametersCheckReceipt = new Dictionary<string, object> { { "@cashierFormId", ReceiptIdTextBox.Text } };
                    DataTable dtCheckReceipt = databaseClass.executeSqlCommand(queryCheckReceipt, connection, transaction, parametersCheckReceipt);

                    if (dtCheckReceipt.Rows.Count == 0)
                    {
                        // Insert new receipt
                        string queryInsertReceipt = @"INSERT INTO vpsPosReceipts
                    (receiptDate,cashierFormId,customerName,employeeName,totalAmount,paidAmount,change,totalDiscount,noOfItems,totalCost)
                    VALUES (GETDATE(),@cashierFormId,@customerName,@employeeName,@totalAmount,@paidAmount,@change,@totalDiscount,@noOfItems,@totalCost);
                    SELECT SCOPE_IDENTITY();";

                        var parametersInsert = new Dictionary<string, object>
                {
                    {"@cashierFormId", ReceiptIdTextBox.Text},
                    {"@customerName", customerNameTextBox.Text},
                    {"@employeeName", UserNameLogin.Text},
                    {"@totalAmount", AmountTotalTextBox.Text},
                    {"@paidAmount", ChargeTextBox.Text},
                    {"@change", ChangeTextBox.Text},
                    {"@totalDiscount", discountTextBox.Text},
                    {"@noOfItems", noOfItems},
                    {"@totalCost", totalCost}
                };

                        DataTable dtSaleId = databaseClass.executeSqlCommand(queryInsertReceipt, connection, transaction, parametersInsert);
                        SaleId = Convert.ToInt32(dtSaleId.Rows[0][0]);
                        Console.WriteLine("Inserted new receipt with ID: " + SaleId);
                    }
                    else
                    {
                        // Update existing receipt
                        SaleId = Convert.ToInt32(dtCheckReceipt.Rows[0]["receiptId"]);

                        // --- NEW ADVANCED SECURITY BLOCK ---

                        // 1. Get Values
                        double oldTotalDb = Convert.ToDouble(dtCheckReceipt.Rows[0]["totalAmount"]);
                        DateTime originalDateDb = Convert.ToDateTime(dtCheckReceipt.Rows[0]["receiptDate"]); // Catch Original Time
                        double newTotalUi = Convert.ToDouble(AmountTotalTextBox.Text);
                        double diff = oldTotalDb - newTotalUi;

                        // 2. Threshold Check (Huge Difference only)
                        // We only trigger if > 500 Rs drop OR if the bill is > 4 hours old (Ghost Return)
                        double hoursOld = (DateTime.Now - originalDateDb).TotalHours;
                        bool isHugeDrop = diff > 500;
                        bool isGhostReturn = hoursOld > 4 && diff > 100; // Even small edits on old bills are suspicious

                        if (isHugeDrop || isGhostReturn)
                        {
                            // 3. Capture the "Before" state immediately
                            string oldItemsStr = GetOldItemsFromDB(SaleId);

                            // 4. Force Signature
                            string cName, uName, reason;
                            DialogResult res = SecurityInputBox.Show(out cName, out uName, out reason); // Uses the class from previous response

                            if (res != DialogResult.OK) return false; // Stop Save

                            // 5. Log EVERYTHING
                            LogSuspiciousActivity(SaleId, oldTotalDb, newTotalUi, uName, cName, reason, oldItemsStr, originalDateDb);
                        }
                        // --- END SECURITY BLOCK ---

                        string queryUpdateReceipt = @"UPDATE vpsPosReceipts SET
                    receiptDate=GETDATE(), customerName=@customerName, employeeName=@employeeName,
                    totalAmount=@totalAmount, paidAmount=@paidAmount, change=@change,
                    totalDiscount=@totalDiscount, noOfItems=@noOfItems, totalCost=@totalCost
                    WHERE receiptId=@receiptId";

                        var parametersUpdateReceipt = new Dictionary<string, object>
                {
                    {"@customerName", customerNameTextBox.Text},
                    {"@employeeName", UserNameLogin.Text},
                    {"@totalAmount", AmountTotalTextBox.Text},
                    {"@paidAmount", ChargeTextBox.Text},
                    {"@change", ChangeTextBox.Text},
                    {"@totalDiscount", discountTextBox.Text},
                    {"@noOfItems", noOfItems},
                    {"@totalCost", totalCost},
                    {"@receiptId", SaleId}
                };

                        databaseClass.executeSqlCommand(queryUpdateReceipt, connection, transaction, parametersUpdateReceipt);
                        Console.WriteLine("Updated existing receipt ID: " + SaleId);

                        // Delete old items before inserting updated items
                        string queryDeleteItems = "DELETE FROM vpsPosReceiptItems WHERE receiptId=@receiptId";
                        var parametersDelete = new Dictionary<string, object> { { "@receiptId", SaleId } };
                        databaseClass.executeSqlCommand(queryDeleteItems, connection, transaction, parametersDelete);
                    }

                    // ---------- STEP 3: Insert receipt items ----------
                    foreach (DataGridViewRow row in CashierGridView.Rows)
                    {
                        string queryInsertItem = @"INSERT INTO vpsPosReceiptItems
                    (receiptId,ITEM_ID,itemName,sinhalaName,quantity,salesPrice,salesPriceTotal,
                     discount,billPrice,billPriceTotal,costPrice,costPriceTotal,orderOfItems)
                    VALUES (@receiptId,@itemId,@itemName,@sinhalaName,@quantity,@salesPrice,@salesPriceTotal,
                            @discount,@billPrice,@billPriceTotal,@costPrice,@costPriceTotal,@orderOfItems)";

                        var parametersItem = new Dictionary<string, object>
                {
                    {"@receiptId", SaleId},
                    {"@itemId", row.Cells[0].Value},
                    {"@itemName", row.Cells[1].Value},
                    {"@sinhalaName", row.Cells[2].Value},
                    {"@quantity", row.Cells[3].Value},
                    {"@salesPrice", row.Cells[4].Value},
                    {"@salesPriceTotal", row.Cells[5].Value},
                    {"@discount", row.Cells[6].Value},
                    {"@billPrice", row.Cells[8].Value},
                    {"@billPriceTotal", row.Cells[9].Value},
                    {"@costPrice", row.Cells[10].Value},
                    {"@costPriceTotal", row.Cells[11].Value},
                    {"@orderOfItems", row.Cells[12].Value}
                };

                        databaseClass.executeSqlCommand(queryInsertItem, connection, transaction, parametersItem);
                    }

                    // ---------- STEP 4: Commit transaction ----------
                    transaction.Commit();
                    Console.WriteLine("Transaction committed successfully.");

                    // Show last bill
                    richTextBox1.Clear();
                    richTextBox1.Text = $"*Last Bill Details*\nTotal       : {AmountTotalTextBox.Text}\nReceived : {ChargeTextBox.Text}\nChange   : {ChangeTextBox.Text}";

                    ClearAllData();
                    return true; // success
                }
                catch (Exception ex)
                {
                    // Rollback on error
                    transaction.Rollback();
                    ErrorLogger.Log(ex, "CashierForm", nameof(InsertBillDetailToDatabase), GetCurrentFormData());
                    Console.WriteLine("Transaction rolled back due to error.");
                    MessageBox.Show("Transaction rolledbacked. " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; // Failure
                }
            }
        }


        private void RefreshButton_Click(object sender, EventArgs e)
        {

            InfoForm infoForm = new InfoForm();
            infoForm.Show();

            //adding itemnames to  combobox
            string sqlcommand2 = "SELECT ItemName FROM iteminformation";

            conn.Open();

            SqlDataAdapter dtAdapter = new SqlDataAdapter(sqlcommand2, conn);
            dtSet.Clear();
            dtAdapter.Fill(dtSet);
            ItemsComboBox.Items.Clear();

            int count = dtSet.Tables[0].Rows.Count;
            int i = 0;
            while (i < count)
            {
                ItemsComboBox.Items.Add(dtSet.Tables[0].Rows[i]["ItemName"].ToString());
                i++;
            }

            conn.Close();

            infoForm.Close();

        }

        private void ItemsComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (e.KeyCode == Keys.Enter)
            {
                recheckBillType();
                // message will occur when user enter ' or " (This done cause errors when user mistakenly enter ')
                if (ItemsComboBox.Text.Contains("'") || ItemsComboBox.Text.Contains("\""))
                {
                    MessageBox.Show("Invalid character has been entered !\nවැරදි සලකුනක් ඇතුලත් කර ඇත.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    //getting id
                    DataTable scannedItem = new DataTable();
                    scannedItem = this.databaseClass.executeSqlCommand("SELECT * FROM iteminformation WHERE ItemName = '" + ItemsComboBox.Text + "'");

                    if (scannedItem.Rows.Count == 1)
                    {
                        addAnItemToGridView(scannedItem);
                    }

                }
                catch (Exception error)
                {
                    ErrorLogger.Log(error, "CashierForm", nameof(ItemsComboBox_KeyDown), GetCurrentFormData());
                    MessageBox.Show(error.ToString());

                }
            }

        }


        //display grid view when same barcode having  and insert  items
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string itemId = sameBarcodeGridView.CurrentRow.Cells[0].Value.ToString();
            //getting id
            DataTable scannedItem = new DataTable();
            scannedItem = this.databaseClass.executeSqlCommand("SELECT * FROM iteminformation WHERE ITEM_ID = '" + itemId + "'");

            addAnItemToGridView(scannedItem);

            sameBarcodeGridView.Rows.Clear();
            sameBarcodeGridView.Enabled = false;
            sameBarcodeGridView.Visible = false;
        }


        private void CloseButton_Click(object sender, EventArgs e)
        {

            if (CashierGridView.Rows.Count != 0 && isFromBillsGrid == false)
            {
                if (MessageBox.Show($"Grid has {CashierGridView.Rows.Count} items.\nIf you close this window , owner will be informed.\nDo you need to continue ?\n" +
                    $"වගුවේ භාණ්ඩ {CashierGridView.Rows.Count} ක් ඇත.\n" +
                    $"\nඔබ මේ පිටුව වැසුවොත් ඒ බව ප්‍රධානියාට දැනුම්දෙනු ඇත !\nඉදිරියට යාමට අවශ්‍යද ? ", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                if (MessageBox.Show("Do you really want to close this window ?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.Close();
                }
            }

        }

        private void CashierGridView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            int currentCellId = CashierGridView.CurrentCell.ColumnIndex;
            int currentRow = CashierGridView.CurrentRow.Index;

            CashierGridView.Rows[currentRow].Cells[currentCellId].Selected = true;
            CashierGridView.CurrentCell = CashierGridView.Rows[currentRow].Cells[currentCellId];
            CashierGridView.BeginEdit(true);

            // 2022/12/31 this fuction also created today

        }

        private void CashierGridView_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (e.KeyValue == (char)Keys.Enter)
            {
                e.Handled = true;

                if (CashierGridView.Rows.Count > 0)
                {
                    int i = 0;
                    int count = CashierGridView.Rows.Count;
                    while (i < count)
                    {
                        try
                        {
                            // fix unknownly entering '.' when adding quantities
                            if (CashierGridView.Rows[i].Cells[3].Value.ToString() == ".")
                            {
                                if (MessageBox.Show($"Row number{i}'s quantity has a value of '.' .Would you like to change it to 1 ?  (වැරදිමකින් ඇතුලත් වූ තිත වෙනිවට 1 ඉලක්කම එකතු කරන්නද?)", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                { CashierGridView.Rows[i].Cells[3].Value = 1; }
                            }
                        }
                        catch (Exception error)
                        {
                            ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_KeyUp), GetCurrentFormData());
                            MessageBox.Show(error.Message);
                        }


                        //cHange cell
                        // totalSalePriceUpdate
                        CashierGridView.Rows[i].Cells[3].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value);

                        CashierGridView.Rows[i].Cells[5].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        //totalBillPriceUpdate
                        CashierGridView.Rows[i].Cells[9].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);
                        //totalCostPriceUpdate
                        CashierGridView.Rows[i].Cells[11].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);

                        try
                        {
                            // logic to alert saleprice is hugely changed
                            // saleprice = 4
                            // billprice = 8
                            double diff = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                            double percentageChange = (diff / Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)) * 100;

                            // Alert if price drop > 50% OR absolute drop > 500
                            if ((percentageChange > 50 || diff > 500) && RetailCheckBox.Checked == true && RetailCheckBox.Checked)
                            {
                                MessageBox.Show(
                                    $"⚠️ Price drop is unusually high!\n\n" +
                                    $"Bill Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)}\n" +
                                    $"New Sale Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value)}\n\n" +
                                    $"Please double-check. (විකුනුම් මිලේ ලොකු වෙනසක් සිදුවී ඇත.එය නැවත බලන්න.)",
                                    "Abnormal Price Change",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                            }

                            // check cost price is less than sale price
                            // cost = 10
                            // sale = 4
                            double costPrice = Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);
                            double salePrice = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                            string idItem = Convert.ToString(CashierGridView.Rows[i].Cells[0].Value);

                            DataTable wholesalePriceDt = new DataTable();
                            wholesalePriceDt = this.databaseClass.executeSqlCommand("SELECT Wholesale FROM iteminformation WHERE ITEM_ID=" + idItem);
                            double wholesalePrice = Convert.ToDouble(wholesalePriceDt.Rows[0][0].ToString());

                            if (salePrice < costPrice)
                            {
                                MessageBox.Show(
                                   $"Cost Price is higher than sale price.\n" +

                                   $"So wholesale Price was added for that field.\nවිකුණන මිල කඩයට භාණ්ඩ ලැබෙන මිලට වඩා අඩුවී ඇත. එමනිසා එම ස්ථානයට තොග මිල ඇතුලත් කරන ලදි.",
                                   "Abnormal Price Change",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                                CashierGridView.Rows[i].Cells[4].Value = wholesalePrice;
                            }

                            // check whether bill price < sale price
                            double salePrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                            double billPrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);

                            if (billPrice3 < salePrice3)
                            {
                                MessageBox.Show($"Sale Price is higher than bill price.\n" +

                                                               $"So bill Price was added for that field.\nවිකුණන මිල භාණ්ඩයේ සදහන් මිලට වඩා වැඩිවී ඇත. එමනිසා එම ස්ථානයට භාණ්ඩයේ සදහන් මිල ඇතුලත් කරන ලදි.",
                                                               "Abnormal Price Change",
                                                               MessageBoxButtons.OK,
                                                               MessageBoxIcon.Warning);
                                CashierGridView.Rows[i].Cells[4].Value = billPrice3;
                            }
                        }
                        catch (Exception error)
                        {
                            ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_KeyUp), GetCurrentFormData());
                            MessageBox.Show(error.Message);
                        }

                        if (RetailCheckBox.Checked == true)
                        {
                            CashierGridView.Rows[i].Cells[6].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[9].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[5].Value);

                        }
                        else
                        {
                            CashierGridView.Rows[i].Cells[6].Value = 0;
                        }

                        i++;

                    }

                    int currentrow = CashierGridView.CurrentCell.RowIndex;
                    if (CashierGridView.CurrentCell.ColumnIndex == 3)
                    {

                        //select
                        CashierGridView.Rows[currentrow].Cells[4].Selected = true;
                        CashierGridView.CurrentCell = CashierGridView.Rows[currentrow].Cells[4];
                        CashierGridView.BeginEdit(true);

                    }
                    else
                    {
                        BarcodeTextBox.Focus();
                    }

                }
                AddTotal();
                calculateChange();
            }

        }

        private void BarcodeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (e.KeyData == (Keys.Add))
            {
                BarcodeTextBox.Clear();
                ChargeTextBox.Focus();
            }
        }

        // change checkbox's check state when enter key press
        private void RetailCheckBox_KeyUp(object sender, KeyEventArgs e)
        {
            // removed , cause message showing functionality of whther retail box checked or not , not working by this
        }

        // save details of receipt when save button clicked
        private void saveButton_Click(object sender, EventArgs e)
        {
            makingOrderOfItems();
            bool isSuccess = InsertBillDetailToDatabase();
            if (isSuccess) { MessageBox.Show("Receipt Details saved !", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }

        private void ChargeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {

                if (printButton.Enabled == true && ChargeTextBox.Text.Length < 8)
                {
                    printButton.PerformClick();
                }
            }
        }


        // making order of items
        public void makingOrderOfItems()
        {
            int rowsCountOfCashierGridView = CashierGridView.RowCount;
            int i = 0;
            while (i < rowsCountOfCashierGridView)
            {
                CashierGridView.Rows[i].Cells[12].Value = i;
                i++;
            }
        }

        public void AddingNoOfItemsToTextBox()
        {
            noOfItemsTextBox.Text = CashierGridView.RowCount.ToString();
        }

        // print bill when a user press enter key on customer name
        private void customerNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void recheckBillType()
        {
            // checking whether this bill is wholesale or retail(pradeep traders only)
            XmlDocument doc = new XmlDocument();
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("store");
            string storeName = nodes[0].InnerText;
            if (storeName == "pradeep")
            {
                if (CashierGridView.Rows.Count == 0)
                {
                    bool isChecked = RetailCheckBox.Checked;
                    if (isChecked == true)
                    {
                        DialogResult result = MessageBox.Show("මෙය සිල්ලර බිලක්ද?", "බිල් වර් ගය", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.No)
                        {
                            RetailCheckBox.Checked = false;
                        }
                    }
                    else if (isChecked == false)
                    {
                        DialogResult result = MessageBox.Show("මෙය තොග බිලක්ද?", "බිල් වර් ගය", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                        if (result == DialogResult.No)
                        {
                            RetailCheckBox.Checked = true;
                        }
                    }

                }
            }
        }

        private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {

            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (e.KeyCode == Keys.P && e.Control)
            {

                clCostPrice.Visible = true;
            }
            if (e.KeyCode == Keys.P && e.Control && e.Shift)
            {
                clCostPrice.Visible = false;
            }

            /*if (e.KeyCode == Keys.N && e.Control)
            {
                richTextBox1.Visible = true;
            }
            if (e.KeyCode == Keys.N && e.Control && e.Shift)
            {
                richTextBox1.Visible = false;
            }
            */

            // getting item info when barcode enters
            if (e.KeyCode == Keys.Enter)
            {
                if (BarcodeTextBox.Text != "")
                {

                    // message will occur when user enter ' or " (This done cause errors when user mistakenly enter ')
                    if (BarcodeTextBox.Text.Contains("'") || BarcodeTextBox.Text.Contains("\""))
                    {
                        MessageBox.Show("Invalid character has been entered !\nවැරදි සලකුනක් ඇතුලත් කර ඇත.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    recheckBillType();

                    //getting id
                    DataTable scannedItem = new DataTable();
                    scannedItem = this.databaseClass.executeSqlCommand("SELECT * FROM iteminformation WHERE Barcode = '" + BarcodeTextBox.Text + "'");

                    // check whether many items related to that barcode are consisted
                    if (scannedItem.Rows.Count > 1)
                    {

                        // show same barcode grid view
                        sameBarcodeGridView.Enabled = true;
                        sameBarcodeGridView.Visible = true;

                        sameBarcodeGridView.Focus();

                        DataTable samebarcode = new DataTable();
                        samebarcode = scannedItem;
                        int i = 0;
                        int count = samebarcode.Rows.Count;
                        while (i < count)
                        {
                            sameBarcodeGridView.Rows.Add(samebarcode.Rows[i][0], samebarcode.Rows[i][1], samebarcode.Rows[i][6], samebarcode.Rows[i][15], samebarcode.Rows[i][7], samebarcode.Rows[i][14]);

                            i++;
                        }
                    }
                    else if (scannedItem.Rows.Count == 1)
                    {
                        addAnItemToGridView(scannedItem);

                    }
                    else
                    {
                        MessageBox.Show("There is no item related to this barcode.\nමෙම බාකෝඩ් එකට අදාල භාණ්ඩ නැත.", "error");
                    }

                }

            }

        }

        // insert item when also samebarcode included when pressing enter
        private void sameBarcodeGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string itemId = sameBarcodeGridView.CurrentRow.Cells[0].Value.ToString();
                //getting id
                DataTable scannedItem = new DataTable();
                scannedItem = this.databaseClass.executeSqlCommand("SELECT * FROM iteminformation WHERE ITEM_ID = '" + itemId + "'");

                addAnItemToGridView(scannedItem);

                sameBarcodeGridView.Rows.Clear();
                sameBarcodeGridView.Enabled = false;
                sameBarcodeGridView.Visible = false;
            }
        }

        private void CashierGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            try
            {
                // if block for adding or removing ✓ mark in sinhala text
                if (CashierGridView.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
                {
                    bool isChecked = (bool)CashierGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    if (isChecked == true)
                    {
                        string oldName = CashierGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                        string newName = "✅   " + oldName;
                        CashierGridView.Rows[e.RowIndex].Cells[2].Value = newName;
                    }
                    else if (isChecked == false)
                    {
                        string oldName = CashierGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                        string newName = oldName.Replace("✅   ", "");
                        CashierGridView.Rows[e.RowIndex].Cells[2].Value = newName;

                    }
                }
            }
            catch (Exception ex)
            {
            }

            // fix error coming when '.' added in quantity text box when stock checking
            try
            {
                if (CashierGridView.Rows.Count != 0)
                {
                    // fix unknownly entering '.' when adding quantities
                    if (CashierGridView.Rows[e.RowIndex].Cells[3].Value.ToString() == ".")
                    {
                        if (MessageBox.Show($"Row number{e.RowIndex}'s quantity has a value of '.' .Would you like to change it to 1 ? \nවැරදීමකින් '.' ඇතුලත් වී ඇත.එය වෙනුවට 1 යොදන්නද?", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        { CashierGridView.Rows[e.RowIndex].Cells[3].Value = 1; }
                    }
                }
            }
            catch (Exception error)
            {
                ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_CellValueChanged), GetCurrentFormData());
                MessageBox.Show(error.Message);
            }

            if (CashierGridView.Rows.Count > 0)
            {
                string name = CashierGridView.Rows[e.RowIndex].Cells[1].Value.ToString();

                if (!isFromBillsGrid)
                {
                    float newQuantity = float.Parse(CashierGridView.Rows[e.RowIndex].Cells[3].Value.ToString());
                    int itemId = Convert.ToInt32(CashierGridView.Rows[e.RowIndex].Cells[0].Value);
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT Quantity FROM stock WHERE ITEM_ID=@itemId";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            if (float.Parse(dataTable.Rows[0][0].ToString()) < newQuantity)
                            {
                                MessageBox.Show($"The stock of {name} is not enough to set this quantity.So quantity changed to stock amount.\nමෙම භාණ්ඩයේ මෙපමන භාණ්ඩ සංක්‍යාවක් නොමැත.එමනිසා තොග ඇති ප්‍රමාණය ඇතුලත් කර ඇත.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                // Prevent the user from updating the quantity
                                CashierGridView.Rows[e.RowIndex].Cells[3].Value = dataTable.Rows[0][0].ToString();

                            }
                        }
                    }
                }
                else
                {
                    float newQuantity = float.Parse(CashierGridView.Rows[e.RowIndex].Cells[3].Value.ToString());
                    int itemId = Convert.ToInt32(CashierGridView.Rows[e.RowIndex].Cells[0].Value);

                    DataTable quantityFromStockDatatable = new DataTable();

                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT Quantity FROM stock WHERE ITEM_ID=@itemId";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(quantityFromStockDatatable);

                        }
                    }

                    DataTable quantityFromReceiptItemsDataTable = new DataTable();
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT quantity FROM vpsPosReceiptItems WHERE ITEM_ID=@itemId AND receiptId=@receiptId";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@itemId", SqlDbType.Int).Value = itemId;
                            command.Parameters.AddWithValue("@receiptId", SqlDbType.Int).Value = receiptIdFromBillsGrid;
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(quantityFromReceiptItemsDataTable);

                        }
                    }

                    float stock = 0;
                    if (quantityFromReceiptItemsDataTable.Rows.Count > 0)
                    {
                        stock = float.Parse(quantityFromStockDatatable.Rows[0][0].ToString()) + float.Parse(quantityFromReceiptItemsDataTable.Rows[0][0].ToString());
                    }
                    else
                    {
                        stock = float.Parse(quantityFromStockDatatable.Rows[0][0].ToString());
                    }

                    if (stock < newQuantity)
                    {
                        MessageBox.Show($"The stock of {name} is not enough to set this quantity.So quantity changed to stock amount.\nමෙම භාණ්ඩයේ මෙපමන භාණ්ඩ සංක්‍යාවක් නොමැත.එමනිසා තොග ඇති ප්‍රමාණය ඇතුලත් කර ඇත.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Prevent the user from updating the quantity
                        CashierGridView.Rows[e.RowIndex].Cells[3].Value = stock;

                    }
                }
            }

        }

        private void printCopiesTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                printButton.PerformClick();
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void CashierGridView_CellLeave(object sender, DataGridViewCellEventArgs e)
        {

        }

        // if quantity added and click something without pressing enter, grid view details will update
        private void CashierGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (CashierGridView.Rows.Count > 0)
            {
                int i = 0;
                int count = CashierGridView.Rows.Count;
                while (i < count)
                {
                    try
                    {
                        // fix unknownly entering '.' when adding quantities
                        if (CashierGridView.Rows[i].Cells[3].Value.ToString() == ".")
                        {
                            if (MessageBox.Show($"Row number{i}'s quantity has a value of '.' .Would you like to change it to 1 ? (වැරදිමකින් ඇතුලත් වූ තිත වෙනිවට 1 ඉලක්කම එකතු කරන්නද?)", "Ask", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            { CashierGridView.Rows[i].Cells[3].Value = 1; }
                        }
                    }
                    catch (Exception error)
                    {
                        ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_CellEndEdit), GetCurrentFormData());
                        MessageBox.Show(error.Message);
                    }


                    //cHange cell
                    // totalSalePriceUpdate
                    CashierGridView.Rows[i].Cells[3].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value);

                    CashierGridView.Rows[i].Cells[5].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                    //totalBillPriceUpdate
                    CashierGridView.Rows[i].Cells[9].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);
                    //totalCostPriceUpdate
                    CashierGridView.Rows[i].Cells[11].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[3].Value) * Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);

                    try
                    {
                        // logic to alert saleprice is hugely changed
                        // saleprice = 4
                        // billprice = 8
                        double diff = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        double percentageChange = (diff / Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)) * 100;

                        // Alert if price drop > 50% OR absolute drop > 500
                        if ((percentageChange > 50 || diff > 500) && RetailCheckBox.Checked == true && RetailCheckBox.Checked)
                        {
                            MessageBox.Show(
                                $"⚠️ Price drop is unusually high!\n\n" +
                                $"Bill Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value)}\n" +
                                $"New Sale Price: {Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value)}\n\n" +
                                $"Please double-check. (විකුනුම් මිලේ ලොකු වෙනසක් සිදුවී ඇත.එය නැවත බලන්න.)",
                                "Abnormal Price Change",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }

                        // check cost price is less than sale price
                        // cost = 10
                        // sale = 4
                        double costPrice = Convert.ToDouble(CashierGridView.Rows[i].Cells[10].Value);
                        double salePrice = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        string idItem = Convert.ToString(CashierGridView.Rows[i].Cells[0].Value);

                        DataTable wholesalePriceDt = new DataTable();
                        wholesalePriceDt = this.databaseClass.executeSqlCommand("SELECT Wholesale FROM iteminformation WHERE ITEM_ID=" + idItem);
                        double wholesalePrice = Convert.ToDouble(wholesalePriceDt.Rows[0][0].ToString());

                        if (salePrice < costPrice)
                        {
                            MessageBox.Show(
                               $"Cost Price is higher than sale price.\n" +

                               $"So wholesale Price was added for that field.\nවිකුණන මිල කඩයට භාණ්ඩ ලැබෙන මිලට වඩා අඩුවී ඇත. එමනිසා එම ස්ථානයට තොග මිල ඇතුලත් කරන ලදි.",
                               "Abnormal Price Change",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                            CashierGridView.Rows[i].Cells[4].Value = wholesalePrice;
                        }

                        // check whether bill price < sale price
                        double salePrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[4].Value);
                        double billPrice3 = Convert.ToDouble(CashierGridView.Rows[i].Cells[8].Value);

                        if (billPrice3 < salePrice3)
                        {
                            MessageBox.Show($"Sale Price is higher than bill price.\n" +

                                                           $"So bill Price was added for that field.\nවිකුණන මිල භාණ්ඩයේ සදහන් මිලට වඩා වැඩිවී ඇත. එමනිසා එම ස්ථානයට භාණ්ඩයේ සදහන් මිල ඇතුලත් කරන ලදි.",
                                                           "Abnormal Price Change",
                                                           MessageBoxButtons.OK,
                                                           MessageBoxIcon.Warning);
                            CashierGridView.Rows[i].Cells[4].Value = billPrice3;
                        }
                    }
                    catch (Exception error)
                    {
                        ErrorLogger.Log(error, "CashierForm", nameof(CashierGridView_CellEndEdit), GetCurrentFormData());
                        MessageBox.Show(error.Message);
                    }

                    if (RetailCheckBox.Checked == true)
                    {
                        CashierGridView.Rows[i].Cells[6].Value = Convert.ToDouble(CashierGridView.Rows[i].Cells[9].Value) - Convert.ToDouble(CashierGridView.Rows[i].Cells[5].Value);

                    }
                    else
                    {
                        CashierGridView.Rows[i].Cells[6].Value = 0;
                    }

                    i++;

                }
            }
            AddTotal();
            calculateChange();

        }

        private void CashierGridView_KeyDown(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            try
            {
                // when enter key press checkbox will checked 
                if (CashierGridView.SelectedCells[0].ColumnIndex == 7)
                {
                    int idR = CashierGridView.CurrentCell.RowIndex;
                    int idC = CashierGridView.CurrentCell.ColumnIndex;

                    e.Handled = true;

                    bool isChecked = (bool)CashierGridView.SelectedCells[0].Value;
                    if (isChecked == true)
                    {
                        CashierGridView.SelectedCells[0].Value = false;
                    }
                    else if (isChecked == false)
                    {
                        CashierGridView.SelectedCells[0].Value = true;
                    }
                    CashierGridView.CurrentCell = CashierGridView[idC, idR];

                }

            }
            catch (Exception ex)
            {
                ErrorLogger.Log(ex, "CashierForm", nameof(CashierGridView_KeyDown), GetCurrentFormData());
            }

        }

        private void ChargeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and control keys (like Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // block the key press
            }
        }

    }

}
