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
        /// <param name="filePath">The path of the video file.</param>
        /// <returns>The detected codec, or <see cref="Codec.Unknown"/>.</returns>
        /// <remarks>
        /// Only <c>.mp4</c> and <c>.avi</c> are supported; any other extension, or a file that does not exist,
        /// returns <see cref="Codec.Unknown"/> without reading the file.
        /// Only the first 64 KB of the file is read. The following are supported:
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

        /// <summary>
        /// Number of bytes read from the start of the file.
        /// Large enough to cover the AVI header list (<c>hdrl</c>) in typical files.
        /// </summary>
        private const int HeaderReadSize = 64 * 1024;

        /// <summary>
        /// Read from the stream until the buffer is full or the end of the stream is reached.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="buffer">The buffer to fill.</param>
        /// <returns>The number of bytes read.</returns>
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

        /// <summary>
        /// Detect the codec of an MP4 file from the major brand of its <c>ftyp</c> box.
        /// </summary>
        /// <param name="bytes">The start of the file.</param>
        /// <param name="length">The number of valid bytes in <paramref name="bytes"/>.</param>
        /// <returns>The detected codec, or <see cref="Codec.Unknown"/>.</returns>
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

        /// <summary>
        /// Detect the codec of an AVI file from the FourCC of its video stream.
        /// </summary>
        /// <param name="bytes">The start of the file.</param>
        /// <param name="length">The number of valid bytes in <paramref name="bytes"/>.</param>
        /// <returns>The detected codec, or <see cref="Codec.Unknown"/>.</returns>
        /// <remarks>
        /// The codec is stored in the video stream header:
        /// <c>RIFF 'AVI ' &gt; LIST 'hdrl' &gt; LIST 'strl' &gt; 'strh'</c>, where <c>fccType</c> is <c>'vids'</c>
        /// and <c>fccHandler</c> is the codec FourCC.
        /// <para>
        /// <c>fccHandler</c> may be empty, so it falls back to the <c>'strf'</c> chunk that follows.
        /// Its layout is <c>'strf'(4) size(4)</c> followed by a BITMAPINFOHEADER
        /// (<c>biSize(4) biWidth(4) biHeight(4) biPlanes(2) biBitCount(2) biCompression(4)</c>),
        /// so <c>biCompression</c> is at offset 24 from the chunk start.
        /// </para>
        /// </remarks>
        private static Codec DetectAvi(byte[] bytes, int length)
        {
            if (length < 12 || !MatchAscii(bytes, 0, "RIFF") || !MatchAscii(bytes, 8, "AVI "))
                return Codec.Unknown;

            for (int i = 12; i + 16 <= length; i++)
            {
                if (!MatchAscii(bytes, i, "strh") || !MatchAscii(bytes, i + 8, "vids"))
                    continue;

                var handler = FourCC(bytes, i + 12);
                var codec = FromFourCC(handler);
                if (codec != Codec.Unknown)
                    return codec;

                int strhSize = BitConverter.ToInt32(bytes, i + 4);
                if (strhSize < 0)
                    return Codec.Unknown;
                long strf = (long)i + 8 + strhSize + (strhSize & 1);
                if (strf + 28 <= length && MatchAscii(bytes, (int)strf, "strf"))
                    return FromFourCC(FourCC(bytes, (int)strf + 24));

                return Codec.Unknown;
            }

            return Codec.Unknown;
        }

        /// <summary>
        /// Map a video FourCC to a <see cref="Codec"/>. The comparison ignores case.
        /// </summary>
        /// <param name="fourcc">The four-character code.</param>
        /// <returns>The matching codec, or <see cref="Codec.Unknown"/>.</returns>
        /// <remarks>Ut Video uses several FourCCs, such as <c>ULRG</c>, <c>ULY2</c>, <c>ULH0</c> and <c>UQY2</c>, so it is matched by prefix.</remarks>
        private static Codec FromFourCC(string fourcc)
        {
            var upper = fourcc.ToUpperInvariant();
            switch (upper)
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

            if (upper.StartsWith("UL", StringComparison.Ordinal) || upper.StartsWith("UQ", StringComparison.Ordinal))
                return Codec.UtVideo;

            return Codec.Unknown;
        }

        /// <summary>
        /// Read a four-character code at the given offset.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <param name="offset">The offset of the first character.</param>
        /// <returns>The FourCC as a string.</returns>
        private static string FourCC(byte[] bytes, int offset) =>
            new string(new[] { (char)bytes[offset], (char)bytes[offset + 1], (char)bytes[offset + 2], (char)bytes[offset + 3] });

        /// <summary>
        /// Check whether the bytes at the given offset equal an ASCII string.
        /// </summary>
        /// <param name="bytes">The source bytes.</param>
        /// <param name="offset">The offset to compare at.</param>
        /// <param name="text">The ASCII text to look for.</param>
        /// <returns><c>true</c> if the bytes match; <c>false</c> if they differ or the range is out of bounds.</returns>
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