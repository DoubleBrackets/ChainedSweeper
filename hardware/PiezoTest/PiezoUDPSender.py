"""
notes:
- run `pip install pyserial` first for serial lib
- we have a 2 byte broom "protocol":
    - byte 1 = broom command type
        0 for broom tilt 
        1 for broom sweep (hence this script hardcodes 1)
    - byte 2 = data
        if tilt:
            0 (not tilted) or 1 (tilted)
        if sweep:
            0-7 index of the sensor sweep detected

"""

import re
import socket

import serial
import argparse

parser = argparse.ArgumentParser()
parser.add_argument('-s', '--sound', action='store_true')
args = parser.parse_args()

if args.sound:
    from playsound import playsound

SERIAL_PORT = "COM7" # "COM3"
BAUD_RATE = 9600
UDP_IP = "127.0.0.1"
UDP_PORT = 5002 # port the game is bound to
HEADER_BYTE = 1 # 1 for a sweep as defined by above protocol


sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
dest = (UDP_IP, UDP_PORT)

serial_connection = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=3)
print(f"Listening on {SERIAL_PORT} baud {BAUD_RATE}, forwarding to {UDP_IP}:{UDP_PORT}")

try:
    while True:
        data = serial_connection.readline()
        line = data.decode("utf-8", errors="ignore").strip()
        if not line.isdigit():
            continue # ignore any corrutped data

        sensor_num = int(line)
        packet = bytes([HEADER_BYTE, sensor_num])
        sock.sendto(packet, dest)
        print(f"Sweep at sensor {sensor_num} -> Sent {list(packet)}")

        if args.sound:
            playsound('guzhen.mp3') 
except KeyboardInterrupt:
    pass
finally:
    serial_connection.close()
    sock.close()

