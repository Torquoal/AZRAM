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

    private float eventWeight = 0.7f;
    private float moodWeight = 0.3f;

    private float moodValenceOnWakeup;
    private float arousalArousalOnWakup;

    // Set initial current emotional state
    [SerializeField] private string currentTemperament;
    [SerializeField] private string currentMood;
    
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool useAcceleratedTesting = false;
    [SerializeField] private bool usePersistentEmotions = true;  // Toggle for persistent emotions
    [SerializeField] private float testingMultiplier = 180f; // Speed up time for testing

    [Header("Response Settings")]
    [SerializeField] [Range(0, 100)] private float responseChance = 100f; // Percentage chance to show emotional response

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

    // Base response values for each mood state
    private Dictionary<string, EmotionalResponseValues> moodBaseValues = new Dictionary<string, EmotionalResponseValues>()
    {
        { "Excited", new EmotionalResponseValues { Valence = 8f, Arousal = 8f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Happy", new EmotionalResponseValues { Valence = 8f, Arousal = 4f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Relaxed", new EmotionalResponseValues { Valence = 4f, Arousal = -4f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Energetic", new EmotionalResponseValues { Valence = 4f, Arousal = 8f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Neutral", new EmotionalResponseValues { Valence = 0f, Arousal = 0f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Tired", new EmotionalResponseValues { Valence = -2f, Arousal = -6f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Annoyed", new EmotionalResponseValues { Valence = -4f, Arousal = 4f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Sad", new EmotionalResponseValues { Valence = -6f, Arousal = -2f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "Gloomy", new EmotionalResponseValues { Valence = -8f, Arousal = -8f, Touch = 0f, Rest = 0f, Social = 0f } }
    };

    // Base response values for each event
    private Dictionary<string, EmotionalResponseValues> eventBaseValues = new Dictionary<string, EmotionalResponseValues>()
    {
        { "HungerNeeded", new EmotionalResponseValues { Valence = -5f, Arousal = 5f, Touch = 0f, Rest = 0f, Social = 0f } },
        { "HungerUnfulfilled", new EmotionalResponseValues { Valence = -10f, Arousal = 5f, Touch = 0f, Rest = 0f, Social = -3f } },
        { "HungerFulfilled", new EmotionalResponseValues { Valence = 5f, Arousal = 0f, Touch = 0f, Rest = 0f, Social = 3f } },
        { "TouchNeeded", new EmotionalResponseValues { Valence = -2f, Arousal = 2f, Touch = -5f, Rest = 0f, Social = -2f } },
        { "TouchUnfulfilled", new EmotionalResponseValues { Valence = -5f, Arousal = 0f, Touch = -10f, Rest = -2f, Social = -5f } },
        { "TouchFulfilled", new EmotionalResponseValues { Valence = 8f, Arousal = 5f, Touch = 10f, Rest = 2f, Social = 5f } },
        { "RestNeeded", new EmotionalResponseValues { Valence = -2f, Arousal = -5f, Touch = 0f, Rest = -5f, Social = -2f } },
        { "RestUnfulfilled", new EmotionalResponseValues { Valence = -5f, Arousal = -8f, Touch = -2f, Rest = -10f, Social = -5f } },
        { "RestFulfilled", new EmotionalResponseValues { Valence = 5f, Arousal = 2f, Touch = 2f, Rest = 10f, Social = 2f } },
        { "SocialNeeded", new EmotionalResponseValues { Valence = -2f, Arousal = 2f, Touch = 0f, Rest = 0f, Social = -5f } },
        { "SocialUnfulfilled", new EmotionalResponseValues { Valence = -5f, Arousal = -2f, Touch = -2f, Rest = -2f, Social = -10f } },
        { "SocialFulfilled", new EmotionalResponseValues { Valence = 5f, Arousal = 5f, Touch = 2f, Rest = 2f, Social = 10f } },
        { "StrokeFrontToBack", new EmotionalResponseValues { Valence = 8f, Arousal = 5f, Touch = 10f, Rest = 2f, Social = 5f } },
        { "StrokeBackToFront", new EmotionalResponseValues { Valence = -10f, Arousal = 3f, Touch = 5f, Rest = -2f, Social = 2f } },
        { "NameHeard", new EmotionalResponseValues { Valence = 5f, Arousal = 5f, Touch = 0f, Rest = 0f, Social = 5f } },
        { "GreetingHeard", new EmotionalResponseValues { Valence = 5f, Arousal = 2f, Touch = 0f, Rest = 0f, Social = 5f } },
        { "FoodHeard", new EmotionalResponseValues { Valence = 5f, Arousal = 2f, Touch = 0f, Rest = 0f, Social = 2f } },
        { "TooFarAway", new EmotionalResponseValues { Valence = -10f, Arousal = 2f, Touch = 0f, Rest = 0f, Social = -5f } },
        { "HappyHeard", new EmotionalResponseValues { Valence = 8f, Arousal = 5f, Touch = 0f, Rest = 0f, Social = 3f } },
        { "SadHeard", new EmotionalResponseValues { Valence = -8f, Arousal = -3f, Touch = 0f, Rest = 0f, Social = 3f } },
        { "AngryHeard", new EmotionalResponseValues { Valence = -8f, Arousal = 3f, Touch = 0f, Rest = 0f, Social = 2f } },
        { "FarewellHeard", new EmotionalResponseValues { Valence = -2f, Arousal = -2f, Touch = 0f, Rest = 0f, Social = 3f } },
        { "PraiseHeard", new EmotionalResponseValues { Valence = 10f, Arousal = 5f, Touch = 0f, Rest = 0f, Social = 5f } },
        { "TouchHeard", new EmotionalResponseValues { Valence = 3f, Arousal = 2f, Touch = 3f, Rest = 0f, Social = 2f } }
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

    // Debug properties
    public string LastTriggeredEvent { get; private set; }
    public string LastDisplayString { get; private set; }

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
        // Trigger sleep display when actually falling asleep
        var response = CalculateEmotionalResponse("RestNeeded");
        emotionController.TryDisplayEmotion(response.EmotionToDisplay, response.TriggerEvent);
    }

    public void WakeUp()
    {
        if (!isAsleep) return;
        
        isAsleep = false;
        // Clear sleep display when waking up
        emotionController.TryDisplayEmotion("neutral", "", true);
        Debug.Log("Robot has woken up naturally");
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
        // Check if value has fallen to zero
        if (currentValue <= 0 && previousValue > 0)
        {
            string unfulfilledEvent = needName + "Unfulfilled";
            EmotionalResponseResult result = CalculateEmotionalResponse(unfulfilledEvent);
            emotionController.DisplayEmotionInternal(result.EmotionToDisplay, result.TriggerEvent);
            return; // Return early as this is the most severe state
        }

        // Check if value has fallen below the needed threshold
        if (currentValue <= neededThreshold && previousValue > neededThreshold)
        {
            string neededEvent = needName + "Needed";
            EmotionalResponseResult result = CalculateEmotionalResponse(neededEvent);
            emotionController.DisplayEmotionInternal(result.EmotionToDisplay, result.TriggerEvent);
        }
        // Check if value has risen above the fulfilled threshold
        else if (currentValue >= fulfilledThreshold && previousValue < fulfilledThreshold)
        { // currently broken I think
            string fulfilledEvent = needName + "Fulfilled";
            EmotionalResponseResult result = CalculateEmotionalResponse(fulfilledEvent);
            emotionController.DisplayEmotionInternal(result.EmotionToDisplay, result.TriggerEvent);
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
        if (showDebugLogs){
            Debug.Log($"Calculating response for event '{triggeredEvent}' in mood '{currentMood}'");
        }
        

        // Get base values for current mood
        if (!moodBaseValues.ContainsKey(currentMood))
        {
            if (showDebugLogs){
                Debug.LogError($"No base values found for mood: {currentMood}");
            }
            return new EmotionalResponseResult("neutral", triggeredEvent);
        }
        EmotionalResponseValues moodValues = moodBaseValues[currentMood];

        // Get base values for event
        if (!eventBaseValues.ContainsKey(triggeredEvent))
        {
            if (showDebugLogs){
                Debug.LogError($"No base values found for event: {triggeredEvent}");
            }
            return new EmotionalResponseResult("neutral", triggeredEvent);
        }
        EmotionalResponseValues eventValues = eventBaseValues[triggeredEvent];

        // Combine mood and event values with weights
        EmotionalResponseValues response = new EmotionalResponseValues
        {
            Valence = moodWeight*moodValues.Valence + eventWeight*eventValues.Valence,
            Arousal = moodWeight*moodValues.Arousal + eventWeight*eventValues.Arousal,
            Touch = eventValues.Touch,     // Keep original event values for needs
            Rest = eventValues.Rest,       // as they directly affect gauges
            Social = eventValues.Social
        };

        if (showDebugLogs){
            Debug.Log($"Combined Response: V={response.Valence:F1}, A={response.Arousal:F1} | From: Mood({currentMood}): V={moodValues.Valence:F1}, A={moodValues.Arousal:F1}, Event({triggeredEvent}): V={eventValues.Valence:F1}, A={eventValues.Arousal:F1}");
        }

        string displayString = emotionalDisplayTable(response);
        
        // Roll for response chance
        float roll = UnityEngine.Random.Range(0f, 100f);
        bool shouldDisplay = roll <= responseChance;
        Debug.Log($"Final Result: Event='{triggeredEvent}', Mood='{currentMood}', Display='{displayString}'");
        Debug.Log($"Response chance roll: {roll:F1}/{responseChance:F1} - Will {(shouldDisplay ? "display" : "skip")} emotional response");
        
        LastTriggeredEvent = triggeredEvent;
        LastDisplayString = shouldDisplay ? displayString : "neutral";
        
        return new EmotionalResponseResult(shouldDisplay ? displayString : "neutral", triggeredEvent);
    }


    /*
    Emotional Response System:

    1. classifyEmotionalState() returns the current emotional state as a semantic string based on
    valence and arousal values.

    2. The emotional response system combines:
       - Base mood values from moodBaseValues dictionary
       - Event values from eventBaseValues dictionary
       These are combined with weights to determine the final emotional response.

    3. displayTable() uses the combined valence and arousal values to determine which
    emotional display to show. Values are fuzzed for variety, and a small percentage
    affects the robot's mood, allowing for gradual mood changes over time.
    */

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
            } else if (valence < 3 && valence > -3){
                if (arousal > 3)
                { 
                    classifiedEmotion = "Energetic";
                } else if (arousal < 3 && arousal > -3){
                    classifiedEmotion = "Neutral";
                } else if (arousal < -3){
                    classifiedEmotion = "Tired";
                }
            } else if (valence < -3)
            {
                if (arousal > 3)
                { 
                    classifiedEmotion = "Annoyed";
                } else if (arousal < 3 && arousal > -3){
                    classifiedEmotion = "Sad";
                } else if (arousal < -3){
                    classifiedEmotion = "Gloomy";
                }
            }     
        //} else {
            //Debug.LogError("Long term valence is out of range");
        //}
        
        return classifiedEmotion;
    }

    private string emotionalDisplayTable(EmotionalResponseValues response)
    {
        string displayString = "Neutral";
        
        //addition to Need Gauges
        touchGauge += response.Touch;
        restGauge += response.Rest;
        socialGauge += response.Social;

        if (showDebugLogs){
            Debug.Log("Added to Gauges: Touch: " + response.Touch + 
                                   " Rest: " + response.Rest + 
                                 " Social: " + response.Social);
        }
        
        //fuzz valence and arousal values
        float fuzzedValence = response.Valence + UnityEngine.Random.Range(-1f, 1f);
        float fuzzedArousal = response.Arousal + UnityEngine.Random.Range(-1f, 1f);

        Debug.Log("Fuzzed Final Values. Valence:" + fuzzedValence + " Arousal: " + fuzzedArousal);

        //add a small percentage (5%?) of these values to the robot's long term valence and arousal.
        moodValence += fuzzedValence * 0.01f;
        moodArousal += fuzzedArousal * 0.01f;

        // Clamp mood values
        moodValence = Mathf.Clamp(moodValence, -10f, 10f);
        moodArousal = Mathf.Clamp(moodArousal, -10f, 10f);

        // Update current mood after changes
        currentMood = classifyEmotionalState(moodValence, moodArousal);

        if (showDebugLogs){
            Debug.Log("Emotion Model: response.Valence: " + response.Valence + 
                " fuzzed to " + fuzzedValence + 
                "and 5%" + fuzzedValence*0.01f + "added to mood valence");
            Debug.Log("Emotion Model: response.Arousal: " + response.Arousal + 
                  " fuzzed to " + fuzzedArousal + 
                  "and 5%" + fuzzedArousal*0.01f + "added to mood arousal");
            Debug.Log("Emotion Model: Updated current mood to: " + currentMood);
        }

        if (fuzzedArousal > 6){
            if (fuzzedValence > 6){
                displayString = "Excited";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Excited";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Surprised";
            } else if (fuzzedValence < -3 && fuzzedValence > -8){
                displayString = "Tense";
            } else if (fuzzedValence < -8){
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
                displayString = "Relaxed";
            } else if (fuzzedValence < 6 && fuzzedValence > 3){
                displayString = "Relaxed";
            } else if (fuzzedValence < 3 && fuzzedValence > 3){
                displayString = "Tired";
            } else if (fuzzedValence < -3 && fuzzedValence > -6){
                displayString = "Gloomy";
            } else if (fuzzedValence < -6){
                displayString = "Gloomy";
            }
        } else {
            Debug.LogError("Fuzzed Arousal is out of range");
        }
        if (showDebugLogs){
            Debug.Log("Emotion Model: selected display string from fuzzed values: " + displayString);
        }
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

            if (showDebugLogs){
                Debug.Log($"Saved long term values - Valence: {longTermValence}, Arousal: {longTermArousal}");
            }
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