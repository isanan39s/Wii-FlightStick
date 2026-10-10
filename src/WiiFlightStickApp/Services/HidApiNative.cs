using System;
using System.Runtime.InteropServices;

namespace WiiFlightStickApp.Services
{
    /// <summary>
    /// HIDAPI を介したデバイス操作のための P/Invoke 定義
    /// </summary>
    public static class HidApiNative
    {
        // HIDAPI の基本関数
        [DllImport("hidapi.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr hid_open(uint vendor_id, uint product_id, uint window_handle);

        [DllImport("hidapi.dll", CharSet = CharSet.Unicode)]
        public static extern int hid_write(IntPtr device, byte[] data, int length);

        [DllImport("hidapi.dll", CharSet = CharSet.Unicode)]
        public static extern int hid_read(IntPtr device, byte[] buffer, int length);

        [DllImport("hidapi.dll", CharSet = CharSet.Unicode)]
        public static extern int hid_close(IntPtr device);

        // Wiiリモコンの識別子 (モデルによって異なる場合があります)
        // 一般的なWii Remote の VID/PID を設定
        public const uint WiiRemoteVendorId = 0x057E; // Nintendo Co., Ltd.
        public const uint WiiRemoteProductId = 0x2001; // 例: RVL-CNT-01

        // 注意: この実装を動かすには、実行環境に hidapi.dll が必要です。
    }
}
