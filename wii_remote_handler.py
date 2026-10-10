import time
import threading
from dataclasses import dataclass
from typing import Optional, Callable

@dataclass
class WiiRemoteData:
    accel_x: float = 0.0
    accel_y: float = 0.0
    accel_z: float = 0.0
    gyro_x: float = 0.0
    gyro_y: float = 0.0
    gyro_z: float = 0.0
    buttons: list[bool] = None  # [A, B, 1, 2, +, -, Left, Right, Up, Down]
    extended_data: dict = None

class WiiRemoteHandler:
    def __init__(self):
        self.data = WiiRemoteData(buttons=[False]*10)
        self.is_connected = False
        self._running = False
        self._thread: Optional[threading.Thread] = None
        self.on_data_received: Optional[Callable[[WiiRemoteData], None]] = None

    def connect(self):
        """リモコンへの接続処理（現在はモック）"""
        print("Connecting to Wii Remote...")
        time.sleep(1)
        self.is_connected = True
        self._running = True
        self._thread = threading.Thread(target=self._update_loop, daemon=True)
        self._thread.start()
        print("Connected.")

    def disconnect(self):
        self._running = False
        if self._thread:
            self._thread.join()
        self.is_connected = False
        print("Disconnected.")

    def _update_loop(self):
        """バックグラウンドでデータを更新するループ（現在はモック）"""
        import random
        while self._running:
            # モックデータの生成
            self.data.accel_x = random.uniform(-10, 10)
            self.data.accel_y = random.uniform(-10, 10)
            self.data.accel_z = random.uniform(-10, 10)
            self.data.gyro_x = random.uniform(-5, 5)
            self.data.gyro_y = random.uniform(-5, 5)
            self.data.gyro_z = random.uniform(-5, 5)
            
            # ボタン状態の擬似更新（例：Aボタンを時々押す）
            self.data.buttons[0] = random.random() > 0.9

            if self.on_data_received:
                self.on_data_received(self.data)
            
            time.sleep(0.02)  # 50Hz程度で更新

    def send_command(self, command_type: str, value: any):
        """LEDやモーターへのコマンド送信（現在はモック）"""
        print(f"Sending Command: {command_type} = {value}")
