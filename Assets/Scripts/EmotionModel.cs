using UnityEngine;
using System.Collections.Generic;

public class EmotionModel : MonoBehaviour
{
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

    // Enum for emotional states
    private enum EmotionalState
    {
        Happy,
        Sad,
        Angry,
        Neutral
    }

    // Enum for threshold events
    private enum ThresholdEvent
    {
        TouchNeeded,
        TouchFulfilled,
        RestNeeded,
        RestFulfilled,
        SocialNeeded,
        SocialFulfilled,
        HungerNeeded,
        HungerFulfilled
    }

    // The emotional response lookup table
    private Dictionary<ThresholdEvent, Dictionary<EmotionalState, EmotionalResponseValues>> responseTable;

    // Current emotional state
    private EmotionalState currentEmotionalState = EmotionalState.Happy;

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
    [SerializeField] [Range(0, 100)] private int touchGauge = 50;
    [SerializeField] [Range(0, 100)] private int restGauge = 50;
    [SerializeField] [Range(0, 100)] private int socialGauge = 50;
    [SerializeField] [Range(0, 100)] private int hungerGauge = 50;

    [Header("Need Thresholds")]
    [SerializeField] [Range(0, 100)] private int touchFulfilled = 70;
    [SerializeField] [Range(0, 100)] private int touchNeeded = 30;
    [SerializeField] [Range(0, 100)] private int restFulfilled = 70;
    [SerializeField] [Range(0, 100)] private int restNeeded = 30;
    [SerializeField] [Range(0, 100)] private int socialFulfilled = 70;
    [SerializeField] [Range(0, 100)] private int socialNeeded = 30;
    [SerializeField] [Range(0, 100)] private int hungerFulfilled = 70;
    [SerializeField] [Range(0, 100)] private int hungerNeeded = 30;

    [Header("Decay Settings")]
    [SerializeField] private bool useAcceleratedTesting = false;
    [SerializeField] private float testingMultiplier = 180f; // Speed up time for testing

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

    private void Awake()
    {
        InitializeResponseTable();
    }

    private void InitializeResponseTable()
    {
        responseTable = new Dictionary<ThresholdEvent, Dictionary<EmotionalState, EmotionalResponseValues>>();

        // Initialize the table structure
        foreach (ThresholdEvent threshold in System.Enum.GetValues(typeof(ThresholdEvent)))
        {
            responseTable[threshold] = new Dictionary<EmotionalState, EmotionalResponseValues>();
            
            foreach (EmotionalState state in System.Enum.GetValues(typeof(EmotionalState)))
            {
                // Example values - you should replace these with your actual desired values
                responseTable[threshold][state] = new EmotionalResponseValues
                {
                    Valence = 0.5f,
                    Arousal = 0.5f,
                    Touch = 0.5f,
                    Rest = 0.5f,
                    Social = 0.5f
                };
            }
        }

        // Set specific values for different combinations
        // Example for TouchNeeded when Happy:
        responseTable[ThresholdEvent.TouchNeeded][EmotionalState.Happy] = new EmotionalResponseValues
        {
            Valence = -5f, Arousal = 0f, Touch = 0f, Rest = 0f, Social = 0f
        };

        // Add more specific values here...
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
        float timeMultiplier = useAcceleratedTesting ? testingMultiplier : 1f;
        float deltaTime = Time.deltaTime * timeMultiplier;

        // Store previous values to detect threshold crossings
        int prevTouch = touchGauge;
        int prevRest = restGauge;
        int prevSocial = socialGauge;
        int prevHunger = hungerGauge;

        // Accumulate decay values
        touchDecayAccumulator += touchDecayRate * deltaTime;
        restDecayAccumulator += restDecayRate * deltaTime;
        socialDecayAccumulator += socialDecayRate * deltaTime;
        hungerDecayAccumulator += hungerDecayRate * deltaTime;

        // Only update gauges when accumulated decay is >= 1
        if (touchDecayAccumulator >= 1f)
        {
            int decayAmount = Mathf.FloorToInt(touchDecayAccumulator);
            touchGauge = Mathf.Max(0, touchGauge - decayAmount);
            touchDecayAccumulator -= decayAmount;
        }

        if (restDecayAccumulator >= 1f)
        {
            int decayAmount = Mathf.FloorToInt(restDecayAccumulator);
            restGauge = Mathf.Max(0, restGauge - decayAmount);
            restDecayAccumulator -= decayAmount;
        }

        if (socialDecayAccumulator >= 1f)
        {
            int decayAmount = Mathf.FloorToInt(socialDecayAccumulator);
            socialGauge = Mathf.Max(0, socialGauge - decayAmount);
            socialDecayAccumulator -= decayAmount;
        }

        if (hungerDecayAccumulator >= 1f)
        {
            int decayAmount = Mathf.FloorToInt(hungerDecayAccumulator);
            hungerGauge = Mathf.Max(0, hungerGauge - decayAmount);
            hungerDecayAccumulator -= decayAmount;
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
            LogGaugeValues();
            // Also log accumulators for debugging
            Debug.Log($"Decay Accumulators:\n" +
                     $"Touch: {touchDecayAccumulator:F3}\n" +
                     $"Rest: {restDecayAccumulator:F3}\n" +
                     $"Social: {socialDecayAccumulator:F3}\n" +
                     $"Hunger: {hungerDecayAccumulator:F3}");
            gaugeLogTimer = 0f;
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

    private void CheckNeedThreshold(string needName, int currentValue, int previousValue, int neededThreshold, int fulfilledThreshold)
    {
        ThresholdEvent? triggeredEvent = null;

        // Check if crossed below Needed threshold
        if (previousValue >= neededThreshold && currentValue < neededThreshold)
        {
            Debug.Log($"{needName} need is now below threshold! Value: {currentValue} < {neededThreshold}");
            
            // Determine which threshold event was triggered
            switch (needName)
            {
                case "Touch": triggeredEvent = ThresholdEvent.TouchNeeded; break;
                case "Rest": triggeredEvent = ThresholdEvent.RestNeeded; break;
                case "Social": triggeredEvent = ThresholdEvent.SocialNeeded; break;
                case "Hunger": triggeredEvent = ThresholdEvent.HungerNeeded; break;
            }
        }
        // Check if crossed above Fulfilled threshold
        else if (previousValue <= fulfilledThreshold && currentValue > fulfilledThreshold)
        {
            Debug.Log($"{needName} need is now fulfilled! Value: {currentValue} > {fulfilledThreshold}");
            
            // Determine which threshold event was triggered
            switch (needName)
            {
                case "Touch": triggeredEvent = ThresholdEvent.TouchFulfilled; break;
                case "Rest": triggeredEvent = ThresholdEvent.RestFulfilled; break;
                case "Social": triggeredEvent = ThresholdEvent.SocialFulfilled; break;
                case "Hunger": triggeredEvent = ThresholdEvent.HungerFulfilled; break;
            }
        }

        // If a threshold was crossed, look up the response in the table
        if (triggeredEvent.HasValue)
        {
            EmotionalResponseValues response = responseTable[triggeredEvent.Value][currentEmotionalState];
            Debug.Log($"Emotional Response for {needName} ({triggeredEvent}) in {currentEmotionalState} state:\n{response}");
        }
    }

    // Method to calculate the emotional response the robot will eventually display
    public Dictionary<string, string> CalculateEmotionalResponse()
    {
        emotionalResponse["face"] = "happy";
        emotionalResponse["light"] = null;
        emotionalResponse["sound"] = "happy";
        emotionalResponse["thought"] = "heart";
        emotionalResponse["tail"] = "happy";

        string debugOutput = "EmotionModel: Emotional Response:\n";
        foreach (var response in emotionalResponse){debugOutput += $"{response.Key}: {response.Value}\n";}
        Debug.Log(debugOutput);
        return emotionalResponse;
    }
} 