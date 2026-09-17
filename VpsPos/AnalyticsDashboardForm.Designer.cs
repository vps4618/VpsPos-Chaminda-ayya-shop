
namespace VpsPos
{
    partial class AnalyticsDashboardForm
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalyticsDashboardForm));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.txtSearchItem = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.btnSearchItem = new System.Windows.Forms.Button();
            this.dgvItemInsights = new System.Windows.Forms.DataGridView();
            this.chartItemSales = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cmbViewBy = new System.Windows.Forms.ComboBox();
            this.tabPage2.SuspendLayout();
            this.mainTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItemInsights)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartItemSales)).BeginInit();
            this.SuspendLayout();
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.cmbViewBy);
            this.tabPage2.Controls.Add(this.chartItemSales);
            this.tabPage2.Controls.Add(this.dgvItemInsights);
            this.tabPage2.Controls.Add(this.btnSearchItem);
            this.tabPage2.Controls.Add(this.dtpEndDate);
            this.tabPage2.Controls.Add(this.dtpStartDate);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.txtSearchItem);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1123, 762);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Item Insights";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 327);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Item : ";
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1123, 762);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Sales Overview";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // mainTabControl
            // 
            this.mainTabControl.Controls.Add(this.tabPage1);
            this.mainTabControl.Controls.Add(this.tabPage2);
            this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTabControl.Location = new System.Drawing.Point(0, 0);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(1131, 795);
            this.mainTabControl.TabIndex = 0;
            // 
            // txtSearchItem
            // 
            this.txtSearchItem.Location = new System.Drawing.Point(104, 321);
            this.txtSearchItem.Name = "txtSearchItem";
            this.txtSearchItem.Size = new System.Drawing.Size(170, 26);
            this.txtSearchItem.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 367);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Start Date : ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 403);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 20);
            this.label3.TabIndex = 3;
            this.label3.Text = "End Date : ";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new System.Drawing.Point(104, 361);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(200, 26);
            this.dtpStartDate.TabIndex = 4;
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEndDate.Location = new System.Drawing.Point(104, 403);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(200, 26);
            this.dtpEndDate.TabIndex = 5;
            // 
            // btnSearchItem
            // 
            this.btnSearchItem.Location = new System.Drawing.Point(325, 400);
            this.btnSearchItem.Name = "btnSearchItem";
            this.btnSearchItem.Size = new System.Drawing.Size(92, 29);
            this.btnSearchItem.TabIndex = 6;
            this.btnSearchItem.Text = "Search";
            this.btnSearchItem.UseVisualStyleBackColor = true;
            this.btnSearchItem.Click += new System.EventHandler(this.btnSearchItem_Click);
            // 
            // dgvItemInsights
            // 
            this.dgvItemInsights.AllowUserToAddRows = false;
            this.dgvItemInsights.AllowUserToDeleteRows = false;
            this.dgvItemInsights.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvItemInsights.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItemInsights.Location = new System.Drawing.Point(6, 435);
            this.dgvItemInsights.Name = "dgvItemInsights";
            this.dgvItemInsights.ReadOnly = true;
            this.dgvItemInsights.RowHeadersWidth = 62;
            this.dgvItemInsights.RowTemplate.Height = 28;
            this.dgvItemInsights.Size = new System.Drawing.Size(520, 264);
            this.dgvItemInsights.TabIndex = 7;
            this.dgvItemInsights.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItemInsights_CellClick);
            this.dgvItemInsights.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvItemInsights_CellContentClick);
            // 
            // chartItemSales
            // 
            chartArea1.Name = "ChartArea1";
            this.chartItemSales.ChartAreas.Add(chartArea1);
            this.chartItemSales.Dock = System.Windows.Forms.DockStyle.Top;
            legend1.Name = "Legend1";
            this.chartItemSales.Legends.Add(legend1);
            this.chartItemSales.Location = new System.Drawing.Point(3, 3);
            this.chartItemSales.Name = "chartItemSales";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chartItemSales.Series.Add(series1);
            this.chartItemSales.Size = new System.Drawing.Size(1117, 283);
            this.chartItemSales.TabIndex = 8;
            this.chartItemSales.Text = "chart1";
            // 
            // cmbViewBy
            // 
            this.cmbViewBy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbViewBy.FormattingEnabled = true;
            this.cmbViewBy.Items.AddRange(new object[] {
            "Day",
            "Month",
            "Year"});
            this.cmbViewBy.Location = new System.Drawing.Point(325, 327);
            this.cmbViewBy.Name = "cmbViewBy";
            this.cmbViewBy.Size = new System.Drawing.Size(121, 28);
            this.cmbViewBy.TabIndex = 9;
            this.cmbViewBy.SelectedIndexChanged += new System.EventHandler(this.cmbViewBy_SelectedIndexChanged);
            // 
            // AnalyticsDashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1131, 795);
            this.ControlBox = false;
            this.Controls.Add(this.mainTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AnalyticsDashboardForm";
            this.ShowIcon = false;
            this.Text = "Analytics Dashboard";
            this.Load += new System.EventHandler(this.AnalyticsDashboardForm_Load);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.mainTabControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvItemInsights)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartItemSales)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabControl mainTabControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvItemInsights;
        private System.Windows.Forms.Button btnSearchItem;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSearchItem;
        private System.Windows.Forms.ComboBox cmbViewBy;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartItemSales;
    }
}