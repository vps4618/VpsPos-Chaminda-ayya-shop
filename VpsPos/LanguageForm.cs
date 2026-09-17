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
    public partial class LanguageForm : Form
    {
        public LanguageForm()
        {
            InitializeComponent();
        }

        // logging helper method
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Language", LanguageComboBox.Text}
    };
            return data;
        }

        XmlDocument doc = new XmlDocument();
        string conString = "";

        private void LanguageForm_Load(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("LanguageForm", GetCurrentFormData());
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            conString = nodes[0].InnerText;

            try
            {
                DataTable languageDatatable = new DataTable();
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosLanguage";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(languageDatatable);
                    }
                }

                if (languageDatatable.Rows[0][0].ToString() == "Sinhala")
                {
                    LanguageComboBox.SelectedIndex = 0;
                }
                else if (languageDatatable.Rows[0][0].ToString() == "English")
                {
                    LanguageComboBox.SelectedIndex = 1;
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "LanguageForm", nameof(LanguageForm_Load), GetCurrentFormData());
            }
            this.WindowState = FormWindowState.Maximized;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("LanguageForm", GetCurrentFormData());
            this.Close();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            ErrorLogger.UpdateFormData("LanguageForm", GetCurrentFormData());
            try
            {
                string language = LanguageComboBox.Text;
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "UPDATE vpsPosLanguage SET language=@language";
                    using(SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@language", SqlDbType.VarChar).Value = language;
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Language updated successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }catch(Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "LanguageForm", nameof(UpdateButton_Click), GetCurrentFormData());
            }
        }
    }
}
