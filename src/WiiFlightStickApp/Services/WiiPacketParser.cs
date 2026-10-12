using System;
using WiiFlightStickApp.Models;

namespace WiiFlightStickApp.Services
{
    public static class WiiPacketParser
    {
        /// <summary>
        /// 生のバイト列を解析してWiiRemoteDataに変換します。
        /// </summary>
        /// <param name="rawData">HIDから受信した生のバイト配列</param>
        /// <returns>解析されたデータ</returns>
        public static WiiRemoteData Parse(byte[] rawData)
        {
            var data = new WiiRemoteData();

            if (rawData == null || rawData.Length < 3) return data;

            // デバッグ用に生のデータを文字列として保持（UI表示用）
            data.RawDataString = BitConverter.ToString(rawData, 0, Math.Min(rawData.Length, 32));

            byte reportId = rawData[0];

            // ボタンデータの解析 (ほとんどのレポートでByte 1-2がボタンデータ)
            if (rawData.Length >= 3)
            {
                // Byte 1 をボタン 0-7 に割り当て
                byte b1 = rawData[1];
                for (int i = 0; i < 8; i++)
                {
                    data.Buttons[i] = (b1 & (1 << i)) != 0;
                }

                // Byte 2 をボタン 8-15 に割り当て
                byte b2 = rawData[2];
                for (int i = 0; i < 8; i++)
                {
                    data.Buttons[i + 8] = (b2 & (1 << i)) != 0;
                }
            }

            switch (reportId)
            {
                case 0x31: // Core Buttons and Accelerometer
                case 0x35: // Core Buttons and Accelerometer with 16 Extension Bytes
                case 0x3E: // Interleaved Core Buttons and Accelerometer (Part 1)
                case 0x3F: // Interleaved Core Buttons and Accelerometer (Part 2)
                    if (rawData.Length >= 6)
                    {
                        // 加速度センサーの生値をそのまま格納（計算なし）
                        data.AccelX = rawData[3];
                        data.AccelY = rawData[4];
                        data.AccelZ = rawData[5];

                        // ジャイロセンサーの生値（もし含まれていれば）
                        if (rawData.Length >= 9||true)
                        {
                            data.GyroX = rawData[6];
                            data.GyroY = rawData[7];
                            data.GyroZ = rawData[8];
                        }
                    }
                    break;

                default:
                    break;
            }

            return data;
        }

        /// <summary>
        /// バイト配列からビッグエンディアンの符号付き16ビット整数を読み取ります。
        /// </summary>
        private static double ReadSignedShort(byte[] data, int offset)
        {
            if (offset + 1 >= data.Length) return 0;
            
            // ビッグエンディアンとして結合
            short value = (short)((data[offset] << 8) | data[offset + 1]);
            return value;
        }
    }
}