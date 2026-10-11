// Multi-sensor sweep detector
// Each piezo sensor is checked every loop; each one tracks its own sweep state.

const int NUM_SENSORS = 8; 
const int piezoSensor[NUM_SENSORS] = {A0, A1, A2, A3, A4, A5, A6, A7};

// analog values are 0-1023, so this is the threshold within that range
// I noticed usually values don't go above ~600 unless you really really hit the sensor
// And usually anything 0 - 50 could still be noise
// so 100 seems like a safe threshold for now 
const int threshold = 90; 

const unsigned long MIN_SWEEP_DURATION = 30; // ms - must stay active at least this long
const unsigned long MAX_GAP = 20;             // ms - allowed dropout within a sweep before it's considered ended

// per-sensor state
int sensorReading[NUM_SENSORS];
bool inActivity[NUM_SENSORS];
unsigned long activityStart[NUM_SENSORS];
unsigned long lastActiveTime[NUM_SENSORS];

void setup() {
  Serial.begin(9600);
  for (int i = 0; i < NUM_SENSORS; i++) {
    sensorReading[i] = 0;
    inActivity[i] = false;
    activityStart[i] = 0;
    lastActiveTime[i] = 0;
  }
}

void loop() {
  unsigned long now = millis();

  for (int i = 0; i < NUM_SENSORS; i++) {
    sensorReading[i] = analogRead(piezoSensor[i]);

    bool isActive = (sensorReading[i] >= threshold);
    bool sweepDetected = false;

    if (isActive) {
      if (!inActivity[i]) {
        // just started a new activity window
        inActivity[i] = true;
        activityStart[i] = now;
      }
      lastActiveTime[i] = now;
    }

    if (inActivity[i] && (now - lastActiveTime[i] > MAX_GAP)) {
      // activity has ended (gap too long) - decide what it was
      unsigned long duration = lastActiveTime[i] - activityStart[i];
      if (duration >= MIN_SWEEP_DURATION) {
        sweepDetected = true; // long enough - call it a sweep
        // Serial.print("Sweep at sensor: ");
        Serial.println(i);
      }
      // else: too short, treat as a knock/spike and ignore it
      inActivity[i] = false;
    }

    // FOR DEBUGGING ONLY
    // Serial.print("sensor");
    // Serial.print(i + 1);
    // Serial.print(":");
    // Serial.print(sensorReading[i]);
    // Serial.print(i < NUM_SENSORS - 1 ? "," : "\n");
  }

  delay(5); // fast sampling keeps duration tracking accurate
}
