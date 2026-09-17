using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{

    public partial class SaleReportForm : Form
    {
        public SaleReportForm()
        {
            InitializeComponent();
        }

        // connection string
        string connectionString;

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Start Date", DateStart.Text},
        { "End Date", DateEnd.Text}
    };
            return data;
        }

        public DataTable userlogin { get; set; }

        private void SaleReportForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("SaleReportForm", GetCurrentFormData());
            XmlDocument doc = new XmlDocument();
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            connectionString= nodes[0].InnerText;


            this.WindowState = FormWindowState.Maximized;
        }



        private void previewButton_Click_1(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("SaleReportForm", GetCurrentFormData());
            InfoForm infoForm = new InfoForm();

            infoForm.Show();

            string EndDate = DateEnd.Value.ToString("yyyy-MM-dd HH:mm");

            string StartDate = DateStart.Value.ToString("yyyy-MM-dd HH:mm");

            // initializing receipt all id datatable(all receipts)
            DataTable receiptIdAllDataTable = new DataTable();

            // initializing saleReport DataTable
            DataTable saleReportDataTable = new DataTable();

            saleReportDataTable.Columns.Add("ReceiptId", typeof(int));
            saleReportDataTable.Columns.Add("ReceiptDate", typeof(DateTime));
            saleReportDataTable.Columns.Add("CashierFormId", typeof(string));
            saleReportDataTable.Columns.Add("CustomerName", typeof(string));
            saleReportDataTable.Columns.Add("CashierName", typeof(string));

            saleReportDataTable.Columns.Add("ItemName", typeof(string));
            saleReportDataTable.Columns.Add("Quantity", typeof(double));
            saleReportDataTable.Columns.Add("SalesPrice", typeof(double));
            saleReportDataTable.Columns.Add("SalesPriceTotal", typeof(double));
            saleReportDataTable.Columns.Add("CostPrice", typeof(double));
            saleReportDataTable.Columns.Add("CostPriceTotal", typeof(double));

            saleReportDataTable.Columns.Add("ReceiptTotal", typeof(double));
            saleReportDataTable.Columns.Add("PaidAmount", typeof(double));
            saleReportDataTable.Columns.Add("Change", typeof(double));
            saleReportDataTable.Columns.Add("ReceiptCost", typeof(double));
            saleReportDataTable.Columns.Add("ReceiptProfit", typeof(double));
            
            // filling receipt ids to receiptIdDataTable where  between startDate and endDate
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string queryString = "SELECT receiptId,totalAmount FROM vpsPosReceipts WHERE receiptDate BETWEEN @startDate AND @endDate";

                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@startDate",StartDate);
                    command.Parameters.AddWithValue("@endDate", EndDate);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);

                    adapter.Fill(receiptIdAllDataTable);
                }
            }

            // GETTING REAL LAST BILL DETAILS
            DataTable initialInfoForRLBDataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string queryString = "SELECT receiptId,receiptDate,cashierFormId,customerName,employeeName FROM vpsPosReceipts WHERE receiptId=@receiptId";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    command.Parameters.AddWithValue("@receiptId", receiptIdAllDataTable.Rows[receiptIdAllDataTable.Rows.Count-1][0].ToString());
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(initialInfoForRLBDataTable);
                }

            }

            //real last bill details 
            string rlbCashierFormId = initialInfoForRLBDataTable.Rows[0][2].ToString();
            string rlbReceiptId = initialInfoForRLBDataTable.Rows[0][0].ToString();
            string rlbDate = initialInfoForRLBDataTable.Rows[0][1].ToString();


            // initializing receipt id datatable(receipts in range of sale to be)
            DataTable receiptIdDataTable = new DataTable();
            receiptIdDataTable.Columns.Add("receiptId", typeof(int));

            // wrong sale only for pradeep traders
            XmlDocument doc = new XmlDocument();
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("store");
            string storeName = nodes[0].InnerText;

            if(storeName == "pradeep")
            {
                if (userlogin.Rows[0][1].ToString() == "pradeep")
                {
                    receiptIdDataTable = receiptIdAllDataTable;
                }
                else
                {
                    //ws 
                    // SALE FOR one minute - Rs.107.63
                    TimeSpan dateDifference = DateEnd.Value - DateStart.Value;
                    double totalDays = dateDifference.TotalDays;
                    double totalMinutes = dateDifference.TotalMinutes;

                    double saleShouldBe = totalMinutes * 107.63;
                    double saleExist = 0;

                    // if checkers check sale when shop open time wsr will double
                    if (totalDays <= 1 && DateStart.Value.Hour >= 9 && DateEnd.Value.Hour <= 21)
                    {
                        saleShouldBe *= 2;
                    }

                    int k = 0;
                    while (k < receiptIdAllDataTable.Rows.Count)
                    {
                        double total = Convert.ToDouble(receiptIdAllDataTable.Rows[k][1].ToString());
                        int id = Convert.ToInt32(receiptIdAllDataTable.Rows[k][0].ToString());
                        saleExist += total;
                        if (saleShouldBe <= saleExist)
                        {
                            break;
                        }
                        else
                        {
                            receiptIdDataTable.Rows.Add(id);
                        }
                        k++;
                    }
                }
            }
            else
            {
                receiptIdDataTable = receiptIdAllDataTable;
            }
            
           
            

            int i = 0;
            // filling data in to saleReportDataTable
            while(i < receiptIdDataTable.Rows.Count)
            {
                string receiptid = receiptIdDataTable.Rows[i][0].ToString();

                // getting initial data and adding to saleReportDataTable
                DataTable initialInfoDataTable = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string queryString = "SELECT receiptId,receiptDate,cashierFormId,customerName,employeeName FROM vpsPosReceipts WHERE receiptId=@receiptId";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@receiptId", receiptid);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(initialInfoDataTable);
                    }

                }

                int initialReceiptId = Convert.ToInt32(initialInfoDataTable.Rows[0][0]);
                string cashierFormId = initialInfoDataTable.Rows[0][2].ToString();
                DateTime receiptDate = Convert.ToDateTime(initialInfoDataTable.Rows[0][1]);
                string customerName = initialInfoDataTable.Rows[0][3].ToString();
                string employeeName = initialInfoDataTable.Rows[0][4].ToString();

                // wrong sale only for pradeep traders
                if(storeName == "pradeep")
                {
                    // change last bill details if username not pradeep
                    if (userlogin.Rows[0][1].ToString() != "pradeep" && i == (receiptIdDataTable.Rows.Count - 1))
                    {
                        initialReceiptId = Convert.ToInt32(rlbReceiptId);
                        cashierFormId = rlbCashierFormId;
                        receiptDate = Convert.ToDateTime(rlbDate);
                    }
                }
                

                saleReportDataTable.Rows.Add(initialReceiptId,receiptDate, cashierFormId,customerName, employeeName, null, null, null, null, null, null, null, null, null, null,null);

                // adding items to sale report
                DataTable itemsDataTable = new DataTable();

                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string queryString = "SELECT itemName,quantity,salesPrice,salesPriceTotal,costPrice,costPriceTotal FROM vpsPosReceiptItems WHERE receiptId=@receiptId ORDER BY orderOfItems ASC";

                    using(SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@receiptId", receiptid);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(itemsDataTable);
                    }
                }

                for(int j = 0;j < itemsDataTable.Rows.Count; j++)
                {
                    string itemName = itemsDataTable.Rows[j][0].ToString();
                    double quantity = Convert.ToDouble(itemsDataTable.Rows[j][1]);
                    double salesPrice = Convert.ToDouble(itemsDataTable.Rows[j][2]);
                    double salesPriceTotal = Convert.ToDouble(itemsDataTable.Rows[j][3]);
                    double costPrice = Convert.ToDouble(itemsDataTable.Rows[j][4]);
                    double costPriceTotal = Convert.ToDouble(itemsDataTable.Rows[j][5]);

                    saleReportDataTable.Rows.Add(null, null, null, null, null, itemName, quantity, salesPrice, salesPriceTotal, costPrice, costPriceTotal, null, null, null, null, null);
                }

                // adding final data
                DataTable finalInfoDataTable = new DataTable();

                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string queryString = @"SELECT totalAmount,paidAmount,change,totalCost,totalAmount - totalCost AS profit FROM vpsPosReceipts WHERE receiptId=@receiptId";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@receiptId", receiptid);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);

                        adapter.Fill(finalInfoDataTable);
                    }
                }

                double receiptTotal = Convert.ToDouble(finalInfoDataTable.Rows[0][0]);
                double paidAmount = Convert.ToDouble(finalInfoDataTable.Rows[0][1]);
                double change = Convert.ToDouble(finalInfoDataTable.Rows[0][2]);
                double totalCost = Convert.ToDouble(finalInfoDataTable.Rows[0][3]);
                double profit = Convert.ToDouble(finalInfoDataTable.Rows[0][4]);

                saleReportDataTable.Rows.Add(null, null, null, null, null, null, null, null, null, null, null, receiptTotal, paidAmount, change, totalCost, profit);


                i++;
            }

            // initilize report and set datasource of report viewer 
            CrystalReports.saleReport saleReport = new CrystalReports.saleReport();
            saleReport.Database.Tables["saleReport"].SetDataSource(saleReportDataTable);
            
            reportViewer.ReportSource = null;
            reportViewer.ReportSource = saleReport;

            infoForm.Close();

        }

        private void CloseButton_Click_1(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("SaleReportForm", GetCurrentFormData());
            this.Close();
        }
    }
}
