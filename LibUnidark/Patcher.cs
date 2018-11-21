using System;
using System.IO;
using System.Linq;

namespace LibUnidark
{
    public static class Patcher
    {
        const byte LightByte = 0x75;
        const byte DarkByte = 0x74;

        // Sequence from YT/DarkMental
        static byte[] SearchBytes => new byte[] { 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };
        
        static bool IsReverseMode;

        public static event EventHandler OnCalculateHashStart;
        public static event EventHandler OnOffsetSearchStart;
        public static event EventHandler OnPatchStart;
        public static event EventHandler<bool> OnComplete;


        public static void ApplyThemeToStream(Stream stream, bool reverse = false)
        {
            IsReverseMode = reverse;

            if (stream != null)
            {
                int offset;

                OnCalculateHashStart?.Invoke(null, EventArgs.Empty);
                if (HashHelper.IsKnownFile(stream, out offset))
                {
                    ChangeThemeByte(stream, offset - 1);
                }
                else
                {
                    if (IsExeFile(stream))
                    {
                        OnOffsetSearchStart?.Invoke(null, EventArgs.Empty);
                        
                        offset = stream.GetPositionOf(SearchBytes);
                        if (offset != -1)
                        {
                            ChangeThemeByte(stream, offset - 1);
                        }
                        else
                        {
                            throw new OffsetNotFoundException();
                        }
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }

                stream.Dispose();
            }
        }

        static bool IsExeFile(Stream stream)
        {
            var expectedHeader = new byte[] {0x4D, 0x5A};
            var header = new byte[2];
            stream.Position = 0;
            stream.Read(header, 0, 2);
            return header.SequenceEqual(expectedHeader);
        }

        static void ChangeThemeByte(Stream stream, int locationOfThemeByte)
        {
            OnPatchStart?.Invoke(null, EventArgs.Empty);

            stream.Position = locationOfThemeByte;
            stream.WriteByte(IsReverseMode ? LightByte : DarkByte);

            OnComplete?.Invoke(null, IsReverseMode);
        }
    }
}
