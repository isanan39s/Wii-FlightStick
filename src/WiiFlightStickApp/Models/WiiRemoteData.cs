namespace WiiFlightStickApp.Models
{
    public class WiiRemoteData
    {
        public float AccelX { get; set; }
        public float AccelY { get; set; }
        public float AccelZ { get; set; }
        public float GyroX { get; set; }
        public float GyroY { get; set; }
        public float GyroZ { get; set; }
        public bool[] Buttons { get; set; } = new bool[10]; // [A, B, 1, 2, +, -, Left, Right, Up, Down]
        public Dictionary<string, object> ExtendedData { get; set; } = new Dictionary<string, object>();
    }
}
