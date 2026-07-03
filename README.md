# Modified AZRA: Augmenting Zoomorphic Robotics with Affect - Test explore robot aesthetics

AZRA is a Unity-based augmented reality (AR) framework for prototyping, deploying, and evaluating affective expressions for social robots. It allows researchers and developers to overlay expressive visual and auditory behaviours onto physical robots without modifying their hardware.

This branch provides the implementation used to explore how different aesthetic styles influence human perception of robot emotion.

---

## Overview

Social robots rely on expressive behaviours such as facial animations and sound cues to communicate emotions. However, comparing different aesthetic styles (e.g., realistic, abstract, stylised) is often difficult due to the constraints of physical prototyping.

AZRA addresses this by enabling:

- Rapid prototyping of robot expressions using AR
- Deployment of multiple expression styles onto the same robot
- Controlled evaluation of user perception across conditions
- Reuse of existing robot platforms without hardware modification

The system overlays virtual elements onto a real robot, allowing users to experience expressive behaviours while interacting with the robot in shared physical space.
---

## About the Research

This software was developed to support the study:

**Yipp and Macdonald - “Anime, Animal or Animal Crossing? Comparing Aesthetic Styles for Zoomorphic Robot Faces and Sounds using Augmented Reality” (MobileHCI 2026)**

### Research Goals

The study investigates how different aesthetic styles of robot expressions influence:

- Perceived clarity and recognisability of emotions  
- User preference and emotional response  
- Perceived appropriateness of expressions for a robot’s form  

A central question is whether emotional expressions should align with a robot’s morphology (e.g., animal-like vs human-like) in order to be effective.

---

### How AZRA Was Used

AZRA was used to deploy and evaluate a range of expressive styles on a zoomorphic robot (Qoobo) using augmented reality.

The study implemented:
**Face styles:**
- Zoomorphic  
- Mechanomorphic  
- Anthropomorphic (emoji-like)  
- Anime-inspired  
- Screen-based eyes  

**Sound styles:**
- Animal sounds  
- Human vocalisations  
- Abstract tones  
- Music  
- “Animalese” (stylised nonsensical vocalisations)  

**Emotions:**
- Happy  
- Sad  
- Angry  
- Scared  
- Surprised  

Assets for these aesthetic styles can be found in **\Assets\Resources\Modalities**

Participants interacted with the robot and evaluated each expression based on:
- Emotion recognition accuracy  
- Perceived clarity  
- Empathy toward the robot  
- Appropriateness of the expression  

The AR framework enabled all styles to be tested on a single physical platform without modifying the robot hardware, supporting controlled comparative evaluation 

---

### Key Findings

The study produced several important insights into the design of affective robot expressions:
- **Perceived appropriateness is a key driver of preference**  
  Users preferred expressions that matched the robot’s form over those that were simply easy to recognise 

- **Anime-inspired faces performed best overall**  
  These styles achieved a strong balance of clarity, recognisability, and emotional engagement 

- **Recognition accuracy does not predict preference**  
  Participants often preferred styles they found appropriate or evocative, even when those styles were harder to interpret 

- **Sound expressions were less effective in isolation**  
  - Human sounds were recognisable but often disliked  
  - Animal sounds were preferred but harder to interpret

- **Matching robot morphology is critical**  
  Aesthetic coherence between the robot’s body and its expressions strongly influenced user perception and emotional connection 
---

## Setup

### Requirements

- Unity (2022.3.x or compatible)
- Meta Quest 3 headset
- Windows PC capable of running Unity and AR streaming

### Installation

git clone https://github.com/Torquoal/AZRA-Augmenting-Zoomorphic-Robotics-with-Affect.git

Open the project in Unity

Download appropriate editor version

Ensure XR plugins and dependencies are installed

Configure your AR headset (e.g., enable developer mode, Quest Link)

Connect the headset to your machine
