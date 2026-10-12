using System;
using System.Drawing;
using System.Windows.Forms;
using WiiFlightStickApp.Models;
using WiiFlightStickApp.Services;

namespace WiiFlightStickApp;

public partial class Form1 : Form
{
    private WiiRemoteHandler _handler;
    private Label? _accelXLabel;
    private Label? _accelYLabel;
    private Label? _accelZLabel;
    private Label? _gyroXLabel;
    private Label? _gyroYLabel;
    private Label? _gyroZLabel;
    private Label? _rawDataLabel;
    private Label? _modeLabel;
    private Label[] _btnLabels = new Label[16];
    private Button? _ledBtn;
    private Button? _motorBtn;
    private Button? _disconnectBtn;
    private Button? _connectBtn;

    // UI更新用のタイマー
    private System.Windows.Forms.Timer? _uiUpdateTimer;

    public Form1()
    {
        this.Text = "Wii Flight Stick - Debug Mode";
        this.Size = new Size(450, 880); // ボタンやモード表示のために少し高さを拡張
        this.StartPosition = FormStartPosition.CenterScreen;

        _handler = new WiiRemoteHandler();

        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        var mainLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        // --- Mode & Connection Section ---
        var statusGroup = new GroupBox { Text = "Current Connection Status", Size = new Size(400, 120), Margin = new Padding(0, 0, 0, 20) };
        var statusLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = false };
        _modeLabel = new Label { Text = $"MODE: 0x30", AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.Red };
        var modeInput = new NumericUpDown { Minimum = 0x30, Maximum = 0x3f, Value = 0x31, Width = 60 };
        var applyModeBtn = new Button { Text = "Set Mode", Size = new Size(100, 40) };
        applyModeBtn.Click += (s, e) => _handler.SetMode((byte)modeInput.Value);

        statusLayout.Controls.Add(_modeLabel);
        statusLayout.Controls.Add(new Label { Text = "Change Mode:", AutoSize = true });
        statusLayout.Controls.Add(modeInput);
        statusLayout.Controls.Add(applyModeBtn);
        statusGroup.Controls.Add(statusLayout);
        mainLayout.Controls.Add(statusGroup);

        // --- IMU Section ---
        var imuGroup = new GroupBox { Text = "Raw Sensor Values (No Calculation)", Size = new Size(400, 180), Margin = new Padding(0, 0, 0, 20) };
        var imuLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = false };
        
        _accelXLabel = new Label { Text = "Accel X: 0", AutoSize = true, Font = new Font("Consolas", 14) };
        _accelYLabel = new Label { Text = "Accel Y: 0", AutoSize = true, Font = new Font("Consolas", 14) };
        _accelZLabel = new Label { Text = "Accel Z: 0", AutoSize = true, Font = new Font("Consolas", 14) };
        
        _gyroXLabel = new Label { Text = "Gyro X: 0", AutoSize = true, Font = new Font("Consolas", 14) };
        _gyroYLabel = new Label { Text = "Gyro Y: 0", AutoSize = true, Font = new Font("Consolas", 14) };
        _gyroZLabel = new Label { Text = "Gyro Z: 0", AutoSize = true, Font = new Font("Consolas", 14) };

        imuLayout.Controls.Add(_accelXLabel);
        imuLayout.Controls.Add(_accelYLabel);
        imuLayout.Controls.Add(_accelZLabel);
        imuLayout.Controls.Add(new Label { Text = "--------------------", AutoSize = true });
        imuLayout.Controls.Add(_gyroXLabel);
        imuLayout.Controls.Add(_gyroYLabel);
        imuLayout.Controls.Add(_gyroZLabel);
        imuGroup.Controls.Add(imuLayout);
        mainLayout.Controls.Add(imuGroup);

        // --- Raw Data Section (Debug) ---
        var debugGroup = new GroupBox { Text = "Raw Hex Stream", Size = new Size(400, 60), Margin = new Padding(0, 0, 0, 20) };
        var debugLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = false };
        _rawDataLabel = new Label { Text = "Raw Data: ", Size = new Size(370, 36), ForeColor = Color.Blue, Font = new Font("Consolas", 9) };
        debugLayout.Controls.Add(_rawDataLabel);
        debugGroup.Controls.Add(debugLayout);
        mainLayout.Controls.Add(debugGroup);

        // --- Buttons Section ---
        var btnGroup = new GroupBox { Text = "Buttons (Byte1: 0-7, Byte2: 8-15)", Size = new Size(400, 220), Margin = new Padding(0, 0, 0, 20) };
        var btnLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = false };
        for (int i = 0; i < 16; i++)
        {
            // B5, B6, B13, B14 を非表示にする
            if (i == 5 || i == 6 || i == 13 || i == 14) continue;

            _btnLabels[i] = new Label { Text = $"B{i}: Off", AutoSize = true, Margin = new Padding(2) };
            btnLayout.Controls.Add(_btnLabels[i]);
        }
        btnGroup.Controls.Add(btnLayout);
        mainLayout.Controls.Add(btnGroup);

        // --- Controls Section ---
        var ctrlGroup = new GroupBox { Text = "Commands", Size = new Size(400, 180), Margin = new Padding(0, 0, 0, 20) };
        var ctrlLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = false };
        _ledBtn = new Button { Text = "Toggle LED", Size = new Size(150, 40) };
        _ledBtn.Click += (s, e) => _handler.SendCommand("LED", null);
        _motorBtn = new Button { Text = "Rumble ON", Size = new Size(150, 40) };
        _motorBtn.Click += (s, e) => _handler.SendCommand("MOTOR", null);
        var motorOffBtn = new Button { Text = "Rumble OFF", Size = new Size(150, 40) };
        motorOffBtn.Click += (s, e) => _handler.SendCommand("MOTOR", "OFF");
        var gyroInitBtn = new Button { Text = "Initialize Gyro", Size = new Size(150, 40), ForeColor = Color.DarkBlue };
        gyroInitBtn.Click += (s, e) => _handler.SendCommand("INIT_GYRO", null);

        ctrlLayout.Controls.Add(_ledBtn);
        ctrlLayout.Controls.Add(_motorBtn);
        ctrlLayout.Controls.Add(motorOffBtn);
        ctrlLayout.Controls.Add(gyroInitBtn);
        ctrlGroup.Controls.Add(ctrlLayout);
        mainLayout.Controls.Add(ctrlGroup);

        // --- Connection Section ---
        var connLayout = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, FlowDirection = FlowDirection.LeftToRight };
        _connectBtn = new Button { Text = "Connect", Size = new Size(100, 40) };
        _disconnectBtn = new Button { Text = "Disconnect", Size = new Size(100, 40), Enabled = false };

        _connectBtn.Click += (s, e) =>
        {
            _handler.Connect();
            _connectBtn.Enabled = false;
            _disconnectBtn.Enabled = true;
        };
        _disconnectBtn.Click += (s, e) =>
        {
            _handler.Disconnect();
            _connectBtn.Enabled = true;
            _disconnectBtn.Enabled = false;
        };

        connLayout.Controls.Add(_connectBtn);
        connLayout.Controls.Add(_disconnectBtn);
        mainLayout.Controls.Add(connLayout);

        this.Controls.Add(mainLayout);

        // --- UI Update Timer Setup ---
        _uiUpdateTimer = new System.Windows.Forms.Timer { Interval = 30 }; // ~33 FPS
        _uiUpdateTimer.Tick += UpdateUi;
        _uiUpdateTimer.Start();
    }

    private void UpdateUi(object? sender, EventArgs e)
    {
        var data = _handler.CurrentData;

        if (data == null) return;

        // モードIDの更新
        if (_modeLabel != null)
        {
            _modeLabel.Text = $"MODE: 0x{_handler.CurrentModeId:X2}";
            // もしモードが31未満なら警告色にする（IMUデータがない可能性が高いため）
            _modeLabel.ForeColor = _handler.CurrentModeId < 0x31 ? Color.Red : Color.Green;
        }

        if (_accelXLabel != null) { _accelXLabel.Text = $"Accel X: {data.AccelX}"; }
        if (_accelYLabel != null) { _accelYLabel.Text = $"Accel Y: {data.AccelY}"; }
        if (_accelZLabel != null) { _accelZLabel.Text = $"Accel Z: {data.AccelZ}"; }

        if (_gyroXLabel != null) { _gyroXLabel.Text = $"Gyro X: {data.GyroX}"; }
        if (_gyroYLabel != null) { _gyroYLabel.Text = $"Gyro Y: {data.GyroY}"; }
        if (_gyroZLabel != null) { _gyroZLabel.Text = $"Gyro Z: {data.GyroZ}"; }

        if (_rawDataLabel != null)
        {
            _rawDataLabel.Text = $"Raw Data: {data.RawDataString}";
        }

        for (int i = 0; i < 16; i++)
        {
            // 非表示にしたボタンのラベルも更新は行う（配列要素を維持するため）
            if (_btnLabels[i] != null)
            {
                _btnLabels[i].Text = $"B{i}: {(data.Buttons[i] ? "ON" : "OFF")}";
                if (data.Buttons[i])
                    _btnLabels[i].ForeColor = Color.Red;
                else
                    _btnLabels[i].ForeColor = Color.Black;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _uiUpdateTimer?.Stop();
        _handler.Disconnect();
        base.OnFormClosing(e);
    }
}