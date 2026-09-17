namespace VpsPos
{
    partial class ShortcutsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutsForm));
            this.ShortcutsTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ShortcutsTextBox
            // 
            this.ShortcutsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ShortcutsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ShortcutsTextBox.Font = new System.Drawing.Font("Nirmala UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShortcutsTextBox.Location = new System.Drawing.Point(36, 19);
            this.ShortcutsTextBox.Margin = new System.Windows.Forms.Padding(40, 50, 40, 50);
            this.ShortcutsTextBox.Name = "ShortcutsTextBox";
            this.ShortcutsTextBox.ReadOnly = true;
            this.ShortcutsTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.ShortcutsTextBox.Size = new System.Drawing.Size(913, 653);
            this.ShortcutsTextBox.TabIndex = 0;
            this.ShortcutsTextBox.Text = "";
            // 
            // ShortcutsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 692);
            this.Controls.Add(this.ShortcutsTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShortcutsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shortcuts";
            this.Load += new System.EventHandler(this.ShortcutsForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox ShortcutsTextBox;
    }
}