using UnityEngine;
using System.Collections;

public class LightEmissionSphere : MonoBehaviour
{
    [Header("Emission Settings")]
    [SerializeField] private Color emissionColor = Color.yellow;
    [SerializeField] [Range(0f, 1f)] private float intensity = 0.5f;
    [SerializeField] [Range(1f, 10f)] private float sphereScale = 3f;
    [SerializeField] [Range(0f, 1f)] private float transparency = 0.2f;
    [SerializeField] private Material emissiveSphereMaterial;

    [Header("Size Fluctuation")]
    [SerializeField] private bool enableSizeFluctuation = false;
    [SerializeField] [Range(0f, 0.15f)] private float fluctuationAmount = 0.05f;
    [SerializeField] [Range(0.1f, 2f)] private float fluctuationSpeed = 0.5f;

    // Add preset colors for easy access
    public static readonly Color PresetRed = new Color(1f, 0.2f, 0.2f);
    public static readonly Color PresetBlue = new Color(0.2f, 0.4f, 1f);
    public static readonly Color PresetGreen = new Color(0.2f, 1f, 0.4f);
    public static readonly Color PresetYellow = new Color(1f, 0.9f, 0.2f);
    public static readonly Color PresetPurple = new Color(0.8f, 0.2f, 1f);
    public static readonly Color PresetPink = new Color(1f, 0.4f, 0.7f);
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
        emissionMaterial = new Material(emissiveSphereMaterial);
        
        // Apply material
        Renderer sphereRenderer = emissionSphere.GetComponent<Renderer>();
        sphereRenderer.material = emissionMaterial;
        
        // Initial update of properties
        UpdateEmissionProperties();
        
        // Remove collider
        Destroy(emissionSphere.GetComponent<Collider>());
        
        // Make the emission sphere follow the object's position
        StartCoroutine(FollowTarget());

        // Start size fluctuation if enabled
        if (enableSizeFluctuation)
        {
            StartCoroutine(SizeFluctuationRoutine());
        }
    }
    
    System.Collections.IEnumerator FollowTarget()
    {
        while (true)
        {
            if (emissionSphere != null)
            {
                emissionSphere.transform.position = transform.position;
            }
            yield return null;
        }
    }
    
    void OnValidate()
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
    
    void UpdateEmissionProperties()
    {
        if (emissionMaterial != null)
        {
            // Make emission color more intense
            Color brightColor = new Color(
                emissionColor.r * 2f,
                emissionColor.g * 2f,
                emissionColor.b * 2f,
                1f
            );
            
            emissionMaterial.SetColor("_EmissionColor", brightColor);
            emissionMaterial.SetFloat("_EmissionIntensity", intensity * 20f); // Increased multiplier
            emissionMaterial.SetFloat("_Transparency", transparency); // Direct transparency control
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
        StartCoroutine(FollowTarget());
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
        float baseScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
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
                Vector3 newScale = Vector3.one * baseScale * currentScale;
                emissionSphere.transform.localScale = newScale;
                
                // Debug log every few seconds
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Current scale: {currentScale}, Target: {targetScale}");
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
} 