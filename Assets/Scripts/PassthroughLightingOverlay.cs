using UnityEngine;

public class PassthroughLightingOverlay : MonoBehaviour
{
    [SerializeField] private Color overlayColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField] private float intensity = 0.5f;
    
    private Material overlayMaterial;
    
    void Start()
    {
        // Create a transparent material for the overlay
        overlayMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        overlayMaterial.SetColor("_BaseColor", overlayColor);
        overlayMaterial.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        overlayMaterial.SetFloat("_Blend", 0);   // 0 = Alpha, 1 = Premultiply
        
        // Apply material to the mesh renderer
        GetComponent<MeshRenderer>().material = overlayMaterial;
    }
    
    public void UpdateIntensity(float newIntensity)
    {
        Color newColor = overlayColor;
        newColor.a *= newIntensity;
        overlayMaterial.SetColor("_BaseColor", newColor);
    }
} 