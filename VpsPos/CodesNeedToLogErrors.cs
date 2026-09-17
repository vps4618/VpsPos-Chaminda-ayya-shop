/*
// put this in every data changing function
ErrorLogger.UpdateFormData(form, GetCurrentFormData());

// data update function - insert this in every form and update it necessarily
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

// put this code in every catch block
ErrorLogger.Log(ex, formname, nameof(funtion), GetCurrentFormData());

*/