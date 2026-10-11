// Multi-sensor sweep detector
// Each piezo sensor is checked every loop; each one tracks its own sweep state.

const int NUM_SENSORS = 5; 
const int piezoSensor[NUM_SENSORS] = {A0, A1, A2, A3, A4};

const int threshold  = 1000;   // upper bound of "activity" band
const int threshold2 = 400;    // lower bound of "activity" band

const unsigned long MIN_SWEEP_DURATION = 100; // ms - must stay active at least this long
const unsigned long MAX_GAP = 40;             // ms - allowed dropout within a sweep before it's considered ended

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

    bool isActive = (sensorReading[i] <= threshold && sensorReading[i] >= threshold2);
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
      }
      // else: too short, treat as a knock/spike and ignore it
      inActivity[i] = false;
    }

    // Only plot sensor readings
    Serial.print("sensor");
    Serial.print(i + 1);
    Serial.print(":");
    Serial.print(sensorReading[i]);
    Serial.print(i < NUM_SENSORS - 1 ? "," : "\n");

    // // Plotter-friendly output: raw reading + sweep flag for this sensor
    // Serial.print("sensor");
    // Serial.print(i + 1);
    // Serial.print(":");
    // Serial.print(sensorReading[i]);
    // Serial.print(",");
    // Serial.print("sweep");
    // Serial.print(i + 1);
    // Serial.print(":");
    // Serial.print(sweepDetected ? 1100 : 0);
    // Serial.print(i < NUM_SENSORS - 1 ? "," : "\n");
  }

  delay(10); // fast sampling keeps duration tracking accurate
}
