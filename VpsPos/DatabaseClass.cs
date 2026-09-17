// previous code before 2025/08/26

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace VpsPos
//{
//    class DatabaseClass
//    {

//        //Data Source
//        private string strcon;
//        XmlDocument doc = new XmlDocument();

//        //SQL connection object
//        private SqlConnection Sqlcon;

//        public DatabaseClass()
//        {
//            doc.Load("requiredInfo.xml");
//            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
//            strcon = nodes[0].InnerText;

//            Sqlcon = new SqlConnection(this.strcon);
//        }

//        public DataTable executeSqlCommand(string sqlCommand)
//        {
//            DataTable dtTable = new DataTable();
//            try
//            {
//                //open sql connection
//                this.Sqlcon.Open();
//                //create sqlAdapter
//                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand,this.Sqlcon);
//                dataAdapter.Fill(dtTable);
//                this.Sqlcon.Close();
//            }
//            catch(Exception error)
//            {
//                Console.WriteLine(error.ToString());
//                dtTable = null;
//            }
//            return dtTable;

//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace VpsPos
{
    class DatabaseClass
    {
        private string strcon;
        XmlDocument doc = new XmlDocument();

        // SQL connection object
        private SqlConnection Sqlcon;

        public DatabaseClass()
        {
            doc.Load("requiredInfo.xml");
            XmlNodeList nodes = doc.GetElementsByTagName("sqlString");
            strcon = nodes[0].InnerText;

            Sqlcon = new SqlConnection(this.strcon);
        }

        // Existing method for backward compatibility
        public DataTable executeSqlCommand(string sqlCommand)
        {
            DataTable dtTable = new DataTable();
            try
            {
                this.Sqlcon.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand, this.Sqlcon);
                dataAdapter.Fill(dtTable);
                this.Sqlcon.Close();
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
                dtTable = null;
            }
            return dtTable;
        }

        // New method for transactions and parameters
        public DataTable executeSqlCommand(string sqlCommand, SqlConnection connection, SqlTransaction transaction, Dictionary<string, object> parameters = null)
        {
            DataTable dtTable = new DataTable();
            try
            {
                using (SqlCommand cmd = new SqlCommand(sqlCommand, connection, transaction))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dtTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                dtTable = null;
            }
            return dtTable;
        }
    }
}
