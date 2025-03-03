using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public class EmotionModel : MonoBehaviour
{
    
    [SerializeField] [Range(-10, 10)] private float longTermValence = 10f;
    [SerializeField] [Range(-10, 10)] private float longTermArousal = 0f;

    [SerializeField] [Range(-10, 10)] private float moodValence;
    [SerializeField] [Range(-10, 10)] private float moodArousal;

    private float moodValenceOnWakeup;
    private float arousalArousalOnWakup;

    // Set initial current emotional state
    [SerializeField] private string currentTemperament;
    [SerializeField] private string currentMood;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugText = true;
    [SerializeField] private bool useAcceleratedTesting = false;
    [SerializeField] private bool usePersistentEmotions = true;  // Toggle for persistent emotions
    [SerializeField] private float testingMultiplier = 180f; // Speed up time for testing

    // Struct to hold the emotional response values
    private struct EmotionalResponseValues
    {
        public float Valence;
        public float Arousal;
        public float Touch;
        public float Rest;
        public float Social;

        public override string ToString()
        {
            return $"Valence: {Valence}, Arousal: {Arousal}, Touch: {Touch}, Rest: {Rest}, Social: {Social}";
        }
    }

    // The emotional response lookup table using strings
    private Dictionary<string, Dictionary<string, EmotionalResponseValues>> responseTable;

    

    // Dictionary to store the emotional response the robot will eventually display
    private Dictionary<string, string> emotionalResponse = new Dictionary<string, string>
    {
        { "face", "" },
        { "light", "" },
        { "sound", "" },
        { "thought", "" },
        { "tail", "" }
    };

    [Header("Need Values")]
    [SerializeField] [Range(0, 100)] private float touchGauge = 50f;
    [SerializeField] [Range(0, 100)] private float restGauge = 50f;
    [SerializeField] [Range(0, 100)] private float socialGauge = 50f;
    [SerializeField] [Range(0, 100)] private float hungerGauge = 50f;

    [Header("Need Thresholds")]
    [SerializeField] [Range(0, 100)] private float touchFulfilled = 70f;
    [SerializeField] [Range(0, 100)] private float touchNeeded = 30f;
    [SerializeField] [Range(0, 100)] private float restFulfilled = 70f;
    [SerializeField] [Range(0, 100)] private float restNeeded = 30f;
    [SerializeField] [Range(0, 100)] private float socialFulfilled = 70;
    [SerializeField] [Range(0, 100)] private float socialNeeded = 30f;
    [SerializeField] [Range(0, 100)] private float hungerFulfilled = 70f;
    [SerializeField] [Range(0, 100)] private float hungerNeeded = 30f;

    [Header("Decay Settings")]
    private float gaugeLogTimer = 0f;
    private const float GAUGE_LOG_INTERVAL = 5f;

    // Accumulated decay values (to handle small changes over time)
    private float touchDecayAccumulator = 0f;
    private float restDecayAccumulator = 0f;
    private float socialDecayAccumulator = 0f;
    private float hungerDecayAccumulator = 0f;

    // Real-time decay rates (points per second)
    private readonly float touchDecayRate = 100f / (3f * 60f * 60f);    // 100 points / 3 hours
    private readonly float restDecayRate = 100f / (12f * 60f * 60f);    // 100 points / 12 hours
    private readonly float socialDecayRate = 100f / (6f * 60f * 60f);   // 100 points / 6 hours
    private readonly float hungerDecayRate = 100f / (6f * 60f * 60f);   // 100 points / 6 hours

    // Constants for PlayerPrefs keys
    private const string LONG_TERM_VALENCE_KEY = "LongTermValence";
    private const string LONG_TERM_AROUSAL_KEY = "LongTermArousal";

    [Header("Sleep Settings")]
    [SerializeField] private float maxSleepDuration = 4f * 60f * 60f; // 4 hours in seconds
    [SerializeField] private float restRegenerationRate = 100f / (4f * 60f * 60f); // 100 points / 4 hours
    [SerializeField] private SceneController sceneController;
    private bool isAsleep = false;
    private float sleepStartTime = 0f;
    public bool IsAsleep => isAsleep;

    [Header("References")]
    [SerializeField] private EmotionController emotionController;

    //test Functions
    [ContextMenu("Test Emotional Model - StrokeFrontToBack")]
    private void TestStrokeFrontToBack()
    {
        string triggeredEvent = "StrokeFrontToBack";
        EmotionalResponseResult response = CalculateEmotionalResponse(triggeredEvent);
        Debug.Log($"Test Emotional Model - StrokeFrontToBack: {response.EmotionToDisplay} (Trigger: {response.TriggerEvent})");
    }

    [ContextMenu("Test Emotional Model - Touch Fulfilled")]
    private void TestTouchFulfilled()
    {
        string triggeredEvent = "TouchFulfilled";
        EmotionalResponseResult response = CalculateEmotionalResponse(triggeredEvent);
        Debug.Log($"Test Emotional Model - Touch Fulfilled: {response.EmotionToDisplay} (Trigger: {response.TriggerEvent})");
    }



    private void Awake()
    {
        // Load long term values based on persistence setting
        if (usePersistentEmotions && PlayerPrefs.HasKey(LONG_TERM_VALENCE_KEY))
        {
            longTermValence = PlayerPrefs.GetFloat(LONG_TERM_VALENCE_KEY);
            if (PlayerPrefs.HasKey(LONG_TERM_AROUSAL_KEY))
            {
                longTermArousal = PlayerPrefs.GetFloat(LONG_TERM_AROUSAL_KEY);
            }
            Debug.Log($"Loaded persistent emotions - Valence: {longTermValence}, Arousal: {longTermArousal}");
        }
        else
        {
            // Use inspector values
            Debug.Log($"Using inspector values - Valence: {longTermValence}, Arousal: {longTermArousal}");
        }

        InitializeResponseTable();
        currentTemperament = classifyEmotionalState(longTermValence, longTermArousal);
        Debug.Log($"Current emotional state on wakeup: {currentTemperament}");

        // calculate this session's mood 
        moodValence = longTermValence + UnityEngine.Random.Range(-5f, 5f);
        moodArousal = longTermArousal + UnityEngine.Random.Range(-5f, 5f);

        // Clamp mood values
        moodValence = Mathf.Clamp(moodValence, -10f, 10f);
        moodArousal = Mathf.Clamp(moodArousal, -10f, 10f);

        currentMood = classifyEmotionalState(moodValence, moodArousal);
        Debug.Log($"Current mood on wakeup: {currentMood}");

        Debug.Log($"Emotion Model: Mood on wakeup: V: {moodValence}, A: {moodArousal}");

        // save the mood values from the start of the session to calculate delta later
        moodValenceOnWakeup = moodValence;
        arousalArousalOnWakup = moodArousal;
    }

    private void Start()
    {
        // Log initial decay rates
        Debug.Log($"Decay rates per second:\n" +
                 $"Touch: {touchDecayRate:F6}\n" +
                 $"Rest: {restDecayRate:F6}\n" +
                 $"Social: {socialDecayRate:F6}\n" +
                 $"Hunger: {hungerDecayRate:F6}");
    }

    private void Update()
    {
        // Handle sleep state first
        if (isAsleep)
        {
            HandleSleepState();
            return; // Skip normal update if sleeping
        }

        float timeMultiplier = useAcceleratedTesting ? testingMultiplier : 1f;
        float deltaTime = Time.deltaTime * timeMultiplier;

        // Store previous values to detect threshold crossings
        float prevTouch = touchGauge;
        float prevRest = restGauge;
        float prevSocial = socialGauge;
        float prevHunger = hungerGauge;

        // Accumulate decay values
        touchDecayAccumulator += touchDecayRate * deltaTime;
        restDecayAccumulator += restDecayRate * deltaTime;
        socialDecayAccumulator += socialDecayRate * deltaTime;
        hungerDecayAccumulator += hungerDecayRate * deltaTime;

        // Only update gauges when accumulated decay is >= 1
        if (touchDecayAccumulator >= 1f)
        {
            float decayAmount = touchDecayAccumulator;
            touchGauge = Mathf.Max(0f, touchGauge - decayAmount);
            touchDecayAccumulator = 0f;
        }

        if (restDecayAccumulator >= 1f)
        {
            float decayAmount = restDecayAccumulator;
            restGauge = Mathf.Max(0f, restGauge - decayAmount);
            if (restGauge <= 0f && !isAsleep)
            {
                StartSleep();
            }
            restDecayAccumulator = 0f;
        }

        if (socialDecayAccumulator >= 1f)
        {
            float decayAmount = socialDecayAccumulator;
            socialGauge = Mathf.Max(0f, socialGauge - decayAmount);
            socialDecayAccumulator = 0f;
        }

        if (hungerDecayAccumulator >= 1f)
        {
            float decayAmount = hungerDecayAccumulator;
            hungerGauge = Mathf.Max(0f, hungerGauge - decayAmount);
            hungerDecayAccumulator = 0f;
        }

        
        // Check for threshold crossings
        CheckNeedThreshold("Touch", touchGauge, prevTouch, touchNeeded, touchFulfilled);
        CheckNeedThreshold("Rest", restGauge, prevRest, restNeeded, restFulfilled);
        CheckNeedThreshold("Social", socialGauge, prevSocial, socialNeeded, socialFulfilled);
        CheckNeedThreshold("Hunger", hungerGauge, prevHunger, hungerNeeded, hungerFulfilled);

        // Log gauge values every 5 seconds
        gaugeLogTimer += Time.deltaTime;
        if (gaugeLogTimer >= GAUGE_LOG_INTERVAL)
        {
            //LogGaugeValues();
            // Also log accumulators for debugging
            //Debug.Log($"Decay Accumulators:\n" +
            //         $"Touch: {touchDecayAccumulator:F3}\n" +
            //         $"Rest: {restDecayAccumulator:F3}\n" +
            //         $"Social: {socialDecayAccumulator:F3}\n" +
            //         $"Hunger: {hungerDecayAccumulator:F3}");
            gaugeLogTimer = 0f;
        }
    }

    private void HandleSleepState()
    {
        float timeMultiplier = useAcceleratedTesting ? testingMultiplier : 1f;
        float deltaTime = Time.deltaTime * timeMultiplier;

        // Accumulate rest regeneration
        float regenerationAmount = restRegenerationRate * deltaTime;
        restGauge = Mathf.Min(100f, restGauge + regenerationAmount);

        // Check if we should wake up
        float timeAsleep = Time.time - sleepStartTime;
        if (restGauge >= 100f || timeAsleep >= maxSleepDuration)
        {
            WakeUp();
        }
    }

    private void StartSleep()
    {
        isAsleep = true;
        sleepStartTime = Time.time;
        Debug.Log("Robot has fallen asleep");
    }

    public void WakeUp()
    {
        isAsleep = false;
        float sleepDuration = Time.time - sleepStartTime;
        Debug.Log($"Robot waking up after {sleepDuration / 60f:F1} minutes of sleep");

        // Only hide the thought bubble if waking up naturally (not from loud noise or other events)
        if (restGauge >= 100f || sleepDuration >= maxSleepDuration)
        {
            sceneController.HideThought();
        }
    }

    private void LogGaugeValues()
    {
        string gaugeStatus = "Current Gauge Values:\n" +
            $"Touch:  {touchGauge,3}/100 (Need: {touchNeeded}, Fulfilled: {touchFulfilled})\n" +
            $"Rest:   {restGauge,3}/100 (Need: {restNeeded}, Fulfilled: {restFulfilled})\n" +
            $"Social: {socialGauge,3}/100 (Need: {socialNeeded}, Fulfilled: {socialFulfilled})\n" +
            $"Hunger: {hungerGauge,3}/100 (Need: {hungerNeeded}, Fulfilled: {hungerFulfilled})";
        Debug.Log(gaugeStatus);
    }

    private void CheckNeedThreshold(string needName, float currentValue, float previousValue, float neededThreshold, float fulfilledThreshold)
    {
        
        string triggeredEvent = "";

        // Check if need has fallen below the needed threshold
        if (currentValue <= neededThreshold && previousValue > neededThreshold)
        {
            triggeredEvent = $"{needName}Needed";
            if (showDebugText)
                Debug.Log($"{needName} has fallen below needed threshold ({neededThreshold})");
        }
        // Check if need has risen above the fulfilled threshold
        else if (currentValue >= fulfilledThreshold && previousValue < fulfilledThreshold)
        {
            triggeredEvent = $"{needName}Fulfilled";
            if (showDebugText)
                Debug.Log($"{needName} has risen above fulfilled threshold ({fulfilledThreshold})");
        }

        // If an event was triggered, calculate and display emotional response
        if (!string.IsNullOrEmpty(triggeredEvent))
        {
            EmotionalResponseResult result = CalculateEmotionalResponse(triggeredEvent);
            if (showDebugText)
                Debug.Log($"Triggered {triggeredEvent} - Emotional Response: {result.EmotionToDisplay}");
            emotionController.DisplayEmotionInternal(result.EmotionToDisplay, triggeredEvent);
        }
    }

    public struct EmotionalResponseResult
    {
        public string EmotionToDisplay;
        public string TriggerEvent;

        public EmotionalResponseResult(string emotion, string trigger)
        {
            EmotionToDisplay = emotion;
            TriggerEvent = trigger;
        }
    }

    // Modify the return type of CalculateEmotionalResponse
    public EmotionalResponseResult CalculateEmotionalResponse(string triggeredEvent)
    {
        // Special handling for loud noise
        if (triggeredEvent.ToLower() == "loudnoise")
        {
            // If we're asleep, wake up
            if (isAsleep)
            {
                WakeUp();
            }
            return new EmotionalResponseResult("loudnoise", triggeredEvent);
        }

        // If asleep, only respond to special events (like loudnoise above)
        if (isAsleep)
        {
            return new EmotionalResponseResult("sleep", triggeredEvent);
        }

        currentMood = classifyEmotionalState(moodValence, moodArousal);
        Debug.Log("Emotion Model: currentMood: " + currentMood);
        
        // Debug logging
        Debug.Log($"Calculating response for event '{triggeredEvent}' in mood '{currentMood}'");
        Debug.Log($"Response table contains {responseTable.Count} events");
        
        if (!responseTable.ContainsKey(triggeredEvent))
        {
            Debug.LogError($"No responses found for event: {triggeredEvent}");
            return new EmotionalResponseResult("neutral", triggeredEvent);
        }
        
        if (!responseTable[triggeredEvent].ContainsKey(currentMood))
        {
            Debug.LogError($"No response found for mood: {currentMood} in event: {triggeredEvent}");
            return new EmotionalResponseResult("neutral", triggeredEvent);
        }

        EmotionalResponseValues response = responseTable[triggeredEvent][currentMood];
        Debug.Log("Calculated emotional response to " + triggeredEvent + " in " + currentMood + " is:\n" + response);

        string displayString = emotionalDisplayTable(response);

        return new EmotionalResponseResult(displayString, triggeredEvent);
    }


    /*
   Tables!

   1. classifyEmotionalState() returns the current emotional state as a semantic string based on
   long term valence and arousal values.

   2. InitializeResponseTable() this a table that returns a specific set of emotional response values
   based on an A) a user action or robot need gauge threshold (e.g., TouchNeeded or StrokeFrontToBack)
   and B) the current emotional state as returned by classifyEmotionalState(). These responses have five
   values. Valence and Arousal are used to identify what short term emotional response will display using
   the displayTable. Touch, Rest and Social are values to be added to the robot's need gauges, allowing
   certain user actions to impact the robot's needs.

   3. displayTable() this is a table that uses the valence and arousal values from the responseTable
   to return a specific string that SceneController will use to select what emotional displays are
   shown. The valence and arousal values are fuzzed first enough that emotional responses are not 
   perfectly consistent and the robot can sometimes show a display one column or row beyond the norrmal
   response (fuzz of 3?). Additionally, a small percentage (10%?) of these values is added to the
   robot's long term valence and arousal, allowing for slow mood change and long-term evolution.
    */

    // Array of special events that bypass normal emotion classification
    private readonly string[] specialEvents = new string[]
    {
        "HungerNeeded",
        "RestNeeded",
        "TouchNeeded",
        "SocialNeeded",
        "HungerFulfilled",
        "RestFulfilled",
        "TouchFulfilled",
        "SocialFulfilled",
    };

    private string classifyEmotionalState(float valence, float arousal)
    {

        string classifiedEmotion = "Neutral";
        
        // Existing emotion classification logic...
        //if (valence >= -10 && valence <= 10)
        //{
            // right hand column
            if (valence > 3)
            {
                if (arousal > 3)
                { 
                    classifiedEmotion = "Excited";
                } else if (arousal < 3 && arousal > -3){
                    classifiedEmotion = "Happy";
                } else if (arousal < -3){
                    classifiedEmotion = "Relaxed";
                }
            } else if (valence < 3 && valence > -2){
                if (arousal > 8){
                    classifiedEmotion = "Surprised";
                } else if (arousal < 8 && arousal > 5){
                    classifiedEmotion = "Energetic";
                } else if (arousal < 5 && arousal > -3){
                    classifiedEmotion = "Neutral";
                } else if (arousal < -3){
                    classifiedEmotion = "Tired";
                }
            } else if (valence < -2)
            {
            // top left corner
                if (arousal > 5){
                    if (valence > -5){
                        classifiedEmotion = "Tense";
                    } else if (valence < -5){
                        if (arousal > 8){
                            classifiedEmotion = "Scared";
                        } else if (arousal > 6){
                            classifiedEmotion = "Angry";
                        }
                    }
                } else if (arousal > -3){
                    classifiedEmotion = "Miserable";
                } else if (arousal < -3 && arousal > -7){
                    classifiedEmotion = "Sad";
                } else if (arousal < -7){
                    classifiedEmotion = "Gloomy";
                }
            }     
        //} else {
            //Debug.LogError("Long term valence is out of range");
        //}
        
        return classifiedEmotion;
    }


    private void InitializeResponseTable()
    {
        responseTable = new Dictionary<string, Dictionary<string, EmotionalResponseValues>>();
        
        // Path to the CSV file in the StreamingAssets folder
        string csvPath = Path.Combine(Application.streamingAssetsPath, "EmotionalResponses.csv");

        try
        {
            // Create StreamingAssets directory if it doesn't exist
            Directory.CreateDirectory(Application.streamingAssetsPath);
            
            // Read all lines from the CSV file
            string[] lines = File.ReadAllLines(csvPath);
            
            // Skip the header row
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                
                // Parse the CSV values
                string eventType = values[0];
                string emotionalState = values[1];
                float valence = float.Parse(values[2]);
                float arousal = float.Parse(values[3]);
                float touch = float.Parse(values[4]);
                float rest = float.Parse(values[5]);
                float social = float.Parse(values[6]);
                
                // Create response value
                EmotionalResponseValues response = new EmotionalResponseValues
                {
                    Valence = valence,
                    Arousal = arousal,
                    Touch = touch,
                    Rest = rest,
                    Social = social
                };

                // Initialize dictionary for this event type if it doesn't exist
                if (!responseTable.ContainsKey(eventType))
                {
                    responseTable[eventType] = new Dictionary<string, EmotionalResponseValues>();
                }
                
                // Add the response to the table
                responseTable[eventType][emotionalState] = response;
            }
            
            Debug.Log($"Successfully loaded emotional responses from {csvPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading emotional responses from CSV: {e.Message}");
            // Initialize with default values as fallback
        }
    }

    private string emotionalDisplayTable(EmotionalResponseValues response)
    {
        string displayString = "Neutral";
        
        //addition to Need Gauges
        touchGauge += response.Touch;
        restGauge += response.Rest;
        socialGauge += response.Social;

        Debug.Log("Added to Gauges: Touch: " + response.Touch + 
                                   " Rest: " + response.Rest + 
                                 " Social: " + response.Social);
        
        //fuzz valence and arousal values
        float fuzzedValence = response.Valence + UnityEngine.Random.Range(-3f, 3f);
        float fuzzedArousal = response.Arousal + UnityEngine.Random.Range(-3f, 3f);

        //add a small percentage (5%?) of these values to the robot's long term valence and arousal.
        moodValence += fuzzedValence * 0.01f;
        moodArousal += fuzzedArousal * 0.01f;

        // Clamp mood values
        moodValence = Mathf.Clamp(moodValence, -10f, 10f);
        moodArousal = Mathf.Clamp(moodArousal, -10f, 10f);

        // Update current mood after changes
        currentMood = classifyEmotionalState(moodValence, moodArousal);

        Debug.Log("Emotion Model: response.Valence: " + response.Valence + 
                " fuzzed to " + fuzzedValence + 
                "and 5%" + fuzzedValence*0.01f + "added to mood valence");
        Debug.Log("Emotion Model: response.Arousal: " + response.Arousal + 
                  " fuzzed to " + fuzzedArousal + 
                  "and 5%" + fuzzedArousal*0.01f + "added to mood arousal");
        Debug.Log("Emotion Model: Updated current mood to: " + currentMood);



        //TODO: could also add the logic here to handle special events, e.g., if Rest <10 a special
        //sleeping display is triggered and the normal table is bypassed. SceneController will 
        //do something similar for user-initiated special events, such as saying Qoobo's name.
        //Only if the triggeredEvent is not a special event will the normal table be used.

        if (fuzzedArousal > 6){
            if (fuzzedValence > 6){
                displayString = "Excited";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Excited";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Surprised";
            } else if (fuzzedValence < -3 && fuzzedValence > -6){
                displayString = "Tense";
            } else if (fuzzedValence < -6){
                displayString = "Scared";
            }
        } else if (fuzzedArousal < 6 && fuzzedArousal > 3){
            if (fuzzedValence > 6){
                displayString = "Happy";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Happy";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Energetic";
            } else if (fuzzedValence < -3 && fuzzedValence > -6){
                displayString = "Annoyed";
            } else if (fuzzedValence < -6){
                displayString = "Angry";
            }
        } else if (fuzzedArousal < 3 && fuzzedArousal > -3){
            if (fuzzedValence > 6){
                displayString = "Happy";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Happy";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Neutral";
            } else if (fuzzedValence < -3 && fuzzedValence > -6){
                displayString = "Sad";
            } else if (fuzzedValence < -6){
                displayString = "Miserable";
            }
        } else if (fuzzedArousal < -3 && fuzzedArousal > -6){
            if (fuzzedValence > 6){
                displayString = "Relaxed";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Relaxed";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Tired";
            } else if (fuzzedValence < -3 && fuzzedValence > -6){
                displayString = "Sad";
            } else if (fuzzedValence < -6){
                displayString = "Sad";
            }
        } else if (fuzzedArousal < -6){
            if (fuzzedValence > 6){
                displayString = "Happy Sleepy";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Happy Sleepy";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Sleepy";
            } else if (fuzzedValence < -3 && fuzzedValence > -6){
                displayString = "Sad Sleepy";
            } else if (fuzzedValence < -6){
                displayString = "Sad Sleepy";
            }
        } else {
            Debug.LogError("Fuzzed Arousal is out of range");
        }
        Debug.Log("Emotion Model: selected display string from fuzzed values: " + displayString);
        return displayString;
    }

    private void OnApplicationQuit()
    {
        if (usePersistentEmotions)
        {
            // Calculate the change in mood from this session
            float moodValenceDelta = moodValence - moodValenceOnWakeup;
            float moodArousalDelta = moodArousal - arousalArousalOnWakup;

            // Apply a fraction of the mood change to the long term values
            longTermValence += moodValenceDelta * 0.1f; // 10% of mood change affects long term
            longTermArousal += moodArousalDelta * 0.1f;

            // Clamp values
            longTermValence = Mathf.Clamp(longTermValence, -10f, 10f);
            longTermArousal = Mathf.Clamp(longTermArousal, -10f, 10f);

            // Save to persistent storage
            PlayerPrefs.SetFloat(LONG_TERM_VALENCE_KEY, longTermValence);
            PlayerPrefs.SetFloat(LONG_TERM_AROUSAL_KEY, longTermArousal);
            PlayerPrefs.Save();

            Debug.Log($"Saved long term values - Valence: {longTermValence}, Arousal: {longTermArousal}");
        }
        else
        {
            Debug.Log("Persistent emotions disabled - not saving long term values");
        }
    }

    // Method to reset long-term values if needed (for testing)
    [ContextMenu("Reset Long Term Values")]
    private void ResetLongTermValues()
    {
        PlayerPrefs.DeleteKey(LONG_TERM_VALENCE_KEY);
        PlayerPrefs.DeleteKey(LONG_TERM_AROUSAL_KEY);
        longTermValence = 10f;  // Default starting value
        longTermArousal = 0f;   // Default starting value
        Debug.Log("Reset long term emotional values to defaults");
    }

    // Public accessors for current mood state
    public string GetCurrentMood()
    {
        return classifyEmotionalState(moodValence, moodArousal);
    }

    public float GetMoodValence()
    {
        return moodValence;
    }

    public float GetMoodArousal()
    {
        return moodArousal;
    }
} 