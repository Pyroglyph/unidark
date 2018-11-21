using System;
using System.IO;

namespace LibUnidark
{
    public static class Patcher
    {
        const byte LightByte = 0x75;
        const byte DarkByte = 0x74;

        // Sequence from YT/DarkMental
        static byte[] SearchBytes => new byte[] { 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };

        public static int ThemeByteOffset = -1;

        static bool IsReverseMode;

        public static event EventHandler OnCalculateHashStart;
        public static event EventHandler OnOffsetSearchStart;
        public static event EventHandler OnPatchStart;
        public static event EventHandler<bool> OnComplete;


        private static int FindThemeByteOffset(Stream stream)
        {
            OnOffsetSearchStart?.Invoke(null, EventArgs.Empty);

            var offset = stream.GetPositionOf(SearchBytes);
            if (offset == -1)
            {
                throw new OffsetNotFoundException();
            }

            return offset;
        }

        public static bool IsAlreadyPatched(Stream stream)
        {
            OnCalculateHashStart?.Invoke(null, EventArgs.Empty);

            if (!HashHelper.IsKnownFile(stream, out var offset))
            {
                offset = FindThemeByteOffset(stream);
            }

            ThemeByteOffset = offset - 1;

            stream.Position = ThemeByteOffset;
            var themeByte = Convert.ToByte(stream.ReadByte());

            return themeByte == DarkByte;
        }

        public static void ApplyThemeToStream(Stream stream, bool reverse = false)
        {
            IsReverseMode = reverse;

            if (stream != null)
            {
                OnPatchStart?.Invoke(null, EventArgs.Empty);

                ChangeThemeByte(stream);

                stream.Dispose();
            }
        }
        
        static void ChangeThemeByte(Stream stream)
        {
            if (ThemeByteOffset != -1)
            {
                OnPatchStart?.Invoke(null, EventArgs.Empty);

                stream.Position = ThemeByteOffset;
                stream.WriteByte(IsReverseMode ? LightByte : DarkByte);

                OnComplete?.Invoke(null, IsReverseMode);
            }
            else
            {
                throw new OffsetNotFoundException();
            }
        }
    }
}
