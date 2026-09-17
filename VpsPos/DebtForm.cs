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
    public partial class DebtForm : Form
    {
        private string conString;
        private DataTable tDatatable = new DataTable();

        public DebtForm()
        {
            InitializeComponent();
        }

        // logging helper function
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "T id", TIdTextBox.Text},
        {"Name", NameComboBox.Text},
        { "Type", TypeComboBox.Text},
        { "Id",IdComboBox.Text },
        { "Amount", AmountTextBox.Text },
        { "From Date", FromDateTimePicker.Text },
        { "To Date", ToDateTimePicker.Text },
        { "Search with button", SearchWithButtonCheckBox.Checked.ToString()},
        { "Search between dates", SearchWithDateCheckBox.Checked.ToString() },
        { "Search type", SearchComboBox.Text },
                {"Search Text",SearchTextBox.Text },
                {"Category",CategoryComboBox.Text }
    };

            // Dump all grid rows (small grids only!)
            string gridDump = "";
            foreach (DataGridViewRow row in DebtGridView.Rows)
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

        private void ClearControls()
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            this.TIdTextBox.Clear();
            this.NameComboBox.SelectedIndex = 0;
            this.IdComboBox.SelectedIndex = 0;
            this.TypeComboBox.SelectedIndex = 0;
            this.AmountTextBox.Clear();
        }

        // Modifier Helper : It now asks for the Connection and Transaction to use 
        // This function fixes the math for ONE customer
        private void FixCustomerBalance(string customerId, SqlConnection connection, SqlTransaction transaction)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            try
            {
                // 1. Get All transactions for this customer
                // Notice we reuse the passed 'connection' and 'transaction'

                // Get ALL transactions for this customer, sorted by Date (Oldest first)
                // We order by ID as a tie-breaker so the order never changes.
                string query = "SELECT id, type, amount FROM vpsPosDebtTransactions WHERE customer_id = @cid ORDER BY date ASC, id ASC";
                SqlCommand selectCmd = new SqlCommand(query, connection, transaction);
                selectCmd.Parameters.AddWithValue("@cid", customerId);

                SqlDataAdapter adapter = new SqlDataAdapter(selectCmd);
                DataTable history = new DataTable();
                adapter.Fill(history);

                // 2. Start the math from zero
                double runningBalance = 0;

                // 3. Loop through every single transaction
                foreach (DataRow row in history.Rows)
                {
                    string type = row["type"].ToString();
                    double amount = Convert.ToDouble(row["amount"]);
                    int transId = Convert.ToInt32(row["id"]);

                    // Do the math
                    if (type == "Debt")
                    {
                        runningBalance = runningBalance + amount;
                    }
                    else if (type == "Pay")
                    {
                        runningBalance = runningBalance - amount;
                    }

                    // 4. Update THIS transaction with the correct new balance
                    string updateRowQuery = "UPDATE vpsPosDebtTransactions SET current_debt = @newBal WHERE id = @tid";
                    SqlCommand updateCmd = new SqlCommand(updateRowQuery, connection, transaction);
                    updateCmd.Parameters.AddWithValue("@newBal", runningBalance);
                    updateCmd.Parameters.AddWithValue("@tid", transId);
                    updateCmd.ExecuteNonQuery();
                }

                // 5. Finally, update the Customer's main total debt
                string updateCustomerQuery = "UPDATE vpsPosCustomers SET debt = @finalDebt WHERE id = @cid";
                SqlCommand finalCmd = new SqlCommand(updateCustomerQuery, connection, transaction);
                finalCmd.Parameters.AddWithValue("@finalDebt", runningBalance);
                finalCmd.Parameters.AddWithValue("@cid", customerId);
                finalCmd.ExecuteNonQuery();
            }
            catch (Exception error)
            {
                MessageBox.Show("Error when fixing customer balances !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DebtForm", nameof(FixCustomerBalance), GetCurrentFormData());
            }
        }

        private void DebtForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load("requiredInfo.xml");
                this.conString = xmlDocument.GetElementsByTagName("sqlString")[0].InnerText;
                this.WindowState = FormWindowState.Maximized;

                this.TypeComboBox.SelectedIndex = 0;
                this.SearchComboBox.SelectedIndex = 1;
                this.CategoryComboBox.SelectedIndex = 0;

                using (SqlConnection connection = new SqlConnection(this.conString))
                {
                    connection.Open();
                    using (SqlCommand selectCommand = new SqlCommand("SELECT id,name FROM vpsPosCustomers", connection))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                        DataTable dataTable = new DataTable();
                        sqlDataAdapter.Fill(dataTable);
                        for (int index = 0; index < dataTable.Rows.Count; ++index)
                        {
                            this.NameComboBox.Items.Add(dataTable.Rows[index][1]);
                            this.IdComboBox.Items.Add(dataTable.Rows[index][0]);
                        }
                    }
                }

                NameComboBox.SelectedIndex = 0;
                IdComboBox.SelectedIndex = 0;

                // Get the current date
                DateTime currentDate = DateTime.Today;


                DateTime startDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0);

                DateTime endDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 23, 59, 59);

                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT t.id,customer_id,c.name as name,date,type,amount,current_debt FROM vpsPosDebtTransactions t\r\nJOIN vpsPosCustomers  c ON c.id = t.customer_id\r\nWHERE t.date BETWEEN @startDate AND @endDate";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", SqlDbType.DateTime).Value = startDateTime;
                        command.Parameters.AddWithValue("@endDate", SqlDbType.DateTime).Value = endDateTime;
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                        DebtGridView.DataSource = dataTable;
                    }
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DebtForm", nameof(DebtForm_Load), GetCurrentFormData());
            }

        }

        private void NameComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return)
                return;
            this.IdComboBox.SelectedIndex = this.NameComboBox.SelectedIndex;
        }

        private void IdComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            if (e.KeyCode != Keys.Return)
                return;
            this.NameComboBox.SelectedIndex = this.IdComboBox.SelectedIndex;
        }

        private void NameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.IdComboBox.SelectedIndex = this.NameComboBox.SelectedIndex;
        }

        private void IdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            this.NameComboBox.SelectedIndex = this.IdComboBox.SelectedIndex;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());

            // 1. Basic Validation
            if(this.TIdTextBox.Text != "")
            {
                MessageBox.Show("This record already exist in database !");
                return;
            }

            if (this.NameComboBox.Text.Trim() == "" || this.IdComboBox.Text.Trim() == "" ||
        this.TypeComboBox.Text == "" || this.AmountTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please fill all fields!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Prepare Variables
            string name = this.NameComboBox.Text.Trim();
            string id = this.IdComboBox.Text.Trim();
            string type = this.TypeComboBox.Text;
            string amount = this.AmountTextBox.Text.Trim();

            using (SqlConnection connection = new SqlConnection(this.conString))
            {
                connection.Open();

                // 3. Verify Customer Exists (Safety Check)
                // We do this BEFORE the transaction to keep it simple.
                string verifyQuery = "SELECT COUNT(*) FROM vpsPosCustomers WHERE id=@id AND name=@name";
                using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, connection))
                {
                    verifyCmd.Parameters.AddWithValue("@id", id);
                    verifyCmd.Parameters.AddWithValue("@name", name);
                    int count = (int)verifyCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("There is no customer with this name and id !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return; // Stop here
                    }
                }

                // 4. Start Transaction
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // ---------------------------------------------------------
                    // STEP A: Insert the New Transaction
                    // ---------------------------------------------------------
                    // We insert '0' for current_debt. The Fixer will calculate the real number in a microsecond.
                    // Notice: We don't need a Switch Statement! We just save "Debt" or "Pay" text.
                    string insertQuery = @"INSERT INTO vpsPosDebtTransactions 
                                   (customer_id, date, type, amount, current_debt)
                                   VALUES (@cid, GETDATE(), @type, @amount, 0)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@cid", id);
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.ExecuteNonQuery();
                    }

                    // ---------------------------------------------------------
                    // STEP B: The "Magic" Fixer
                    // ---------------------------------------------------------
                    // This calculates the math for us.
                    FixCustomerBalance(id, connection, transaction);

                    // ---------------------------------------------------------
                    // STEP C: Commit
                    // ---------------------------------------------------------
                    transaction.Commit();

                    MessageBox.Show("Transaction added successfully to the database !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    ClearControls();
                    DebtGridView.DataSource = null;
                    getSearchResultToGrid();
                }
                catch (Exception ex)
                {
                    // If anything fails, undo insert and math updates
                    transaction.Rollback();

                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(ex, "DebtForm", nameof(AddButton_Click), GetCurrentFormData());
                }
            }
            /*  ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
              if (this.TIdTextBox.Text == "")
              {
                  if (this.NameComboBox.Text.Trim() == "")
                  {
                      int num1 = (int)MessageBox.Show("Please enter customer name !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  }
                  else if (this.IdComboBox.Text.Trim() == "")
                  {
                      int num2 = (int)MessageBox.Show("Please enter customer id !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  }
                  else if (this.TypeComboBox.Text == "")
                  {
                      int num3 = (int)MessageBox.Show("Please enter type !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  }
                  else if (this.AmountTextBox.Text.Trim() == "")
                  {
                      int num4 = (int)MessageBox.Show("Please enter amount !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                  }
                  else
                  {
                      try
                      {
                          string name = this.NameComboBox.Text.Trim();
                          string id = this.IdComboBox.Text.Trim();
                          string type = this.TypeComboBox.Text;
                          string amount = this.AmountTextBox.Text.Trim();
                          using (SqlConnection connection1 = new SqlConnection(this.conString))
                          {
                              connection1.Open();
                              using (SqlCommand selectCommand = new SqlCommand("SELECT * FROM vpsPosCustomers WHERE id=@id AND name=@name", connection1))
                              {
                                  selectCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)id;
                                  selectCommand.Parameters.AddWithValue("@name", (object)SqlDbType.NVarChar).Value = (object)name;
                                  SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                                  DataTable dataTable = new DataTable();
                                  sqlDataAdapter.Fill(dataTable);
                                  if (dataTable.Rows.Count > 0)
                                  {
                                      switch (type)
                                      {
                                          case "Debt":
                                              using (SqlConnection connection2 = new SqlConnection(this.conString))
                                              {
                                                  connection2.Open();
                                                  SqlTransaction transaction = connection2.BeginTransaction();
                                                  try
                                                  {
                                                      SqlCommand sqlCommand = new SqlCommand("BEGIN TRANSACTION;\r\n                                                UPDATE vpsPosCustomers\r\n                                                SET debt = debt + @amount\r\n                                                WHERE id = @id;\r\n\r\n                                                DECLARE @current_debt float;\r\n                                                SELECT @current_debt = debt FROM vpsPosCustomers WHERE id = @id;\r\n\r\n                                                INSERT INTO vpsPosDebtTransactions (customer_id,date, type,amount, current_debt)\r\n                                                VALUES (@id,GETDATE(),@type, @amount, @current_debt);\r\n                                                COMMIT;", connection2, transaction);

                                                      sqlCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)id;
                                                      sqlCommand.Parameters.AddWithValue("@type", (object)SqlDbType.VarChar).Value = (object)type;
                                                      sqlCommand.Parameters.AddWithValue("@amount", (object)SqlDbType.Float).Value = (object)amount;

                                                      sqlCommand.ExecuteNonQuery();

                                                      transaction.Commit();
                                                      int num5 = (int)MessageBox.Show("Transaction added successfully to the database !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                                  }
                                                  catch (Exception error)
                                                  {
                                                      transaction.Rollback();
                                                      MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                      ErrorLogger.Log(error, "DebtForm", nameof(AddButton_Click), GetCurrentFormData());
                                                  }
                                                  finally
                                                  {
                                                      connection2.Close();
                                                  }
                                              }
                                              break;
                                          case "Pay":
                                              using (SqlConnection connection3 = new SqlConnection(this.conString))
                                              {
                                                  connection3.Open();
                                                  SqlTransaction transaction = connection3.BeginTransaction();
                                                  try
                                                  {
                                                      SqlCommand sqlCommand = new SqlCommand("                                              UPDATE vpsPosCustomers\r\n                                                SET debt = debt - @amount\r\n                                                WHERE id = @id;\r\n\r\n                                                DECLARE @current_debt float;\r\n                                                SELECT @current_debt = debt FROM vpsPosCustomers WHERE id = @id;\r\n\r\n                                                INSERT INTO vpsPosDebtTransactions (customer_id,date,type,amount,current_debt)\r\n                                                VALUES (@id,GETDATE(),@type, @amount, @current_debt);\r\n ", connection3, transaction);

                                                      sqlCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)id;
                                                      sqlCommand.Parameters.AddWithValue("@type", (object)SqlDbType.VarChar).Value = (object)type;
                                                      sqlCommand.Parameters.AddWithValue("@amount", (object)SqlDbType.Float).Value = (object)amount;

                                                      sqlCommand.ExecuteNonQuery();
                                                      transaction.Commit();

                                                      int num6 = (int)MessageBox.Show("Transaction added successfully to the database !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                                  }
                                                  catch (Exception error)
                                                  {
                                                      transaction.Rollback();
                                                      MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                      ErrorLogger.Log(error, "DebtForm", nameof(AddButton_Click), GetCurrentFormData());
                                                  }
                                                  finally
                                                  {
                                                      connection3.Close();
                                                  }

                                              }
                                              break;
                                      }
                                      ClearControls();
                                  }
                                  else
                                  {
                                      int num7 = (int)MessageBox.Show("There is no customer with this name and id !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                  }
                              }
                          }
                      }
                      catch (Exception ex)
                      {
                          int num8 = (int)MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                          ErrorLogger.Log(ex, "DebtForm", nameof(AddButton_Click), GetCurrentFormData());
                      }
                  }
              }
              else
              {
                  int num9 = (int)MessageBox.Show("This record already exist in database !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
              }*/
        }

        private void getSearchResultToGrid()
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            string queryString = "";
            string searchColumn = this.SearchComboBox.Text;
            string phrase = this.SearchTextBox.Text.Trim();
            if (this.CategoryComboBox.Text == "debt")
                queryString = "SELECT id,name,debt FROM vpsPosCustomers WHERE " + searchColumn + " like @phrase";
            else if (this.CategoryComboBox.Text == "transactions")
            {
                switch (searchColumn)
                {
                    case "id":
                        queryString = "SELECT t.id,customer_id,c.name AS name,date,type,amount,current_debt FROM vpsPosDebtTransactions t JOIN vpsPosCustomers  c ON c.id = t.customer_id  WHERE customer_id like @phrase";
                        break;
                    case "name":
                        queryString = "SELECT t.id,customer_id,c.name AS name,date,type,amount,current_debt FROM vpsPosDebtTransactions t\r\nJOIN vpsPosCustomers  c ON c.id = t.customer_id\r\nWHERE c.name like @phrase";
                        break;
                }
                if (this.SearchWithDateCheckBox.Checked)
                {
                    switch (searchColumn)
                    {
                        case "id":
                            queryString = "SELECT t.id,customer_id,c.name as name,date,type,amount,current_debt FROM vpsPosDebtTransactions t JOIN vpsPosCustomers  c ON c.id = t.customer_id WHERE customer_id like @phrase AND date BETWEEN @startDate AND @endDate";
                            break;
                        case "name":
                            queryString = "SELECT t.id,customer_id,c.name as name,date,type,amount,current_debt FROM vpsPosDebtTransactions t\r\nJOIN vpsPosCustomers  c ON c.id = t.customer_id\r\nWHERE c.name like @phrase AND t.date BETWEEN @startDate AND @endDate";
                            break;
                    }
                    DateTime startDate = this.FromDateTimePicker.Value;
                    DateTime endDate = this.ToDateTimePicker.Value;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(this.conString))
                        {
                            connection.Open();
                            using (SqlCommand selectCommand = new SqlCommand(queryString, connection))
                            {
                                selectCommand.Parameters.AddWithValue("@phrase", (object)SqlDbType.NVarChar).Value = (object)("%" + phrase + "%");
                                selectCommand.Parameters.AddWithValue("@startDate", (object)SqlDbType.DateTime).Value = (object)startDate;
                                selectCommand.Parameters.AddWithValue("@endDate", (object)SqlDbType.DateTime).Value = (object)endDate;
                                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                                DataTable dataTable = new DataTable();
                                sqlDataAdapter.Fill(dataTable);
                                this.DebtGridView.DataSource = (object)dataTable;
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        ErrorLogger.Log(ex, "DebtForm", nameof(getSearchResultToGrid), GetCurrentFormData());
                        return;
                    }
                }
            }
            else if (this.CategoryComboBox.Text == "customers")
                queryString = "SELECT id,name,phoneNumber,email,address,age FROM vpsPosCustomers WHERE " + searchColumn + " like @phrase";
            try
            {
                using (SqlConnection connection = new SqlConnection(this.conString))
                {
                    connection.Open();
                    using (SqlCommand selectCommand = new SqlCommand(queryString, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@phrase", (object)SqlDbType.NVarChar).Value = (object)("%" + phrase + "%");
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                        DataTable dataTable = new DataTable();
                        sqlDataAdapter.Fill(dataTable);
                        this.DebtGridView.DataSource = (object)dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                ErrorLogger.Log(ex, "DebtForm", nameof(getSearchResultToGrid), GetCurrentFormData());
            }

        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            if (this.SearchWithButtonCheckBox.Checked)
            {
                if (this.SearchTextBox.Text == "")
                    this.DebtGridView.DataSource = (object)null;
                else
                    this.getSearchResultToGrid();
            }
            else
            {
                int num = (int)MessageBox.Show("Search with button check box not checked !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            this.Close();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            if (this.SearchWithButtonCheckBox.Checked)
                return;
            if (this.SearchTextBox.Text.Trim() == "")
            {
                this.DebtGridView.DataSource = (object)null;
            }
            else
            {
                InfoForm infoForm = new InfoForm();
                if (this.SearchTextBox.Text.Trim().Count<char>() <= 3)
                    infoForm.Show();
                this.getSearchResultToGrid();
                if (this.SearchTextBox.Text.Trim().Count<char>() <= 3)
                    infoForm.Close();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            this.ClearControls();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            if (this.TIdTextBox.Text == "" || this.NameComboBox.Text == "" || this.IdComboBox.Text == "" || this.TypeComboBox.SelectedItem.ToString() == "" || this.AmountTextBox.Text == "")
            {
                int num1 = (int)MessageBox.Show("Empty field !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                try
                {
                    // Prepare data
                    string tId = this.TIdTextBox.Text;
                    string customerId = this.IdComboBox.Text.Trim(); // we need ID to fix balance
                    string type = this.TypeComboBox.Text;
                    string amount = this.AmountTextBox.Text.Trim();

                    using (SqlConnection connection = new SqlConnection(this.conString))
                    {
                        connection.Open();
                        // start the safety net
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            // ---------------------------------------------------------
                            // STEP A: Update the Transaction Row ONLY
                            // ---------------------------------------------------------
                            // We do NOT manually update vpsPosCustomers here. 
                            // We temporarily set current_debt = 0 because the fixer handles it.
                            string updateQuery = @"UPDATE vpsPosDebtTransactions 
                                   SET customer_id = @cid, 
                                       type = @type, 
                                       amount = @amount, 
                                       current_debt = 0 
                                   WHERE id = @tid";
                            using (SqlCommand cmd = new SqlCommand(updateQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@cid", customerId);
                                cmd.Parameters.AddWithValue("@type", type);
                                cmd.Parameters.AddWithValue("@amount", amount);
                                cmd.Parameters.AddWithValue("@tid", tId);
                                cmd.ExecuteNonQuery();
                            }

                            // ---------------------------------------------------------
                            // STEP B: The "Magic" Fixer
                            // ---------------------------------------------------------
                            // This recalculates the entire history for this customer 
                            // using the SAME transaction we started above.
                            FixCustomerBalance(customerId, connection, transaction);
                            // ---------------------------------------------------------
                            // STEP C: Save Everything
                            // ---------------------------------------------------------
                            transaction.Commit();

                            MessageBox.Show("Transaction updated and calculated successfully!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                            // Cleanup
                            this.tDatatable.Clear();
                            this.ClearControls();
                            this.DebtGridView.DataSource = null;
                            getSearchResultToGrid();
                        }
                        catch (Exception ex)
                        {
                            // If anything fails (Step A or Step B), undo EVERYTHING.
                            transaction.Rollback();

                            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(ex, "DebtForm", nameof(UpdateButton_Click), GetCurrentFormData());
                        }
                        /*
                                                using (SqlCommand selectCommand = new SqlCommand("SELECT * FROM vpsPosCustomers WHERE id=@id AND name=@name", connection1))
                                                {
                                                    selectCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)id;
                                                    selectCommand.Parameters.AddWithValue("@name", (object)SqlDbType.NVarChar).Value = (object)name;
                                                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                                                    DataTable dataTable = new DataTable();
                                                    sqlDataAdapter.Fill(dataTable);
                                                    if (dataTable.Rows.Count > 0)
                                                    {
                                                        string operation = "";
                                                        if (this.tDatatable.Rows[0][3].ToString() == "Pay")
                                                            operation = "+";
                                                        else if (this.tDatatable.Rows[0][3].ToString() == "Debt")
                                                            operation = "-";
                                                        switch (type)
                                                        {
                                                            case "Debt":
                                                                using (SqlConnection connection2 = new SqlConnection(this.conString))
                                                                {
                                                                    connection2.Open();
                                                                    SqlTransaction transaction = connection2.BeginTransaction();
                                                                    try
                                                                    {
                                                                        SqlCommand sqlCommand = new SqlCommand("UPDATE vpsPosCustomers SET debt=debt" + operation + "@prevAmount WHERE id=@prevId;\r\n\r\n                                                UPDATE vpsPosCustomers\r\n                                                SET debt = debt + @amount\r\n                                                WHERE id = @id;\r\n\r\n                                                DECLARE @current_debt float;\r\n                                                SELECT @current_debt = debt FROM vpsPosCustomers WHERE id = @id;\r\n\r\n                                                UPDATE vpsPosDebtTransactions SET customer_id=@id,date=GETDATE(),type=@type,amount=@amount,current_debt=@current_debt WHERE id=@tId;\r\n                                       \r\n                                                ", connection2, transaction);

                                                                        sqlCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)id;
                                                                        sqlCommand.Parameters.AddWithValue("@type", (object)SqlDbType.VarChar).Value = (object)type;
                                                                        sqlCommand.Parameters.AddWithValue("@amount", (object)SqlDbType.Float).Value = (object)amount;
                                                                        sqlCommand.Parameters.AddWithValue("@prevAmount", (object)SqlDbType.Float).Value = (object)this.tDatatable.Rows[0][4].ToString();
                                                                        sqlCommand.Parameters.AddWithValue("@prevId", (object)SqlDbType.Int).Value = (object)this.tDatatable.Rows[0][1].ToString();
                                                                        sqlCommand.Parameters.AddWithValue("@tId", (object)SqlDbType.Int).Value = (object)tId;

                                                                        sqlCommand.ExecuteNonQuery();
                                                                        transaction.Commit();


                                                                        int num2 = (int)MessageBox.Show("Transaction added successfully to the database !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                                                    }
                                                                    catch (Exception error)
                                                                    {
                                                                        transaction.Rollback();
                                                                        MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                                        ErrorLogger.Log(error, "DebtForm", nameof(UpdateButton_Click), GetCurrentFormData());

                                                                    }
                                                                    finally
                                                                    {
                                                                        connection2.Close();
                                                                    }
                                                                }
                                                                break;
                                                            case "Pay":
                                                                using (SqlConnection connection3 = new SqlConnection(this.conString))
                                                                {
                                                                    connection3.Open();
                                                                    SqlTransaction transaction = connection3.BeginTransaction();
                                                                    try
                                                                    {
                                                                        SqlCommand sqlCommand = new SqlCommand("\r\n\r\n                                                UPDATE vpsPosCustomers SET debt=debt" + operation + "@prevAmount WHERE id=@prevId;\r\n\r\n                                                UPDATE vpsPosCustomers\r\n                                                SET debt = debt - @amount\r\n                                                WHERE id = @id;\r\n\r\n                                                DECLARE @current_debt float;\r\n                                                SELECT @current_debt = debt FROM vpsPosCustomers WHERE id = @id;\r\n\r\n                                                UPDATE vpsPosDebtTransactions SET customer_id=@id,date=GETDATE(),type=@type,amount=@amount,current_debt=@current_debt WHERE id=@tId;\r\n                                       \r\n\r\n                                           ", connection3, transaction);

                                                                        sqlCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)id;
                                                                        sqlCommand.Parameters.AddWithValue("@type", (object)SqlDbType.VarChar).Value = (object)type;
                                                                        sqlCommand.Parameters.AddWithValue("@amount", (object)SqlDbType.Float).Value = (object)amount;
                                                                        sqlCommand.Parameters.AddWithValue("@prevAmount", (object)SqlDbType.Float).Value = (object)this.tDatatable.Rows[0][4].ToString();
                                                                        sqlCommand.Parameters.AddWithValue("@prevId", (object)SqlDbType.Int).Value = (object)this.tDatatable.Rows[0][1].ToString();
                                                                        sqlCommand.Parameters.AddWithValue("@tId", (object)SqlDbType.Int).Value = (object)tId;

                                                                        sqlCommand.ExecuteNonQuery();
                                                                        transaction.Commit();

                                                                        int num3 = (int)MessageBox.Show("Transaction added successfully to the database !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                                                                    }
                                                                    catch (Exception error)
                                                                    {
                                                                        transaction.Rollback();
                                                                        MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                                        ErrorLogger.Log(error, "DebtForm", nameof(UpdateButton_Click), GetCurrentFormData());
                                                                    }
                                                                }

                                                                break;
                                                        }
                                                        this.tDatatable.Clear();
                                                        this.ClearControls();
                                                    }
                                                    else
                                                    {
                                                        int num4 = (int)MessageBox.Show("There is no customer with this name and id !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            int num5 = (int)MessageBox.Show(ex.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                            ErrorLogger.Log(ex, "DebtForm", nameof(UpdateButton_Click), GetCurrentFormData());
                                        }
                                    }*/
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ErrorLogger.Log(error, "DebtForm", nameof(UpdateButton_Click), GetCurrentFormData());
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            if (TIdTextBox.Text == "")
            {
                MessageBox.Show("Not selected a transaction !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {

                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        // 1. START the single transaction that covers EVERYTING
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {

                            string tId = TIdTextBox.Text;
                            string customerId = tDatatable.Rows[0][1].ToString();

                            // 2. perform the delete
                            string queryString = $@"
                        DELETE FROM vpsPosDebtTransactions WHERE id=@id;
                       ";

                            SqlCommand command = new SqlCommand(queryString, connection, transaction);

                            command.Parameters.AddWithValue("@id", SqlDbType.Int).Value = tId;
                            command.ExecuteNonQuery();

                            // 3. call the helper , passing the SAME transaction
                            // Now the helper becomes part of this safety net
                            FixCustomerBalance(customerId, connection, transaction);

                            // 4. Commit EVERYTHING together
                            transaction.Commit();

                            tDatatable.Clear();
                            ClearControls();

                            DebtGridView.DataSource = null;
                            getSearchResultToGrid();

                            MessageBox.Show("Transaction deleted successfully from the database !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception error)
                        {
                            // If power fails here , BOTH the delete AND the Fix are undone
                            transaction.Rollback();
                            MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            ErrorLogger.Log(error, "DebtForm", nameof(DeleteButton_Click), GetCurrentFormData());
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
                    ErrorLogger.Log(error, "DebtForm", nameof(DeleteButton_Click), GetCurrentFormData());
                }
            }
        }

        private void DebtGridView_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            if (this.DebtGridView.Rows.Count <= 0 || this.DebtGridView.Columns.Count != 7)
                return;
            if (this.DebtGridView.Columns[6].Name == "current_debt")
            {
                try
                {
                    string tId = this.DebtGridView.CurrentRow.Cells[0].Value.ToString();
                    using (SqlConnection connection = new SqlConnection(this.conString))
                    {
                        using (SqlCommand selectCommand = new SqlCommand("SELECT t.id,customer_id,c.name,type,amount FROM vpsPosDebtTransactions t\r\nJOIN vpsPosCustomers  c ON c.id = t.customer_id\r\nWHERE t.id=@id", connection))
                        {
                            selectCommand.Parameters.AddWithValue("@id", (object)SqlDbType.Int).Value = (object)tId;
                            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                            this.tDatatable.Clear();
                            sqlDataAdapter.Fill(this.tDatatable);
                        }

                        this.ClearControls();

                        this.TIdTextBox.Text = this.tDatatable.Rows[0][0].ToString();
                        this.NameComboBox.Text = this.tDatatable.Rows[0][2].ToString();
                        this.IdComboBox.Text = this.tDatatable.Rows[0][1].ToString();
                        this.TypeComboBox.SelectedItem = (object)this.tDatatable.Rows[0][3].ToString();
                        this.AmountTextBox.Text = this.tDatatable.Rows[0][4].ToString();
                    }
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    ErrorLogger.Log(ex, "DebtForm", nameof(DebtGridView_CellClick_1), GetCurrentFormData());
                }
            }
        }

        private void DateSearchButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("DebtForm", GetCurrentFormData());
            try
            {
                if (CategoryComboBox.Text == "transactions")
                {
                    InfoForm infoForm = new InfoForm();
                    infoForm.Show();
                    using (SqlConnection connection = new SqlConnection(conString))
                    {
                        connection.Open();
                        string queryString = "SELECT t.id,customer_id,c.name as name,date,type,amount,current_debt FROM vpsPosDebtTransactions t\r\nJOIN vpsPosCustomers  c ON c.id = t.customer_id\r\nWHERE t.date BETWEEN @startDate AND @endDate";
                        using (SqlCommand command = new SqlCommand(queryString, connection))
                        {
                            command.Parameters.AddWithValue("@startDate", SqlDbType.DateTime).Value = FromDateTimePicker.Value;
                            command.Parameters.AddWithValue("@endDate", SqlDbType.DateTime).Value = ToDateTimePicker.Value;

                            DataTable dataTable = new DataTable();
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            adapter.Fill(dataTable);
                            DebtGridView.DataSource = dataTable;
                        }
                    }
                    infoForm.Close();
                }
                else
                {
                    MessageBox.Show("Please select \"Transactions\" combo box !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "DebtForm", nameof(DateSearchButton_Click), GetCurrentFormData());
            }
        }

        private void DebtGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
