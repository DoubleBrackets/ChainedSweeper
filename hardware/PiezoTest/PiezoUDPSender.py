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

import asyncio
from concurrent.futures import ThreadPoolExecutor
import re
import socket
from time import sleep

import serial
import argparse
import threading

parser = argparse.ArgumentParser()
parser.add_argument('-s', '--sound', action='store_true')
args = parser.parse_args()

if args.sound:
    from playsound import playsound

SERIAL_PORT = "COM7" # "COM3"
BAUD_RATE = 9600
UDP_IP = "127.0.0.1"
BOUND_UDP_PORT = 5003 # port this server binds to
UDP_PORT = 5002 # port the game is bound to
HEADER_BYTE = 1 # 1 for a sweep as defined by above protocol


sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setblocking(False)
sock.bind((UDP_IP, BOUND_UDP_PORT))
dest = (UDP_IP, UDP_PORT)


serial_connection = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=3)

def piezo_main():
    print(f"Starting serial/udp piezo loop at {SERIAL_PORT}")
    while not exiting.is_set():
        data = serial_connection.readline()
        line = data.decode("utf-8", errors="ignore").strip()
        if not line.isdigit():
            print(f"Junk Data -> Sent {line}")
            continue # ignore any corrutped data

        sensor_num = int(line)
        packet = bytes([HEADER_BYTE, sensor_num])
        sock.sendto(packet, dest)
        print(f"Sweep @ {sensor_num} -> Sent {list(packet)}")

        if args.sound:
            playsound('guzhen.mp3') 


def led_main():
    print(f"Starting led listen loop at {UDP_IP}:{BOUND_UDP_PORT}")
    while not exiting.is_set():
        # Read from game
        print("Waiting for game LED UDP...")
        try:
            data, addr = sock.recvfrom(1024) # buffer size is 1024 bytes
        except socket.error:
            data = []

        if len(data):
            print("Writing to serial:", list(data))
            serial_connection.write(data)


try:
    exiting = threading.Event()
    with ThreadPoolExecutor() as executor:
        piezo_task = executor.submit(piezo_main)
        led_task = executor.submit(led_main)
        print(f"Listening on {SERIAL_PORT} baud {BAUD_RATE}, forwarding to {UDP_IP}:{UDP_PORT}")
        while not exiting.is_set():
            sleep(1)
except KeyboardInterrupt:
    print("Caught keyboardInterrupt")
    exiting.set()
finally:
    print("Main thread finished")
    sock.close()
    serial_connection.close()
