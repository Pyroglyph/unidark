using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace Unidark
{
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Stream stream = null;

            if (!e.Args.Any())
            {
                var dialog = new OpenFileDialog
                {
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Unity Editor|*.exe",
                    Multiselect = false,
                    InitialDirectory = "C:\\Program Files\\Unity\\Editor",
                    DefaultExt = "exe",
                    FileName = "C:\\Program Files\\Unity\\Editor\\Unity.exe"
                };

                if (dialog.ShowDialog() == true)
                    stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.ReadWrite);
                else
                    Environment.Exit(1);
            }
            else
            {
                string exePath = null;
                try
                {
                    exePath = e.Args.Single(arg => arg.Contains(".exe"));
                }
                catch
                {
                    ShowFatalError("An exectable was not provided.\nTry dropping Unity.exe onto Unidark.exe");
                }

                if (exePath != null)
                try
                {
                    stream = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite);
                }
                catch
                {
                    ShowFatalError($"Failed to open {exePath}.\nTry running Unidark as an administrator.");
                }
            }

            if (stream != null)
            {
                var mainWindow = new MainWindow(stream);
                mainWindow.Show();
            }
            else
            {
                ShowFatalError("Something has gone very very wrong!");
            }
        }

        public static void ShowFatalError(string message)
        {
            MessageBox.Show(message, "Unidark - Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }
}
