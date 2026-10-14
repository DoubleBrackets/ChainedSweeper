#include <FastLED.h>

#define LED_PIN     13
#define NUM_LEDS    160 // TODO: NEED TO MEASUSRE
#define BRIGHTNESS  48
#define LED_TYPE    WS2811
#define COLOR_ORDER GRB
#define UPDATES_PER_SECOND 100

#define SWEEP_SEGMENTS 8
#define LEDS_PER_SWEEP_SEGMENT 20 // TODO: NEED TO MEASURE

int currentSweepPoints[SWEEP_SEGMENTS];
int sweepDir = 1; // 1 or -1 for sweeping in either direction

CRGB leds[NUM_LEDS];

CRGBPalette16 currentPalette = OceanColors_p;
TBlendType    currentBlending = LINEARBLEND; 
// note to self currentBlending can be LINEARBLEND or NOBLEND;


// first LED of a segment
// gonna assume we will 1-index the sweeps and use 0 to clear a sweep, so subtract 1 for calculating here
int SegmentStart(int point)
{
  return (point - 1) * LEDS_PER_SWEEP_SEGMENT;
}

void UpdateSweepState(int newSweepState)
{
  // invalid
  if (newSweepState < 0 || newSweepState > SWEEP_SEGMENTS)
  {
    return;
  }

  // clear the state
  if (newSweepState == 0)
  {
    SetLEDsDark();
    for (int i = 0; i < SWEEP_SEGMENTS; i++)
    {
      currentSweepPoints[i] = 0;
    }
    return;
  }

  // update points and last direction swept
  for (int i = 0; i < SWEEP_SEGMENTS; i++)
  {
    if (currentSweepPoints[i] == 0)
    {
      if (i == 0)
      {
        sweepDir = 0; // first point of a new sweep, no "direction" yet so don't move the LEDs
      }
      else
      {
        int prev = currentSweepPoints[i - 1];
        int next = newSweepState;

        // have to check if it wrapped around the mat
        bool movedForward  = (next == prev + 1) || (prev == SWEEP_SEGMENTS && next == 1);
        bool movedBackward = (next == prev - 1) || (prev == 1 && next == SWEEP_SEGMENTS);

        if (movedForward) sweepDir = 1;
        else if (movedBackward) sweepDir = -1;
      }
      currentSweepPoints[i] = newSweepState;
      return;
    }
  }
}

void FillLEDsFromSweepState(uint8_t offset)
{
  for (int i = 0; i < SWEEP_SEGMENTS; i++)
  {
    if (currentSweepPoints[i] == 0) return;

    // light the sweep segment
    int start = SegmentStart(currentSweepPoints[i]);
    int end = start + LEDS_PER_SWEEP_SEGMENT;

    for (int j = start; j < end && j < NUM_LEDS; j++)
    {
      // calculate color from LED position and palette size (256 by default)
      // and offset inherently gives the LEDs direction each loop
      uint8_t idx = offset + (j * 256) / NUM_LEDS;
      leds[j] = ColorFromPalette(currentPalette, idx, BRIGHTNESS, currentBlending);
    }
  }
}


// update LEDs
void RefreshSweepLEDs(uint8_t colorIndex)
{
  SetLEDsDark();
  FillLEDsFromSweepState(colorIndex);
  FastLED.show();
}

// read serial buffer, assumes 1 byte = 1 new sweep point
void CheckGameState()
{
  while (Serial.available() > 0)
  {
    int value = Serial.read();
    Serial.print("received: ");
    Serial.println(value);
    UpdateSweepState(value);
  }
}

void setup() {
  delay(2000);
  Serial.begin(9600);
  FastLED.addLeds<LED_TYPE, LED_PIN, COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);
  FastLED.setBrightness(BRIGHTNESS);
}

void loop()
{
  static uint8_t offset = 0;

  // to make things seem to move
  offset -= sweepDir; // -1 or +1 for pos or neg "direction"

  CheckGameState();
  RefreshSweepLEDs(offset);
  FastLED.delay(1000 / UPDATES_PER_SECOND);
}


void FillLEDsFromPaletteColors( uint8_t colorIndex)
{
    uint8_t brightness = BRIGHTNESS;
    
    for( int i = 0; i < NUM_LEDS; ++i) {
        leds[i] = ColorFromPalette( currentPalette, colorIndex, brightness, currentBlending);
        colorIndex += 3;
    }
}

void SetLEDsDark()
{
  for( int i = 0; i < NUM_LEDS; ++i) {
    leds[i] = CRGB::Black;
  }
}
