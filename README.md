# MagicFight! <img src="https://github.com/zxypro1/MagicFight/blob/57d80a56a4cd70d88ee913dbb41bb81e6b5797bd/imgs/magicfight_logo.png" align="right" alt="logo" width="110" height="100">

MagicFight! A tiny-ML game project based on [Arduino Nano 33 BLE Sense](https://store-usa.arduino.cc/products/arduino-nano-33-ble-sense), [Tensorflow Lite](https://www.tensorflow.org/lite) and Unity.

## Introduction
MagicFight! is a virtual game project in which player can speak spells to fight others, like a wizard in Harry Potter. In this project, we use Arduino Nano 33 BLE Sense as the ‘magic wand’ to receive and recongize sound signals and geature signals, using its microphone, acceleration sensor and gyroscope.  
[A presentation video](https://www.youtube.com/watch?v=05lCZf7qrKU) has been upload to YouTube.  
![Playing gif](https://github.com/zxypro1/MagicFight/blob/8916cc56866ad91707f7d7bce8f744fad2424c48/imgs/Playing.gif)

## Document and Report
[Project Report](https://github.com/zxypro1/MagicFight/tree/main/ProjectReport)

## Requirement
- A **Mac** computer that can run Unity.
- Two Arduino Nano 33 BLE Sense boards.

## Installation
### Game Deployment
The exe executable file of the Unity game is still on development. However, if you want to try the game, you can import the scene from `unity` folder and play the game in debug mode. You should also import these five packages to make it run:
- [Health System by Code Monkey](https://assetstore.unity.com/packages/tools/utilities/health-system-includes-learning-video-211787)
- [Flat Platformer Template](https://assetstore.unity.com/packages/2d/environments/flat-platformer-template-108101)
- [Cute 2D Girl - Wizard](https://assetstore.unity.com/packages/2d/characters/cute-2d-girl-wizard-155796)
- [Arduino Bluetooth Plugin](https://assetstore.unity.com/packages/tools/input-management/arduino-bluetooth-plugin-98960)
- [Animated Fireballs](https://www.gamedevmarket.net/asset/animated-fireballs/)

### Board Scripts Upload
1. To upload scripts to your boards, you should first install Arduino IDE and install necessary libraries. You can follow the [installation guidelines](https://docs.arduino.cc/hardware/nano-33-ble-sense) on the official website.
2. Install **EloquentML** library, **LSM9DS1** library, **ArduinoBLE** library and **Arduino_CMSIS-DSP** library in Arduino IDE’s library manager.
3. Open `board1.ino` and `board2.ino` files (in `/Board/board1` and `/Board/board2` folder) in Arduino IDE. 
4. Compile and upload to your boards.
