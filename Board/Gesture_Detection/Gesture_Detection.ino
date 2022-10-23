/* Copyright 2019 The TensorFlow Authors. All Rights Reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
==============================================================================*/

#include <TensorFlowLite.h>

#include "main_functions.h"

#include "accelerometer_handler.h"
#include "constants.h"
#include "gesture_predictor.h"
#include "magic_wand_model_data.h"
#include "output_handler.h"
#include "tensorflow/lite/micro/kernels/micro_ops.h"
#include "tensorflow/lite/micro/micro_error_reporter.h"
#include "tensorflow/lite/micro/micro_interpreter.h"
#include "tensorflow/lite/micro/micro_mutable_op_resolver.h"
#include "tensorflow/lite/schema/schema_generated.h"
#include "tensorflow/lite/version.h"

// Globals, used for compatibility with Arduino-style sketches.
namespace {
tflite::ErrorReporter* error_reporter = nullptr;
const tflite::Model* model = nullptr;
tflite::MicroInterpreter* interpreter = nullptr;
TfLiteTensor* model_input = nullptr;
int input_length;

// Create an area of memory to use for input, output, and intermediate arrays.
// The size of this will depend on the model you're using, and may need to be
// determined by experimentation.
constexpr int kTensorArenaSize = 60 * 1024;
uint8_t tensor_arena[kTensorArenaSize];
}  // namespace

// The name of this function is important for Arduino compatibility.
void setup() {
  // Set up logging. Google style is to avoid globals or statics because of
  // lifetime uncertainty, but since this has a trivial destructor it's okay.
  static tflite::MicroErrorReporter micro_error_reporter;  // NOLINT
  error_reporter = &micro_error_reporter;

  // Map the model into a usable data structure. This doesn't involve any
  // copying or parsing, it's a very lightweight operation.
  model = tflite::GetModel(g_magic_wand_model_data);
  if (model->version() != TFLITE_SCHEMA_VERSION) {
    TF_LITE_REPORT_ERROR(error_reporter,
                         "Model provided is schema version %d not equal "
                         "to supported version %d.",
                         model->version(), TFLITE_SCHEMA_VERSION);
    return;
  }

  // Pull in only the operation implementations we need.
  // This relies on a complete list of all the ops needed by this graph.
  // An easier approach is to just use the AllOpsResolver, but this will
  // incur some penalty in code space for op implementations that are not
  // needed by this graph.
  static tflite::MicroOpResolver<5> micro_op_resolver;  // NOLINT
  micro_op_resolver.AddBuiltin(
      tflite::BuiltinOperator_DEPTHWISE_CONV_2D,
      tflite::ops::micro::Register_DEPTHWISE_CONV_2D());
  micro_op_resolver.AddBuiltin(tflite::BuiltinOperator_MAX_POOL_2D,
                               tflite::ops::micro::Register_MAX_POOL_2D());
  micro_op_resolver.AddBuiltin(tflite::BuiltinOperator_CONV_2D,
                               tflite::ops::micro::Register_CONV_2D());
  micro_op_resolver.AddBuiltin(tflite::BuiltinOperator_FULLY_CONNECTED,
                               tflite::ops::micro::Register_FULLY_CONNECTED());
  micro_op_resolver.AddBuiltin(tflite::BuiltinOperator_SOFTMAX,
                               tflite::ops::micro::Register_SOFTMAX());

  // Build an interpreter to run the model with.
  static tflite::MicroInterpreter static_interpreter(
      model, micro_op_resolver, tensor_arena, kTensorArenaSize, error_reporter);
  interpreter = &static_interpreter;

  // Allocate memory from the tensor_arena for the model's tensors.
  interpreter->AllocateTensors();

  // Obtain pointer to the model's input tensor.
  model_input = interpreter->input(0);
  if ((model_input->dims->size != 4) || (model_input->dims->data[0] != 1) ||
      (model_input->dims->data[1] != 128) ||
      (model_input->dims->data[2] != kChannelNumber) ||
      (model_input->type != kTfLiteFloat32)) {
    TF_LITE_REPORT_ERROR(error_reporter,
                         "Bad input tensor parameters in model");
    return;
  }

  input_length = model_input->bytes / sizeof(float);

  TfLiteStatus setup_status = SetupAccelerometer(error_reporter);
  if (setup_status != kTfLiteOk) {
    TF_LITE_REPORT_ERROR(error_reporter, "Set up failed\n");
  }
}

bool IsMoving() {
  // Look at the most recent accelerometer values.
  const float* input_data = model_input->data.f;
  const float last_x = input_data[input_length - 3];
  const float last_y = input_data[input_length - 2];
  const float last_z = input_data[input_length - 1];

  // Figure out the total amount of acceleration being felt by the device.
  const float last_x_squared = last_x * last_x;
  const float last_y_squared = last_y * last_y;
  const float last_z_squared = last_z * last_z;
  const float acceleration_magnitude =
      sqrtf(last_x_squared + last_y_squared + last_z_squared);

  // Acceleration is in milli-Gs, so normal gravity is 1,000 units.
  const float gravity = 1000.0f;

  // Subtract out gravity to get the actual movement magnitude.
  const float movement = acceleration_magnitude - gravity;

  // How much acceleration is needed before it's considered movement.
  const float movement_threshold = 40.0f;
  const bool is_moving = (movement > movement_threshold);

  return is_moving;
}

// This is the regular function we run to recognize gestures from a pretrained
// model.
void RecognizeGestures() {
  const bool is_moving = IsMoving();

  // Static state used to control the capturing process.
  static int counter = 0;
  static enum {
    ePendingStillness,
    eInStillness,
    ePendingMovement,
    eRecordingGesture
  } state = ePendingStillness;
  static int still_found_time;
  static int gesture_start_time;
  // State machine that controls gathering user input.
  switch (state) {
    case ePendingStillness: {
      if (!is_moving) {
        still_found_time = counter;
        state = eInStillness;
      }
    } break;

    case eInStillness: {
      if (is_moving) {
        state = ePendingStillness;
      } else {
        const int duration = counter - still_found_time;
        if (duration > 25) {
          state = ePendingMovement;
        }
      }
    } break;

    case ePendingMovement: {
      if (is_moving) {
        state = eRecordingGesture;
        gesture_start_time = counter;
      }
    } break;

    case eRecordingGesture: {
      const int recording_time = 128;
      if ((counter - gesture_start_time) > recording_time) {
        // Run inference, and report any error.
        TfLiteStatus invoke_status = interpreter->Invoke();
        if (invoke_status != kTfLiteOk) {
          TF_LITE_REPORT_ERROR(error_reporter, "Invoke failed on index: %d\n",
                               begin_index);
          return;
        }

        const float* prediction_scores = interpreter->output(0)->data.f;
        const int found_gesture = PredictGesture(prediction_scores);

        // Produce an output
        HandleOutput(error_reporter, found_gesture);

        state = ePendingStillness;
      }
    } break;

    default: {
      TF_LITE_REPORT_ERROR(error_reporter, "Logic error - unknown state");
    } break;
  }

  // Increment the timing counter.
  ++counter;
}

// If you need to gather training data, call this function from the main loop
// and it will guide the user through contributing data.
// The output that's logged to the console can be fed into the Python training
// scripts for this example.
void CaptureGestureData() {
  const bool is_moving = IsMoving();

  // Static state used to control the capturing process.
  static int counter = 0;
  static int gesture_count = 0;
  static enum {
    eStarting,
    ePendingStillness,
    eInStillness,
    ePendingMovement,
    eRecordingGesture
  } state = eStarting;
  static int still_found_time;
  static int gesture_start_time;
  static const char* next_gesture = nullptr;
  // State machine that controls gathering user input.
  switch (state) {
    case eStarting: {
      if (!next_gesture || (strcmp(next_gesture, "other") == 0)) {
        next_gesture = "wing";
      } else if (strcmp(next_gesture, "wing") == 0) {
        next_gesture = "ring";
      } else if (strcmp(next_gesture, "ring") == 0) {
        next_gesture = "slope";
      } else {
        next_gesture = "other";
      }
      TF_LITE_REPORT_ERROR(error_reporter, "# Hold the wand still");
      state = ePendingStillness;
    } break;

    case ePendingStillness: {
      if (!is_moving) {
        still_found_time = counter;
        state = eInStillness;
      }
    } break;

    case eInStillness: {
      if (is_moving) {
        state = ePendingStillness;
      } else {
        const int duration = counter - still_found_time;
        if (duration > 25) {
          state = ePendingMovement;
          TF_LITE_REPORT_ERROR(error_reporter,
                               "# When you're ready, perform the %s gesture",
                               next_gesture);
        }
      }
    } break;

    case ePendingMovement: {
      if (is_moving) {
        state = eRecordingGesture;
        gesture_start_time = counter;
        TF_LITE_REPORT_ERROR(error_reporter, "# Perform the %s gesture now",
                             next_gesture);
      }
    } break;

    case eRecordingGesture: {
      const int recording_time = 100;
      if ((counter - gesture_start_time) > recording_time) {
        ++gesture_count;
        TF_LITE_REPORT_ERROR(error_reporter, "****************");
        TF_LITE_REPORT_ERROR(error_reporter, "gesture: %s", next_gesture);
        const float* input_data = model_input->data.f;
        for (int offset = recording_time - 10; offset > 0; --offset) {
          const int array_offset = (input_length - (offset * 3));
          const int x = static_cast<int>(input_data[array_offset + 0]);
          const int y = static_cast<int>(input_data[array_offset + 1]);
          const int z = static_cast<int>(input_data[array_offset + 2]);
          TF_LITE_REPORT_ERROR(error_reporter, "x: %d y:%d z:%d", x, y, z);
        }
        TF_LITE_REPORT_ERROR(error_reporter, "~~~~~~~~~~~~~~~~");
        TF_LITE_REPORT_ERROR(error_reporter, "# %d gestures recorded",
                             gesture_count);
        state = eStarting;
      }
    } break;

    default: {
      TF_LITE_REPORT_ERROR(error_reporter, "Logic error - unknown state");
    } break;
  }

  // Increment the timing counter.
  ++counter;
}

void loop() {
  // Attempt to read new data from the accelerometer.
  bool got_data =
      ReadAccelerometer(error_reporter, model_input->data.f, input_length);
  // If there was no new data, wait until next time.
  if (!got_data) return;

  // In the future we should decide whether to capture data based on a user
  // action (like pressing a button), but since some of the devices we're
  // targeting don't have any built-in input devices you'll need to manually
  // switch between recognizing gestures and capturing training data by changing
  // this variable and recompiling.
  const bool should_capture_data = false;
  if (should_capture_data) {
    CaptureGestureData();
  } else {
    RecognizeGestures();
  }
}
