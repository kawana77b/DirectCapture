#pragma warning disable CS1587
/**
 * This is not currently in use, but is left for reference purposes
 *

using System.Runtime.InteropServices;

namespace DirectCapture.DirectShow
{
    /// <summary>
    /// 左上隅と右下隅の座標によって四角形を定義します <br />
    /// https://learn.microsoft.com/ja-jp/windows/win32/api/windef/ns-windef-rect
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        /// <summary>
        /// 四角形の左上隅のx座標を指定します
        /// </summary>
        private int left;

        /// <summary>
        /// 四角形の左上隅のy座標を指定します
        /// </summary>
        private int top;

        /// <summary>
        /// 四角形の右下隅のx座標を指定します
        /// </summary>
        private int right;

        /// <summary>
        /// 四角形の右下隅のy座標を指定します
        /// </summary>
        private int bottom;
    }

    /// <summary>
    /// ビデオストリームに関する情報が含まれており、個々のフレームの形式を定義する_BITMAPINFOHEADER構造が含まれています <br />
    /// https://learn.microsoft.com/ja-jp/windows/win32/wmdm/-videoinfoheader
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct VIDEOINFOHEADER
    {
        /// <summary>
        /// ソースビデオウィンドウを指定する
        /// </summary>
        public RECT rcSource;

        /// <summary>
        /// 転送先のビデオウィンドウを指定する
        /// </summary>
        public RECT rcTarget;

        /// <summary>
        /// ビデオストリームのおおよそのデータレートを 1 秒あたりのビット数で指定する
        /// </summary>
        public uint dwBitRate;

        /// <summary>
        /// ビデオストリームのデータエラーレートを 1 秒あたりのビットエラー数で指定する
        /// </summary>
        public uint dwBitErrorRate;

        /// <summary>
        /// ビデオフレームの平均表示時間を 100 ナノ秒単位で指定する値です
        /// </summary>
        public long AvgTimePerFrame;

        /// <summary>
        /// ビデオ イメージビットマップの色と寸法情報を含む構造体
        /// </summary>
        public BITMAPINFOHEADER bmiHeader;
    }

    /// <summary>
    /// ビデオフレームの形式を定義します <br />
    /// https://learn.microsoft.com/ja-jp/windows/win32/wmdm/-bitmapinfoheader
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BITMAPINFOHEADER
    {
        /// <summary>
        /// 構造体に必要なバイト数を指定します
        /// </summary>
        public uint biSize;

        /// <summary>
        /// ビットマップの幅をピクセル単位で指定します
        /// </summary>
        public int biWidth;

        /// <summary>
        /// ビットマップの高さをピクセル単位で指定します
        /// </summary>
        /// <remarks>
        /// biHeight が正の場合、ビットマップはボトムアップ DIB になり、その原点は左下隅になります。
        /// biHeight が負の場合、ビットマップはトップダウン DIB になり、その原点は左上隅になります。
        /// biHeight が負の場合(トップダウン DIB を示す)、biCompression はBI_RGBまたはBI_BITFIELDSである必要があります。
        /// トップダウン DIB は圧縮できません
        /// </remarks>
        public int biHeight;

        /// <summary>
        /// ターゲット デバイスの平面の数を指定します。 この値は 1 に設定する必要があります。
        /// </summary>
        public ushort biPlanes;

        /// <summary>
        /// ピクセルあたりのビット数を指定します
        /// </summary>
        /// <remarks>
        /// BITMAPINFOHEADER 構造体の biBitCount メンバーは、各ピクセルを定義するビット数とビットマップ内の色の最大数を決定します
        /// </remarks>
        public ushort biBitCount;

        /// <summary>
        /// 圧縮されたボトムアップ ビットマップの圧縮の種類を指定します
        /// </summary>
        public uint biCompression;

        /// <summary>
        /// イメージのサイズをバイト単位で指定します。 これは、BI_RGBビットマップの場合は 0 に設定できます
        /// </summary>
        public uint biSizeImage;

        /// <summary>
        /// ビットマップのターゲット デバイスの水平方向の解像度 (メートルあたりのピクセル単位) を指定します
        /// </summary>
        public int biXPelsPerMeter;

        /// <summary>
        /// ビットマップのターゲット デバイスの垂直方向の解像度 (メートルあたりのピクセル単位) を指定します
        /// </summary>
        public int biYPelsPerMeter;

        /// <summary>
        /// ビットマップで実際に使用されるカラー テーブル内のカラーインデックスの数を指定します
        /// </summary>
        /// <remarks>
        /// この値が 0 の場合、ビットマップは、biCompression で指定された圧縮モードの biBitCount メンバーの値に対応する色の最大数を使用します
        /// </remarks>
        public uint biClrUsed;

        /// <summary>
        /// ビットマップを表示するために必要なカラー インデックスの数を指定します。 この値が 0 の場合は、すべての色が必要です
        /// </summary>
        public uint biClrImportant;
    }
}

*/
#pragma warning restore CS1587