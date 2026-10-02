import asyncio
import socket
from bleak import BleakScanner, BleakClient

DEVICE_NAME = "ESP32_BROOM"
CHAR_UUID = "beb5483e-36e1-4688-b7f5-ea07361b26a8"

UDP_IP = "127.0.0.1"
UDP_PORT = 5001

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setblocking(False)

def on_notify(_, data: bytearray):
    try:
        print(data.decode())
    except UnicodeDecodeError:
        print(data.hex())
    try:
        sock.sendto(bytes(data), (UDP_IP, UDP_PORT))
    except OSError as e:
        print(f"UDP send failed: {e}")

async def connect_and_forward():
    print("Scanning...")
    device = await BleakScanner.find_device_by_name(DEVICE_NAME, timeout=10)
    if not device:
        print("Device not found")
        return

    print(f"Found {device.name} ({device.address}), connecting...")
    disconnected = asyncio.Event()

    def on_disconnect(_):
        disconnected.set()

    async with BleakClient(device, disconnected_callback=on_disconnect) as client:
        print(f"Connected. Forwarding to udp://{UDP_IP}:{UDP_PORT} (Ctrl+C to quit)")
        await client.start_notify(CHAR_UUID, on_notify)
        await disconnected.wait()
    print("Disconnected")

async def main():
    while True:
        try:
            await connect_and_forward()
        except asyncio.CancelledError:
            raise
        except Exception as e:
            print(f"Error: {e}")

try:
    asyncio.run(main())
except KeyboardInterrupt:
    pass
finally:
    sock.close()
