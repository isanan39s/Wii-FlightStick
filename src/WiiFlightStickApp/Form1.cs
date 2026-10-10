using System;
using System.Drawing;
using System.Windows.Forms;
using WiiFlightStickApp.Models;
using WiiFlightStickApp.Services;

namespace WiiFlightStickApp;

public partial class Form1 : Form
{
    private WiiRemoteHandler _handler;
    private Label? _accelLabel;
    private Label? _gyroLabel;
    private Label[] _btnLabels = new Label[10];
    private Button? _ledBtn;
    private Button? _motorBtn;
    private Button? _connectBtn;
    private Button? _disconnectBtn;

    public Form1()
    {
        this.Text = "Wii Flight Stick Controller";
        this.Size = new Size(450, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        _handler = new WiiRemoteHandler();
        _handler.OnDataReceived += HandleDataReceived;

        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        var mainLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20)
        };

        // --- IMU Section ---
        var imuGroup = new GroupBox { Text = "IMU Data", Size = new Size(400, 100), Margin = new Padding(0, 0, 0, 20) };
        var imuLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = false };
        _accelLabel = new Label { Text = "Accel: X: 0.0, Y: 0.0, Z: 0.0", AutoSize = true };
        _gyroLabel = new Label { Text = "Gyro:  X: 0.0, Y: 0.0, Z: 0.0", AutoSize = true };
        imuLayout.Controls.Add(_accelLabel);
        imuLayout.Controls.Add(_gyroLabel);
        imuGroup.Controls.Add(imuLayout);
        mainLayout.Controls.Add(imuGroup);

        // --- Buttons Section ---
        var btnGroup = new GroupBox { Text = "Buttons", Size = new Size(400, 120), Margin = new Padding(0, 0, 0, 20) };
        var btnLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = false };
        for (int i = 0; i < 10; i++)
        {
            _btnLabels[i] = new Label { Text = $"B{i}: Off", AutoSize = true, Margin = new Padding(5) };
            btnLayout.Controls.Add(_btnLabels[i]);
        }
        btnGroup.Controls.Add(btnLayout);
        mainLayout.Controls.Add(btnGroup);

        // --- Controls Section ---
        var ctrlGroup = new GroupBox { Text = "Controls (LED/Motor)", Size = new Size(400, 120), Margin = new Padding(0, 0, 0, 20) };
        var ctrlLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = false };
        _ledBtn = new Button { Text = "Toggle LED", Size = new Size(150, 40) };
        _ledBtn.Click += (s, e) => _handler.SendCommand("LED", "TOGGLE");
        _motorBtn = new Button { Text = "Motor Speed Up", Size = new Size(150, 40) };
        _motorBtn.Click += (s, e) => _handler.SendCommand("MOTOR", "SPEED_UP");
        ctrlLayout.Controls.Add(_ledBtn);
        ctrlLayout.Controls.Add(_motorBtn);
        ctrlGroup.Controls.Add(ctrlLayout);
        mainLayout.Controls.Add(ctrlGroup);

        // --- Connection Section ---
        var connLayout = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, FlowDirection = FlowDirection.LeftToRight };
        _connectBtn = new Button { Text = "Connect", Size = new Size(100, 40) };
        _disconnectBtn = new Button { Text = "Disconnect", Size = new Size(100, 40), Enabled = false };
        _connectBtn.Click += (s, e) => {
            _handler.Connect();
            _connectBtn.Enabled = false;
            _disconnectBtn.Enabled = true;
        };
        _disconnectBtn.Click += (s, e) => {
            _handler.Disconnect();
            _connectBtn.Enabled = true;
            _disconnectBtn.Enabled = false;
        };
        connLayout.Controls.Add(_connectBtn);
        connLayout.Controls.Add(_disconnectBtn);
        mainLayout.Controls.Add(connLayout);

        this.Controls.Add(mainLayout);
    }

    private void HandleDataReceived(WiiRemoteData data)
    {
        // UIスレッドで更新を実行
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => HandleDataReceived(data)));
            return;
        }

        if (_accelLabel != null)
        {
            _accelLabel.Text = $"Accel: X: {data.AccelX:F2}, Y: {data.AccelY:F2}, Z: {data.AccelZ:F2}";
        }
        if (_gyroLabel != null)
        {
            _gyroLabel.Text = $"Gyro:  X: {data.GyroX:F2}, Y: {data.GyroY:F2}, Z: {data.GyroZ:F2}";
        }

        for (int i = 0; i < 10; i++)
        {
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
        _handler.Disconnect();
        base.OnFormClosing(e);
    }
}
