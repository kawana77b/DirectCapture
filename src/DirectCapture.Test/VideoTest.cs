namespace DirectCapture.Test;

/**
 * This test must be run locally and the AMV4 codec must be available
 * Windows Media Player must be initially configured and ready to play.
 * http://www.amarectv.com/english/amv4.html
 */

public class VideoTests
{
    public static string DataDir
    {
        get
        {
            var baseDir = Path.GetFullPath(Path.Join(AppContext.BaseDirectory));
            var rootDir = Path.GetFullPath(Path.Join(baseDir, "..", "..", "..", "..", "DirectCapture.Test"));
            var dataDir = Path.Join(rootDir, "data");

            return dataDir;
        }
    }

    [SetUp]
    public void Setup()
    {
    }

    [Test(Description = "Open the test data and check if various types of information can be obtained.")]
    public void VideoInfoTest()
    {
        var videoPath = Path.Join(DataDir, "FF1.avi");
        var fi = new VideoInfo(videoPath);

        Assert.Multiple(() =>
        {
            Assert.That(fi.Duration, Is.EqualTo(5.6666667));
            Assert.That(fi.Width, Is.EqualTo(824));
            Assert.That(fi.Height, Is.EqualTo(576));
        });
    }

    [Test(Description = "Open the test data and check if the frame can be obtained.")]
    public void VideoCaptureTest()
    {
        var videoPath = Path.Join(DataDir, "FF1.avi");

        using var capture = new VideoCapture(videoPath);
        capture.CurrentPosition = 3.0;
        using var bmp = capture.GetBitmap();
        bmp.Save(Path.Join(DataDir, "FF1.png"));
        Assert.Multiple(() =>
        {
            if (bmp != null)
            {
                Assert.That(bmp.Width, Is.EqualTo(824));
                Assert.That(bmp.Height, Is.EqualTo(576));
            }
            else
            {
                Assert.Fail("Failed to get bitmap.");
            }
        });
    }
}