using System;
using System.IO;
using System.Linq;

namespace Unidark
{
    class Program
    {
        const byte LightByte = 0x75;
        const byte DarkByte = 0x74;

        // Sequence from YT/DarkMental
        static byte[] SearchBytes => new byte[] { IsReverseMode ? DarkByte : LightByte, 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };

        static FileStream stream;

        static bool IsReverseMode;

        static void Main(string[] args)
        {
            if (args.Any(arg => arg.ToLower() == "-reverse"))
            {
                IsReverseMode = true;
            }

            try
            {
                var unityPath = args.Single(arg => arg.Contains(".exe"));
                
                if (!File.Exists(unityPath))
                {
                    Interop.ShowMessageBox("File does not exist.", MessageBoxType.Error);
                    return;
                }

                try
                {
                    stream = new FileStream(unityPath, FileMode.Open, FileAccess.ReadWrite);
                }
                catch
                {
                    const string runAsAdminInstructions = @"Otherwise, try and run Unidark as Admin:
- Right-click on Unidark.exe
- Click the Compatibility tab
- Check the 'Run this program as an administrator' box
- Try again";
                    Interop.ShowMessageBox($"Could not open {unityPath}.\n\nIf Unity is open, please close it.\n\n{runAsAdminInstructions}", MessageBoxType.Error);
                }

                if (stream != null)
                {
                    int offset;

                    if (HashHelper.IsKnownFile(stream, out offset))
                    {
                        ChangeColorByte(stream, offset);
                    }
                    else
                    {
                        if (IsExeFile(stream))
                        {
                            Interop.ShowMessageBox($"Searching {Path.GetFileName(unityPath)} for the offset. This might take a minute.");

                            offset = stream.GetPositionOf(SearchBytes);
                            if (offset != -1)
                            {
                                ChangeColorByte(stream, offset);
                            }
                            else
                            {
                                Interop.ShowMessageBox("Offset was not found. This version of Unity is either incompatible or already patched!", MessageBoxType.Error);
                            }
                        }
                        else
                        {
                            Interop.ShowMessageBox($"{Path.GetFileName(unityPath)} does not seem to be an executable file or is corrupt.", MessageBoxType.Error);
                        }
                    }

                    stream.Dispose();
                }
                
            }
            catch
            {
                Interop.ShowMessageBox("Unidark does not run on it's own (yet). Please drop Unity.exe onto Unidark.exe.", MessageBoxType.Error);
            }

        }

        static bool IsExeFile(FileStream fs)
        {
            var expectedHeader = new byte[] {0x4D, 0x5A};
            var header = new byte[2];
            fs.Position = 0;
            fs.Read(header, 0, 2);
            return header.SequenceEqual(expectedHeader);
        }

        static void ChangeColorByte(FileStream fs, int offset)
        {
            try
            {
                fs.Position = offset;
                fs.WriteByte(IsReverseMode ? LightByte : DarkByte);

                Interop.ShowMessageBox($"Success! {(IsReverseMode ? "Reverted to the light theme!" : "Enjoy the dark theme!")}");
            }
            catch (Exception e)
            {
                Interop.ShowMessageBox($"Unable to edit file.\n\n[{e.GetType()}] {e.Message}", MessageBoxType.Error);
            }
        }
    }
}
