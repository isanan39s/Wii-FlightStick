using System;
using System.Collections.Generic;
using System.Threading;
using WiiFlightStickApp.Models;

namespace WiiFlightStickApp.Services
{
    public class WiiRemoteHandler
    {
        private WiiRemoteData _currentData = new WiiRemoteData();
        private bool _isRunning = false;
        private Thread? _workerThread;
        private readonly object _lock = new object();
        
        // HIDAPI デバイスハンドル
        private IntPtr _deviceHandle = IntPtr.Zero;

        public event Action<WiiRemoteData>? OnDataReceived;

        public WiiRemoteData CurrentData
        {
            get { lock (_lock) return _currentData; }
        }

        public bool IsConnected { get; private set; }

        /// <summary>
        /// HIDAPI を使用して指定された VID/PID のデバイスに接続します。
        /// </summary>
        public void Connect()
        {
            Console.WriteLine($"Attempting to connect to Wii Remote (VID: 0x{HidApiNative.WiiRemoteVendorId:X4}, PID: 0x{HidApiNative.WiiRemoteProductId:X4})...");

            // HIDAPI を使用してデバイスを開く
            _deviceHandle = HidApiNative.hid_open(HidApiNative.WiiRemoteVendorId, HidApiNative.WiiRemoteProductId, 0);

            if (_deviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open Wii Remote. Ensure it is connected and recognized as a HID device.");
                return;
            }

            IsConnected = true;
            _isRunning = true;

            _workerThread = new Thread(UpdateLoop)
            {
                IsBackground = true,
                Name = "WiiRemoteWorker"
            };
            _workerThread.Start();
            Console.WriteLine("Connected successfully.");
        }

        public void Disconnect()
        {
            _isRunning = false;
            _workerThread?.Join();

            if (_deviceHandle != IntPtr.Zero)
            {
                HidApiNative.hid_close(_deviceHandle);
                _deviceHandle = IntPtr.Zero;
            }

            IsConnected = false;
            Console.WriteLine("Disconnected.");
        }

        private void UpdateLoop()
        {
            byte[] buffer = new byte[64]; // Wiiリモコンのレポートサイズに合わせて調整

            while (_isRunning)
            {
                if (_deviceHandle != IntPtr.Zero && IsConnected)
                {
                    int bytesRead = HidApiNative.hid_read(_deviceHandle, buffer, buffer.Length);
                    
                    if (bytesRead > 0)
                    {
                        // 受信したバイト列を解析してデータに変換
                        var parsedData = WiiPacketParser.Parse(buffer);

                        lock (_lock)
                        {
                            _currentData = parsedData;
                        }

                        OnDataReceived?.Invoke(_currentData);
                    }
                }
                else
                {
                    // 切断された場合の処理（必要に応じて再接続を試行）
                    Thread.Sleep(1000);
                }

                Thread.Sleep(10); // 高速なポーリング
            }
        }

        public void SendCommand(string commandType, object value)
        {
            if (_deviceHandle != IntPtr.Zero && IsConnected)
            {
                // HIDAPI を使用してデータを書き込む処理をここに実装します。
                Console.WriteLine($"Sending Command: {commandType} = {value}");
            }
            else
            {
                Console.WriteLine("Cannot send command: Device not connected.");
            }
        }
    }
}
