using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class LoginForm : Form
    {
        private DatabaseClass myDatabase = new DatabaseClass();

        XmlDocument doc = new XmlDocument();
        string conString = "";

        public LoginForm()
        {
            InitializeComponent();
        }

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "User Name", UsernameTextBox.Text},
        { "Password", PasswordTextBox.Text}
    };

            return data;
        }


        private void LoginButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("LoginForm", GetCurrentFormData());
            if (UsernameTextBox.Text == "" && PasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter username and password.", "Warning");
            }
            else if (UsernameTextBox.Text == "")
            {
                MessageBox.Show("Please enter username.", "Warning");
            }
            else if (PasswordTextBox.Text == "")
            {
                MessageBox.Show("Please enter password.", "Warning");
            }
            else
            {

                string username = UsernameTextBox.Text.Trim();
                string password = PasswordTextBox.Text.Trim();
                DataTable loginTable = new DataTable();

                byte[] salt;
                byte[] hash;

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosUsers WHERE userName=@username";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@username", SqlDbType.VarChar).Value = username;
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            salt = (byte[])reader["salt"];
                            hash = (byte[])reader["hash"];

                            reader.Close();
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(loginTable);
                        }
                        else
                        {
                            MessageBox.Show("Username or Password is incorrect !", "warning");
                            return;
                        }

                    }
                }

                byte[] enteredHash;
                using (var sha256 = SHA256.Create())
                    enteredHash = sha256.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray());

                if (enteredHash.SequenceEqual(hash))
                {
                    MessageBox.Show("Login Successfull !", "info");
                    MainForm mainForm = new MainForm();
                    mainForm.userlogin = loginTable;
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Username or Password is incorrect !", "warning");
                }

            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("LoginForm", GetCurrentFormData());
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;

            try
            {
                // creating datatables and new user at first when needed
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = @"IF OBJECT_ID('dbo.vpsPosReceipts', 'U') IS NULL
begin
CREATE TABLE vpsPosReceipts (
  receiptId INTEGER PRIMARY KEY IDENTITY(1,1) NOT NULL,
  receiptDate DATETIME NOT NULL,
  cashierFormId VARCHAR(255) NOT NULL,
  customerName VARCHAR(255) NOT NULL,
  employeeName VARCHAR(255) NOT NULL,
  totalAmount float NOT NULL,
  paidAmount float NOT NULL,
  change float NOT NULL,
  totalDiscount float NOT NULL,
  noOfItems INTEGER NOT NULL,
  totalCost float NOT NULL		
);
end

/* use this for suspicious bill save*/
IF OBJECT_ID('dbo.SuspiciousBills', 'U') IS NULL
begin
CREATE TABLE SuspiciousBills (
    Id INT IDENTITY PRIMARY KEY,
    ReceiptId NVARCHAR(50),
    CashierName NVARCHAR(100),
    PastItems NVARCHAR(MAX),
    NewItems NVARCHAR(MAX),
    OriginalDate DATETIME,
    Reason NVARCHAR(250),
    CustomerName NVARCHAR(100),
    DateCreated DATETIME,
    SuspiciousType NVARCHAR(100),
    OldTotal DECIMAL(18,2),
    NewTotal DECIMAL(18,2)
);
end

IF OBJECT_ID('dbo.SuspiciousBillItems', 'U') IS NULL
begin
CREATE TABLE SuspiciousBillItems (
    Id INT IDENTITY PRIMARY KEY,
    SuspiciousBillId INT FOREIGN KEY REFERENCES SuspiciousBills(Id),
    IsPrevious BIT, -- 1 for previous items, 0 for new
    ItemId INT,
    ItemName NVARCHAR(100),
    SinhalaName NVARCHAR(100),
    Quantity FLOAT,
    SalesPrice FLOAT,
    Total FLOAT,
    Discount FLOAT,
    Mark BIT,
    BillPrice FLOAT,
    TotalBillPrice FLOAT,
    CostPrice FLOAT,
    TotalCostPrice FLOAT,
    OrderNo INT
);
end

IF OBJECT_ID('dbo.vpsPosReceiptItems', 'U') IS NULL
begin
CREATE TABLE vpsPosReceiptItems (
  receiptId INTEGER NOT NULL,
  ITEM_ID INTEGER NOT NULL,
  itemName VARCHAR(255) NOT NULL,
  sinhalaName NVARCHAR (255) NOT NULL,
  quantity float NOT NULL,
  salesPrice float NOT NULL,
  salesPriceTotal float NOT NULL,
  discount float NOT NULL,
  billPrice float NOT NULL,
  billPriceTotal float NOT NULL,
  costPrice float NOT NULL,
  costPriceTotal float NOT NULL,
  orderOfItems INTEGER NOT NULL,
  PRIMARY KEY (receiptId, ITEM_ID),
  FOREIGN KEY (receiptId) REFERENCES vpsPosReceipts(receiptId),
  FOREIGN KEY (ITEM_ID) REFERENCES iteminformation(ITEM_ID)
);
end

IF OBJECT_ID('dbo.vpsPosCustomers', 'U') IS NULL
begin
CREATE TABLE vpsPosCustomers(
    id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    name NVARCHAR(255) NOT NULL,
	phoneNumber VARCHAR(255),
	email VARCHAR(255),
	address NVARCHAR(255),
	age VARCHAR(255),
    debt float NOT NULL
);
end

IF OBJECT_ID('dbo.vpsPosDebtTransactions', 'U') IS NULL
begin
CREATE TABLE vpsPosDebtTransactions (
    id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
    customer_id INT NOT NULL,
    date DATETIME NOT NULL,
    type VARCHAR(255) NOT NULL,
    amount float NOT NULL,
    current_debt float NOT NULL,
    FOREIGN KEY (customer_id) REFERENCES vpsPosCustomers(id)
);
end

IF OBJECT_ID('dbo.vpsPosEmployees', 'U') IS NULL
begin
CREATE TABLE vpsPosEmployees (
  employeeId INTEGER PRIMARY KEY IDENTITY(1,1) NOT NULL,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(255) NOT NULL,
  address VARCHAR(255) NOT NULL
);
end

IF OBJECT_ID('dbo.vpsPosBuisnessInfo', 'U') IS NULL
begin
CREATE TABLE vpsPosBuisnessInfo (
  name NVARCHAR(255) NOT NULL,
  address NVARCHAR(255) NOT NULL,
  firstPhoneNumber VARCHAR(255) NOT NULL,
  secondPhoneNumber VARCHAR(255) NULL,
  email VARCHAR(255) NULL,
  webaddress VARCHAR(255) NULL
  );
end

IF OBJECT_ID('dbo.vpsPosCashierFormMessage', 'U') IS NULL
begin
CREATE TABLE vpsPosCashierFormMessage (
  message NVARCHAR(255) NOT NULL
  );
end

IF OBJECT_ID('dbo.vpsPosInfoHistory', 'U') IS NULL
begin
CREATE TABLE vpsPosInfoHistory (
  id INTEGER PRIMARY KEY IDENTITY(1,1) NOT NULL,
  date DATETIME NOT NULL,
  itemId INTEGER NOT NULL,
  itemName NVARCHAR(255) NOT NULL,
  barcode varchar(255) NULL,
  costPrice float NOT NULL,
  wholesalePrice float NULL,
  salesPrice float NOT NULL,
  billPrice float NOT NULL,
  quantity float NOT NULL,
  FOREIGN KEY (itemId) REFERENCES iteminformation(ITEM_ID)
);
end

IF OBJECT_ID('dbo.vpsPosUsers', 'U') IS NULL
begin
CREATE TABLE vpsPosUsers (
	userId INTEGER PRIMARY KEY IDENTITY(1,1) NOT NULL,
  userName VARCHAR(255) NOT NULL,
  fullName VARCHAR(255) NOT NULL,
  regDate DATETIME NOT NULL,
  salt BINARY(16) NOT NULL,
  hash BINARY(32) NOT NULL
);

end

IF OBJECT_ID('dbo.vpsPosLanguage', 'U') IS NULL
begin
CREATE TABLE vpsPosLanguage (
  language varchar(255) NOT NULL
);
end

/* [] use for avoid error when using column names with '/' */
IF OBJECT_ID('dbo.vpsPosDailyTransactions', 'U') IS NULL
begin
CREATE TABLE vpsPosDailyTransactions (
  Id INTEGER PRIMARY KEY IDENTITY(1,1) NOT NULL,
  Date DATETIME NOT NULL,  
  [Customer/Supplier_Name] NVARCHAR(255) NOT NULL,
  Type VARCHAR(255) NOT NULL,
  [Amount/Items/Note] NVARCHAR(255) NOT NULL,
  Is_Transaction_Ended VARCHAR(255) NOT NULL
);

end

IF OBJECT_ID('dbo.cacheLastBillItemsPc1', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[cacheLastBillItemsPc1](
        [ItemId] [int] NOT NULL,
        [ItemName] [varchar](max) NOT NULL,
        [SinhalaName] [nvarchar](max) NOT NULL,
        [Quantity] [float] NOT NULL,
        [SalesPrice] [float] NOT NULL,
        [Total] [float] NOT NULL,
        [discount] [float] NOT NULL,
        [Mark] [bit] NOT NULL,
        [BillPrice] [float] NOT NULL,
        [TotalBillPrice] [float] NOT NULL,
        [CostPrice] [float] NOT NULL,
        [TotalCostPrice] [float] NOT NULL,
        [OrderNo] [int] NULL,
        [Id] [int] IDENTITY(1,1) NOT NULL,
        CONSTRAINT [PK_cacheLastBillItemsPc1] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )
        WITH (
            PAD_INDEX = OFF, 
            STATISTICS_NORECOMPUTE = OFF, 
            IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS = ON, 
            ALLOW_PAGE_LOCKS = ON
        ) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END

IF OBJECT_ID('dbo.cacheLastBillItemsPc2', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[cacheLastBillItemsPc2](
        [ItemId] [int] NOT NULL,
        [ItemName] [varchar](max) NOT NULL,
        [SinhalaName] [nvarchar](max) NOT NULL,
        [Quantity] [float] NOT NULL,
        [SalesPrice] [float] NOT NULL,
        [Total] [float] NOT NULL,
        [discount] [float] NOT NULL,
        [Mark] [bit] NOT NULL,
        [BillPrice] [float] NOT NULL,
        [TotalBillPrice] [float] NOT NULL,
        [CostPrice] [float] NOT NULL,
        [TotalCostPrice] [float] NOT NULL,
        [OrderNo] [int] NULL,
        [Id] [int] IDENTITY(1,1) NOT NULL,
        CONSTRAINT [PK_cacheLastBillItemsPc2] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        )
        WITH (
            PAD_INDEX = OFF, 
            STATISTICS_NORECOMPUTE = OFF, 
            IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS = ON, 
            ALLOW_PAGE_LOCKS = ON
        ) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END

IF OBJECT_ID('dbo.cacheLastBillDetailsPc1', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[cacheLastBillDetailsPc1](
        [billId] [nvarchar](max) NOT NULL,
        [op1] [nvarchar](max) NULL,
        [op2] [nvarchar](max) NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END

IF OBJECT_ID('dbo.cacheLastBillDetailsPc2', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[cacheLastBillDetailsPc2](
        [billId] [nvarchar](max) NOT NULL,
        [op1] [nvarchar](max) NULL,
        [op2] [nvarchar](max) NULL
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END

/*2025-08-23 add iteminfo,stock sql for when new shop need it cleanly*/

IF OBJECT_ID('dbo.iteminformation', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[iteminformation](
	    [ITEM_ID] [int] IDENTITY(1,1) NOT NULL,
	    [ItemName] [nvarchar](200) NULL,
	    [UnitOfMeasure] [nvarchar](200) NULL,
	    [Batch] [nvarchar](200) NULL,
	    [GROUP_ID] [int] NULL,
	    [Barcode] [nvarchar](200) NULL,
	    [Cost] [float] NULL,
	    [Price] [float] NULL,
	    [ReorderPoint] [float] NULL,
	    [VAT_Applicable] [nvarchar](10) NULL,
	    [WarehouseID] [int] NULL,
	    [PhotoFileName] [nvarchar](200) NULL,
	    [Discount] [float] NOT NULL,
	    [SinhalaName] [nvarchar](4000) NULL,
	    [BillPrice] [decimal](10, 2) NULL,
	    [Wholesale] [decimal](10, 2) NULL,
     CONSTRAINT [iteminformation_PRIMARY] PRIMARY KEY NONCLUSTERED 
    (
	    [ITEM_ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
END


IF OBJECT_ID('dbo.stock', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[stock](
	    [STOCK_ID] [int] IDENTITY(1,1) NOT NULL,
	    [ITEM_ID] [int] NULL,
	    [Quantity] [float] NULL,
	    [ExpiryDate] [nvarchar](100) NULL,
	    [WarehouseID] [int] NULL,
	    [SHELF_ID] [int] NULL,
	    [Expiry] [nvarchar](100) NULL,
     CONSTRAINT [stock_PRIMARY] PRIMARY KEY NONCLUSTERED 
    (
	    [STOCK_ID] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
END
";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.ExecuteNonQuery();

                        string queryString1 = "SELECT * FROM vpsPosUsers";
                        using (SqlCommand command1 = new SqlCommand(queryString1, connection))
                        {
                            SqlDataAdapter adapter = new SqlDataAdapter(command1);
                            DataTable usersDataTable = new DataTable();
                            adapter.Fill(usersDataTable);
                            if (usersDataTable.Rows.Count == 0)
                            {
                                // convert binary values to bytes
                                string hexString = "9D74A16BE4E836FBC7E588E50F77E905";
                                byte[] salt = new byte[hexString.Length / 2];
                                for (int i = 0; i < hexString.Length; i += 2)
                                {
                                    salt[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                                }

                                string hexStringHash = "CE468DCACA90D9EF1C8FD931C6D602727B25F9616A01A91A01653EE374672972";
                                byte[] hash = new byte[hexStringHash.Length / 2];
                                for (int i = 0; i < hexStringHash.Length; i += 2)
                                {
                                    hash[i / 2] = Convert.ToByte(hexStringHash.Substring(i, 2), 16);
                                }

                                string queryString2 = "INSERT INTO vpsPosUsers (userName,fullName,regDate,salt,hash) VALUES ('admin','admin',GETDATE(),@salt,@hash)";
                                using (SqlCommand command2 = new SqlCommand(queryString2, connection))
                                {
                                    command2.Parameters.AddWithValue("@salt", salt);
                                    command2.Parameters.AddWithValue("@hash", hash);
                                    command2.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                }

                DataTable buisnessInfoDataTable = new DataTable();
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosBuisnessInfo";
                    using(SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(buisnessInfoDataTable);
                    }
                }

                if(buisnessInfoDataTable.Rows.Count == 0)
                {
                    using(SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = @"INSERT INTO vpsPosBuisnessInfo (name,address,firstPhoneNumber,secondPhoneNumber,email,webAddress) VALUES (N'ප්‍රදීප් ට්‍රේඩර්ස්',N'බුලත්සිංහල පාර,ඌරුගල,ඉංගිරිය','071-3593086','','','')";
                        using(SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }

                DataTable languageDataTable = new DataTable();
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosLanguage";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(languageDataTable);
                    }
                }

                if (languageDataTable.Rows.Count == 0)
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = @"INSERT INTO vpsPosLanguage (language) VALUES ('Sinhala')";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                
                DataTable messageDataTable = new DataTable();
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosCashierFormMessage";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(messageDataTable);
                    }
                }

                if (messageDataTable.Rows.Count == 0)
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = @"INSERT INTO vpsPosCashierFormMessage (message) VALUES ('')";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
                ErrorLogger.Log(error, "LoginForm", nameof(LoginForm_Load), GetCurrentFormData());
            }

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ErrorLogger.UpdateFormData("LoginForm", GetCurrentFormData());
            // Force kill the process immediately
            Process.GetCurrentProcess().Kill(); 
        }
    }
}
