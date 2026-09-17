namespace VpsPos
{
    partial class BillsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BillsForm));
            this.BillsGridView = new System.Windows.Forms.DataGridView();
            this.PrintButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.ViewButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.SearchComboBox = new System.Windows.Forms.ComboBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.DateEnd = new System.Windows.Forms.DateTimePicker();
            this.DateStart = new System.Windows.Forms.DateTimePicker();
            this.CloseButton = new System.Windows.Forms.Button();
            this.noOfBillsLabel = new System.Windows.Forms.Label();
            this.NoOfBillsTextBox = new System.Windows.Forms.TextBox();
            this.SearchTextBoxButton = new System.Windows.Forms.Button();
            this.SearchCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.columnHideTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.BillsGridView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BillsGridView
            // 
            this.BillsGridView.AllowUserToAddRows = false;
            this.BillsGridView.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            this.BillsGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.BillsGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BillsGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.BillsGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.BillsGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.BillsGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.BillsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.BillsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PrintButtonColumn,
            this.ViewButtonColumn});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.BillsGridView.DefaultCellStyle = dataGridViewCellStyle5;
            this.BillsGridView.Location = new System.Drawing.Point(23, 178);
            this.BillsGridView.Margin = new System.Windows.Forms.Padding(4);
            this.BillsGridView.Name = "BillsGridView";
            this.BillsGridView.ReadOnly = true;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.BillsGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.BillsGridView.RowHeadersVisible = false;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.Black;
            this.BillsGridView.RowsDefaultCellStyle = dataGridViewCellStyle7;
            this.BillsGridView.Size = new System.Drawing.Size(965, 459);
            this.BillsGridView.TabIndex = 0;
            this.BillsGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BillsGridView_CellClick);
            this.BillsGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.BillsGridView_CellContentClick);
            // 
            // PrintButtonColumn
            // 
            this.PrintButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PrintButtonColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.PrintButtonColumn.FillWeight = 5.076141F;
            this.PrintButtonColumn.HeaderText = "";
            this.PrintButtonColumn.Name = "PrintButtonColumn";
            this.PrintButtonColumn.ReadOnly = true;
            this.PrintButtonColumn.Text = "Print";
            this.PrintButtonColumn.UseColumnTextForButtonValue = true;
            this.PrintButtonColumn.Width = 5;
            // 
            // ViewButtonColumn
            // 
            this.ViewButtonColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ViewButtonColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.ViewButtonColumn.FillWeight = 194.9239F;
            this.ViewButtonColumn.HeaderText = "";
            this.ViewButtonColumn.Name = "ViewButtonColumn";
            this.ViewButtonColumn.ReadOnly = true;
            this.ViewButtonColumn.Text = "View";
            this.ViewButtonColumn.UseColumnTextForButtonValue = true;
            this.ViewButtonColumn.Width = 5;
            // 
            // RefreshButton
            // 
            this.RefreshButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.RefreshButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.RefreshButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RefreshButton.ForeColor = System.Drawing.SystemColors.Desktop;
            this.RefreshButton.Location = new System.Drawing.Point(612, 78);
            this.RefreshButton.Margin = new System.Windows.Forms.Padding(4);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(127, 36);
            this.RefreshButton.TabIndex = 16;
            this.RefreshButton.Text = "Refresh";
            this.RefreshButton.UseVisualStyleBackColor = false;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(4, 18);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(185, 28);
            this.label9.TabIndex = 30;
            this.label9.Text = "Search :";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchTextBox.Location = new System.Drawing.Point(583, 15);
            this.SearchTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(185, 34);
            this.SearchTextBox.TabIndex = 29;
            this.SearchTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SearchTextBox_KeyUp);
            // 
            // SearchComboBox
            // 
            this.SearchComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SearchComboBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchComboBox.FormattingEnabled = true;
            this.SearchComboBox.Items.AddRange(new object[] {
            "receiptId",
            "cashierFormId",
            "customerName",
            "employeeName",
            "totalAmount",
            "paidAmount",
            "change"});
            this.SearchComboBox.Location = new System.Drawing.Point(197, 14);
            this.SearchComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.SearchComboBox.Name = "SearchComboBox";
            this.SearchComboBox.Size = new System.Drawing.Size(185, 36);
            this.SearchComboBox.TabIndex = 28;
            // 
            // SearchButton
            // 
            this.SearchButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SearchButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.SearchButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchButton.Location = new System.Drawing.Point(416, 75);
            this.SearchButton.Margin = new System.Windows.Forms.Padding(4);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(133, 41);
            this.SearchButton.TabIndex = 33;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = false;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // DateEnd
            // 
            this.DateEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.DateEnd.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateEnd.CustomFormat = "yyyy-MM-dd HH:mm";
            this.DateEnd.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DateEnd.Location = new System.Drawing.Point(197, 80);
            this.DateEnd.Margin = new System.Windows.Forms.Padding(4);
            this.DateEnd.Name = "DateEnd";
            this.DateEnd.Size = new System.Drawing.Size(185, 32);
            this.DateEnd.TabIndex = 32;
            // 
            // DateStart
            // 
            this.DateStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.DateStart.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateStart.CustomFormat = "yyyy-MM-dd HH:mm";
            this.DateStart.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DateStart.Location = new System.Drawing.Point(4, 80);
            this.DateStart.Margin = new System.Windows.Forms.Padding(4);
            this.DateStart.Name = "DateStart";
            this.DateStart.Size = new System.Drawing.Size(185, 32);
            this.DateStart.TabIndex = 31;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CloseButton.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.CloseButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseButton.ForeColor = System.Drawing.SystemColors.Desktop;
            this.CloseButton.Location = new System.Drawing.Point(872, 14);
            this.CloseButton.Margin = new System.Windows.Forms.Padding(4);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(89, 36);
            this.CloseButton.TabIndex = 34;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // noOfBillsLabel
            // 
            this.noOfBillsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.noOfBillsLabel.AutoSize = true;
            this.noOfBillsLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noOfBillsLabel.Location = new System.Drawing.Point(776, 73);
            this.noOfBillsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.noOfBillsLabel.Name = "noOfBillsLabel";
            this.noOfBillsLabel.Size = new System.Drawing.Size(88, 46);
            this.noOfBillsLabel.TabIndex = 36;
            this.noOfBillsLabel.Text = "No. Of Bills :";
            this.noOfBillsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // NoOfBillsTextBox
            // 
            this.NoOfBillsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NoOfBillsTextBox.Enabled = false;
            this.NoOfBillsTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoOfBillsTextBox.Location = new System.Drawing.Point(872, 81);
            this.NoOfBillsTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.NoOfBillsTextBox.Name = "NoOfBillsTextBox";
            this.NoOfBillsTextBox.Size = new System.Drawing.Size(89, 29);
            this.NoOfBillsTextBox.TabIndex = 35;
            // 
            // SearchTextBoxButton
            // 
            this.SearchTextBoxButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SearchTextBoxButton.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.SearchTextBoxButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchTextBoxButton.ForeColor = System.Drawing.SystemColors.Desktop;
            this.SearchTextBoxButton.Location = new System.Drawing.Point(776, 14);
            this.SearchTextBoxButton.Margin = new System.Windows.Forms.Padding(4);
            this.SearchTextBoxButton.Name = "SearchTextBoxButton";
            this.SearchTextBoxButton.Size = new System.Drawing.Size(88, 36);
            this.SearchTextBoxButton.TabIndex = 37;
            this.SearchTextBoxButton.Text = "Search";
            this.SearchTextBoxButton.UseVisualStyleBackColor = false;
            this.SearchTextBoxButton.Click += new System.EventHandler(this.SearchTextBoxButton_Click);
            // 
            // SearchCheckBox
            // 
            this.SearchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchCheckBox.AutoSize = true;
            this.SearchCheckBox.Checked = true;
            this.SearchCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SearchCheckBox.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchCheckBox.Location = new System.Drawing.Point(390, 17);
            this.SearchCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.SearchCheckBox.Name = "SearchCheckBox";
            this.SearchCheckBox.Size = new System.Drawing.Size(185, 29);
            this.SearchCheckBox.TabIndex = 38;
            this.SearchCheckBox.Text = "Search With Button";
            this.SearchCheckBox.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.NoOfBillsTextBox, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.noOfBillsLabel, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.SearchTextBoxButton, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.SearchCheckBox, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.RefreshButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.SearchButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.CloseButton, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.DateEnd, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.SearchComboBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.DateStart, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.SearchTextBox, 3, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(23, 15);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(965, 128);
            this.tableLayoutPanel1.TabIndex = 39;
            // 
            // columnHideTimer
            // 
            this.columnHideTimer.Interval = 300000;
            this.columnHideTimer.Tick += new System.EventHandler(this.columnHideTimer_Tick);
            // 
            // BillsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1013, 652);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.BillsGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BillsForm";
            this.Text = "Bills";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.BillsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.BillsGridView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView BillsGridView;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.ComboBox SearchComboBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.DateTimePicker DateEnd;
        private System.Windows.Forms.DateTimePicker DateStart;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Label noOfBillsLabel;
        private System.Windows.Forms.TextBox NoOfBillsTextBox;
        private System.Windows.Forms.Button SearchTextBoxButton;
        private System.Windows.Forms.CheckBox SearchCheckBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridViewButtonColumn PrintButtonColumn;
        private System.Windows.Forms.DataGridViewButtonColumn ViewButtonColumn;
        private System.Windows.Forms.Timer columnHideTimer;
    }
}