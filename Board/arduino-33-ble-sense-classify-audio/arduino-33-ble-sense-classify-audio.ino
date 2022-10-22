#include "Mic.h"
#include "Classifier.h"
#include <math.h>
#include <EloquentTinyML.h>      // https://github.com/eloquentarduino/EloquentTinyML
#include "tf_lite_model.h"       // TF Lite model file
#include <eloquent_tinyml/tensorflow.h>

// tune as per your needs
#define SAMPLES 64
#define GAIN (1.0f/50)
#define SOUND_THRESHOLD 1000
#define SAMPLE_DELAY 20
#define NUMBER_OF_LABELS   3     // number of voice labels
const String words[NUMBER_OF_LABELS] = {"p", "a", "e"};  // words for each label
#define NUMBER_OF_INPUTS   SAMPLES
#define TENSOR_ARENA_SIZE  4 * 1024
#define NUMBER_OF_OUTPUTS  NUMBER_OF_LABELS
Eloquent::TinyML::TensorFlow::TensorFlow<NUMBER_OF_INPUTS, NUMBER_OF_OUTPUTS, TENSOR_ARENA_SIZE> tf_model;


float features[SAMPLES];
Mic mic;
Eloquent::ML::Port::SVM clf;

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

        for (int i = 0; i < SAMPLES; i++) {
            while (!mic.hasData())
                delay(1);

            features[i] = mic.pop() * GAIN;
        }

        return true;
    }

    return false;
}


void setup() {
    Serial.begin(115200);
    PDM.onReceive(onAudio);
    mic.begin();
    delay(3000);
    tf_model.begin(model_data);
}


void loop() {
    if (recordAudioSample()) {
//        Serial.print("You said: ");
//        Serial.println(clf.predictLabel(features));
        float prediction[NUMBER_OF_LABELS];
        tf_model.predict(features, prediction);
        for (int i = 0; i < NUMBER_OF_LABELS; i++) {
          Serial.print("Label ");
          Serial.print(words[i]);
          Serial.print(" = ");
          Serial.println(prediction[i]);
        }
        delay(1000);
    }

    delay(SAMPLE_DELAY);
}
