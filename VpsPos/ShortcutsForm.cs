using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace VpsPos
{
    public partial class ShortcutsForm : Form
    {
        public ShortcutsForm()
        {
            InitializeComponent();
        }
        // logging helper function
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Shortcut text", ShortcutsTextBox.Text}
    };

            return data;
        }


        private void ShortcutsForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("ShortcutsForm", GetCurrentFormData());
            string shortcutText = File.ReadAllText("shortcuts.txt");
            ShortcutsTextBox.Text = shortcutText;
        }
    }
}
