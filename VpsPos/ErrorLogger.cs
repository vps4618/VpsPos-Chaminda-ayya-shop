using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace VpsPos
{
    public static class ErrorLogger
    {

        public static string LastActiveForm = "UnknownForm";
        public static Dictionary<string, string> LastActiveFormData = new Dictionary<string, string>();

        // Update the form name and snapshot
        public static void UpdateFormData(string formName, Dictionary<string, string> data)
        {
            try
            {
                LastActiveForm = formName;
                LastActiveFormData = new Dictionary<string, string>(data);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error in logging", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Log error with rolling last 10 entries
        public static void Log(Exception ex, string formName, string methodName = "", Dictionary<string, string> extraData = null)
        {

            try
            {
                // getting no of errors should log in xml file.
                XmlDocument doc1 = new XmlDocument();

                doc1.Load("requiredInfo.xml");
                XmlNodeList nodes = doc1.GetElementsByTagName("noOfErrors");
                int noOfErrors = Convert.ToInt32(nodes[0].InnerText);

                // path converted to this cause , when app put in program files, since writing access doesnt have there, crashing occurs
                string logDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "VPSPOS", "Logs");

                Directory.CreateDirectory(logDir);
                string path = Path.Combine(logDir, "AppErrors.log");

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{formName}] [{methodName}] {ex}\n";

                if (extraData != null)
                {
                    logEntry += "Form Data:\n";
                    foreach (var kvp in extraData)
                    {
                        logEntry += $"  {kvp.Key}: {kvp.Value}\n";
                    }
                }
                logEntry += "---------------------\n";

                List<string> entries = new List<string>();

                if (File.Exists(path))
                {
                    // Split by the separator line
                    string[] existing = File.ReadAllText(path).Split(new[] { "---------------------\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var e in existing)
                    {
                        // Add back the separator (except for the last entry, which will get a new one below)
                        entries.Add(e.Trim() + "\n---------------------\n");
                    }
                }

                // Add the new entry
                entries.Add(logEntry);

                // Keep only the last noOfErrors from xml file
                if (entries.Count > noOfErrors)
                    entries = entries.GetRange(entries.Count - noOfErrors, noOfErrors);

                // Write back to file
                File.WriteAllText(path, string.Concat(entries));
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error in logging", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
