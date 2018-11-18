using System;
using System.IO;
using System.Linq;

namespace Unidark
{
    class Program
    {
        // Sequence from YT/DarkMental
        static readonly byte[] SearchBytes = { 0x75, 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };

        static FileStream stream;

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (!File.Exists(args[0]))
                {
                    FancyConsole.LogError("File does not exist.");
                    return;
                }

                try
                {
                    stream = new FileStream(args[0], FileMode.Open, FileAccess.ReadWrite);
                }
                catch
                {
                    FancyConsole.LogError($"Could not open {args[0]}.");
                    FancyConsole.LogError("Is it running? Do you have permission?");
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
                            FancyConsole.Log("Unknown version of Unity, searching for offset...");
                            offset = stream.GetPositionOf(SearchBytes);
                            if (offset != -1)
                            {
                                ChangeColorByte(stream, offset);
                            }
                            else
                            {
                                FancyConsole.LogError("Offset was not found. This version of Unity is either incompatible or already patched!");
                            }
                        }
                        else
                        {
                            FancyConsole.LogError($"{Path.GetFileName(args[0])} does not seem to be an executable file.");
                        }
                    }

                    stream.Dispose();
                }
            }
            else
            {
                FancyConsole.LogError("An invalid number of arguments were passed. Please drop Unity.exe onto Unidark.exe.");
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
                fs.WriteByte(0x74);
                
                FancyConsole.LogSuccess("Done!");
            }
            catch (Exception e)
            {
                FancyConsole.LogError("Unable to edit file.");
                FancyConsole.LogError($"[{e.GetType()}] {e.Message}");
            }
        }
    }
}
