using System;

namespace WiiFlightStickApp.Models
{
    public class WiiRemoteData
    {
        // 加速度センサーデータ
        public double AccelX { get; set; }
        public double AccelY { get; set; }
        public double AccelZ { get; set; }

        // ジャイロセンサーデータ
        public double GyroX { get; set; }
        public double GyroY { get; set; }
        public double GyroZ { get; set; }

        // ボタン状態 (16ビット分を確保)
        public bool[] Buttons { get; set; } = new bool[16];

        // デバッグ用生のデータ文字列
        public string RawDataString { get; set; } = "";
    }
}