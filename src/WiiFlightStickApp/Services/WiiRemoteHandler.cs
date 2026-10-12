using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using WiiFlightStickApp.Models;

namespace WiiFlightStickApp.Services
{
    public class WiiRemoteHandler
    {
        private WiiRemoteData _currentData = new WiiRemoteData();
        private bool _isRunning = false;
        private Thread? _workerThread;
        private readonly object _lock = new object();
        
        // ネイティブハンドル
        private IntPtr _deviceHandle = IntPtr.Zero;

        public uint VendorId { get; set; } = 0x057E; // Nintendo Co., Ltd.
        public uint ProductId { get; set; } = 0x0330; // RVL-CNT-01 (Detected as 0x0330)

        public event Action<WiiRemoteData>? OnDataReceived;

        public WiiRemoteData CurrentData
        {
            get { lock (_lock) return _currentData; }
        }

        public bool IsConnected { get; private set; }

        // 現在のレポートモードを保持
        public byte CurrentModeId { get; private set; } = 0x30;

        // LEDの状態管理 (true = ON)
        private bool _ledState = false;

        // ログファイルへの書き込み用メソッド
        private void Log(string message)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wii_log.txt");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            try
            {
                File.AppendAllText(logPath, $"[{timestamp}] {message}{Environment.NewLine}");
            }
            catch { /* ログ書き込み失敗は無視 */ }
        }

        /// <summary>
        /// ネイティブ HIDAPI を使用してデバイスに接続します。
        /// </summary>
        public void Connect()
        {
            Log("========================================");
            Console.WriteLine("========================================");
            Console.WriteLine("[DEBUG] Starting Connection Process...");
            Log("[DEBUG] Starting Connection Process...");
            Console.WriteLine($"[DEBUG] Target VID: 0x{VendorId:X4}, PID: 0x{ProductId:X4}");
            Log($"[DEBUG] Target VID: 0x{VendorId:X4}, PID: 0x{ProductId:X4}");

            try
            {
                // HIDAPIの初期化（一部環境で必要）
                HidApiNative.hid_init();
                Console.WriteLine("[DEBUG] hid_init() called.");
                Log("[DEBUG] hid_init() called.");

                // デバイスを開く
                _deviceHandle = HidApiNative.hid_open(VendorId, ProductId, 0);

                if (_deviceHandle == IntPtr.Zero)
                {
                    Console.WriteLine("[ERROR] Failed to open device handle (Returned: 0).");
                    Log("[ERROR] Failed to open device handle (Returned: 0).");
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
                Console.WriteLine("[SUCCESS] Connected successfully!");
                Log("[SUCCESS] Connected successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
                Log($"[CRITICAL ERROR] {ex.Message}");
                IsConnected = false;
            }
        }

        public void Disconnect()
        {
            Console.WriteLine("[DEBUG] Disconnecting...");
            Log("[DEBUG] Disconnecting...");
            _isRunning = false;
            IsConnected = false; 

            if (_deviceHandle != IntPtr.Zero)
            {
                Thread closeThread = new Thread(() => {
                    HidApiNative.hid_close(_deviceHandle);
                    _deviceHandle = IntPtr.Zero;
                    Console.WriteLine("[DEBUG] Device handle closed.");
                    Log("[DEBUG] Device handle closed.");
                });
                closeThread.IsBackground = true;
                closeThread.Start();
            }

            Console.WriteLine("[DEBUG] Disconnected.");
            Log("[DEBUG] Disconnected.");
        }

        private void UpdateLoop()
        {
            byte[] buffer = new byte[64]; 

            while (_isRunning)
            {
                if (_deviceHandle != IntPtr.Zero && IsConnected)
                {
                    try
                    {
                        int bytesRead = HidApiNative.hid_read(_deviceHandle, buffer, buffer.Length);
                        
                        if (bytesRead > 0)
                        {
                            string hexData = BitConverter.ToString(buffer, 0, bytesRead).Replace("-", "");
                            Log($"[RAW] {hexData}");
                            Console.WriteLine($"[RAW] {hexData}");

                            var parsedData = WiiPacketParser.Parse(buffer);

                            lock (_lock)
                            {
                                _currentData = parsedData;
                                // 受信したレポートIDを更新
                                CurrentModeId = buffer[0];
                            }

                            OnDataReceived?.Invoke(_currentData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DEBUG] Read Error: {ex.Message}");
                        Log($"[DEBUG] Read Error: {ex.Message}");
                        IsConnected = false;
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }

                Thread.Sleep(10); 
            }
        }

        public bool SendRawReport(byte[] data)
        {
            if (_deviceHandle != IntPtr.Zero && IsConnected)
            {
                int result = HidApiNative.hid_write(_deviceHandle, data, data.Length);
                if (result > 0)
                {
                    Log($"[RAW_WRITE] {BitConverter.ToString(data).Replace("-", "")}");
                    return true;
                }
            }
            Console.WriteLine("Failed to send raw report.");
            Log("Failed to send raw report.");
            return false;
        }

        public void SetMode(byte mode)
        {
            if (_deviceHandle != IntPtr.Zero && IsConnected)
            {
                // ドキュメントより: (a2) 12 TT MM
                // レポートIDは 0x12 固定。
                // TTバイトにビット2 (0x04) をセットして、連続報告および機能有効化を行う。
                byte[] report = new byte[] { 0x12, 0x04, mode };
                SendRawReport(report);
            }
        }

        /// <summary>
        /// ジャイロ（Motion Plus）を初期化するためのコマンドを送ります。
        /// </summary>
        public void InitGyro()
        {
            if (_deviceHandle != IntPtr.Zero && IsConnected)
            {
                Console.WriteLine("[COMMAND] Init Gyro");
                Log("[COMMAND] Init Gyro");

                // 1. モード0x35（IMUデータを含むモード）への切り替え
                // 正しいレポートID 0x12 を使用。TTバイトにビット2 (0x04) をセット。
                byte[] modeReport = new byte[] { 0x12, 0x04, 0x35 };
                SendRawReport(modeReport);

                // 通信の安定待ち
                Thread.Sleep(200);

                // 2. MotionPlusセンサーを明示的に有効化するためのメモリ書き込み
                // ユーザー提案: 16 04 A6 00 FE 04
                // ドキュメント仕様に基づくパケット構造:
                // [ID] [MM] [AddrH] [AddrL] [Len] [Data...]
                // レポートID 0x16, MM=0x04 (コントロールレジスタ), オフセット=0xA600, 長さ=2, データ=FE 04
                // さらに、すべての出力レポートの最初のバイトにビット2 (0x04) を立てる必要があるため、
                // レポートID 0x16 | 0x04 = 0x1c を使用。
                byte[] memWriteReport = new byte[] { 0x1c, 0x04, 0xA6, 0x00, 0x02, 0xFE, 0x04 };
                SendRawReport(memWriteReport);

                // 通信の安定待ち
                Thread.Sleep(100);

                // 3. ステータス情報の要求 (Report 0x15)
                byte[] statusRequest = new byte[] { 0x15, 0x00 };
                SendRawReport(statusRequest);

                Console.WriteLine("[COMMAND] Init Gyro Sequence Completed");
                Log("[COMMAND] Init Gyro Sequence Completed");
            }
        }

        public void SendCommand(string commandType, object value)
        {
            if (_deviceHandle != IntPtr.Zero && IsConnected)
            {
                Console.WriteLine($"[COMMAND] {commandType} = {value}");
                Log($"[COMMAND] {commandType} = {value}");

                switch (commandType.ToUpper())
                {
                    case "LED":
                        // ドキュメントより、レポートID 0x11 が Player LEDs。
                        // ビット2(0x04)がON/OFFフラグ。
                        _ledState = !_ledState; // トグル処理
                        byte ledByte = (byte)(_ledState ? 0x04 : 0x00);
                        // EEPROM保護のため最初のバイトにビット2 (0x04) をセットした 0x15 を使用。
                        byte[] ledReport = new byte[] { 0x15, ledByte };
                        SendRawReport(ledReport);
                        break;

                    case "MOTOR":
                        // ドキュメントより、レポートID 0x10 が Rumble。
                        // ビット0(0x01)がRumble機能のON/OFF。
                        // EEPROM保護のため最初のバイトにビット2 (0x04) をセットした 0x14 を使用。
                        byte[] rumbleReport = new byte[] { 0x14, 0x01 };
                        SendRawReport(rumbleReport);
                        break;

                    case "INIT_GYRO":
                        InitGyro();
                        break;

                    case "MODE":
                        if (value is byte mode)
                        {
                            SetMode(mode);
                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine("Cannot send command: Device not connected.");
                Log("Cannot send command: Device not connected.");
            }
        }

        ~WiiRemoteHandler()
        {
            Disconnect();
        }
    }
}