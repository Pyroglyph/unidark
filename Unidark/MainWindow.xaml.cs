using System;
using System.IO;
using System.Linq;
using System.Windows;
using LibUnidark;

namespace Unidark
{
    public partial class MainWindow
    {
        private Stream stream { get; }

        public static bool IsComplete { get; set; }
        public static bool IsReverseMode { get; set; }

        public string Status { get; set; }
        public int MainStageProgress { get; set; }
        public int CurrentStageProgress { get; set; }
        public Visibility SuccessMessageVisibility { get; set; } = IsComplete ? Visibility.Visible : Visibility.Collapsed;
        
        public MainWindow(Stream exeStream)
        {
            stream = exeStream;

            InitializeComponent();
            DataContext = this;
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            LibUnidark.LibUnidark.OnStartOffsetSearch += (_, __) => { Status = "Searching for offset..."; };
            LibUnidark.LibUnidark.OnComplete += (_, __) => { IsComplete = true; };
            LibUnidark.LibUnidark.ApplyThemeToStream(stream, IsReverseMode);
        }
    }
}
