using UnityEngine;

public class ObjectGlowOutline : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] [Range(0.0f, 0.5f)] private float outlineWidth = 0.1f;
    [SerializeField] [Range(0.0f, 1.0f)] private float outlineIntensity = 1.0f;
    
    private Material outlineMaterial;
    private Renderer meshRenderer;
    private Material originalMaterial;
    
    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        originalMaterial = meshRenderer.sharedMaterial;
        
        // Set up the outline material using our custom shader
        outlineMaterial = new Material(Shader.Find("Custom/GlowOutline"));
        
        // Copy properties from original material
        outlineMaterial.SetTexture("_MainTex", originalMaterial.mainTexture);
        outlineMaterial.SetColor("_BaseColor", originalMaterial.GetColor("_BaseColor"));
        
        // Set outline properties
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        outlineMaterial.SetFloat("_OutlineIntensity", outlineIntensity);
        
        // Set up materials array with both materials
        Material[] materials = new Material[] { originalMaterial, outlineMaterial };
        meshRenderer.materials = materials;
    }
    
    void OnValidate()
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
            outlineMaterial.SetFloat("_OutlineIntensity", outlineIntensity);
        }
    }
    
    public void SetOutlineColor(Color newColor)
    {
        outlineColor = newColor;
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", newColor);
        }
    }
    
    public void SetOutlineWidth(float width)
    {
        outlineWidth = Mathf.Clamp(width, 0f, 0.5f);
        if (outlineMaterial != null)
        {
            outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }
    }
    
    public void SetOutlineIntensity(float intensity)
    {
        outlineIntensity = Mathf.Clamp(intensity, 0f, 1f);
        if (outlineMaterial != null)
        {
            outlineMaterial.SetFloat("_OutlineIntensity", outlineIntensity);
        }
    }
} 