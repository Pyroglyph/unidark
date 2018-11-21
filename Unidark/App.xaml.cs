using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Unidark
{
    public partial class App
    {
        private const string DEFAULT_UNITY_DIR = "C:\\Program Files\\Unity\\Editor";
        private const string FALLBACK_DIR = "C:\\Program Files";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Stream stream = null;

            if (!e.Args.Any())
            {
                if (Directory.Exists(DEFAULT_UNITY_DIR) && File.Exists(DEFAULT_UNITY_DIR + "\\Unity.exe"))
                {
                    var result = MessageBox.Show(
                        "Unity installation detected at:\n" +
                        DEFAULT_UNITY_DIR + "\\Unity.exe\n\n" +
                        "Do you want to patch this? Select Yes to patch, or select No to choose another installation.",
                        "Unidark - Unity installation detected", 
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            stream = TryOpenFile(DEFAULT_UNITY_DIR + "\\Unity.exe");
                            break;
                        case MessageBoxResult.No:
                            stream = OpenFileStreamFromDialog(e.Args);
                            break;
                        default:
                            Environment.Exit(0);
                            break;
                    }
                }
                else
                {
                    stream = OpenFileStreamFromDialog(e.Args);
                }
            }
            else
            {
                string exePath = null;
                try
                {
                    if (e.Args.Any(arg => arg.EndsWith(".exe")))
                    {
                        exePath = e.Args.Single(arg => arg.EndsWith(".exe"));
                    }
                    else if (e.Args.Any(arg => arg.EndsWith(".lnk")))
                    {
                        exePath = ResolveLnk(e.Args.Single(arg => arg.EndsWith(".lnk")));
                        if (exePath == null)
                        {
                            ShowFatalError("Invalid link provided.");
                            return;
                        }
                    }
                }
                catch
                {
                    ShowFatalError("An exectable was not provided.");
                }

                if (exePath != null)
                    stream = TryOpenFile(exePath);
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

        private static Stream OpenFileStreamFromDialog(IEnumerable<string> args)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Unity Editor|Unity.exe",
                Multiselect = false,
                InitialDirectory = FALLBACK_DIR,
                DefaultExt = "exe",
                Title = "Select Unity.exe"
            };

            if (dialog.ShowDialog() == true)
            {
                var exePath = dialog.FileName;

                if (args.Any(arg => arg.EndsWith(".lnk")))
                {
                    exePath = ResolveLnk(dialog.FileName);
                    if (exePath == null)
                    {
                        ShowFatalError("Invalid link provided.");
                        return null;
                    }
                }

                return TryOpenFile(exePath);
            }

            Environment.Exit(1);
            return null;
        }

        private static Stream TryOpenFile(string exePath)
        {
            try
            {
                return new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite);
            }
            catch
            {
                ShowFatalError($"Failed to open {exePath}.\nTry running Unidark as an administrator.");
            }

            return null;
        }

        private static string ResolveLnk(string pathToLnk)
        {
            IWshShell shell = new WshShell();
            if (shell.CreateShortcut(pathToLnk) is IWshShortcut lnk)
            {
                return lnk.TargetPath;
            }

            return null;
        }

        public static void ShowFatalError(string message)
        {
            MessageBox.Show(message, "Unidark - Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }
}
