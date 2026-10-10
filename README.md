# Wii-FlightStick
WiiリモコンのIMU（加速度・ジャイロ）、ボタン、拡張データを取得し、LEDやモーターを操作するためのデスクトップアプリケーション。

## プロジェクト概要
このプロジェクトは、Wiiリモコンをフライトスティックのような入力デバイスとして利用することを目的としています。C#とWinFormsを使用し、WindowsのネイティブなBluetooth API（Win32 P/Invoke）を叩くことで、低遅延かつ依存関係の少ない配布可能なアプリを目指します。

## 技術スタック
- **Language**: C# (.NET)
- **GUI Framework**: WinForms (Win32級のシンプルなUI)
- **Communication**: Bluetooth Classic (RFCOMM) via Win32 API (P/Invoke)

## 現在の進捗
- [x] プロジェクト構造の構築 (`src/WiiFlightStickApp`)
- [x] データモデルの定義 (`WiiRemoteData.cs`)
- [x] GUIの実装（IMU表示、ボタン状態、操作パネル）
- [x] バックグラウンド通信ハンドラーの枠組みとMACアドレス指定機能
- [ ] Bluetooth接続ロジックの実装 (Win32 API P/Invoke)
- [ ] パケット解析ロジックの詳細実装

## 実行方法
1. `src/WiiFlightStickApp` ディレクトリに移動します。
2. `dotnet run` を実行します。

## 注意事項
現在はモックデータ（ランダムな数値）を使用しています。実際の通信機能は次回の開発ステップで実装予定です。
