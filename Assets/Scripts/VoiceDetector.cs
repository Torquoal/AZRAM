using UnityEngine;
using UnityEngine.Events;
using System;

public class VoiceDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool useDefaultMicrophone = false;
    [SerializeField] private bool monitorAudio = false;
    [SerializeField] [Range(0f, 1f)] private float monitorVolume = 0.5f;
    
    [Header("Events")]
    public UnityEvent<string> OnWordDetected;

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
        Debug.Log("Available microphones:");
        foreach (string device in Microphone.devices)
        {
            Debug.Log($"- {device}");
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
        Debug.Log($"Starting microphone: {selectedDevice}");
        
        // Start recording
        microphoneClip = Microphone.Start(selectedDevice, true, 1, 16000);
        
        if (microphoneClip == null)
        {
            Debug.LogError("Failed to start microphone!");
            return;
        }

        Debug.Log($"Microphone format - Frequency: {microphoneClip.frequency}Hz, Channels: {microphoneClip.channels}, Length: {microphoneClip.length}s, Samples: {microphoneClip.samples}");

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
            // Buffer wrapped around
            newSamples = (microphoneClip.samples - lastProcessedPosition) + currentPosition;
        }
        else
        {
            newSamples = currentPosition - lastProcessedPosition;
        }

        if (newSamples > 0)
        {
            // Create buffer for new data
            float[] newData = new float[newSamples];
            
            if (currentPosition < lastProcessedPosition)
            {
                // Handle buffer wrap-around
                int samplesAtEnd = microphoneClip.samples - lastProcessedPosition;
                
                // Get samples at the end
                microphoneClip.GetData(newData, lastProcessedPosition);
                
                if (currentPosition > 0)
                {
                    // Get samples from the beginning
                    float[] wrappedData = new float[currentPosition];
                    microphoneClip.GetData(wrappedData, 0);
                    System.Array.Copy(wrappedData, 0, newData, samplesAtEnd, currentPosition);
                }
            }
            else
            {
                // Get continuous chunk of new data
                microphoneClip.GetData(newData, lastProcessedPosition);
            }

            // Process only the new audio data
            string result = vosk.ProcessAudio(newData);
            if (!string.IsNullOrEmpty(result))
            {
                Debug.Log($"Detected: {result}");
                OnWordDetected?.Invoke(result);
            }

            lastProcessedPosition = currentPosition;
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
