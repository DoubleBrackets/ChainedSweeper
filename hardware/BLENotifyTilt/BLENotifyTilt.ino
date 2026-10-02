#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>

BLEServer* pServer = NULL;
BLECharacteristic* pCharacteristic = NULL;
bool deviceConnected = false;
bool oldDeviceConnected = false;
uint32_t value = 0;

const int TILT_PIN = D7;
const int TILTED = HIGH; 
const int NOT_TILTED = LOW;
int tiltState;
int lastTiltState;
int stableState;
unsigned long lastChangeTime = 0;
const unsigned long settleDelay = 50; // so the default tilted value doesn't 'bounce'

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/

// these are just still dummy values from the example sketch but they work fine lol
#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"

class MyServerCallbacks: public BLEServerCallbacks {
  void onConnect(BLEServer* pServer) {
        deviceConnected = true;
  };

  void onDisconnect(BLEServer* pServer) {
        deviceConnected = false;
  }
};

void setup() {
Serial.begin(115200);

  pinMode(TILT_PIN, INPUT_PULLUP);
  lastTiltState = digitalRead(TILT_PIN);
  stableState = lastTiltState;

  // Create the BLE Device
  BLEDevice::init("ESP32_BROOM");

  // Create the BLE Server
  pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Create the BLE Service
  BLEService *pService = pServer->createService(SERVICE_UUID);

  // Create a BLE Characteristic
  pCharacteristic = pService->createCharacteristic(
                      CHARACTERISTIC_UUID,
                      BLECharacteristic::PROPERTY_NOTIFY
  );

  // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.descriptor.gatt.client_characteristic_configuration.xml
  // Create a BLE Descriptor
  pCharacteristic->addDescriptor(new BLE2902());

  // Start the service
  pService->start();

  // Start advertising
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(false);
  pAdvertising->setMinPreferred(0x0);  // set value to 0x00 to not advertise this parameter
  BLEDevice::startAdvertising();
  Serial.println("Waiting a client connection to notify...");
}

void loop() {
  tiltState = digitalRead(TILT_PIN);

  if (tiltState != lastTiltState) {
    lastChangeTime = millis();
  }

  if (stableState == TILTED) {
    if (tiltState == NOT_TILTED) {
      stableState = NOT_TILTED;
      Serial.println("No longer tilted");
      if (deviceConnected) {
        String msg = "No longer tilted";
        pCharacteristic->setValue((uint8_t*)msg.c_str(), msg.length());
        pCharacteristic->notify();
      }
    }
  } else {
    if (tiltState == TILTED && (millis() - lastChangeTime) > settleDelay) {
      stableState = TILTED;
      Serial.println("Tilted");
      if (deviceConnected) {
        String msg = "Tilted";
        pCharacteristic->setValue((uint8_t*)msg.c_str(), msg.length());
        pCharacteristic->notify();
      }
    }
  }

  lastTiltState = tiltState;
  delay(1); 

  // disconnecting
  if (!deviceConnected && oldDeviceConnected) {
    delay(500); // give the bluetooth stack the chance to get things ready
    pServer->startAdvertising(); // restart advertising
    Serial.println("start advertising");
    oldDeviceConnected = deviceConnected;
  }
  // connecting
  if (deviceConnected && !oldDeviceConnected) {
    // do stuff here on connecting
    oldDeviceConnected = deviceConnected;
  }
}