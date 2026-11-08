using DirectShowLib;
using DirectShowLib.DES;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DirectCapture
{
    /// <summary>
    /// Open the .avi stream in DirectShow and capture the video.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe. Use each instance from a single thread,
    /// preferably an STA thread, since the underlying DirectShow MediaDet object is an STA COM object.
    /// To process frames in parallel, create one instance per thread.
    /// <para>
    /// Decoding is done by the DirectShow filters and codecs installed on the system, inside this process.
    /// Only open files from trusted sources.
    /// </para>
    /// </remarks>
    public class VideoCapture : IDisposable
    {
        private bool disposedValue;

        private readonly VideoInfo _videoInfo;

        /// <summary>
        /// Get the information of the video file.
        /// </summary>
        public VideoInfo VideoInfo => _videoInfo;

        /// <summary>
        /// Get the length of the file in seconds.
        /// </summary>
        public double Duration => _videoInfo.Duration;

        /// <summary>
        /// Get the frame rate. This value is the same as the frame rate of the video file.
        /// </summary>
        /// <remarks>This does not necessarily correspond to the frame rate of the file itself, since it is determined by the stream.</remarks>
        public double FrameRate => _videoInfo.FrameRate;

        private double _currentPosition = 0.0;

        /// <summary>
        /// Get or set the current position of the video in seconds.
        /// If the value is out of range, it will be adjusted to the nearest value.
        /// </summary>
        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                if (value < 0.0)
                    _currentPosition = 0.0;
                else if (value > Duration)
                    _currentPosition = Duration;
                else
                    _currentPosition = value;
            }
        }

        /// <summary>
        /// The DirectShow media detector used to read frames from the stream.
        /// </summary>
        /// <remarks>
        /// <c>null</c> when the stream is not open or the object has been disposed.
        /// See <see href="https://learn.microsoft.com/en-US/windows/win32/directshow/imediadet">IMediaDet</see>.
        /// </remarks>
        private IMediaDet _mediaDet = null;

        /// <summary>
        /// Generate a VideoCapture object from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path of the video for which to open the stream. This must be an .avi file available in DirectShow.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="System.IO.FileNotFoundException">The file does not exist.</exception>
        /// <exception cref="ArgumentException">The file extension is not supported.</exception>
        /// <exception cref="VideoStreamException">The stream could not be opened.</exception>
        public VideoCapture(string filePath)
        {
            _videoInfo = new VideoInfo(filePath);
            OpenStream();
        }

        /// <summary>
        /// Generate a VideoCapture object from the specified VideoInfo object.
        /// </summary>
        /// <param name="videoInfo">Information about the video file to open.</param>
        /// <exception cref="ArgumentNullException"><paramref name="videoInfo"/> is null.</exception>
        /// <exception cref="VideoStreamException">The stream could not be opened.</exception>
        public VideoCapture(VideoInfo videoInfo)
        {
            _videoInfo = videoInfo ?? throw new ArgumentNullException(nameof(videoInfo));
            OpenStream();
        }

        /// <summary>
        /// Release the DirectShow resources held by this object.
        /// </summary>
        /// <remarks>
        /// This class has no finalizer. The COM object is held by a runtime callable wrapper (RCW),
        /// which releases itself when collected; calling <see cref="Marshal.ReleaseComObject(object)"/>
        /// from the finalizer thread is unsafe, so it is only done here.
        /// Always dispose this object to release the stream promptly.
        /// </remarks>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release the resources held by this object.
        /// </summary>
        /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && _mediaDet != null)
                {
                    Marshal.ReleaseComObject(_mediaDet);
                }
                _mediaDet = null;

                disposedValue = true;
            }
        }

        /// <summary>
        /// Get current position as Frame.
        /// </summary>
        /// <returns>The frame at <see cref="CurrentPosition"/>.</returns>
        /// <remarks>This may not always correspond exactly to the frame of the file, since it is determined by the stream</remarks>
        public Frame GetCurrentPositionAsFrame() => Frame.FromSeconds(CurrentPosition, FrameRate);

        /// <summary>
        /// Set current position from Frame.
        /// </summary>
        /// <param name="frame">The frame to move to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
        /// <remarks>This may not always correspond exactly to the frame of the file, since it is determined by the stream</remarks>
        public void SetCurrentPositionFromFrame(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            CurrentPosition = frame.Seconds;
        }

        /// <summary>
        /// Open the video stream.
        /// </summary>
        /// <exception cref="VideoStreamException">DirectShow could not open the file. The MediaDet object is released before throwing.</exception>
        protected void OpenStream()
        {
            _mediaDet = (IMediaDet)new MediaDet();
            try
            {
                var filePath = VideoInfo.FilePath;
                int hr = _mediaDet.put_Filename(filePath);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (COMException e)
            {
                Marshal.ReleaseComObject(_mediaDet);
                _mediaDet = null;
                throw new VideoStreamException(e);
            }
        }

        /// <summary>
        /// Get the bitmap information of the frame at the specified second.
        /// </summary>
        /// <returns>A 24bpp bitmap of the frame at <see cref="CurrentPosition"/>. The caller must dispose it.</returns>
        /// <exception cref="ObjectDisposedException">This object has been disposed</exception>
        /// <exception cref="InvalidOperationException">The stream is not open</exception>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="CurrentPosition"/> is out of range</exception>
        /// <exception cref="VideoStreamException">DirectShow failed to decode the frame, or returned unexpected data.</exception>
        public Bitmap GetBitmap()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(VideoCapture));

            if (_mediaDet == null)
                throw new InvalidOperationException("The stream is not open");

            var second = CurrentPosition;
            if (second < 0 || second > Duration)
                throw new ArgumentOutOfRangeException(nameof(CurrentPosition), second, "The position is out of range.");

            return GetBitmap(_mediaDet, second);
        }

        /// <summary>
        /// Copies a block of memory from one location to another. <br/>
        /// https://learn.microsoft.com/en-US/previous-versions/windows/desktop/legacy/aa366535(v=vs.85)
        /// </summary>
        /// <param name="Destination">A pointer to the starting address of the copied block's destination.</param>
        /// <param name="Source">A pointer to the starting address of the block of memory to copy.</param>
        /// <param name="Length">The size of the block of memory to copy, in bytes.</param>
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        /// <summary>
        /// Create an exception for frame data that DirectShow returned in an unexpected form.
        /// </summary>
        /// <param name="message">Details of what was wrong.</param>
        /// <returns>An exception with HRESULT <c>E_FAIL</c>.</returns>
        private static VideoStreamException InvalidFrame(string message) =>
            new VideoStreamException(new COMException($"Invalid frame data ({message})", unchecked((int)0x80004005)));

        /// <summary>
        /// Decode the frame at the given time and copy it into a new <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="mediaDet">The open media detector.</param>
        /// <param name="second">The time of the frame, in seconds.</param>
        /// <returns>A 24bpp bitmap of the frame.</returns>
        /// <exception cref="VideoStreamException">DirectShow returned an error or unexpected data.</exception>
        /// <remarks>
        /// <c>IMediaDet.GetBitmapBits</c> is called twice: first with a null buffer to get the required size,
        /// then with an allocated buffer to receive the frame.
        /// <para>
        /// The buffer holds a BITMAPINFOHEADER followed by 24-bit pixel data. The header and buffer size are
        /// validated before any pixel is read, so malformed data can never cause an out-of-bounds read.
        /// </para>
        /// <para>
        /// DIB rows are padded to a multiple of 4 bytes, and a positive height means the rows are stored
        /// bottom-up, so rows are copied one by one in display order.
        /// </para>
        /// </remarks>
        private Bitmap GetBitmap(IMediaDet mediaDet, double second)
        {
            Bitmap bitmap = null;

            IntPtr pBuffer = IntPtr.Zero;
            var width = _videoInfo.Width;
            var height = _videoInfo.Height;
            try
            {
                int hr;
                hr = mediaDet.GetBitmapBits(second, out int bufferSize, pBuffer, width, height);
                DsError.ThrowExceptionForHR(hr);

                pBuffer = Marshal.AllocCoTaskMem(bufferSize);

                hr = mediaDet.GetBitmapBits(second, out bufferSize, pBuffer, width, height);
                DsError.ThrowExceptionForHR(hr);

                int headerSize = Marshal.SizeOf(typeof(BitmapInfoHeader));
                if (bufferSize < headerSize)
                {
                    throw InvalidFrame($"buffer too small: {bufferSize} bytes");
                }

                BitmapInfoHeader bmpHeader = (BitmapInfoHeader)Marshal.PtrToStructure(pBuffer, typeof(BitmapInfoHeader));
                if (
                    bmpHeader.Size < headerSize ||
                    bmpHeader.BitCount != 24 ||
                    bmpHeader.Width != width ||
                    Math.Abs(bmpHeader.Height) != height)
                {
                    throw InvalidFrame($"unexpected bitmap format: {bmpHeader.Width}x{bmpHeader.Height} {bmpHeader.BitCount}bpp");
                }

                long srcStride = ((long)width * 3 + 3) & ~3L;
                if ((long)bmpHeader.Size + srcStride * height > bufferSize)
                {
                    throw InvalidFrame($"buffer too small for {width}x{height}: {bufferSize} bytes");
                }

                bool bottomUp = bmpHeader.Height > 0;
                IntPtr pPixels = IntPtr.Add(pBuffer, bmpHeader.Size);

                bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                try
                {
                    BitmapData data = bitmap.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format24bppRgb
                    );
                    try
                    {
                        int rowBytes = width * 3;
                        for (int y = 0; y < height; y++)
                        {
                            int srcRow = bottomUp ? height - 1 - y : y;
                            IntPtr src = new IntPtr(pPixels.ToInt64() + srcRow * srcStride);
                            IntPtr dst = new IntPtr(data.Scan0.ToInt64() + (long)y * data.Stride);
                            CopyMemory(dst, src, rowBytes);
                        }
                    }
                    finally
                    {
                        bitmap.UnlockBits(data);
                    }
                }
                catch
                {
                    bitmap.Dispose();
                    throw;
                }
            }
            catch (COMException e) when (!(e is VideoStreamException))
            {
                throw new VideoStreamException(e);
            }
            finally
            {
                if (pBuffer != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pBuffer);
            }

            return bitmap;
        }
    }
}