using UnityEngine;
using System;
using System.Collections.Generic;

public class VoiceDetector : MonoBehaviour
{
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private EmotionModel emotionModel;
    [SerializeField] private SceneController sceneController;
    [System.Serializable]
    public class WordGroup
    {
        public string groupName;
        public List<string> variations = new List<string>();
    }

    [Header("Settings")]
    [SerializeField] private bool useDefaultMicrophone = false;
    [SerializeField] private bool monitorAudio = false;
    [SerializeField] [Range(0f, 1f)] private float monitorVolume = 0.5f;
    [SerializeField] private bool showDebugLogs = false;  // Debug log toggle

    [Header("Voice Recognition")]
    [SerializeField] private List<WordGroup> wordGroups = new List<WordGroup>() {
        new WordGroup { 
            groupName = "Name", 
            variations = new List<string> { "qoobo", "coobo", "kubo", "cubo", "koobo", "robot"}
        },
        new WordGroup {
            groupName = "Happy",
            variations = new List<string> { "happy", "happiness", "joy", "joyful", "glad" }
        },
        new WordGroup {
            groupName = "Sad",
            variations = new List<string> { "sad", "sadness", "unhappy", "sorrow", "sorrowful" }
        },
        new WordGroup {
            groupName = "Angry",
            variations = new List<string> { "angry", "anger", "mad", "furious", "rage" }
        },
        new WordGroup {
            groupName = "Greeting",
            variations = new List<string> { "hello", "hi", "hey", "greetings", "how are you doing", "good morning", "good afternoon", "good evening" }
        },
        new WordGroup {
            groupName = "Farewell",
            variations = new List<string> { "goodbye", "bye", "farewell", "see you" }
        },
        new WordGroup {
            groupName = "Praise",
            variations = new List<string> { "good boy", "good kitty", "good robot", "nice", "well done" }
        },
        new WordGroup {
            groupName = "Food",
            variations = new List<string> { "food", "hungary", "hungry", "eat", "eating", "ate", "ate it", "ate it all", "ate it all the time" }
        }
    };

    [Header("Volume Detection")]
    [SerializeField] [Range(0f, 1f)] private float loudnessThreshold = 0.5f;
    [SerializeField] private float volumeCheckInterval = 0.1f;
    private float lastVolumeCheckTime = 0f;

    public delegate void LoudSoundDetectedHandler();
    public event LoudSoundDetectedHandler OnLoudSoundDetected;

    private AudioClip microphoneClip;
    private float[] audioBuffer;
    private string selectedDevice;
    private bool isListening = false;
    private VoskRecognizer vosk;
    private AudioSource monitorSource;
    private int lastProcessedPosition = 0;

    private void Start()
    {
        // Initialize Vosk
        vosk = new VoskRecognizer();
        vosk.SetDebugLogging(showDebugLogs);  // Pass debug setting to VoskRecognizer
        if (!vosk.Initialize(16000))
        {
            Debug.LogError("Failed to initialize Vosk");
            return;
        }

        InitializeMicrophone();
    }

    private void InitializeMicrophone()
    {
        // List available microphones
        if (showDebugLogs)
        {
            Debug.Log("Available microphones:");
            foreach (string device in Microphone.devices)
            {
                Debug.Log($"- {device}");
            }
        }

        // Select microphone
        if (useDefaultMicrophone && Microphone.devices.Length > 0)
        {
            selectedDevice = Microphone.devices[0];
        }
        else
        {
            foreach (string device in Microphone.devices)
            {
                if (device.Contains("Oculus") || device.Contains("Quest") || device.Contains("Meta") || device.Contains("VR"))
                {
                    selectedDevice = device;
                    break;
                }
            }

            if (string.IsNullOrEmpty(selectedDevice) && Microphone.devices.Length > 0)
            {
                selectedDevice = Microphone.devices[0];
            }
        }

        if (!string.IsNullOrEmpty(selectedDevice))
        {
            StartListening();
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    private void StartListening()
    {
        if (showDebugLogs)
            Debug.Log($"Starting microphone: {selectedDevice}");
        
        // Start recording
        microphoneClip = Microphone.Start(selectedDevice, true, 1, 16000);
        
        if (microphoneClip == null)
        {
            Debug.LogError("Failed to start microphone!");
            return;
        }

        //Debug.Log($"Microphone format - Frequency: {microphoneClip.frequency}Hz, Channels: {microphoneClip.channels}, Length: {microphoneClip.length}s, Samples: {microphoneClip.samples}");

        // Set up audio monitoring
        if (monitorAudio)
        {
            monitorSource = gameObject.AddComponent<AudioSource>();
            monitorSource.clip = microphoneClip;
            monitorSource.loop = true;
            monitorSource.volume = monitorVolume;
            monitorSource.Play();
        }

        audioBuffer = new float[microphoneClip.samples * microphoneClip.channels];
        lastProcessedPosition = 0;
        isListening = true;
    }

    private void Update()
    {
        if (!isListening) return;

        // Update monitor volume
        if (monitorSource != null)
        {
            monitorSource.volume = monitorAudio ? monitorVolume : 0f;
        }

        // Get current position and audio data
        int currentPosition = Microphone.GetPosition(selectedDevice);
        if (currentPosition < 0) return;

        // Calculate how many new samples we have
        int newSamples;
        if (currentPosition < lastProcessedPosition)
        {
            newSamples = (microphoneClip.samples - lastProcessedPosition) + currentPosition;
        }
        else
        {
            newSamples = currentPosition - lastProcessedPosition;
        }

        if (newSamples > 0)
        {
            float[] newData = new float[newSamples];
            
            if (currentPosition < lastProcessedPosition)
            {
                int samplesAtEnd = microphoneClip.samples - lastProcessedPosition;
                microphoneClip.GetData(newData, lastProcessedPosition);
                
                if (currentPosition > 0)
                {
                    float[] wrappedData = new float[currentPosition];
                    microphoneClip.GetData(wrappedData, 0);
                    System.Array.Copy(wrappedData, 0, newData, samplesAtEnd, currentPosition);
                }
            }
            else
            {
                microphoneClip.GetData(newData, lastProcessedPosition);
            }

            string result = vosk.ProcessAudio(newData);
            if (!string.IsNullOrEmpty(result))
            {
                ProcessRecognizedSpeech(result.ToLower());
            }

            lastProcessedPosition = currentPosition;
        }

        // Check volume at regular intervals
        if (Time.time - lastVolumeCheckTime >= volumeCheckInterval)
        {
            CheckVolume();
            lastVolumeCheckTime = Time.time;
        }
    }

    private void ProcessRecognizedSpeech(string speech)
    {
        if (showDebugLogs)
            Debug.Log($"Processing speech: {speech}");

        // Check each word group for matches
        foreach (var group in wordGroups)
        {
            foreach (var variation in group.variations)
            {
                if (speech.Contains(variation.ToLower()))
                {
                    HandleWordGroupMatch(group.groupName);
                    break;
                }
            }
        }
    }

    private void HandleWordGroupMatch(string groupName)
    {
        
        switch (groupName)
        {
            case "Name":
                if (showDebugLogs)
                    Debug.Log("Detected: Qoobo's name was called!");
                var response = emotionModel.CalculateEmotionalResponse("NameHeard");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                break;

            case "Happy":
                if (showDebugLogs)
                    Debug.Log("Detected: A happy word!");
                // add triggeredEvent
                break;

            case "Sad":
                if (showDebugLogs)
                    Debug.Log("Detected: A sad word!");
                // add triggeredEvent
                break;

            case "Angry":
                if (showDebugLogs)
                    Debug.Log("Detected: An angry word!");
                // add triggeredEvent
                break;

            case "Greeting":
                if (showDebugLogs)
                    Debug.Log("Detected: A greeting!");
                response = emotionModel.CalculateEmotionalResponse("GreetingHeard");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
                break;

            case "Farewell":
                if (showDebugLogs)
                    Debug.Log("Detected: A farewell!");
                // add triggeredEvent
                break;

            case "Praise":
                if (showDebugLogs)
                    Debug.Log("Detected: Words of praise!");
                // add triggeredEvent
                break;
            case "Food":
                if (showDebugLogs)
                    Debug.Log("Detected: Food word!");
                response = emotionModel.CalculateEmotionalResponse("FoodHeard");
                emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);

                break;
        }
    }

    private void CheckVolume()
    {
        if (microphoneClip == null) return;

        int pos = Microphone.GetPosition(selectedDevice);
        if (pos <= 0) return;

        float[] samples = new float[1024];
        microphoneClip.GetData(samples, pos - Mathf.Min(pos, samples.Length));

        // Calculate RMS
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        float rms = Mathf.Sqrt(sum / samples.Length);

        if (rms > loudnessThreshold)
        {
            if (showDebugLogs)
                Debug.Log($"Loud sound detected! RMS: {rms:F3}");
            OnLoudSoundDetected?.Invoke();
        }
    }

    private void OnDisable()
    {
        if (isListening)
        {
            if (monitorSource != null)
            {
                monitorSource.Stop();
                Destroy(monitorSource);
            }
            Microphone.End(selectedDevice);
            isListening = false;
        }
    }

    private void OnDestroy()
    {
        if (vosk != null)
        {
            vosk.Cleanup();
        }
    }
}
