using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class VoskRecognizer
{
    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_model_new(string model_path);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_recognizer_new(IntPtr model, float sample_rate);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern void vosk_recognizer_free(IntPtr recognizer);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern void vosk_model_free(IntPtr model);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool vosk_recognizer_accept_waveform(IntPtr recognizer, short[] data, int length);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_recognizer_result(IntPtr recognizer);

    [DllImport("libvosk", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr vosk_recognizer_partial_result(IntPtr recognizer);

    private IntPtr model;
    private IntPtr recognizer;
    private bool isInitialized;

    public bool Initialize(int sampleRate = 16000)
    {
        try
        {
            string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, "VoskModel");
            Debug.Log($"VoskRecognizer: Loading model from {modelPath}");

            model = vosk_model_new(modelPath);
            if (model == IntPtr.Zero)
            {
                Debug.LogError("VoskRecognizer: Failed to create model");
                return false;
            }

            recognizer = vosk_recognizer_new(model, sampleRate);
            if (recognizer == IntPtr.Zero)
            {
                Debug.LogError("VoskRecognizer: Failed to create recognizer");
                vosk_model_free(model);
                return false;
            }

            isInitialized = true;
            Debug.Log("VoskRecognizer: Initialized successfully");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"VoskRecognizer: Initialization error - {e.Message}");
            Debug.LogException(e);
            return false;
        }
    }

    public string ProcessAudio(float[] audioData)
    {
        if (!isInitialized || audioData == null || audioData.Length == 0)
            return null;

        try
        {
            // Log audio format details
            Debug.Log($"Processing audio - Buffer length: {audioData.Length} samples");

            // Convert float audio to 16-bit PCM
            short[] pcmData = new short[audioData.Length];
            
            // Calculate some stats about the audio
            float maxAmp = 0f;
            float minAmp = 0f;
            for (int i = 0; i < audioData.Length; i++)
            {
                maxAmp = Mathf.Max(maxAmp, audioData[i]);
                minAmp = Mathf.Min(minAmp, audioData[i]);
                pcmData[i] = (short)(audioData[i] * 32768f);
            }
            Debug.Log($"Audio range - Min: {minAmp:F3}, Max: {maxAmp:F3}, PCM range will be: {minAmp * 32768:F0} to {maxAmp * 32768:F0}");

            // Process the audio
            if (vosk_recognizer_accept_waveform(recognizer, pcmData, pcmData.Length))
            {
                IntPtr resultPtr = vosk_recognizer_result(recognizer);
                if (resultPtr != IntPtr.Zero)
                {
                    string result = Marshal.PtrToStringAnsi(resultPtr);
                    Debug.Log($"VoskRecognizer: Raw result: {result}");
                    return result;
                }
            }
            else
            {
                // Check for partial results
                IntPtr partialPtr = vosk_recognizer_partial_result(recognizer);
                if (partialPtr != IntPtr.Zero)
                {
                    string partial = Marshal.PtrToStringAnsi(partialPtr);
                    Debug.Log($"VoskRecognizer: Raw partial: {partial}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"VoskRecognizer: Processing error - {e.Message}");
            Debug.LogException(e);
        }

        return null;
    }

    public void Cleanup()
    {
        if (isInitialized)
        {
            if (recognizer != IntPtr.Zero)
            {
                vosk_recognizer_free(recognizer);
                recognizer = IntPtr.Zero;
            }
            if (model != IntPtr.Zero)
            {
                vosk_model_free(model);
                model = IntPtr.Zero;
            }
            isInitialized = false;
        }
    }
} 