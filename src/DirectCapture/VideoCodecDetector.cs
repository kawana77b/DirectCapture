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
            var ext = Path.GetExtension(filePath).ToLower();
            if (new string[] { ".mp4", ".avi" }.Any(x => x == ext))
                return Codec.Unknown;

            var codec = Codec.Unknown;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[512];
                fs.Read(bytes, 0, bytes.Length);

                if (ext == ".mp4")
                {
                    if (bytes.Skip(4).Take(6).SequenceEqual(new byte[] { 0x66, 0x74, 0x79, 0x70, 0x71, 0x74 })) // ftypqt
                    {
                        codec = Codec.QuickTimeMP4;
                    }
                    else if (bytes.Skip(4).Take(8).SequenceEqual(new byte[] { 0x66, 0x74, 0x79, 0x70, 0x6d, 0x70, 0x34, 0x32 })) // ftypmp42
                    {
                        codec = Codec.H264_OR_H265_OR_OTHER;
                    }
                }

                if (ext == ".avi")
                {
                    if (bytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x4d, 0x54, 0x68, 0x64 })) // MThd
                    {
                        codec = Codec.MotionJPEG;
                    }
                    else if (bytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x75, 0x74, 0x76, 0x64 })) // utvd
                    {
                        codec = Codec.UtVideo;
                    }
                    else if (bytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x31, 0x41, 0x56, 0x49 })) // 1AVI
                    {
                        codec = Codec.MPEG4ASP;
                    }
                    else if (bytes.Skip(8).Take(4).SequenceEqual(new byte[] { 0x68, 0x64, 0x61, 0x76 })) // hdav
                    {
                        codec = Codec.MPEG4AVC;
                    }
                }
            }

            return codec;
        }
    }
}