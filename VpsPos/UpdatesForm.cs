using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VpsPos
{
    public partial class UpdatesForm : Form
    {
        public UpdatesForm()
        {
            InitializeComponent();
        }

        // logging helper function
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Shortcut text",UpdatesRichTextBox.Text}
    };

            return data;
        }

        private void UpdatesForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("UpdatesForm", GetCurrentFormData());
            string updatesText = File.ReadAllText("versionLogVpsPos.txt");
            UpdatesRichTextBox.Text = updatesText;
        }
    }
}
