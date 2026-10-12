# MotionPlus Control Documentation

This document summarizes the control logic and communication protocol for the Wii MotionPlus sensor as implemented in `wii_motionplus_reader01.cpp` and `wii_motionplus_reader02.cpp`.

## Overview
The project interfaces with a Wii Remote and an attached MotionPlus accessory using Windows HID (Human Interface Device) protocols. The goal is to initialize the MotionPlus sensor, switch it into the correct operating mode, and extract gyroscope data (Yaw, Pitch, Roll).

## Hardware Identification
- **Vendor ID (VID)**: `0x057e` (Nintendo)
- **Product ID (PID)**: `0x0306` (Wii Remote)
- **MotionPlus I2C Addresses**: 
    - Primary: `0xA6`
    - Secondary/Alternative: `0xA4`

## Communication Protocol
Communication is handled via HID reports. The most critical report for MotionPlus interaction is **Report ID 0x16**.

### Report 0x16 Structure
When sending a command to the MotionPlus, the following byte structure is used:
| Byte | Description | Value/Example |
| :--- | :--- | :--- |
| 0 | Report ID | `0x16` |
| 1 | Device Type | `0x04` (Other I2C device) |
| 2 | I2C Address | `0xA6` or `0xA4` |
| 3 | Reserved | `0x00` |
| 4 | Memory Address / Command | Target register/command |
| 5 | Data Length | Number of bytes to send (e.g., `1` for a single byte) |
| 6+ | Data Content | The actual command or data payload |

## Initialization Sequence (v0.2 Logic)
To ensure the MotionPlus is ready for data acquisition, the following sequence is performed:

1.  **Mode Selection**: The Wii Remote is put into a specific communication mode (e.g., `Wii_Remote_mode_0x31`).
2.  **Address Detection**: 
    - The system attempts to read from address `0xA6`.
    - If `0xA6` does not respond correctly, it checks address `0xA4`.
3.  **Mode Switching**:
    - If the device is found at `0xA6`, a command is sent to switch its mode:
        - **Command**: Report `0x16` with Address `0xA6`, Length `2`, Data `0xFE, 0x04`.
    - This ensures the MotionPlus enters the correct state for gyroscope output.

## Sensor Data Parsing
Once initialized, the system reads raw bytes from the sensor and parses them into human-readable angles. The calculation logic is as follows:

### Gyroscope Formulas
The raw data is extracted from a buffer (`buf`) where each axis uses two bytes (one for the base value and one for the high bits):

| Axis | Calculation Formula | Description |
| :--- | :--- | :--- |
| **Yaw** | `buf[0] + (buf[3] >> 2) * 256` | Horizontal rotation |
| **Pitch** | `buf[1] + (buf[4] >> 2) * 256` | Vertical tilt |
| **Roll** | `buf[2] + (buf[5] >> 2) * 256` | Side-to-side tilt |

*Note: The bitwise shift `>> 2` is used because the high bits of the data are shifted in the raw HID report.*

## Summary of Version Changes
- **v0.1**: Focused on basic connectivity, hardware reset via Report `0x16`, and raw memory dumping for debugging.
- **v0.2**: Introduced automated address detection (`0xA4`/`0xA6`), formal mode switching logic, and the mathematical parsing of gyroscope data into Yaw/Pitch/Roll values.
