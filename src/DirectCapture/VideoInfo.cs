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
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException">Occurs when filePath is null.</exception>
        /// <exception cref="FileNotFoundException">Occurs when the file specified by filePath does not exist.</exception>
        /// <exception cref="ArgumentException">Occurs when the file extension is not supported.</exception>
        /// <remarks>Only <c>.avi</c> extension is supported for filePath</remarks>
        public VideoInfo(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            if (!new string[] { ".avi" }.Any(x => Path.GetExtension(filePath).ToLower() == x))
                throw new ArgumentException("File extension is not supported", nameof(filePath));

            _fileInfo = new FileInfo(filePath);
            GetVideoProperties();
        }

        // Here we open a stream from a file,
        // retrieve the necessary information and set it to a member.
        private void GetVideoProperties()
        {
            IMediaDet mediaDet = (IMediaDet)new MediaDet();
            try
            {
                int hr;
                // Open the file
                hr = mediaDet.put_Filename(FilePath);
                DESError.ThrowExceptionForHR(hr);

                // Get the length of the file
                hr = mediaDet.get_StreamLength(out _duration);
                DESError.ThrowExceptionForHR(hr);

                // Get the frame rate
                hr = mediaDet.get_FrameRate(out _frameRate);
                DESError.ThrowExceptionForHR(hr);

                // Get the video size
                _displaySize = GetVideoSize(mediaDet);
            }
            catch (COMException e)
            {
                // DESError.ThrowExceptionForHR(hr) throws COMException internally,
                // so catch it with a wrapped exception
                throw new VideoStreamException(e);
            }
            finally
            {
                Marshal.ReleaseComObject(mediaDet);
            }
        }

        // Get video rendering size
        private Size GetVideoSize(IMediaDet mediaDet)
        {
            double width, height;

            // Get the media type, to get the video size by using the VideoInfoHeader
            AMMediaType mediaType = new AMMediaType();
            try
            {
                int hr;
                hr = mediaDet.get_StreamMediaType(mediaType);
                DESError.ThrowExceptionForHR(hr);

                // Check if the major type is video
                if (mediaType.majorType != MediaType.Video)
                    throw new ArgumentException("Major type is not video");

                VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));

                width = videoInfoHeader.BmiHeader.Width;
                height = videoInfoHeader.BmiHeader.Height;
            }
            catch (COMException e)
            {
                // DESError.ThrowExceptionForHR(hr) throws COMException internally,
                // so catch it with a wrapped exception
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