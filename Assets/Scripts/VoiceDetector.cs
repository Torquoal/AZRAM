using UnityEngine;
using System.Collections;

public class VoiceDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionThreshold = 0.01f;
    [SerializeField] private float minimumInterval = 0.5f; // Minimum time between reactions
    [SerializeField] private int sampleDataLength = 1024;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showVolumeLevel = true;
    [SerializeField] private float debugUpdateInterval = 0.5f; // How often to show debug volume
    private float lastDebugTime;
    
    [Header("References")]
    [SerializeField] private SceneController sceneController;

    private AudioClip microphoneClip;
    private bool isListening = false;
    private float[] sampleData;
    private float lastReactionTime;
    private string selectedDevice;

    private string SelectPreferredMicrophone()
    {
        // First, look for VR/headset microphones
        foreach (string device in Microphone.devices)
        {
            if (device.Contains("Rift") || device.Contains("Quest") || device.Contains("Headset") || device.Contains("VR"))
            {
                Debug.Log($"Found VR/headset microphone: {device}");
                return device;
            }
        }

        // If no VR mic found, use the first available
        if (Microphone.devices.Length > 0)
        {
            Debug.Log($"No VR microphone found, using default: {Microphone.devices[0]}");
            return Microphone.devices[0];
        }

        return null;
    }

    private void Start()
    {
        Debug.Log("VoiceDetector starting...");
        sampleData = new float[sampleDataLength];
        
        if (sceneController == null)
        {
            sceneController = FindObjectOfType<SceneController>();
            if (sceneController == null)
                Debug.LogError("No SceneController found!");
            else
                Debug.Log("SceneController found successfully");
        }

        // List available microphones
        Debug.Log($"Number of microphones found: {Microphone.devices.Length}");
        if (showDebugLogs)
        {
            Debug.Log("Available microphones:");
            foreach (string device in Microphone.devices)
            {
                Debug.Log("- " + device);
            }
        }

        // Select preferred microphone
        selectedDevice = SelectPreferredMicrophone();
        if (selectedDevice != null)
        {
            Debug.Log($"Selected microphone: {selectedDevice}");
            StartListening();
        }
        else
        {
            Debug.LogError("No microphone found!");
        }
    }

    private void StartListening()
    {
        Debug.Log("Attempting to start microphone...");
        // Start recording (loop indefinitely)
        microphoneClip = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);
        
        if (microphoneClip == null)
        {
            Debug.LogError("Failed to create microphone AudioClip!");
            return;
        }

        // Wait until the microphone starts recording
        while (!(Microphone.GetPosition(selectedDevice) > 0)) { }

        isListening = true;
        Debug.Log($"Microphone started successfully. Sample rate: {AudioSettings.outputSampleRate}Hz");
    }

    private void StopListening()
    {
        if (isListening)
        {
            Microphone.End(selectedDevice);
            isListening = false;
            
            if (showDebugLogs)
                Debug.Log("Stopped listening");
        }
    }

    private void Update()
    {
        if (!isListening || sceneController == null) 
        {
            if (showDebugLogs && Time.time - lastDebugTime > debugUpdateInterval)
            {
                Debug.Log($"Not listening. isListening: {isListening}, sceneController: {(sceneController != null)}");
                lastDebugTime = Time.time;
            }
            return;
        }

        int micPosition = Microphone.GetPosition(selectedDevice);
        if (micPosition < 0)
        {
            Debug.LogError("Invalid microphone position!");
            return;
        }

        // Get current audio data
        if (!microphoneClip.GetData(sampleData, micPosition))
        {
            Debug.LogError("Failed to get audio data!");
            return;
        }

        // Calculate volume
        float sum = 0;
        int nonZeroSamples = 0;
        for (int i = 0; i < sampleData.Length; i++)
        {
            float sample = Mathf.Abs(sampleData[i]);
            sum += sample;
            if (sample > 0.0001f) nonZeroSamples++;
        }
        float averageVolume = sum / sampleData.Length;

        // Show continuous volume levels if enabled
        if (showVolumeLevel && Time.time - lastDebugTime > debugUpdateInterval)
        {
            Debug.Log($"Audio stats - Volume: {averageVolume:F5}, Active samples: {nonZeroSamples}/{sampleData.Length}, Position: {micPosition}");
            lastDebugTime = Time.time;
        }

        // Check if volume exceeds threshold and enough time has passed
        if (averageVolume > detectionThreshold && Time.time - lastReactionTime > minimumInterval)
        {
            Debug.Log($"Sound detected! Volume: {averageVolume:F5} (Threshold: {detectionThreshold})");
            
            // Trigger reaction
            ReactToSound(averageVolume);
            lastReactionTime = Time.time;
        }
    }

    private void ReactToSound(float volume)
    {
        Debug.Log($"Reacting to sound. Volume: {volume:F3}");
        // For now, just show a happy reaction
        sceneController.SetFaceExpression("happy");
        sceneController.ShowColouredLight("happy");
        sceneController.PlaySound("happy");
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void OnDestroy()
    {
        StopListening();
    }
} 