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

        /// <summary>
        /// Finalizer of VideoCapture.
        /// </summary>
        ~VideoCapture()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

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
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                if (_mediaDet != null)
                {
                    Marshal.ReleaseComObject(_mediaDet);
                    _mediaDet = null;
                }

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
        /// <exception cref="InvalidOperationException">The stream is not open</exception>
        /// <exception cref="ArgumentOutOfRangeException">Second is out of range</exception>
        public Bitmap GetBitmap()
        {
            if (_mediaDet == null)
                throw new InvalidOperationException("The stream is not open");

            var second = CurrentPosition;
            if (second < 0 && second > Duration)
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

                unsafe
                {
                    // Get the pointer to the bitmap bits
                    byte* pBmpBuffer = (byte*)pBuffer.ToPointer();
                    BitmapInfoHeader bmpHeader = (BitmapInfoHeader)Marshal.PtrToStructure(pBuffer, typeof(BitmapInfoHeader));

                    pBmpBuffer += bmpHeader.Size;

                    // Create an empty Bitmap with screen information in it to write data
                    bitmap = new Bitmap(
                        width,
                        height,
                        PixelFormat.Format24bppRgb);

                    // Lock the bitmap's bits
                    BitmapData data = bitmap.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format24bppRgb);

                    // Copy data into the bitmap
                    IntPtr pBmpData = new IntPtr(pBmpBuffer);
                    CopyMemory(
                        data.Scan0,
                        pBmpData,
                        width * height * 3); // To use RGB channels, multiply by 3 for the number of bytes in a pixel

                    bitmap.UnlockBits(data);
                }

                // The generated bitmap must be inverted
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
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