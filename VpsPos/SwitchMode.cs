using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace VpsPos
{
    class SwitchMode
    {
        // --- MODERN DARK PALETTE ---
        private static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);      // Main Background
        private static readonly Color DarkSurface = Color.FromArgb(45, 45, 48);         // Headers
        private static readonly Color DarkControl = Color.FromArgb(60, 60, 60);         // TextBoxes/Inputs
        private static readonly Color LightText = Color.FromArgb(240, 240, 240);        // Off-white text
        private static readonly Color DarkBorder = Color.FromArgb(80, 80, 80);          // Borders
        private static readonly Color AccentBlue = Color.FromArgb(0, 122, 204);         // Selection

        public static void ApplyTheme(Control parent, bool darkMode)
        {
            if (parent is Form)
            {
                parent.BackColor = darkMode ? DarkBackground : SystemColors.Control;
                parent.ForeColor = darkMode ? LightText : SystemColors.ControlText;
            }

            foreach (Control c in parent.Controls)
            {
                ApplyToControl(c, darkMode);
                if (c.HasChildren) ApplyTheme(c, darkMode);
            }
        }

        private static void ApplyToControl(Control c, bool darkMode)
        {
            c.ForeColor = darkMode ? LightText : SystemColors.ControlText;

            // --- DATAGRIDVIEW ---
            if (c is DataGridView dgv)
            {
                ApplyDataGridViewStyle(dgv, darkMode);
                return;
            }

            // --- BUTTONS ---
            if (c is Button btn)
            {
                if (darkMode)
                {
                    btn.BackColor = DarkSurface;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = DarkBorder;
                }
                else
                {
                    // Reverting to standard system look. 
                    // Note: This won't restore custom Designer colors (like GradientActiveCaption)
                    // unless we hardcode them, but it ensures a clean Light mode.
                    btn.BackColor = SystemColors.Control;
                    btn.UseVisualStyleBackColor = true;
                    btn.FlatStyle = FlatStyle.Standard;
                }
                return;
            }

            // --- COMBOBOX ---
            if (c is ComboBox cb)
            {
                if (darkMode)
                {
                    cb.BackColor = DarkControl;
                    cb.FlatStyle = FlatStyle.Flat;
                    cb.DrawMode = DrawMode.OwnerDrawFixed;
                    cb.DrawItem -= ComboBox_DrawItem;
                    cb.DrawItem += ComboBox_DrawItem;
                }
                else
                {
                    cb.BackColor = SystemColors.Window;
                    cb.FlatStyle = FlatStyle.Standard;
                    cb.DrawMode = DrawMode.Normal;
                    cb.DrawItem -= ComboBox_DrawItem;
                }
                return;
            }

            // --- INPUTS ---
            if (c is TextBox || c is NumericUpDown || c is RichTextBox)
            {
                if (darkMode)
                {
                    c.BackColor = DarkControl;
                    if (c is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;
                    if (c is RichTextBox rtb) rtb.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    c.BackColor = SystemColors.Window;
                    if (c is TextBox tb) tb.BorderStyle = BorderStyle.Fixed3D;
                    if (c is RichTextBox rtb) rtb.BorderStyle = BorderStyle.Fixed3D;
                }
                return;
            }

            // --- CONTAINERS ---
            if (c is Panel || c is GroupBox || c is TabControl || c is TableLayoutPanel)
            {
                c.BackColor = darkMode ? DarkBackground : SystemColors.Control;
            }
        }

        // Helper for ComboBox dark dropdown
        private static void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            ComboBox cb = sender as ComboBox;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = isSelected ? AccentBlue : DarkControl;
            Color textColor = isSelected ? Color.White : LightText;

            using (SolidBrush bgBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            string text = cb.Items[e.Index].ToString();
            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Near;
                e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds, sf);
                }
            }
        }

        // --- FIXED DATAGRIDVIEW STYLE ---
        private static void ApplyDataGridViewStyle(DataGridView dgv, bool darkMode)
        {
            if (darkMode)
            {
                dgv.BackgroundColor = DarkBackground;
                dgv.GridColor = DarkBorder;
                dgv.BorderStyle = BorderStyle.None;
                dgv.EnableHeadersVisualStyles = false;

                // Headers
                dgv.ColumnHeadersDefaultCellStyle.BackColor = DarkSurface;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = LightText;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = DarkSurface;
                dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = LightText;
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

                // Rows
                dgv.DefaultCellStyle.BackColor = DarkBackground;
                dgv.DefaultCellStyle.ForeColor = LightText;
                dgv.DefaultCellStyle.SelectionBackColor = AccentBlue;
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;

                // Fix Zebra & Row Style layers
                dgv.RowsDefaultCellStyle.BackColor = DarkBackground;
                dgv.RowsDefaultCellStyle.ForeColor = LightText;
                dgv.RowsDefaultCellStyle.SelectionBackColor = AccentBlue;
                dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;

                dgv.AlternatingRowsDefaultCellStyle.BackColor = DarkBackground;
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = LightText;
                dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = AccentBlue;
                dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                // --- RESTORE ORIGINAL DESIGNER SETTINGS ---

                // 1. Grid Basics
                dgv.BackgroundColor = SystemColors.AppWorkspace; // Standard default
                dgv.GridColor = SystemColors.ControlDark;        // Standard default
                dgv.BorderStyle = BorderStyle.Fixed3D;           // Original 3D Border
                dgv.EnableHeadersVisualStyles = true;

                // 2. Column Headers (Matches dataGridViewCellStyle2)
                dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.GradientActiveCaption;
                dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised; // Default look

                // 3. Default Cell Style (Matches dataGridViewCellStyle3)
                dgv.DefaultCellStyle.BackColor = SystemColors.InactiveBorder;
                dgv.DefaultCellStyle.ForeColor = SystemColors.ControlText;
                dgv.DefaultCellStyle.SelectionBackColor = SystemColors.GradientActiveCaption;
                dgv.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;

                // 4. Rows Default Style (Matches dataGridViewCellStyle5)
                dgv.RowsDefaultCellStyle.BackColor = Color.Empty; // Let DefaultCellStyle take over
                dgv.RowsDefaultCellStyle.ForeColor = Color.Empty;
                dgv.RowsDefaultCellStyle.SelectionBackColor = SystemColors.GradientActiveCaption;
                dgv.RowsDefaultCellStyle.SelectionForeColor = Color.Black;

                // 5. Alternating Rows (Matches dataGridViewCellStyle1)
                // Note: Designer only set Font/Selection, not BackColor, so BackColor is empty (zebra off or default)
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.Empty;
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = Color.Empty;
                dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = SystemColors.GradientActiveCaption;
                dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.Black;
            }
        }
    }
}