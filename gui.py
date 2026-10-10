import sys
from PyQt6.QtWidgets import (QApplication, QMainWindow, QVBoxLayout, QHBoxLayout, 
                             QLabel, QPushButton, QWidget, QGroupBox)
from PyQt6.QtCore import QTimer, Qt
from wii_remote_handler import WiiRemoteHandler, WiiRemoteData

class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Wii Flight Stick Controller")
        self.resize(400, 600)

        # リモコンハンドラーの初期化
        self.handler = WiiRemoteHandler()
        self.handler.on_data_received = self.update_ui

        # メインウィジェットとレイアウト
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        layout = QVBoxLayout(central_widget)

        # --- IMU データ表示エリア ---
        imu_group = QGroupBox("IMU Data")
        imu_layout = QVBoxLayout()
        self.accel_label = QLabel("Accel: X: 0.0, Y: 0.0, Z: 0.0")
        self.gyro_label = QLabel("Gyro:  X: 0.0, Y: 0.0, Z: 0.0")
        imu_layout.addWidget(self.accel_label)
        imu_layout.addWidget(self.gyro_label)
        imu_group.setLayout(imu_layout)
        layout.addWidget(imu_group)

        # --- ボタン状態表示エリア ---
        btn_group = QGroupBox("Buttons")
        btn_layout = QHBoxLayout()
        self.btn_labels = []
        for i in range(10):
            lbl = QLabel(f"B{i}: Off")
            btn_layout.addWidget(lbl)
            self.btn_labels.append(lbl)
        btn_group.setLayout(btn_layout)
        layout.addWidget(btn_group)

        # --- LED / モーター操作エリア ---
        control_group = QGroupBox("Controls (LED/Motor)")
        ctrl_layout = QVBoxLayout()
        
        self.led_btn = QPushButton("Toggle LED")
        self.led_btn.clicked.connect(lambda: self.handler.send_command("LED", "TOGGLE"))
        
        self.motor_slider = QPushButton("Motor Speed (Mock)") # スライダーの代わりにボタンで簡易実装
        self.motor_slider.clicked.connect(lambda: self.handler.send_command("MOTOR", "SPEED_UP"))

        ctrl_layout.addWidget(self.led_btn)
        ctrl_layout.addWidget(self.motor_slider)
        control_group.setLayout(ctrl_layout)
        layout.addWidget(control_group)

        # --- 接続操作エリア ---
        conn_layout = QHBoxLayout()
        self.connect_btn = QPushButton("Connect")
        self.connect_btn.clicked.connect(self.connect_remote)
        self.disconnect_btn = QPushButton("Disconnect")
        self.disconnect_btn.clicked.connect(self.disconnect_remote)
        self.disconnect_btn.setEnabled(False)

        conn_layout.addWidget(self.connect_btn)
        conn_layout.addWidget(self.disconnect_btn)
        layout.addLayout(conn_layout)

    def connect_remote(self):
        self.handler.connect()
        self.connect_btn.setEnabled(False)
        self.disconnect_btn.setEnabled(True)

    def disconnect_remote(self):
        self.handler.disconnect()
        self.connect_btn.setEnabled(True)
        self.disconnect_btn.setEnabled(False)

    def update_ui(self, data: WiiRemoteData):
        # IMU更新
        self.accel_label.setText(f"Accel: X: {data.accel_x:.2f}, Y: {data.accel_y:.2f}, Z: {data.accel_z:.2f}")
        self.gyro_label.setText(f"Gyro:  X: {data.gyro_x:.2f}, Y: {data.gyro_y:.2f}, Z: {data.gyro_z:.2f}")

        # ボタン更新
        for i, pressed in enumerate(data.buttons):
            self.btn_labels[i].setText(f"B{i}: {'ON' if pressed else 'OFF'}")
            if pressed:
                self.btn_labels[i].setStyleSheet("color: red; font-weight: bold;")
            else:
                self.btn_labels[i].setStyleSheet("")

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())
