using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using LibUnidark;

namespace Unidark
{
    public partial class MainWindow
    {
        private Stream stream { get; }

        public static bool IsComplete { get; set; }
        public static bool IsReverseMode { get; set; }

        private string Status
        {
            set => lblStatus.Content = value;
        }

        private Stages _currentStage;
        private Stages CurrentStage
        {
            set
            {
                _currentStage = value;
                pbarStage.Value = (int)_currentStage + 1;
            }
        }


        public MainWindow(Stream exeStream)
        {
            stream = exeStream;

            InitializeComponent();

            pbarStage.Maximum = Enum.GetValues(typeof(Stages)).Length;
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Patcher.OnCalculateHashStart += (_, __) =>
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentStage = Stages.CalculateHash;
                    Status = "Checking for known version...";
                });
            };
            Patcher.OnOffsetSearchStart += (_, __) =>
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentStage = Stages.SearchForOffset;
                    Status = "Unknown version, searching for offset... (this might take a minute)";
                });
            };
            Patcher.OnPatchStart += (_, __) =>
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentStage = Stages.Patch;
                    Status = "Patching";
                });
            };
            Patcher.OnComplete += (_, isReversed) =>
            {
                Dispatcher.Invoke(() =>
                {
                    CurrentStage = Stages.Complete;
                    Status = isReversed ? "Done! Reverted to light theme!" : "Done! Enjoy the dark theme!";

                    IsComplete = true;
                });
            };

            Task.Run(() => Patcher.ApplyThemeToStream(stream, IsReverseMode));
        }
    }

    public enum Stages
    {
        CalculateHash,
        SearchForOffset,
        Patch,
        Complete
    }
}
