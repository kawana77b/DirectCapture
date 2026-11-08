using System;
using System.IO;
using System.Linq;

namespace DirectCapture
{
    /// <summary>
    ///  Determines the video codec
    /// </summary>
    /// <remarks>This is a simple mini utility. It does not support so many codecs</remarks>
    public static class VideoCodecDetector
    {
        /// <summary>
        /// Indicates the video codec
        /// </summary>
        public enum Codec
        {
            /// <summary>
            /// Codec cannot be determined
            /// </summary>
            Unknown,

            /// <summary>
            /// H.264 or H.265 or other
            /// </summary>
            H264_OR_H265_OR_OTHER,

            /// <summary>
            /// MP4 (QuickTime)
            /// </summary>
            QuickTimeMP4,

            /// <summary>
            /// MPEG4 ASP
            /// </summary>
            MPEG4ASP,

            /// <summary>
            /// MPEG4 AVC
            /// </summary>
            MPEG4AVC,

            /// <summary>
            /// Ut-Video
            /// </summary>
            UtVideo,

            /// <summary>
            /// Motion JPEG
            /// </summary>
            MotionJPEG,
        }

        /// <summary>
        /// Gets the video codec. Returns <c>Unknown</c> if the codec cannot be determined.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>codec type</returns>
        /// <remarks>
        /// Only <c>.mp4</c> and <c>.avi</c> are supported. The following are supported:
        /// <list type="bullet">
        /// <item>H.264 or H.265 or other</item>
        /// <item>Quick Time (MP4/QuickTime)</item>
        /// <item>MPEG-4 ASP (AVI)</item>
        /// <item>MPEG-4 AVC (AVI)</item>
        /// <item>Ut-Video (AVI)</item>
        /// <item>Motion JPEG (AVI)</item>
        /// </list>
        /// </remarks>
        public static Codec GetVideoCodec(string filePath)
        {
            if (!File.Exists(filePath))
                return Codec.Unknown;

            // Check file extension.
            // If the file extension is different, it is not checked strictly; it is marked as Unknown.
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (!new string[] { ".mp4", ".avi" }.Any(x => x == ext))
                return Codec.Unknown;

            byte[] bytes = new byte[HeaderReadSize];
            int read;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                read = ReadFully(fs, bytes);
            }

            if (ext == ".mp4")
                return DetectMp4(bytes, read);

            return DetectAvi(bytes, read);
        }

        // Large enough to cover the AVI header list (hdrl) in typical files.
        private const int HeaderReadSize = 64 * 1024;

        private static int ReadFully(Stream stream, byte[] buffer)
        {
            int total = 0;
            while (total < buffer.Length)
            {
                int n = stream.Read(buffer, total, buffer.Length - total);
                if (n <= 0)
                    break;
                total += n;
            }
            return total;
        }

        private static Codec DetectMp4(byte[] bytes, int length)
        {
            if (length < 12 || !MatchAscii(bytes, 4, "ftyp"))
                return Codec.Unknown;

            if (MatchAscii(bytes, 8, "qt"))
                return Codec.QuickTimeMP4;

            if (MatchAscii(bytes, 8, "mp42"))
                return Codec.H264_OR_H265_OR_OTHER;

            return Codec.Unknown;
        }

        // The codec of an AVI file is stored as a FourCC in the video stream header:
        // RIFF 'AVI ' > LIST 'hdrl' > LIST 'strl' > 'strh' (fccType = 'vids', fccHandler = codec).
        // The 'strf' chunk that follows holds a BITMAPINFOHEADER whose biCompression is also a FourCC.
        private static Codec DetectAvi(byte[] bytes, int length)
        {
            if (length < 12 || !MatchAscii(bytes, 0, "RIFF") || !MatchAscii(bytes, 8, "AVI "))
                return Codec.Unknown;

            // Scan for a 'strh' chunk whose fccType is 'vids'.
            for (int i = 12; i + 16 <= length; i++)
            {
                if (!MatchAscii(bytes, i, "strh") || !MatchAscii(bytes, i + 8, "vids"))
                    continue;

                var handler = FourCC(bytes, i + 12);
                var codec = FromFourCC(handler);
                if (codec != Codec.Unknown)
                    return codec;

                // fccHandler may be empty; fall back to biCompression in the following 'strf'.
                int strhSize = BitConverter.ToInt32(bytes, i + 4);
                if (strhSize < 0)
                    return Codec.Unknown;
                long strf = (long)i + 8 + strhSize + (strhSize & 1);
                // strf: 'strf'(4) size(4) BITMAPINFOHEADER: biSize(4) biWidth(4) biHeight(4) biPlanes(2) biBitCount(2) biCompression(4)
                if (strf + 28 <= length && MatchAscii(bytes, (int)strf, "strf"))
                    return FromFourCC(FourCC(bytes, (int)strf + 24));

                return Codec.Unknown;
            }

            return Codec.Unknown;
        }

        private static Codec FromFourCC(string fourcc)
        {
            switch (fourcc.ToUpperInvariant())
            {
                case "MJPG":
                    return Codec.MotionJPEG;

                case "DIVX":
                case "DX50":
                case "XVID":
                case "FMP4":
                case "MP4V":
                    return Codec.MPEG4ASP;

                case "H264":
                case "X264":
                case "AVC1":
                    return Codec.MPEG4AVC;
            }

            // Ut Video uses FourCCs such as ULRG, ULRA, ULY0, ULY2, ULY4, ULH0, UQY2...
            if (fourcc.StartsWith("UL", StringComparison.Ordinal) || fourcc.StartsWith("UQ", StringComparison.Ordinal))
                return Codec.UtVideo;

            return Codec.Unknown;
        }

        private static string FourCC(byte[] bytes, int offset) =>
            new string(new[] { (char)bytes[offset], (char)bytes[offset + 1], (char)bytes[offset + 2], (char)bytes[offset + 3] });

        private static bool MatchAscii(byte[] bytes, int offset, string text)
        {
            if (offset < 0 || offset + text.Length > bytes.Length)
                return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (bytes[offset + i] != (byte)text[i])
                    return false;
            }
            return true;
        }
    }
}