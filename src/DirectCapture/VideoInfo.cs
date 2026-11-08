using DirectShowLib;
using DirectShowLib.DES;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace DirectCapture
{
    /// <summary>
    /// Retrieve information about the video file
    /// </summary>
    /// <remarks>This object to obtain information on <c>.avi</c> files that can be handled by DirectShow</remarks>
    public class VideoInfo
    {
        private double _duration;

        /// <summary>
        /// Get the length of the file in seconds.
        /// </summary>
        public double Duration => _duration;

        private double _frameRate;

        /// <summary>
        /// Get the frame rate.
        /// </summary>
        public double FrameRate => _frameRate;

        private Size _displaySize;

        /// <summary>
        /// Get the height of the video.
        /// </summary>
        public int Height => _displaySize.Height;

        /// <summary>
        /// Get the width of the video.
        /// </summary>
        public int Width => _displaySize.Width;

        private readonly FileInfo _fileInfo;

        /// <summary>
        /// Get file information.
        /// </summary>
        public FileInfo FileInfo => _fileInfo;

        /// <summary>
        /// Get the absolute path of the file.
        /// </summary>
        public string FilePath => _fileInfo.FullName;

        /// <summary>
        /// Get the file name.
        /// </summary>
        public string FileName => _fileInfo.Name;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSize => _fileInfo.Length;

        /// <summary>
        /// Get the file extension.
        /// </summary>
        public string Extension => _fileInfo.Extension;

        /// <summary>
        /// Generate a new instance.
        /// </summary>
        /// <param name="filePath">The path of the <c>.avi</c> file.</param>
        /// <exception cref="ArgumentNullException">Occurs when filePath is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Occurs when the file specified by filePath does not exist.</exception>
        /// <exception cref="ArgumentException">Occurs when the file extension is not supported.</exception>
        /// <exception cref="VideoStreamException">Occurs when DirectShow cannot open the file, or the stream is not a supported video.</exception>
        /// <remarks>Only <c>.avi</c> extension is supported for filePath</remarks>
        public VideoInfo(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            if (!new string[] { ".avi" }.Any(x => Path.GetExtension(filePath).ToLowerInvariant() == x))
                throw new ArgumentException("File extension is not supported", nameof(filePath));

            _fileInfo = new FileInfo(filePath);
            GetVideoProperties();
        }

        /// <summary>
        /// Open the file with DirectShow and read its duration, frame rate and size into this object.
        /// </summary>
        /// <exception cref="VideoStreamException">DirectShow returned an error, or the stream is not a supported video.</exception>
        /// <remarks>
        /// DirectShow reports failures as HRESULTs; <c>DESError.ThrowExceptionForHR</c> turns them into
        /// <see cref="COMException"/>, which is wrapped in <see cref="VideoStreamException"/>.
        /// </remarks>
        private void GetVideoProperties()
        {
            IMediaDet mediaDet = (IMediaDet)new MediaDet();
            try
            {
                int hr;
                hr = mediaDet.put_Filename(FilePath);
                DESError.ThrowExceptionForHR(hr);

                hr = mediaDet.get_StreamLength(out _duration);
                DESError.ThrowExceptionForHR(hr);

                hr = mediaDet.get_FrameRate(out _frameRate);
                DESError.ThrowExceptionForHR(hr);

                _displaySize = GetVideoSize(mediaDet);
            }
            catch (COMException e) when (!(e is VideoStreamException))
            {
                throw new VideoStreamException(e);
            }
            finally
            {
                Marshal.ReleaseComObject(mediaDet);
            }
        }

        /// <summary>
        /// Create an exception for a stream that is not a supported video.
        /// </summary>
        /// <param name="message">Details of what was wrong.</param>
        /// <returns>An exception with HRESULT <c>VFW_E_INVALIDMEDIATYPE</c>.</returns>
        private static VideoStreamException InvalidFormat(string message) =>
            new VideoStreamException(new COMException($"Unsupported video stream ({message})", unchecked((int)0x80040200)));

        /// <summary>
        /// Get the video size from the stream's media type.
        /// </summary>
        /// <param name="mediaDet">A media detector with the file already open.</param>
        /// <returns>The width and height of the video, in pixels.</returns>
        /// <exception cref="VideoStreamException">DirectShow returned an error, or the stream is not a supported video.</exception>
        /// <remarks>
        /// The layout of the format block depends on <c>formatType</c>, so the size is read from
        /// <see cref="VideoInfoHeader"/> or <see cref="VideoInfoHeader2"/> accordingly;
        /// reading the wrong struct would give a wrong size.
        /// A negative bitmap height means a top-down bitmap, so the absolute value is used.
        /// </remarks>
        private Size GetVideoSize(IMediaDet mediaDet)
        {
            double width, height;

            AMMediaType mediaType = new AMMediaType();
            try
            {
                int hr;
                hr = mediaDet.get_StreamMediaType(mediaType);
                DESError.ThrowExceptionForHR(hr);

                if (mediaType.majorType != MediaType.Video)
                    throw InvalidFormat("major type is not video");

                if (mediaType.formatPtr == IntPtr.Zero)
                    throw InvalidFormat("format block is missing");

                BitmapInfoHeader bmiHeader;
                if (mediaType.formatType == FormatType.VideoInfo
                    && mediaType.formatSize >= Marshal.SizeOf(typeof(VideoInfoHeader)))
                {
                    bmiHeader = ((VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader))).BmiHeader;
                }
                else if (mediaType.formatType == FormatType.VideoInfo2
                    && mediaType.formatSize >= Marshal.SizeOf(typeof(VideoInfoHeader2)))
                {
                    bmiHeader = ((VideoInfoHeader2)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader2))).BmiHeader;
                }
                else
                {
                    throw InvalidFormat($"unsupported format type {mediaType.formatType}");
                }

                width = bmiHeader.Width;
                height = Math.Abs(bmiHeader.Height);
                if (width <= 0 || height <= 0)
                    throw InvalidFormat($"invalid video size {width}x{height}");
            }
            catch (COMException e) when (!(e is VideoStreamException))
            {
                throw new VideoStreamException(e);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }

            return new Size((int)width, (int)height);
        }
    }
}