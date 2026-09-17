using System;
using System.Drawing;
using System.Windows.Forms;

public static class SecurityInputBox
{
    public static DialogResult Show(out string customerName, out string cashierName, out string reason)
    {
        Form prompt = new Form()
        {
            Width = 750,
            AutoSize=true,
            Height = 400,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = "Security Audit - Mandatory Signature",
            StartPosition = FormStartPosition.CenterScreen,
            MinimizeBox = false,
            MaximizeBox = false,
            ControlBox = false // Removes 'X' button so they MUST Sign or Cancel
        };

        // Warning Label
        Label lblWarn = new Label() { Left = 20, Top = 10, Width = 750, Height = 40, Text = "⚠️ You are reducing a saved bill value.\nTo authorize this, you must SIGN below. \nඔබ කලින් ඇතුලත් කල බිලක අගය අඩු කරමින් ඇත. ඉදිරියට යෑමට පහත කරුනු ඇතුලත් කරන්න.\nමෙම වෙනස්වීම පිලිබද විස්තර ප්‍රධානියාට දැනුම් දෙනු ඇත.", ForeColor = Color.Red, Font = new Font("Segoe UI", 9 ),AutoSize=true };

        // 1. Customer Name
        Label lblCust = new Label() { Left = 20, Top = 80, Text = "Customer Name / Description (පාරිභෝගිකයාගේ නම/විස්තරය):",AutoSize=true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        TextBox txtCust = new TextBox() { Left = 20, Top = 105, Width = 700, Font = new Font("Segoe UI", 9) };

        // 2. Cashier Signature (The most important field)
        Label lblUser = new Label() { Left = 20, Top = 150, Text = "Your Name (Cashier Signature) (ඔබේ නම):", Font = new Font("Segoe UI", 9, FontStyle.Bold),AutoSize=true };
        TextBox txtUser = new TextBox() { Left = 20, Top = 175, Width = 700 , Font = new Font("Segoe UI", 9) };
        Label lblHint = new Label() { Left = 20, Top = 200, Width = 390, Text = "Do not leave empty. (මෙය හිස්ව නොතබන්න)", ForeColor = Color.Gray,AutoSize=true };

        // 3. Reason
        Label lblReason = new Label() { Left = 20, Top = 230, Text = "Reason for reduction (බිලේ අගය අඩුකිරීමට හේතුව):",AutoSize=true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        TextBox txtReason = new TextBox() { Left = 20, Top = 255, Width = 700 , Font = new Font("Segoe UI", 9) };

        // Buttons
        Button btnConfirm = new Button() { Text = "Sign & Authorize (ඉදිරියට යන්න)", Left = 460, Width = 150, Top = 310, DialogResult = DialogResult.OK, Enabled = false, BackColor = Color.LightGray ,AutoSize=true};
        Button btnCancel = new Button() { Text = "Cancel Transaction (වෙනස් කිරීම නවත්වන්න)", Left = 100, Width = 150, Top = 310, DialogResult = DialogResult.Cancel ,AutoSize=true };

        // Logic: Button enables ONLY if fields are full
        EventHandler validator = (s, e) => {
            bool valid = !string.IsNullOrWhiteSpace(txtCust.Text) &&
                         !string.IsNullOrWhiteSpace(txtUser.Text) &&
                         txtUser.Text.Length > 2 && !string.IsNullOrWhiteSpace(txtReason.Text) && txtReason.Text.Length > 5 && txtCust.Text.Length > 2;
            btnConfirm.Enabled = valid;
            btnConfirm.BackColor = valid ? Color.LightGreen : Color.LightGray;
        };

        txtCust.TextChanged += validator;
        txtUser.TextChanged += validator;
        txtReason.TextChanged += validator;

        prompt.Controls.AddRange(new Control[] { lblWarn, lblCust, txtCust, lblUser, txtUser, lblHint, lblReason, txtReason, btnConfirm, btnCancel });
        prompt.AcceptButton = btnConfirm;

        DialogResult result = prompt.ShowDialog();

        customerName = txtCust.Text;
        cashierName = txtUser.Text;
        reason = txtReason.Text;

        return result;
    }
}