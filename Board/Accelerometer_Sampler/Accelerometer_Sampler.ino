/*
  Arduino LSM9DS1 - Simple Accelerometer

  This example reads the acceleration values from the LSM9DS1
  sensor and continuously prints them to the Serial Monitor
  or Serial Plotter.

  The circuit:
  - Arduino Nano 33 BLE Sense

  created 10 Jul 2019
  by Riccardo Rizzo

  This example code is in the public domain.
*/

#include <Arduino_LSM9DS1.h>
#define SAMPLE_NUM 128
#define SAMPLE_DELAY 10
#define TOTAL_SAMPLE 10
#define TOTAL_SAMPLE_DELAY 1500

unsigned int total_counter = 0;

void setup() {
  Serial.begin(9600);
  while (!Serial);
  Serial.println("Started");
  int count = 0;


  if (!IMU.begin()) {
    Serial.println("Failed to initialize IMU!");
    while (1);
  }
  Serial.print("Accelerometer sample rate = ");
  Serial.print(IMU.accelerationSampleRate());
  Serial.println(" Hz");
  Serial.println();
  Serial.println("Acceleration in g's");
  Serial.println("X\tY\tZ");
  delay(TOTAL_SAMPLE_DELAY);
}

void loop() {
  float x, y, z;

  Serial.print("-,-,-\n");

  for (int i = 0; i < SAMPLE_NUM; i++) {
    if (IMU.accelerationAvailable()) {
      IMU.readAcceleration(x, y, z);

      Serial.print(x);
      Serial.print(',');
      Serial.print(y);
      Serial.print(',');
      Serial.println(z);
      delay(SAMPLE_DELAY);
    }
  }
  total_counter ++;
  if (total_counter == TOTAL_SAMPLE) {
    Serial.print("-----------------------SAMPLE FINISH---------------------");
    while (1) {
        delay(100);
        digitalWrite(LED_BUILTIN, HIGH);
        delay(100);
        digitalWrite(LED_BUILTIN, LOW);
      }
  }
  Serial.print('\n');
  Serial.print('\n');
  Serial.print('\n');
  delay(1500);
}
