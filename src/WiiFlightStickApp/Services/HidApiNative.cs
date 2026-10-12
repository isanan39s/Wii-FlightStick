using System;
using System.Runtime.InteropServices;

namespace WiiFlightStickApp.Services
{
    public static class HidApiNative
    {
        private const string DllName = "hidapi"; // hidapi.dll を探します

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hid_open(uint vendor_id, uint product_id, uint window_handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_read(IntPtr device, byte[] buffer, int length);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void hid_close(IntPtr device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hid_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void hid_exit();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_write(IntPtr device, byte[] data, int length);
    }
}
