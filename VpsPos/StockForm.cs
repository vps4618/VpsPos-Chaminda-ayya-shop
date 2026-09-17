using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VpsPos
{
    public partial class StockForm : Form
    {
        private DatabaseClass databaseClass = new DatabaseClass();

        public StockForm()
        {
            InitializeComponent();
        }

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Search type", SearchStockComboBox.Text},
        { "Search Text", SearchStockTextBox.Text}
    };

            return data;
        }

        private void StockForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("StockForm", GetCurrentFormData());
            this.WindowState = FormWindowState.Maximized;
            SearchStockComboBox.SelectedIndex = 0;
            StockGridView.DataSource = this.databaseClass.executeSqlCommand("SELECT s.ITEM_ID,i.ItemName,s.Quantity,s.SHELF_ID FROM stock s JOIN iteminformation i ON i.ITEM_ID = s.ITEM_ID");
        }

        private void SearchStockTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ErrorLogger.UpdateFormData("StockForm", GetCurrentFormData());
            string searchColumn = SearchStockComboBox.Text;
            string sqlcommand = @"SELECT s.ITEM_ID,i.ItemName,s.Quantity,s.SHELF_ID FROM stock s JOIN iteminformation i ON i.ITEM_ID = s.ITEM_ID
 WHERE s." + searchColumn + " like N'" + SearchStockTextBox.Text + "%'";
            StockGridView.DataSource = this.databaseClass.executeSqlCommand(sqlcommand);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("StockForm", GetCurrentFormData());
            this.Close();
        }
    }
}
