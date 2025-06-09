# AZRAM

<b>Click below to watch the video</b>
[![Watch the video](https://img.youtube.com/vi/SVHohefeliU/maxresdefault.jpg)](https://youtu.be/SVHohefeliU)

## Project Overview
AZRAM is an interactive robot companion project designed for use with Meta Quest 3 and Unity. The robot responds to hand gestures, voice, and environmental cues, displaying a range of emotional states and behaviors.

## Requirements
- Meta Quest 3 headset
- Meta Quest Link cable or Air Link
- Meta Quest PC app (installed and running)
- Unity (recommended version: 2021.3 LTS or later)
- Windows PC

## Setup
1. **Connect Meta Quest 3 to your PC**
   - Use Meta Quest Link cable or Air Link.
   - Ensure the Meta Quest PC app is open and your headset is connected.
2. **Open the Project in Unity**
   - Launch Unity Hub and open the AZRAM project folder.
3. **Navigate to the Correct Scene**
   - In the Unity Editor, open the main scene (e.g., `AR.unity`) from the `Assets/Scenes` directory.

## Usage
1. **Enter Play Mode**
   - In Unity, press the Play button.
2. **Put on the Headset**
   - Wear your Meta Quest 3 headset. You should see the AR scene in the headset display.
3. **Wake Up the Robot**
   - Place your right hand on the robot, aligning your index finger with the tail at the back.
   - Pinch with your left hand to wake up the robot **OR**
   - Press the `Spacebar` on your keyboard to wake up the robot.

## Controls & Interactions
- **Hand Gestures:** Interact with the robot using hand tracking and gestures.
- **Voice Commands:** (If enabled) Speak to the robot to trigger responses.
- **Need Gauges:** The robot's emotional state is influenced by touch, rest, social, and hunger needs.
- **Debugging:** Use the Debug Canvas in the scene to view internal state and events.

## Notes
- Ensure all required hardware and software are connected and running before entering Play mode.
- For best results, use the latest Meta Quest PC app and Unity LTS version.

---
## SceneController.cs
Central manager for the robot and scene. Handles user input, coordinates emotional responses, manages distance checks, controls sound, light, face, tail, and thought bubble displays, and orchestrates the wake-up sequence.

## EmotionModel.cs
Implements the robot's emotional state and need gauges (touch, rest, social, hunger). Calculates emotional responses to events, manages mood persistence, and provides the logic for mood decay and sleep/wake transitions.

## EmotionController.cs
Displays emotional responses visually and audibly. Manages emotion display timing, cooldowns, and passive facial expressions based on mood. Interfaces with the SceneController to trigger face, sound, light, and thought bubble changes.

## StrokeDetector.cs
Detects stroke gestures and holds using multiple colliders (front, back, top, left, right). Emits events for recognized stroke directions and holds, which are used to trigger emotional responses.

## StrokeTriggerDetector.cs
Attached to individual collider objects. Forwards trigger enter/exit events to the main StrokeDetector for centralized stroke/hold detection.

## UserFacingTracker.cs
Monitors the user's gaze direction relative to the robot. Detects when the user is looking at or away from the robot for a sustained period, triggering corresponding emotional events.

## QooboPositioner.cs
Handles positioning of the robot in the scene using hand tracking (Meta Quest) or keyboard input. Allows the robot to be placed via pinch gesture or spacebar, and restricts repositioning after initial placement.

## DistanceTracker.cs
Calculates and displays the distance between the robot and the user (camera). Updates a UI element and can show debug information.

## FaceController.cs
Controls the robot's face mesh and material. Sets facial expressions based on emotion, manages fade-in/out, and supports animated faces via the FaceAnimationController.

## FaceAnimationController.cs
Handles frame-based facial animations for different emotions. Loads animation frames from resources and plays looping or one-shot animations on the face material.

## ThoughtBubbleController.cs
Manages the display and animation of thought bubbles above the robot. Shows different sprites based on emotion or event, animates their appearance, and ensures they face the camera.

## LightEmissionSphere.cs
Controls a glowing sphere used for emotional light feedback. Changes color, intensity, and size based on emotion, and can animate/pulse or fade in/out.

## AudioController.cs
Plays audio clips corresponding to different emotions or events. Provides convenience methods for common sounds.

## TailAnimations.cs
Controls the robot's tail animations using an Animator. Plays different tail movements based on the current emotion.

## VoiceDetector.cs
Handles voice recognition and loudness detection. Uses Vosk for speech-to-text, matches recognized words to emotional events, and triggers emotional responses. Also detects loud sounds for surprise reactions.

## TransformLocker.cs
Locks the position, rotation, and/or scale of a GameObject to its initial values, preventing unwanted movement or transformation.

