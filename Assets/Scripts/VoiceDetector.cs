using UnityEngine;
using System.IO;
using UnityEngine.Events;

public class VoiceDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionThreshold = 0.01f;
    [SerializeField] private int sampleDataLength = 1024;
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private string[] keywords = new string[] { "hello", "stop", "go" }; // Add your keywords here

    [Header("Events")]
    public UnityEvent<string> OnWordDetected;

    private AudioClip microphoneClip;
    private float[] sampleData;
    private string selectedDevice;
    private bool isListening = false;
    private bool isProcessing = false;
    private float lastProcessTime = 0f;
    private const float PROCESS_INTERVAL = 0.5f; // Time between processing attempts

    // Buffer for storing audio when volume is above threshold
    private float[] audioBuffer;
    private int bufferSize = 16000; // 1 second at 16kHz
    private int bufferIndex = 0;
    private bool isBuffering = false;

    private void Start()
    {
        Debug.Log("VoiceDetector: Starting...");
        sampleData = new float[sampleDataLength];
        audioBuffer = new float[bufferSize];
        InitializeMicrophone();
    }

    private void InitializeMicrophone()
    {
        // List available microphones
        Debug.Log("VoiceDetector: Available microphones:");
        foreach (string device in Microphone.devices)
        {
            Debug.Log($"- {device}");
        }

        // Try to find the Quest/Oculus microphone
        foreach (string device in Microphone.devices)
        {
            if (device.Contains("Oculus") || device.Contains("Quest") || device.Contains("Meta") || device.Contains("VR"))
            {
                selectedDevice = device;
                Debug.Log($"VoiceDetector: Found VR headset microphone: {device}");
                break;
            }
        }

        // If no VR mic found, use the first available one
        if (string.IsNullOrEmpty(selectedDevice) && Microphone.devices.Length > 0)
        {
            selectedDevice = Microphone.devices[0];
            Debug.Log($"VoiceDetector: No VR microphone found, using default: {selectedDevice}");
        }

        if (!string.IsNullOrEmpty(selectedDevice))
        {
            StartListening();
        }
        else
        {
            Debug.LogError("VoiceDetector: No microphone found!");
        }
    }

    private void StartListening()
    {
        Debug.Log($"VoiceDetector: Starting microphone on device: {selectedDevice}");
        
        // Start recording (loop indefinitely)
        microphoneClip = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);
        
        if (microphoneClip == null)
        {
            Debug.LogError("VoiceDetector: Failed to create microphone AudioClip!");
            return;
        }

        // Wait until the microphone starts recording
        while (!(Microphone.GetPosition(selectedDevice) > 0)) { }

        isListening = true;
        Debug.Log($"VoiceDetector: Microphone started successfully. Sample rate: {AudioSettings.outputSampleRate}Hz");
    }

    private void Update()
    {
        if (!isListening) return;

        int micPosition = Microphone.GetPosition(selectedDevice);
        if (micPosition < 0) return;

        // Get current audio data
        microphoneClip.GetData(sampleData, micPosition);

        // Calculate volume
        float sum = 0;
        for (int i = 0; i < sampleData.Length; i++)
        {
            sum += Mathf.Abs(sampleData[i]);
        }
        float averageVolume = sum / sampleData.Length;

        // Handle audio buffering when volume is above threshold
        if (averageVolume > detectionThreshold)
        {
            if (!isBuffering)
            {
                isBuffering = true;
                bufferIndex = 0;
                if (showDebugLogs) Debug.Log("VoiceDetector: Started buffering audio");
            }

            // Add current samples to buffer
            for (int i = 0; i < sampleData.Length && bufferIndex < bufferSize; i++)
            {
                audioBuffer[bufferIndex++] = sampleData[i];
            }

            // If buffer is full, process it
            if (bufferIndex >= bufferSize && !isProcessing && Time.time - lastProcessTime > PROCESS_INTERVAL)
            {
                ProcessAudioBuffer();
            }
        }
        else if (isBuffering)
        {
            // Volume dropped below threshold, process what we have if enough data
            if (bufferIndex > bufferSize / 4) // Process if we have at least 1/4 of the buffer
            {
                ProcessAudioBuffer();
            }
            isBuffering = false;
        }

        // Log if volume exceeds threshold
        if (averageVolume > detectionThreshold && showDebugLogs)
        {
            Debug.Log($"VoiceDetector: Sound detected! Volume: {averageVolume:F5}");
        }
    }

    private void ProcessAudioBuffer()
    {
        isProcessing = true;
        lastProcessTime = Time.time;

        // TODO: Add PocketSphinx processing here
        // For now, we'll just log that we would process the audio
        if (showDebugLogs)
        {
            Debug.Log($"VoiceDetector: Would process {bufferIndex} samples of audio");
        }

        // Reset buffer
        bufferIndex = 0;
        isProcessing = false;
    }

    private void OnDisable()
    {
        if (isListening)
        {
            Microphone.End(selectedDevice);
            isListening = false;
            Debug.Log("VoiceDetector: Stopped listening");
        }
    }

    private void OnDestroy()
    {
        if (isListening)
        {
            Microphone.End(selectedDevice);
            isListening = false;
        }
    }
}
