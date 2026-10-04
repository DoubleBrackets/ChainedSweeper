// these constants won't change:
const int knockSensor = A0;   // the piezo is connected to analog pin 0
const int threshold = 1000;    // upper bound of "activity" band
const int threshold2 = 400;   // lower bound of "activity" band

const unsigned long MIN_SWEEP_DURATION = 100; // ms — must stay active at least this long
const unsigned long MAX_GAP = 40;             // ms — allowed dropout within a sweep before we consider it ended

// these variables will change:
int sensorReading = 0;
bool inActivity = false;
unsigned long activityStart = 0;
unsigned long lastActiveTime = 0;

void setup() {
  Serial.begin(9600);
}

void loop() {
  sensorReading = analogRead(knockSensor);

  bool isActive = (sensorReading <= threshold && sensorReading >= threshold2);
  bool sweepDetected = false;

  unsigned long now = millis();

  if (isActive) {
    if (!inActivity) {
      // just started a new activity window
      inActivity = true;
      activityStart = now;
    }
    lastActiveTime = now;
  }

  if (inActivity && (now - lastActiveTime > MAX_GAP)) {
    // activity has ended (gap too long) — decide what it was
    unsigned long duration = lastActiveTime - activityStart;
    if (duration >= MIN_SWEEP_DURATION) {
      sweepDetected = true; // long enough — call it a sweep
    }
    // else: too short, treat as a knock/spike and ignore it
    inActivity = false;
  }

  // Plotter-friendly output: raw reading + a flag series for confirmed sweeps
  Serial.print("sensorReading:");
  Serial.print(sensorReading);
  Serial.print(",");
  Serial.print("sweep:");
  Serial.println(sweepDetected ? 1100 : 0); // spikes above your band when a sweep is confirmed

  delay(10); // faster sampling than 50ms — needed for duration tracking to be accurate
}