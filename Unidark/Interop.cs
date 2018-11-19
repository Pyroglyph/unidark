using System;
using System.Runtime.InteropServices;

namespace Unidark
{
    public static class Interop
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int MessageBoxTimeout(IntPtr hwnd, string text, string title, uint type, short wLanguageId, int milliseconds);

        public static void ShowMessageBox(string message, MessageBoxType type = MessageBoxType.Information)
        {
            var title = "Unidark";

            switch (type)
            {
                case MessageBoxType.Error:
                    title += " - Error";
                    break;
                case MessageBoxType.Warning:
                    title += " - Warning";
                    break;
            }

            MessageBoxTimeout(IntPtr.Zero, message, title, (uint)type, 0, -1);
        }
    }

    public enum MessageBoxType : uint
    {
        Error = 16,
        Warning = 48,
        Information = 64
    }
}
