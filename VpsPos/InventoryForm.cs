using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public partial class InventoryForm : Form
    {
        //making conString  and conn global by using public static keywords
        public static string conString;


        private DatabaseClass databaseClass = new DatabaseClass();

        public InventoryForm()
        {
            InitializeComponent();
        }

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Id", IdTextBox.Text},
        { "Item Name", ItemNameTextBox.Text},
        { "Sinhala Name", SinhalaNameTextBox.Text},
        { "Barcode", BarcodeTextBox.Text },
        { "Cost Price", CostPriceTextBox.Text },
        { "Wholesale Price", WholeSalePriceTextBox.Text },
        { "Sale Price", SalePriceTextBox.Text },
        { "Bill Price", BillPriceTextBox.Text},
        { "Quantity", QuantityTextBox.Text },
        { "Discount", DiscountTextBox.Text },
        { "Search Type", SearchComboBox.Text },
        { "Search with button", SearchCheckBox.Checked.ToString()},
        { "Search Text", SearchTextBox.Text },
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in ItemsGridView.Rows)
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

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            XmlDocument doc = new XmlDocument();
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;

            this.WindowState = FormWindowState.Maximized;
            SearchComboBox.SelectedIndex = 0;

            // filling grid view when form loads
            //using(SqlConnection connection = new SqlConnection(conString))
            //{
            //    connection.Open();
            //    string queryString = "SELECT ITEM_ID,ItemName,Barcode,Cost,Wholesale,Price FROM iteminformation";
            //    using(SqlCommand command = new SqlCommand(queryString, connection))
            //    {
            //        SqlDataAdapter adapter = new SqlDataAdapter(command);
            //        DataTable itemsDataTable = new DataTable();
            //        adapter.Fill(itemsDataTable);
            //        ItemsGridView.DataSource = itemsDataTable;
            //    }
            //}
        }

        private void ItemsGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            clearTextBoxes();

            //ITEM_ID,ItemName,Barcode,SinhalaName,Cost,Wholesale,BillPrice,Price
            DataTable selectedItemInfoDataTable = new DataTable();
            int id = Convert.ToInt32(ItemsGridView.CurrentRow.Cells[0].Value);

            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT ITEM_ID,ItemName,SinhalaName,Barcode,Cost,Wholesale,BillPrice,Price,Discount FROM iteminformation WHERE ITEM_ID=@id";

                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(selectedItemInfoDataTable);
                    }
                }
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT Quantity FROM stock WHERE ITEM_ID=@id";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        SqlDataReader dataReader = command.ExecuteReader();
                        while (dataReader.Read())
                        {
                            QuantityTextBox.Text = dataReader.GetDouble(0).ToString();
                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "InventoryForm", nameof(ItemsGridView_CellClick), GetCurrentFormData());
            }


            IdTextBox.Text = selectedItemInfoDataTable.Rows[0][0].ToString();
            ItemNameTextBox.Text = selectedItemInfoDataTable.Rows[0][1].ToString();
            SinhalaNameTextBox.Text = selectedItemInfoDataTable.Rows[0][2].ToString();
            BarcodeTextBox.Text = selectedItemInfoDataTable.Rows[0][3].ToString();
            CostPriceTextBox.Text = selectedItemInfoDataTable.Rows[0][4].ToString();
            WholeSalePriceTextBox.Text = selectedItemInfoDataTable.Rows[0][5].ToString();
            BillPriceTextBox.Text = selectedItemInfoDataTable.Rows[0][6].ToString();
            SalePriceTextBox.Text = selectedItemInfoDataTable.Rows[0][7].ToString();
            DiscountTextBox.Text = selectedItemInfoDataTable.Rows[0][8].ToString();
            SaveButton.Enabled = false;

        }

        //creating clear method {void keyword use beacause we don't want anything return}
        private void clearTextBoxes()
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            DiscountTextBox.Clear();
            IdTextBox.Clear();
            ItemNameTextBox.Clear();
            SinhalaNameTextBox.Clear();
            BarcodeTextBox.Clear();
            CostPriceTextBox.Clear();
            WholeSalePriceTextBox.Clear();
            BillPriceTextBox.Clear();
            SalePriceTextBox.Clear();
            QuantityTextBox.Clear();
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            DataTable checkItem = new DataTable();
            try
            {

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM iteminformation WHERE ItemName=@itemname";

                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        string itemName = ItemNameTextBox.Text.Trim();
                        command.Parameters.AddWithValue("@itemname", itemName);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(checkItem);
                    }
                    connection.Close();
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "InventoryForm", nameof(SaveButton_Click), GetCurrentFormData());
            }

            // To validate 
            double costPriceCheck = Convert.ToDouble(CostPriceTextBox.Text);
            double wholesalePriceCheck = Convert.ToDouble(WholeSalePriceTextBox.Text);
            double salePriceCheck = Convert.ToDouble(SalePriceTextBox.Text);
            double billPriceCheck = Convert.ToDouble(BillPriceTextBox.Text);

            if (ItemNameTextBox.Text == "" && SinhalaNameTextBox.Text == "" && BarcodeTextBox.Text == "" && CostPriceTextBox.Text == "" && WholeSalePriceTextBox.Text == "" && BillPriceTextBox.Text == "" && SalePriceTextBox.Text == "" && QuantityTextBox.Text == "")
            {
                MessageBox.Show("Please enter all information.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (ItemNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter item name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (SinhalaNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter sinhala name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (CostPriceTextBox.Text == "")
            {
                MessageBox.Show("Please enter cost price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (BillPriceTextBox.Text == "")
            {
                MessageBox.Show("Please enter bill price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (SalePriceTextBox.Text == "")
            {
                MessageBox.Show("Please enter sale price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (QuantityTextBox.Text == "")
            {
                MessageBox.Show("Please enter quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (checkItem.Rows.Count > 0)
            {
                MessageBox.Show("An Item with the item name you entered already saved in database.Item name cannot duplicate.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            // validating prices

            else if (!((costPriceCheck < wholesalePriceCheck) && (costPriceCheck < salePriceCheck) && (costPriceCheck < billPriceCheck)))
            {
                MessageBox.Show("Cost Price should be less than Wholesale Price,Sale Price and Bill Price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!((wholesalePriceCheck <= salePriceCheck) && (wholesalePriceCheck <= billPriceCheck)))
            {
                MessageBox.Show("Wholesale Price should be less than or equal to Sale Price and Bill Price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!(salePriceCheck <= billPriceCheck))
            {
                MessageBox.Show("Sale Price should be less than or equal to Bill Price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            else
            {

                try
                {
                    // inser new item
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            string queryString = @"
DECLARE @newRecordId INTEGER;

INSERT INTO iteminformation (ItemName,UnitOfMeasure,Batch,GROUP_ID,Barcode,Cost,Price,ReorderPoint,VAT_Applicable,WarehouseID,PhotoFileName,Discount,SinhalaName,BillPrice,Wholesale) VALUES (@itemName,'Glossary','','4',@barcode,@cost,@salePrice,'0','N','1','3.png',@discount,@sinhalaName,@billPrice,@wholesalePrice);SELECT @newRecordId = SCOPE_IDENTITY();

INSERT INTO stock (ITEM_ID,Quantity,ExpiryDate,WarehouseID,SHELF_ID,Expiry) VALUES(@newRecordId,@quantity,'','1','2','N');

INSERT INTO vpsPosInfoHistory (date,itemId,itemName,barcode,costPrice,wholesalePrice,salesPrice,billPrice,quantity) VALUES (GETDATE(),@newRecordId,@itemName,@barcode,@cost,@wholesalePrice,@salePrice,@billPrice,@quantity);

";

                            string itemName = ItemNameTextBox.Text.Replace("'", "").Trim();
                            string barcode = BarcodeTextBox.Text.Trim();
                            double cost = Convert.ToDouble(CostPriceTextBox.Text.Trim());
                            double salePrice = Convert.ToDouble(SalePriceTextBox.Text.Trim());
                            string sinhalaName = SinhalaNameTextBox.Text.Replace("'", "").Trim();
                            double billPrice = Convert.ToDouble(BillPriceTextBox.Text.Trim());
                            double wholesalePrice = 0;
                            if (WholeSalePriceTextBox.Text.Trim() == "")
                            {
                                wholesalePrice = 0;
                            }
                            else
                            {
                                wholesalePrice = Convert.ToDouble(WholeSalePriceTextBox.Text.Trim());
                            }

                            double discount = Convert.ToDouble(DiscountTextBox.Text);

                            // 2023/01/06 - quantity coming from stock . so make update
                            SqlCommand command = new SqlCommand(queryString, connection, transaction);

                            command.Parameters.AddWithValue("@itemName", itemName);
                            command.Parameters.AddWithValue("@barcode", barcode);
                            command.Parameters.AddWithValue("@cost", cost);
                            command.Parameters.AddWithValue("@salePrice", salePrice);
                            command.Parameters.AddWithValue("@sinhalaName", SqlDbType.NVarChar).Value = sinhalaName;
                            command.Parameters.AddWithValue("@billPrice", billPrice);
                            command.Parameters.AddWithValue("@wholesalePrice", wholesalePrice);
                            command.Parameters.AddWithValue("@discount", discount);
                            command.Parameters.AddWithValue("@quantity", Convert.ToDouble(QuantityTextBox.Text));

                            command.ExecuteNonQuery();
                            transaction.Commit();
                            MessageBox.Show("Item added to the database !", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            clearTextBoxes();
                        }
                        catch (Exception error)
                        {
                            transaction.Rollback();
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "InventoryForm", nameof(SaveButton_Click), GetCurrentFormData());
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }

                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "InventoryForm", nameof(SaveButton_Click), GetCurrentFormData());
                }
                //}

            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            clearTextBoxes();
            SaveButton.Enabled = true;
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            // To validate 
            double costPriceCheck = Convert.ToDouble(CostPriceTextBox.Text);
            double wholesalePriceCheck = Convert.ToDouble(WholeSalePriceTextBox.Text);
            double salePriceCheck = Convert.ToDouble(SalePriceTextBox.Text);
            double billPriceCheck = Convert.ToDouble(BillPriceTextBox.Text);

            if (IdTextBox.Text == "" && ItemNameTextBox.Text == "" && SinhalaNameTextBox.Text == "" && BarcodeTextBox.Text == "" && CostPriceTextBox.Text == "" && WholeSalePriceTextBox.Text == "" && BillPriceTextBox.Text == "" && SalePriceTextBox.Text == "" && QuantityTextBox.Text == "")
            {
                MessageBox.Show("Please select an item.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (IdTextBox.Text == "")
            {
                MessageBox.Show("Please select an item.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (ItemNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter item name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (SinhalaNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter sinhala name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (CostPriceTextBox.Text == "")
            {
                MessageBox.Show("Please enter cost price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (BillPriceTextBox.Text == "")
            {
                MessageBox.Show("Please enter bill price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (SalePriceTextBox.Text == "")
            {
                MessageBox.Show("Please enter sale price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (QuantityTextBox.Text == "")
            {
                MessageBox.Show("Please enter quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // validating prices

            else if (!((costPriceCheck < wholesalePriceCheck) && (costPriceCheck < salePriceCheck) && (costPriceCheck < billPriceCheck)))
            {
                MessageBox.Show("Cost Price should be less than Wholesale Price,Sale Price and Bill Price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!((wholesalePriceCheck <= salePriceCheck) && (wholesalePriceCheck <= billPriceCheck)))
            {
                MessageBox.Show("Wholesale Price should be less than or equal to Sale Price and Bill Price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!(salePriceCheck <= billPriceCheck))
            {
                MessageBox.Show("Sale Price should be less than or equal to Bill Price.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            else
            {
                //ITEM_ID,ItemName,Barcode,SinhalaName,Cost,Wholesale,BillPrice,Price
                DataTable selectedItemInfoDataTable = new DataTable();
                int id1 = Convert.ToInt32(IdTextBox.Text);

                try
                {
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT Cost FROM iteminformation WHERE ITEM_ID=@id";

                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@id", id1);
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(selectedItemInfoDataTable);
                        }
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "InventoryForm", nameof(UpdateButton_Click), GetCurrentFormData());
                }

                // for unusual cost mistakes
                double oldCost = Convert.ToDouble(selectedItemInfoDataTable.Rows[0][0].ToString());
                double newCost = Convert.ToDouble(CostPriceTextBox.Text);

                double difference = Math.Abs(newCost - oldCost);
                double percentageChange = (oldCost > 0) ? (difference / oldCost) * 100 : 100;

                // Adaptive thresholds
                double maxAllowedDifference = oldCost < 100 ? 50 : 500;
                double maxAllowedPercent = oldCost > 5000 ? 80 : 50;


                if (difference > maxAllowedDifference || percentageChange > maxAllowedPercent)
                {
                    DialogResult result = MessageBox.Show(
                        $"You entered Rs. {newCost} as cost. Previous cost was Rs. {oldCost}.\n" +
                        $"That's a big change ({percentageChange:0.##}%).\nAre you sure you want to continue?",
                        "Unusual Cost Detected",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.No)
                    {
                        return;
                    }
                    else if (result == DialogResult.Yes)
                    {

                        try
                        {
                            DataTable itemsDatatable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(conString))
                            {
                                connection.Open();
                                string queryString = "SELECT * FROM iteminformation WHERE ItemName=@itemName AND ITEM_ID != @id";
                                using (SqlCommand command = new SqlCommand(queryString, connection))
                                {
                                    command.Parameters.AddWithValue("@itemName", SqlDbType.VarChar).Value = ItemNameTextBox.Text.Trim();
                                    command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = IdTextBox.Text.Trim();
                                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                                    adapter.Fill(itemsDatatable);
                                }
                                connection.Close();
                            }

                            if (itemsDatatable.Rows.Count > 0)
                            {
                                MessageBox.Show("There is an item with item name you provided already in the database. Item name can't be duplicate.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {

                                //update item details
                                using (SqlConnection connection = new SqlConnection(conString))
                                {
                                    connection.Open();
                                    SqlTransaction transaction = connection.BeginTransaction();

                                    try
                                    {
                                        string queryString = @"
UPDATE iteminformation SET ItemName = @itemname,Barcode = @barcode,Cost = @cost,Price = @salePrice,SinhalaName =@sinhalaName,BillPrice = @billPrice,Wholesale = @wholesalePrice,Discount=@discount WHERE ITEM_ID = @id;

UPDATE stock SET Quantity = @quantity WHERE ITEM_ID = @id;

INSERT INTO vpsPosInfoHistory (date,itemId,itemName,barcode,costPrice,wholesalePrice,salesPrice,billPrice,quantity) VALUES (GETDATE(),@id,@itemName,@barcode,@cost,@wholesalePrice,@salePrice,@billPrice,@quantity);

";

                                        string itemName = ItemNameTextBox.Text.Replace("'", "").Trim();
                                        string barcode = BarcodeTextBox.Text.Trim();
                                        double cost = Convert.ToDouble(CostPriceTextBox.Text.Trim());
                                        double salePrice = Convert.ToDouble(SalePriceTextBox.Text.Trim());
                                        string sinhalaName = SinhalaNameTextBox.Text.Replace("'", "").Trim();
                                        double billPrice = Convert.ToDouble(BillPriceTextBox.Text.Trim());
                                        double wholesalePrice = 0;
                                        if (WholeSalePriceTextBox.Text.Trim() == "")
                                        {
                                            wholesalePrice = 0;
                                        }
                                        else
                                        {
                                            wholesalePrice = Convert.ToDouble(WholeSalePriceTextBox.Text.Trim());
                                        }

                                        int id = Convert.ToInt32(IdTextBox.Text);
                                        double discount = Convert.ToDouble(DiscountTextBox.Text);

                                        SqlCommand command = new SqlCommand(queryString, connection, transaction);

                                        command.Parameters.AddWithValue("@itemName", itemName);
                                        command.Parameters.AddWithValue("@barcode", barcode);
                                        command.Parameters.AddWithValue("@cost", cost);
                                        command.Parameters.AddWithValue("@salePrice", salePrice);
                                        command.Parameters.AddWithValue("@sinhalaName", SqlDbType.NVarChar).Value = sinhalaName;
                                        command.Parameters.AddWithValue("@billPrice", billPrice);
                                        command.Parameters.AddWithValue("@wholesalePrice", wholesalePrice);
                                        command.Parameters.AddWithValue("@id", id);
                                        command.Parameters.AddWithValue("@discount", discount);

                                        command.Parameters.AddWithValue("@quantity", Convert.ToDouble(QuantityTextBox.Text));
                                        command.ExecuteNonQuery();

                                        transaction.Commit();

                                        MessageBox.Show("Item successfuly updated !", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        clearTextBoxes();
                                        SaveButton.Enabled = true;
                                    }
                                    catch (Exception error)
                                    {
                                        transaction.Rollback();
                                        MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        ErrorLogger.Log(error, "InventoryForm", nameof(UpdateButton_Click), GetCurrentFormData());
                                    }
                                    finally
                                    {
                                        connection.Close();
                                    }

                                }

                            }
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "InventoryForm", nameof(UpdateButton_Click), GetCurrentFormData());
                        }
                    }

                }
                else
                {
                    try
                    {
                        DataTable itemsDatatable = new DataTable();

                        using (SqlConnection connection = new SqlConnection(conString))
                        {
                            connection.Open();
                            string queryString = "SELECT * FROM iteminformation WHERE ItemName=@itemName AND ITEM_ID != @id";
                            using (SqlCommand command = new SqlCommand(queryString, connection))
                            {
                                command.Parameters.AddWithValue("@itemName", SqlDbType.VarChar).Value = ItemNameTextBox.Text.Trim();
                                command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = IdTextBox.Text.Trim();
                                SqlDataAdapter adapter = new SqlDataAdapter(command);
                                adapter.Fill(itemsDatatable);
                            }
                            connection.Close();
                        }

                        if (itemsDatatable.Rows.Count > 0)
                        {
                            MessageBox.Show("There is an item with item name you provided already in the database. Item name can't be duplicate.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {

                            //update item details
                            using (SqlConnection connection = new SqlConnection(conString))
                            {
                                connection.Open();
                                SqlTransaction transaction = connection.BeginTransaction();

                                try
                                {
                                    string queryString = @"
UPDATE iteminformation SET ItemName = @itemname,Barcode = @barcode,Cost = @cost,Price = @salePrice,SinhalaName =@sinhalaName,BillPrice = @billPrice,Wholesale = @wholesalePrice,Discount=@discount WHERE ITEM_ID = @id;

UPDATE stock SET Quantity = @quantity WHERE ITEM_ID = @id;

INSERT INTO vpsPosInfoHistory (date,itemId,itemName,barcode,costPrice,wholesalePrice,salesPrice,billPrice,quantity) VALUES (GETDATE(),@id,@itemName,@barcode,@cost,@wholesalePrice,@salePrice,@billPrice,@quantity);

";

                                    string itemName = ItemNameTextBox.Text.Replace("'", "").Trim();
                                    string barcode = BarcodeTextBox.Text.Trim();
                                    double cost = Convert.ToDouble(CostPriceTextBox.Text.Trim());
                                    double salePrice = Convert.ToDouble(SalePriceTextBox.Text.Trim());
                                    string sinhalaName = SinhalaNameTextBox.Text.Replace("'", "").Trim();
                                    double billPrice = Convert.ToDouble(BillPriceTextBox.Text.Trim());
                                    double wholesalePrice = 0;
                                    if (WholeSalePriceTextBox.Text.Trim() == "")
                                    {
                                        wholesalePrice = 0;
                                    }
                                    else
                                    {
                                        wholesalePrice = Convert.ToDouble(WholeSalePriceTextBox.Text.Trim());
                                    }

                                    int id = Convert.ToInt32(IdTextBox.Text);
                                    double discount = Convert.ToDouble(DiscountTextBox.Text);

                                    SqlCommand command = new SqlCommand(queryString, connection, transaction);

                                    command.Parameters.AddWithValue("@itemName", itemName);
                                    command.Parameters.AddWithValue("@barcode", barcode);
                                    command.Parameters.AddWithValue("@cost", cost);
                                    command.Parameters.AddWithValue("@salePrice", salePrice);
                                    command.Parameters.AddWithValue("@sinhalaName", SqlDbType.NVarChar).Value = sinhalaName;
                                    command.Parameters.AddWithValue("@billPrice", billPrice);
                                    command.Parameters.AddWithValue("@wholesalePrice", wholesalePrice);
                                    command.Parameters.AddWithValue("@id", id);
                                    command.Parameters.AddWithValue("@discount", discount);

                                    command.Parameters.AddWithValue("@quantity", Convert.ToDouble(QuantityTextBox.Text));
                                    command.ExecuteNonQuery();

                                    transaction.Commit();

                                    MessageBox.Show("Item successfuly updated !", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    clearTextBoxes();
                                    SaveButton.Enabled = true;
                                }
                                catch (Exception error)
                                {
                                    transaction.Rollback();
                                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    ErrorLogger.Log(error, "InventoryForm", nameof(UpdateButton_Click), GetCurrentFormData());
                                }
                                finally
                                {
                                    connection.Close();
                                }

                            }

                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ErrorLogger.Log(error, "InventoryForm", nameof(UpdateButton_Click), GetCurrentFormData());
                    }
                }

            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            if (IdTextBox.Text == "" && ItemNameTextBox.Text == "" && SinhalaNameTextBox.Text == "" && BarcodeTextBox.Text == "" && CostPriceTextBox.Text == "" && WholeSalePriceTextBox.Text == "" && BillPriceTextBox.Text == "" && SalePriceTextBox.Text == "" && QuantityTextBox.Text == "")
            {
                MessageBox.Show("Please select an item.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (IdTextBox.Text == "")
            {
                MessageBox.Show("Please select an item.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                //remove information
                if (MessageBox.Show("Do you want to delete this item?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //update item details
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            string queryString = @"
INSERT INTO vpsPosInfoHistory (date,itemId,itemName,barcode,costPrice,wholesalePrice,salesPrice,billPrice,quantity) VALUES (GETDATE(),@id,@itemName,@barcode,@cost,@wholesalePrice,@salePrice,@billPrice,@quantity);

";
                            string itemName = ItemNameTextBox.Text.Trim();
                            string barcode = "*Deleted* ";
                            barcode += BarcodeTextBox.Text.Trim();
                            double cost = Convert.ToDouble(CostPriceTextBox.Text.Trim());
                            double salePrice = Convert.ToDouble(SalePriceTextBox.Text.Trim());
                            string sinhalaName = SinhalaNameTextBox.Text.Trim();
                            double billPrice = Convert.ToDouble(BillPriceTextBox.Text.Trim());
                            double wholesalePrice = 0;
                            if (WholeSalePriceTextBox.Text.Trim() == "")
                            {
                                wholesalePrice = 0;
                            }
                            else
                            {
                                wholesalePrice = Convert.ToDouble(WholeSalePriceTextBox.Text.Trim());
                            }

                            int id = Convert.ToInt32(IdTextBox.Text);
                            double discount = Convert.ToDouble(DiscountTextBox.Text);

                            SqlCommand command = new SqlCommand(queryString, connection, transaction);

                            command.Parameters.AddWithValue("@itemName", itemName);
                            command.Parameters.AddWithValue("@barcode", barcode);
                            command.Parameters.AddWithValue("@cost", cost);
                            command.Parameters.AddWithValue("@salePrice", salePrice);
                            command.Parameters.AddWithValue("@billPrice", billPrice);
                            command.Parameters.AddWithValue("@wholesalePrice", wholesalePrice);
                            command.Parameters.AddWithValue("@id", id);

                            command.Parameters.AddWithValue("@quantity", Convert.ToDouble(QuantityTextBox.Text));
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception error)
                        {
                            transaction.Rollback();
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "InventoryForm", nameof(DeleteButton_Click), GetCurrentFormData());
                        }
                        finally
                        {
                            connection.Close();
                        }

                    }


                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            // Disable foreign key constraints. This use to avoid reference errors
                            SqlCommand disableConstraintsCommand = new SqlCommand("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'", connection, transaction);
                            disableConstraintsCommand.ExecuteNonQuery();

                            // Delete record from tables
                            SqlCommand deleteFromInfoTableCommand = new SqlCommand("DELETE FROM iteminformation WHERE ITEM_ID=@id", connection, transaction);
                            deleteFromInfoTableCommand.Parameters.AddWithValue("@id", SqlDbType.Int).Value = IdTextBox.Text;
                            deleteFromInfoTableCommand.ExecuteNonQuery();

                            SqlCommand deleteFromStockTableCommand = new SqlCommand("DELETE FROM stock WHERE ITEM_ID = @id;", connection, transaction);
                            deleteFromStockTableCommand.Parameters.AddWithValue("@id", SqlDbType.Int).Value = IdTextBox.Text;
                            deleteFromStockTableCommand.ExecuteNonQuery();

                            // Re enable foreign key constraints
                            SqlCommand enableConstraintsCommand = new SqlCommand("EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'", connection, transaction);
                            enableConstraintsCommand.ExecuteNonQuery();

                            // commit transaction
                            transaction.Commit();

                            MessageBox.Show("Item deleted successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }
                        catch (Exception error)
                        {
                            // Rollback transaction if an exeception occurred
                            transaction.Rollback();
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "InventoryForm", nameof(DeleteButton_Click), GetCurrentFormData());
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    clearTextBoxes();
                    SaveButton.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Item not deleted !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            if (SearchCheckBox.Checked == false)
            {
                if (SearchTextBox.Text.Trim() == "")
                {
                    ItemsGridView.DataSource = null;
                }
                else
                {
                    if (SearchComboBox.Text == "ItemName" || SearchComboBox.Text == "ITEM_ID")
                    {
                        try
                        {
                            InfoForm infoForm = new InfoForm();

                            if (SearchTextBox.Text.Trim().Count() < 3)
                            {
                                infoForm.Show();
                            }

                            SearchItemsInItemsGridView();

                            if (SearchTextBox.Text.Trim().Count() < 3)
                            {
                                infoForm.Close();
                            }
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "InventoryForm", nameof(SearchTextBox_KeyUp), GetCurrentFormData());
                        }
                    }
                    else
                    {
                        if (e.KeyCode == Keys.Enter)
                        {
                            SearchItemsInItemsGridView();
                        }
                    }
                }
            }
        }

        private void SearchItemsInItemsGridView()
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            DataTable searchesItemsDatatable = new DataTable();
            using (SqlConnection connection = new SqlConnection(conString))
            {
                connection.Open();

                string queryString = $"SELECT ITEM_ID,ItemName,Barcode,Cost,Wholesale,Price AS Sale_Price,BillPrice AS Bill_Price FROM iteminformation WHERE {SearchComboBox.SelectedItem} like @phrase";
                using (SqlCommand command = new SqlCommand(queryString, connection))
                {
                    string phrase = "%" + SearchTextBox.Text.Trim() + "%";

                    command.Parameters.AddWithValue("@phrase", phrase);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);

                    adapter.Fill(searchesItemsDatatable);
                    ItemsGridView.DataSource = searchesItemsDatatable;
                }
            }

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            this.Close();
        }

        private void SalePriceTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            try
            {
                double discount = Convert.ToDouble(BillPriceTextBox.Text) - Convert.ToDouble(SalePriceTextBox.Text);
                DiscountTextBox.Text = discount.ToString();
            }
            catch
            {

            }
        }

        private void BillPriceTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            try
            {
                double discount = Convert.ToDouble(BillPriceTextBox.Text.Trim()) - Convert.ToDouble(SalePriceTextBox.Text.Trim());
                DiscountTextBox.Text = discount.ToString();
            }
            catch
            {

            }


        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            string searchColumn = SearchComboBox.SelectedItem.ToString();
            string phrase = "%" + SearchTextBox.Text.Trim() + "%";
            if (phrase == "")
            {
                ItemsGridView.DataSource = null;
            }
            else
            {
                try
                {
                    DataTable refreshedItemsDataTable = new DataTable();
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = $"SELECT ITEM_ID,ItemName,Barcode,Cost,Wholesale,Price AS Sale_Price,BillPrice AS Bill_Price FROM iteminformation WHERE {searchColumn} like @phrase";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@phrase", phrase);
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(refreshedItemsDataTable);
                            ItemsGridView.DataSource = refreshedItemsDataTable;
                        }
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "InventoryForm", nameof(RefreshButton_Click), GetCurrentFormData());
                }
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            if (SearchCheckBox.Checked == true)
            {
                if (SearchTextBox.Text.Trim() == "")
                {
                    ItemsGridView.DataSource = null;
                }
                else
                {
                    InfoForm infoForm = new InfoForm();
                    infoForm.Show();

                    SearchItemsInItemsGridView();

                    infoForm.Close();
                }
            }
            else
            {
                MessageBox.Show("Check box not checked !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SearchCheckBox_KeyDown(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("InventoryForm", GetCurrentFormData());
            if (e.KeyCode == Keys.Enter)
            {
                SearchCheckBox.Checked = !SearchCheckBox.Checked;
            }
        }
    }
}
