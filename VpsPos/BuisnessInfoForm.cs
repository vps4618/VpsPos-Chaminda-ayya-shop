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
    public partial class BuisnessInfoForm : Form
    {

        XmlDocument doc = new XmlDocument();
        string conString = "";

        public BuisnessInfoForm()
        {
            InitializeComponent();
        }

        // log helper function 
        private Dictionary<string, string> GetCurrentFormData()
        {
            var data = new Dictionary<string, string>
    {   { "Business Name", NameTextBox.Text},
    { "Address", AddressTextBox.Text},
    { "Phone number 1", PhoneNumber1TextBox.Text},
    { "Phone number 2", PhoneNumber2TextBox.Text},
    { "Email", EmailTextBox.Text},
    { "Web address", WebAddressTextBox.Text},
    };


            return data;
        }

        private void BuisnessInfoForm_Load(object sender, EventArgs e)
        {

            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            try
            {
                doc.Load("requiredInfo.xml");
                XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
                conString = nodes[0].InnerText;

                this.WindowState = FormWindowState.Maximized;

                DataTable buisnessInfoDataTable = new DataTable();
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "SELECT * FROM vpsPosBuisnessInfo";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(buisnessInfoDataTable);
                    }
                }

                NameTextBox.Text = buisnessInfoDataTable.Rows[0][0].ToString();
                AddressTextBox.Text = buisnessInfoDataTable.Rows[0][1].ToString();
                PhoneNumber1TextBox.Text = buisnessInfoDataTable.Rows[0][2].ToString();
                PhoneNumber2TextBox.Text = buisnessInfoDataTable.Rows[0][3].ToString();
                EmailTextBox.Text = buisnessInfoDataTable.Rows[0][4].ToString();
                WebAddressTextBox.Text = buisnessInfoDataTable.Rows[0][5].ToString();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Log(error, "BusinessInfoForm", nameof(BuisnessInfoForm_Load), GetCurrentFormData());
            }
            
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {

            ErrorLogger.UpdateFormData("CashierForm", GetCurrentFormData());
            if (NameTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter buisness name !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (AddressTextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter buisness address !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else if (PhoneNumber1TextBox.Text.Trim() == "" && PhoneNumber2TextBox.Text.Trim() == "")
            {
                MessageBox.Show("Please enter a phone number !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else
            {
                string name = NameTextBox.Text.Trim();
                string address = AddressTextBox.Text.Trim();
                string phoneNumber1 = PhoneNumber1TextBox.Text.Trim();
                string phoneNumber2 = PhoneNumber2TextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string webAddress = WebAddressTextBox.Text.Trim();


                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string queryString = "UPDATE vpsPosBuisnessInfo SET name=@name,address=@address,firstPhoneNumber=@firstPhoneNumber,secondPhoneNumber=@secondPhoneNumber,email=@email,webAddress=@webAddress";
                    using (SqlCommand command = new SqlCommand(queryString, connection))
                    {
                        command.Parameters.AddWithValue("@name", SqlDbType.NVarChar).Value = name;
                        command.Parameters.AddWithValue("@address", SqlDbType.NVarChar).Value = address;
                        command.Parameters.AddWithValue("@firstPhoneNumber", SqlDbType.VarChar).Value = phoneNumber1;
                        command.Parameters.AddWithValue("@secondPhoneNumber", SqlDbType.VarChar).Value = phoneNumber2;
                        command.Parameters.AddWithValue("@email", SqlDbType.VarChar).Value = email;
                        command.Parameters.AddWithValue("@webAddress", SqlDbType.VarChar).Value = webAddress;
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Buisness Info Updated Successfully !", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
