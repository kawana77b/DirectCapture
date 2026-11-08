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

        // IMediaDet provides information about the video and important methods for capturing it.
        // https://learn.microsoft.com/en-US/windows/win32/directshow/imediadet
        private IMediaDet _mediaDet = null;

        /// <summary>
        /// Generate a VideoCapture object from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path of the video for which to open the stream. This must be an .avi file available in DirectShow.</param>
        public VideoCapture(string filePath)
        {
            _videoInfo = new VideoInfo(filePath);
            OpenStream();
        }

        /// <summary>
        /// Generate a VideoCapture object from the specified VideoInfo object.
        /// </summary>
        /// <param name="videoInfo"></param>
        public VideoCapture(VideoInfo videoInfo)
        {
            _videoInfo = videoInfo;
            OpenStream();
        }

        // No finalizer: the COM object is held by a runtime callable wrapper (RCW),
        // which releases itself when collected. Calling ReleaseComObject from the
        // finalizer thread is unsafe, so it is only done from Dispose().

#pragma warning disable CS1591

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

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

#pragma warning restore CS1591

        /// <summary>
        /// Get current position as Frame.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This may not always correspond exactly to the frame of the file, since it is determined by the stream</remarks>
        public Frame GetCurrentPositionAsFrame() => Frame.FromSeconds(CurrentPosition, FrameRate);

        /// <summary>
        /// Set current position from Frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <remarks>This may not always correspond exactly to the frame of the file, since it is determined by the stream</remarks>
        public void SetCurrentPositionFromFrame(Frame frame)
        {
            CurrentPosition = frame.Seconds;
        }

        /// <summary>
        /// Open the video stream.
        /// </summary>
        /// <exception cref="VideoStreamException"></exception>
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
                // In case of an error, immediately release MediaDet resources
                Marshal.ReleaseComObject(_mediaDet);
                _mediaDet = null;

                // DESError.ThrowExceptionForHR(hr) throws COMException internally,
                // so catch it with a wrapped exception
                throw new VideoStreamException(e);
            }
        }

        /// <summary>
        /// Get the bitmap information of the frame at the specified second.
        /// </summary>
        /// <returns>Bitmap of the current position frame. <c>null</c> may be returned on failure.</returns>
        /// <exception cref="ObjectDisposedException">This object has been disposed</exception>
        /// <exception cref="InvalidOperationException">The stream is not open</exception>
        /// <exception cref="ArgumentOutOfRangeException">Second is out of range</exception>
        public Bitmap GetBitmap()
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(VideoCapture));

            if (_mediaDet == null)
                throw new InvalidOperationException("The stream is not open");

            var second = CurrentPosition;
            if (second < 0 || second > Duration)
                throw new ArgumentOutOfRangeException($"second is out of range: second {second}");

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

        private static VideoStreamException InvalidFrame(string message) =>
            new VideoStreamException(new COMException($"Invalid frame data ({message})", unchecked((int)0x80004005))); // E_FAIL

        private Bitmap GetBitmap(IMediaDet mediaDet, double second)
        {
            Bitmap bitmap = null;

            IntPtr pBuffer = IntPtr.Zero;
            var width = _videoInfo.Width;
            var height = _videoInfo.Height;
            try
            {
                // Need to get the bufferSize and identify the required buffer size
                int hr;
                hr = mediaDet.GetBitmapBits(second, out int bufferSize, pBuffer, width, height);
                DsError.ThrowExceptionForHR(hr);

                pBuffer = Marshal.AllocCoTaskMem(bufferSize);

                // Copy the bitmap bits into the buffer. This is what we actually want to do
                hr = mediaDet.GetBitmapBits(second, out bufferSize, pBuffer, width, height);
                DsError.ThrowExceptionForHR(hr);

                // The buffer is a BITMAPINFOHEADER followed by 24-bit pixel data.
                // Validate it before reading so a malformed result can never cause an out-of-bounds read.
                int headerSize = Marshal.SizeOf(typeof(BitmapInfoHeader));
                if (bufferSize < headerSize)
                    throw InvalidFrame($"buffer too small: {bufferSize} bytes");

                BitmapInfoHeader bmpHeader = (BitmapInfoHeader)Marshal.PtrToStructure(pBuffer, typeof(BitmapInfoHeader));
                if (bmpHeader.Size < headerSize || bmpHeader.BitCount != 24
                    || bmpHeader.Width != width || Math.Abs(bmpHeader.Height) != height)
                    throw InvalidFrame($"unexpected bitmap format: {bmpHeader.Width}x{bmpHeader.Height} {bmpHeader.BitCount}bpp");

                // DIB rows are padded to a multiple of 4 bytes.
                long srcStride = ((long)width * 3 + 3) & ~3L;
                if ((long)bmpHeader.Size + srcStride * height > bufferSize)
                    throw InvalidFrame($"buffer too small for {width}x{height}: {bufferSize} bytes");

                // A positive height means the DIB is stored bottom-up.
                bool bottomUp = bmpHeader.Height > 0;
                IntPtr pPixels = IntPtr.Add(pBuffer, bmpHeader.Size);

                bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                try
                {
                    BitmapData data = bitmap.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format24bppRgb);
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
            catch (COMException e)
            {
                // DESError.ThrowExceptionForHR(hr) throws COMException internally,
                // so catch it with a wrapped exception
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