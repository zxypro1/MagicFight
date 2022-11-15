#include "Mic.h"
//#include "Classifier.h"
#include <math.h>
#include <EloquentTinyML.h>      // https://github.com/eloquentarduino/EloquentTinyML
#include "tf_lite_model.h"       // TF Lite model file
#include <eloquent_tinyml/tensorflow.h>
#include <Arduino_LSM9DS1.h>
//#include <TensorFlowLite.h>
//#include <tensorflow/lite/micro/all_ops_resolver.h>
//#include <tensorflow/lite/micro/micro_error_reporter.h>
//#include <tensorflow/lite/micro/micro_interpreter.h>
//#include <tensorflow/lite/schema/schema_generated.h>

#include <ArduinoBLE.h>

// This is the model you trained in Tiny Motion Trainer, converted to 
// a C style byte array.
#include "model.h"

// Values from Tiny Motion Trainer
#define MOTION_THRESHOLD 0.2
#define CAPTURE_DELAY 200 // This is now in milliseconds
#define NUM_SAMPLES 30
#define INPUT_SIZE (NUM_SAMPLES * 6)

// Array to map gesture index to a name
const char *GESTURES[] = {
    "fire", "block"
};

#define NUM_GESTURES (sizeof(GESTURES) / sizeof(GESTURES[0]))
#define OUTPUT_SIZE NUM_GESTURES

bool isCapturing = false;

// Num samples read from the IMU sensors
// "Full" by default to start in idle
int numSamplesRead = 0;


//==============================================================================
// TensorFlow variables
//==============================================================================
// Global variables used for TensorFlow Lite (Micro)
// tflite::MicroErrorReporter tflErrorReporter;

// Auto resolve all the TensorFlow Lite for MicroInterpreters ops, for reduced memory-footprint change this to only 
// include the op's you need.
// tflite::AllOpsResolver tflOpsResolver;

// Setup model
// const tflite::Model* tflModel = nullptr;
// tflite::MicroInterpreter* tflInterpreter = nullptr;
// TfLiteTensor* tflInputTensor = nullptr;
// TfLiteTensor* tflOutputTensor = nullptr;

// Eloquent::TinyML::TensorFlow::TensorFlow<NUMBER_OF_INPUTS, NUMBER_OF_OUTPUTS, TENSOR_ARENA_SIZE> model;

// Create a static memory buffer for TensorFlow Lite for MicroInterpreters, the size may need to
// be adjusted based on the model you are using
constexpr int tensorArenaSize = 8 * 1024;
byte tensorArena[tensorArenaSize];
Eloquent::TinyML::TensorFlow::TensorFlow<INPUT_SIZE, OUTPUT_SIZE, tensorArenaSize> tflModel;

float acc_features[INPUT_SIZE];

//----------------------------------------------------Audio Part----------------------------------------

// tune as per your needs
#define SAMPLES 64
#define GAIN (1.0f/50)
#define SOUND_THRESHOLD 1000
#define SAMPLE_DELAY 20
#define NUMBER_OF_LABELS   3     // number of voice labels
const String words[NUMBER_OF_LABELS] = {"p", "a", "e"};  // words for each label
const int sends[NUMBER_OF_LABELS] = {7, 8, 9};
int result = 0;
#define NUMBER_OF_INPUTS   SAMPLES
#define TENSOR_ARENA_SIZE  4 * 1024
#define NUMBER_OF_OUTPUTS  NUMBER_OF_LABELS
#define SERVICE_ID "765B"
#define CHARACT_ID "5B23"
#define DEVICE_NAME "Nano 33 BLE Sense2"
Eloquent::TinyML::TensorFlow::TensorFlow<NUMBER_OF_INPUTS, NUMBER_OF_OUTPUTS, TENSOR_ARENA_SIZE> tf_model;

BLEService ledService(SERVICE_ID); // BLE LED Service

// BLE LED Switch Characteristic - custom 128-bit UUID, read and writable by central
BLEByteCharacteristic switchCharacteristic(CHARACT_ID, BLERead | BLEWrite | BLENotify | BLEBroadcast);


float features[SAMPLES];
Mic mic;
// Eloquent::ML::Port::SVM clf;

/**
 * PDM callback to update mic object
 */
void onAudio() {
    mic.update();
}



/**
 * Read given number of samples from mic
 */
bool recordAudioSample() {
    if (mic.hasData() && mic.data() > SOUND_THRESHOLD) {
      int count = 0;
      for (int i = 0; i < SAMPLES; i++) {
        while (!mic.hasData()){
          count ++;
          delay(1);
          if (count == 100)
          break;  
        }
        if (count == 100)
          break;  
        else {
          count = 0;
          features[i] = mic.pop() * GAIN;
        }
//            delay(SAMPLE_DELAY);
      }
      return true;
    }
    return false;
}

void broadcastResult(int _result) {
  result = _result;
  switchCharacteristic.writeValue(result);
  switchCharacteristic.broadcast();
  result = 0;
}

void recordAudio() {
  if (recordAudioSample()) {
    float prediction[NUMBER_OF_LABELS];
    tf_model.predict(features, prediction);
    for (int i = 0; i < NUMBER_OF_LABELS; i++) {
      Serial.print("Label ");
      Serial.print(words[i]);
      Serial.print(" = ");
      Serial.println(prediction[i]);
      if (prediction[i] > 0.5){
        if (i == 0) {
          broadcastResult(sends[i]);
        } else {
          result = sends[i];
        }
      }
    }
  }
}


void setup() {
  Serial.begin(115200);
  PDM.onReceive(onAudio);
  mic.begin();
    // delay(3000);
  tf_model.begin(model_data);
  tflModel.begin(model);

//  while (!Serial);

  // set LED's pin to output mode
  pinMode(LEDR, OUTPUT);
  pinMode(LEDG, OUTPUT);
  pinMode(LEDB, OUTPUT);
  pinMode(LED_BUILTIN, OUTPUT);
  
  digitalWrite(LED_BUILTIN, LOW);         // when the central disconnects, turn off the LED
  digitalWrite(LEDR, HIGH);               // will turn the LED off
  digitalWrite(LEDG, HIGH);               // will turn the LED off
  digitalWrite(LEDB, HIGH);                // will turn the LED off

  // begin initialization
  if (!BLE.begin()) {
    Serial.println("starting Bluetooth® Low Energy failed!");
    while (1);
  }

  // Initialize IMU sensors
  if (!IMU.begin()) {
    Serial.println("Failed to initialize IMU!");
    while (1);
  }

  // Get the TFL representation of the model byte array
  // tflModel = tflite::GetModel(model);
  // if (tflModel->version() != TFLITE_SCHEMA_VERSION) {
  //   Serial.println("Model schema mismatch!");
  //   while (1);
  // }

  // Create an interpreter to run the model
  // tflInterpreter = new tflite::MicroInterpreter(tflModel, tflOpsResolver, tensorArena, tensorArenaSize, &tflErrorReporter);

   // Allocate memory for the model's input and output tensors
  // tflInterpreter->AllocateTensors();
  // Get pointers for the model's input and output tensors
  // tflInputTensor = tflInterpreter->input(0);
  // tflOutputTensor = tflInterpreter->output(0);

  // set advertised local name and service UUID:
  BLE.setLocalName(DEVICE_NAME);
  BLE.setDeviceName(DEVICE_NAME);
  BLE.setAdvertisedService(ledService);

  // add the characteristic to the service
  ledService.addCharacteristic(switchCharacteristic);

  // add service
  BLE.addService(ledService);

  // set the initial value for the characteristic:
  switchCharacteristic.writeValue(0);

  // start advertising
  BLE.advertise();

  Serial.println("BLE LED Peripheral");
    
}


void loop() {
  // listen for Bluetooth® Low Energy peripherals to connect:
  BLEDevice central = BLE.central();

  // if a central is connected to peripheral:
  if (central) {
    Serial.print("Connected to central: ");
    // print the central's MAC address:
    Serial.println(central.address());
    digitalWrite(LED_BUILTIN, HIGH);            // turn on the LED to indicate the connection

    // while the central is still connected to peripheral:
    while (central.connected()) {
      // Variables to hold IMU data
      float aX, aY, aZ, gX, gY, gZ;
      // if the remote device wrote to the characteristic,
      // use the value to control the LED:
      
      // Wait for motion above the threshold setting
      while (!isCapturing) {
        if (IMU.accelerationAvailable() && IMU.gyroscopeAvailable()) {
        
          IMU.readAcceleration(aX, aY, aZ);
          IMU.readGyroscope(gX, gY, gZ);

          recordAudio();

          // Sum absolute values
          float average = fabs(aX / 4.0) + fabs(aY / 4.0) + fabs(aZ / 4.0) + fabs(gX / 2000.0) + fabs(gY / 2000.0) + fabs(gZ / 2000.0);
          average /= 6.;

          // Above the threshold?
          if (average >= MOTION_THRESHOLD) {
            isCapturing = true;
            numSamplesRead = 0;
            break;
          }
        }
      }

      while (isCapturing) {

        // Check if both acceleration and gyroscope data is available
        if (IMU.accelerationAvailable() && IMU.gyroscopeAvailable()) {

          // read the acceleration and gyroscope data
          IMU.readAcceleration(aX, aY, aZ);
          IMU.readGyroscope(gX, gY, gZ);

          // Normalize the IMU data between -1 to 1 and store in the model's
          // input tensor. Accelerometer data ranges between -4 and 4,
          // gyroscope data ranges between -2000 and 2000
          // tflInputTensor->data.f[numSamplesRead * 6 + 0] = aX / 4.0;
          // tflInputTensor->data.f[numSamplesRead * 6 + 1] = aY / 4.0;
          // tflInputTensor->data.f[numSamplesRead * 6 + 2] = aZ / 4.0;
          // tflInputTensor->data.f[numSamplesRead * 6 + 3] = gX / 2000.0;
          // tflInputTensor->data.f[numSamplesRead * 6 + 4] = gY / 2000.0;
          // tflInputTensor->data.f[numSamplesRead * 6 + 5] = gZ / 2000.0;

          acc_features[numSamplesRead * 6 + 0] = aX / 4.0;
          acc_features[numSamplesRead * 6 + 1] = aY / 4.0;
          acc_features[numSamplesRead * 6 + 2] = aZ / 4.0;
          acc_features[numSamplesRead * 6 + 3] = gX / 2000.0;
          acc_features[numSamplesRead * 6 + 4] = gY / 2000.0;
          acc_features[numSamplesRead * 6 + 5] = gZ / 2000.0;

          numSamplesRead++;

          // Do we have the samples we need?
          if (numSamplesRead == NUM_SAMPLES) {
            
            // Stop capturing
            isCapturing = false;
            
            // Run inference
            // TfLiteStatus invokeStatus = tflInterpreter->Invoke();
            // if (invokeStatus != kTfLiteOk) {
            //   Serial.println("Error: Invoke failed!");
            //   while (1);
            //   return;
            // }

            // Loop through the output tensor values from the model
            int maxIndex = 0;
            float maxValue = 0;
            float results[NUM_GESTURES];
            tflModel.predict(acc_features, results);
            for (int i = 0; i < NUM_GESTURES; i++) {
              float _value = results[i];
              if(_value > maxValue){
                maxValue = _value;
                maxIndex = i;
              }
              Serial.print(GESTURES[i]);
              Serial.print(": ");
              Serial.println(results[i], 6);
            }
            
            Serial.print("Winner: ");
            Serial.print(GESTURES[maxIndex]);
            if (maxIndex == 0 && result != 0) { // fire
              broadcastResult(result);
            } else if (maxIndex == 1) { // blocking
              broadcastResult(10);
            }
            
            Serial.println();

            // Add delay to not double trigger
            delay(CAPTURE_DELAY);
          }
        }
      }
    }

    // when the central disconnects, print it out:
    Serial.print(F("Disconnected from central: "));
    Serial.println(central.address());
    digitalWrite(LED_BUILTIN, LOW);         // when the central disconnects, turn off the LED
    digitalWrite(LEDR, HIGH);          // will turn the LED off
    digitalWrite(LEDG, HIGH);        // will turn the LED off
    digitalWrite(LEDB, HIGH);         // will turn the LED off
  }
}
