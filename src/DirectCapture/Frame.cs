using System;

namespace DirectCapture
{
    /// <summary>
    /// Represents a frame of a video.
    /// </summary>
    public class Frame : ICloneable
    {
        /// <summary>
        /// Generate a Frame object from the specified value and frame rate.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="frameRate"></param>
        /// <returns></returns>
        public static Frame FromSeconds(double seconds, double frameRate = 30.0) => new Frame((int)(seconds * frameRate), frameRate);

        private int _value;

        /// <summary>
        /// Frame number.
        /// </summary>
        /// <remarks>If the value is out of range, it will be adjusted to the nearest value. min and start value is 0</remarks>
        public int Value
        {
            get => _value;
            set
            {
                _value = value < 0 ? 0 : value;
            }
        }

        private double _frameRate;

        /// <summary>
        /// Frame rate of the video.
        /// </summary>
        /// <remarks>If the value is out of range, it will be adjusted to the nearest value. min value is 1.0</remarks>
        public double FrameRate
        {
            get => _frameRate;
            private set
            {
                _frameRate = value < 1.0 ? 1.0 : value;
            }
        }

        /// <summary>
        /// Time of the frame.
        /// </summary>
        public TimeSpan Time => TimeSpan.FromSeconds(Value / FrameRate);

        /// <summary>
        /// Time of the frame in seconds.
        /// </summary>
        public double Seconds => Time.TotalSeconds;

        /// <summary>
        /// Generate a Frame object from the specified value and frame rate.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="frameRate"></param>
        public Frame(int value = 0, double frameRate = 30.0)
        {
            Value = value;
            FrameRate = frameRate;
        }

        /// <summary>
        /// Clone the Frame object.
        /// </summary>
        /// <returns></returns>
        public Frame Clone() => new Frame(Value, FrameRate);

        /// <summary>
        /// Clone the Frame object.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Value} ({Time})";
    }
}