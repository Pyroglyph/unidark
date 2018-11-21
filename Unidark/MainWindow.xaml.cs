using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LibUnidark;

namespace Unidark
{
    public partial class MainWindow
    {
        private Stream stream { get; }
        
        public static bool IsReverseMode { get; set; }
        public static bool IsErrored;

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

        private readonly Color successColor = Colors.DarkSeaGreen;
        private readonly Color failColor = Colors.IndianRed;


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
                RunOnUIThread(() =>
                {
                    CurrentStage = Stages.CalculateHash;
                    Status = "Checking for known version...";
                });
            };
            Patcher.OnOffsetSearchStart += (_, __) =>
            {
                RunOnUIThread(() =>
                {
                    CurrentStage = Stages.SearchForOffset;
                    Status = "Unknown version, looking for offset... (this might take a minute)";
                });
            };
            Patcher.OnPatchStart += (_, __) =>
            {
                RunOnUIThread(() =>
                {
                    CurrentStage = Stages.Patch;
                    Status = "Patching...";
                });
            };
            Patcher.OnComplete += (_, isReversed) =>
            {
                RunOnUIThread(() =>
                {
                    CurrentStage = Stages.Complete;
                    Status = isReversed ? "Done! Reverted to light theme!" : "Done! Enjoy the dark theme!";
                    pbarStage.Foreground = new SolidColorBrush(successColor);

                    Exit(0, 2000);
                });
            };

            try
            {
                if (Patcher.IsAlreadyPatched(stream))
                {
                    var result = MessageBox.Show(
                        "It looks like Unity has already been patched to use the dark theme.\nWould you like to revert back to the light theme instead?",
                        "Unidark - Attention",
                        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    IsReverseMode = result == MessageBoxResult.Yes;

                    if (result == MessageBoxResult.Yes)
                    {
                        IsReverseMode = true;
                    }
                    else
                    {
                        CurrentStage = Stages.Complete;
                        Status = "No changes made.";

                        Exit(1, 2000);
                    }
                }
            }
            catch (OffsetNotFoundException)
            {
                Fail("Offset not found");
            }

            Task.Run(() => Patcher.ApplyThemeToStream(stream, IsReverseMode));
        }

        private void RunOnUIThread(Action action)
        {
            if (!IsErrored)
            {
                Dispatcher.Invoke(action);
            }
        }

        private void Fail(string message)
        {
            IsErrored = true;

            Dispatcher.Invoke(() =>
            {
                Status = "Error - " + message;
                pbarStage.Foreground = new SolidColorBrush(failColor);
                
                Exit(1, 2000);
            });
        }

        private void Exit(int exitCode, int delay = 0)
        {
            new DispatcherTimer(
                    TimeSpan.FromMilliseconds(delay), 
                    DispatcherPriority.Normal,
                    (o, args) => Environment.Exit(exitCode),
                    Dispatcher)
                .Start();
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
