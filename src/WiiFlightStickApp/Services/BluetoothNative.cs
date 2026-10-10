using System;
using System.Runtime.InteropServices;

namespace WiiFlightStickApp.Services
{
    /// <summary>
    /// Windows Bluetooth API (Win32) の P/Invoke 定義
    /// </summary>
    public static class BluetoothNative
    {
        // Bluetoothデバイスの検索と情報の取得に関するAPI
        [DllImport("BluetoothAPIs.dll", CharSet = CharSet.Unicode)]
        public static extern int BluetoothFindFirst(ref Guid serviceGuid, out IntPtr pDevice, uint flags);

        [DllImport("BluetoothAPIs.dll", CharSet = CharSet.Unicode)]
        public static extern int BluetoothGetDeviceInfo(IntPtr pDevice, [Out] ref BluetoothDeviceInfo deviceInfo, uint flags);

        // RFCOMM接続やストリーム操作に必要なAPI（必要に応じて追加）
        // ※Wiiリモコンの通信にはRFCOMMプロトコルを使用するため、
        // 実際の実装ではこれらに関連するソケット操作が必要になります。

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BluetoothDeviceInfo
        {
            public int DeviceId;
            public int DeviceClass;
            public int Device_ID;
            public int Device_NameLength;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Device_Name;
        }

        // 定数定義
        public const uint Bluetooth_FLAG_NONE = 0x00000000;
    }
}
