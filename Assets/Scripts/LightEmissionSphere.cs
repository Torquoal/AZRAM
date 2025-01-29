using UnityEngine;
using System.Collections;
using Oculus.Interaction;  // For basic interaction components
using Oculus.Interaction.HandGrab;  // For HandGrabInteractable

public class LightEmissionSphere : MonoBehaviour
{
    [Header("Emission Settings")]
    [SerializeField] private Color emissionColor = Color.yellow;
    [SerializeField] [Range(0f, 1f)] private float intensity = 1f;
    [SerializeField] [Range(1f, 10f)] private float sphereScale = 7f;
    [SerializeField] [Range(0f, 1f)] private float transparency = 1f;
    [SerializeField] private Material emissiveSphereMaterial;

    [Header("Size Fluctuation")]
    [SerializeField] private bool enableSizeFluctuation = false;
    [SerializeField] [Range(0f, 0.15f)] private float fluctuationAmount = 0.15f;
    [SerializeField] [Range(0.1f, 2f)] private float fluctuationSpeed = 0.3f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float defaultTransparency = 1f;

    private bool isVisible = false;
    private Coroutine fadeCoroutine;

    // Add preset colors for easy access
    public static readonly Color PresetRed = new Color(0.45f, 0.03f, 0.0f);
    public static readonly Color PresetBlue = new Color(0.01f, 0.0f, 0.34f);
    public static readonly Color PresetGreen = new Color(0.2f, 1f, 0.4f);
    public static readonly Color PresetYellow = new Color(0.7f, 0.6f, 0.0f);
    public static readonly Color PresetPurple = new Color(0.8f, 0.2f, 1f);
    public static readonly Color PresetPink = new Color(0.6f, 0.0f, 0.5f);
    public static readonly Color PresetGrey = new Color(0.5f, 0.5f, 0.5f);

    // Public property to access current color
    public Color CurrentColor => emissionColor;

    private Material emissionMaterial;
    private GameObject emissionSphere;
    
    void Start()
    {
        // Create the emission sphere
        emissionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        emissionSphere.transform.SetParent(null);
        emissionSphere.transform.position = transform.position;
        
        // Get the renderer and its bounds
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("No renderer found on object!");
            return;
        }
        
        // Set uniform scale
        float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        emissionSphere.transform.localScale = Vector3.one * baseScale * sphereScale;
        
        // Create material instance from the assigned material
        if (emissiveSphereMaterial == null)
        {
            Debug.LogError("Please assign an EmissiveSphere material in the inspector!");
            return;
        }
        
        // Create a new instance of the material to ensure it's unique
        emissionMaterial = new Material(emissiveSphereMaterial);
        
        // Apply material
        Renderer sphereRenderer = emissionSphere.GetComponent<Renderer>();
        sphereRenderer.material = emissionMaterial;
        
        // Initialize with zero transparency
        transparency = 0f;
        UpdateEmissionProperties();
        
        // Remove interaction capabilities
        Destroy(emissionSphere.GetComponent<Collider>());
        emissionSphere.layer = LayerMask.NameToLayer("Default");
        
        // Make the emission sphere follow the object's position
        StartCoroutine(FollowTarget());

        if (enableSizeFluctuation)
        {
            StartCoroutine(SizeFluctuationRoutine());
        }
    }
    
    void OnValidate()
    {
        // Only update if we're in play mode and the sphere exists
        if (Application.isPlaying && emissionMaterial != null)
        {
            UpdateEmissionProperties();
            if (emissionSphere != null)
            {
                // Only update scale directly if fluctuation is disabled
                if (!enableSizeFluctuation)
                {
                    float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                    emissionSphere.transform.localScale = Vector3.one * baseScale * sphereScale;
                }

                // Handle fluctuation toggle in editor
                if (enableSizeFluctuation && !IsInvoking("SizeFluctuationRoutine"))
                {
                    StopCoroutine("SizeFluctuationRoutine");
                    StartCoroutine(SizeFluctuationRoutine());
                    Debug.Log("Started size fluctuation");
                }
                else if (!enableSizeFluctuation)
                {
                    StopCoroutine("SizeFluctuationRoutine");
                    Debug.Log("Stopped size fluctuation");
                }
            }
        }
    }
    
    void UpdateEmissionProperties()
    {
        if (!Application.isPlaying) return; // Don't update in edit mode
        
        if (emissionMaterial != null)
        {
            // Make emission color more intense
            Color brightColor = new Color(
                emissionColor.r * 2f,
                emissionColor.g * 2f,
                emissionColor.b * 2f,
                1f
            );
            
            //Debug.Log($"Setting emission color: {brightColor}, Intensity: {intensity * 20f}, Transparency: {transparency}");
            
            emissionMaterial.SetColor("_EmissionColor", brightColor);
            emissionMaterial.SetFloat("_EmissionIntensity", intensity * 20f);
            emissionMaterial.SetFloat("_Transparency", transparency);
        }
    }
    
    void OnDestroy()
    {
        if (emissionSphere != null)
        {
            Destroy(emissionSphere);
        }
    }
    
    public void SetEmissionColor(Color newColor)
    {
        emissionColor = newColor;
        UpdateEmissionProperties();
    }
    
    public void SetIntensity(float newIntensity)
    {
        intensity = Mathf.Clamp01(newIntensity);
        UpdateEmissionProperties();
    }
    
    public void SetScale(float scale)
    {
        sphereScale = Mathf.Clamp(scale, 1f, 10f);
        if (emissionSphere != null && !enableSizeFluctuation)
        {
            float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            emissionSphere.transform.localScale = Vector3.one * baseScale * sphereScale;
        }
    }
    
    public void SetTransparency(float alpha)
    {
        transparency = Mathf.Clamp01(alpha);
        UpdateEmissionProperties();
    }

    // Easy access methods for common colors
    public void SetColorRed() => SetEmissionColor(PresetRed);
    [ContextMenu("Set Blue")]
    public void SetColorBlue() => SetEmissionColor(PresetBlue);
    public void SetColorGreen() => SetEmissionColor(PresetGreen);
    public void SetColorYellow() => SetEmissionColor(PresetYellow);
    public void SetColorPurple() => SetEmissionColor(PresetPurple);
    public void SetColorPink() => SetEmissionColor(PresetPink);
    public void SetColorGrey() => SetEmissionColor(PresetGrey);

    // Method to smoothly transition to a new color
    public void TransitionToColor(Color targetColor, float duration)
    {
        StartCoroutine(ColorTransitionRoutine(targetColor, duration));
    }

    private IEnumerator ColorTransitionRoutine(Color targetColor, float duration)
    {
        Color startColor = emissionColor;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Smooth transition
            t = t * t * (3f - 2f * t); // Smoothstep interpolation
            
            Color newColor = Color.Lerp(startColor, targetColor, t);
            SetEmissionColor(newColor);
            
            yield return null;
        }

        SetEmissionColor(targetColor);
    }

    // Method to pulse the intensity
    public void PulseIntensity(float minIntensity, float maxIntensity, float duration)
    {
        StartCoroutine(PulseIntensityRoutine(minIntensity, maxIntensity, duration));
    }

    private IEnumerator PulseIntensityRoutine(float minIntensity, float maxIntensity, float duration)
    {
        float startTime = Time.time;
        
        while (true)
        {
            float t = (Time.time - startTime) / duration;
            float pulse = Mathf.PingPong(t, 1f);
            
            // Smooth the pulse
            pulse = pulse * pulse * (3f - 2f * pulse);
            
            float newIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
            SetIntensity(newIntensity);
            
            yield return null;
        }
    }

    // Stop any ongoing effects
    public void StopEffects()
    {
        StopAllCoroutines();
        if (enableSizeFluctuation)
        {
            StartCoroutine(SizeFluctuationRoutine());
        }
    }

    // Example usage method for other scripts
    public void SetEmissionWithIntensity(Color color, float newIntensity)
    {
        SetEmissionColor(color);
        SetIntensity(newIntensity);
    }

    private IEnumerator SizeFluctuationRoutine()
    {
        Debug.Log("Size fluctuation routine started");
        float targetScale = sphereScale;
        float currentScale = sphereScale;
        float timeOffset = Random.Range(0f, 100f);

        while (true)
        {
            float noise = Mathf.PerlinNoise(timeOffset, Time.time * fluctuationSpeed);
            targetScale = sphereScale * (1f + (noise * 2f - 1f) * fluctuationAmount);
            currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * 5f);

            if (emissionSphere != null)
            {
                float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                Vector3 newScale = Vector3.one * baseScale * currentScale;
                emissionSphere.transform.localScale = newScale;
                
                if (Time.frameCount % 60 == 0)
                {
                    //Debug.Log($"Current scale: {currentScale}, Target: {targetScale}");
                }
            }

            yield return null;
        }
    }

    // Method to toggle size fluctuation
    public void SetSizeFluctuation(bool enable)
    {
        enableSizeFluctuation = enable;
        if (enable)
        {
            StartCoroutine(SizeFluctuationRoutine());
        }
        else
        {
            StopCoroutine(SizeFluctuationRoutine());
            // Reset to base scale
            float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            emissionSphere.transform.localScale = Vector3.one * baseScale * sphereScale;
        }
    }

    // Method to adjust fluctuation amount at runtime
    public void SetFluctuationAmount(float amount)
    {
        fluctuationAmount = Mathf.Clamp(amount, 0f, 0.15f);
    }

    // Method to adjust fluctuation speed at runtime
    public void SetFluctuationSpeed(float speed)
    {
        fluctuationSpeed = Mathf.Clamp(speed, 0.1f, 2f);
    }

    System.Collections.IEnumerator FollowTarget()
    {
        while (true)
        {
            if (emissionSphere != null)
            {
                // Update position and maintain uniform scale
                emissionSphere.transform.position = transform.position;
                float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                if (!enableSizeFluctuation)
                {
                    emissionSphere.transform.localScale = Vector3.one * baseScale * sphereScale;
                }
            }
            yield return null;
        }
    }

    // Add this new method to ensure the sphere stays non-interactable
    void LateUpdate()
    {
        if (emissionSphere != null)
        {
            // Ensure any collider that might get added is immediately removed
            var collider = emissionSphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            
            // Ensure any hand interaction components that might get added are removed
            var handGrab = emissionSphere.GetComponent<HandGrabInteractable>();
            if (handGrab != null)
            {
                Destroy(handGrab);
            }
        }
    }

    void OnTransformParentChanged()
    {
        // Re-apply material properties when parent changes
        if (emissionMaterial != null && emissionSphere != null)
        {
            Debug.Log("Parent changed - Reapplying material properties");
            UpdateEmissionProperties();
        }
    }

    public void Show()
    {
        if (!isVisible)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLight(0f, defaultTransparency, fadeInDuration));
            isVisible = true;
        }
    }

    public void Hide()
    {
        if (isVisible)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLight(transparency, 0f, fadeOutDuration));
            isVisible = false;
        }
    }

    private IEnumerator FadeLight(float startTransparency, float endTransparency, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentTransparency = Mathf.Lerp(startTransparency, endTransparency, elapsedTime / duration);
            SetTransparency(currentTransparency);
            yield return null;
        }
        
        SetTransparency(endTransparency);
    }

    // Add emotion to color mapping
    public void SetEmotionColor(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "happy":
                SetColorPink();
                SetIntensity(1f);
                SetTransparency(1f);
                break;
            case "sad":
                SetColorBlue();
                SetIntensity(1f);
                SetTransparency(1f);
                break;
            case "surprised":
                SetColorYellow();
                SetIntensity(1f);
                SetTransparency(1f);
                break;
            case "scared":
                SetColorGrey();
                SetIntensity(1f);
                SetTransparency(1f);
                break;
            case "angry":
                SetColorRed();
                SetIntensity(1f);
                SetTransparency(1f);
                break;
            default:
                Debug.LogWarning($"Unknown emotion: {emotion}");
                break;
        }
    }
} 