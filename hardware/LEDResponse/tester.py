"""
NOTE: pip install pyserial
 
usage: python sweep_test.py [PORT]
 
input 1 byte at a time to mimic serial
1-8 are the piezo/sweep sensors
0 clears the sweep state (finished a sweep sequence)
"""

import argparse
import time
 
import serial
 
SWEEP_SEGMENTS = 8
 
 
parser = argparse.ArgumentParser()
parser.add_argument("port")
args = parser.parse_args()
 
 
def run_manual(ser):
    print(f"Type a number 1-{SWEEP_SEGMENTS} for sweep segment. \nType 0 to clear after a finished sweep sequence")
    while True:
        text = input("> ").strip().lower()
        if not text.isdigit():
            # ignore bad input
            continue
        value = int(text)
        if not 0 <= value <= SWEEP_SEGMENTS:
            # ignore bad input
            continue
 
        # send if valid input
        ser.write(bytes([value]))
 
 
with serial.Serial(args.port, 9600, timeout=0.5) as ser:
    # opening the connection reboots the arduino, and the sketch has a 2s startup delay
    print("Opening serial and waiting for arduino to reboot")
    time.sleep(3)
 
    try:
        run_manual(ser)
    except (KeyboardInterrupt, EOFError):
        print("\nStopped.")
