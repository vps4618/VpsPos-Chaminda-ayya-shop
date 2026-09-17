using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VpsPos
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // This added to prevent creation of new instance of app when one is running
            // Get current process
            Process current = Process.GetCurrentProcess();

            // Check if another process with the same name is already running
            if (Process.GetProcessesByName(current.ProcessName).Length > 1)
            {
                MessageBox.Show("The application is already running.");
                return; // Exit this new instance
            }

            // error logging
            Application.ThreadException += (sender, args) =>
            {
                ErrorLogger.Log(args.Exception, ErrorLogger.LastActiveForm, "ThreadException", ErrorLogger.LastActiveFormData);

                // Optionally show your own MessageBox
                MessageBox.Show("An error occurred: " + args.Exception.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);

                // If you do NOT rethrow or exit, the default .NET MessageBox will not appear
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                // args.ExceptionObject may not always be an Exception
                var ex = args.ExceptionObject as Exception;
                if (ex != null)
                    ErrorLogger.Log(ex, ErrorLogger.LastActiveForm, "UnhandledException", ErrorLogger.LastActiveFormData);
                else
                    ErrorLogger.Log(new Exception(args.ExceptionObject.ToString()), ErrorLogger.LastActiveForm, "UnhandledException", ErrorLogger.LastActiveFormData);

                // Optionally show your own MessageBox
                MessageBox.Show("Critical error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // If you let the process continue, the default dialog won't appear.
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
