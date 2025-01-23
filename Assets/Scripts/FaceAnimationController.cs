using UnityEngine;
using System.Collections;
using System.Linq;

public class FaceAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string neutralLoopPath = "NeutralLoop";  // Path in Resources folder
    [SerializeField] private float frameRate = 24f;      // Animation frame rate

    // Future emotion animation paths
    /*
    [Header("Emotion Animation Paths")]
    [SerializeField] private string happyPath = "HappyAnimation";
    [SerializeField] private string sadPath = "SadAnimation";
    [SerializeField] private string angryPath = "AngryAnimation";
    [SerializeField] private string scaredPath = "ScaredAnimation";
    [SerializeField] private string surprisedPath = "SurprisedAnimation";
    */

    [Header("Animation Behavior")]
    [SerializeField] private bool loopNeutralAnimation = true;
    /*
    [SerializeField] private bool loopHappyAnimation = false;
    [SerializeField] private bool loopSadAnimation = false;
    [SerializeField] private bool loopAngryAnimation = false;
    [SerializeField] private bool loopScaredAnimation = false;
    [SerializeField] private bool loopSurprisedAnimation = false;
    */
    
    private Material animatedMaterial;
    private float frameInterval;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private Coroutine animationCoroutine;
    private Texture2D[] neutralFrames;
    /*
    private Texture2D[] happyFrames;
    private Texture2D[] sadFrames;
    private Texture2D[] angryFrames;
    private Texture2D[] scaredFrames;
    private Texture2D[] surprisedFrames;
    */

    private void Start()
    {
        frameInterval = 1f / frameRate;
        LoadAnimationFrames();
        
        // Create a new material instance
        animatedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        SetupMaterial();
    }

    private void LoadAnimationFrames()
    {
        // Load neutral animation frames
        LoadFramesForEmotion(neutralLoopPath, ref neutralFrames);

        // Future emotion frame loading
        /*
        LoadFramesForEmotion(happyPath, ref happyFrames);
        LoadFramesForEmotion(sadPath, ref sadFrames);
        LoadFramesForEmotion(angryPath, ref angryFrames);
        LoadFramesForEmotion(scaredPath, ref scaredFrames);
        LoadFramesForEmotion(surprisedPath, ref surprisedFrames);
        */
    }

    private void LoadFramesForEmotion(string path, ref Texture2D[] frames)
    {
        // Load all textures from the Resources folder
        Object[] loadedObjects = Resources.LoadAll(path, typeof(Texture2D));
        
        // Convert to Texture2D array and sort by name to ensure correct order
        frames = loadedObjects
            .Cast<Texture2D>()
            .OrderBy(tex => tex.name)
            .ToArray();

        Debug.Log($"Loaded {frames.Length} animation frames from {path}");
        
        if (frames.Length == 0)
        {
            Debug.LogWarning($"No frames found in Resources/{path}");
        }
    }

    private void SetupMaterial()
    {
        if (animatedMaterial != null)
        {
            // Configure the material for transparency
            animatedMaterial.EnableKeyword("_ALPHABLEND_ON");
            animatedMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            
            // Set the material to be transparent
            animatedMaterial.SetFloat("_Surface", 1f); // 1 = Transparent
            animatedMaterial.SetFloat("_Mode", 3); // Transparent mode
            
            // Set up blending
            animatedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            animatedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            animatedMaterial.SetInt("_ZWrite", 0);
            animatedMaterial.renderQueue = 3000;
            
            // Ensure alpha channel is respected
            animatedMaterial.SetOverrideTag("RenderType", "Transparent");
            animatedMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            
            // Set color to white with full alpha to not tint the texture
            animatedMaterial.color = new Color(1, 1, 1, 1);
            
            // Set initial frame
            if (neutralFrames != null && neutralFrames.Length > 0)
            {
                animatedMaterial.mainTexture = neutralFrames[0];
            }
        }
    }

    public void StartAnimation()
    {
        StartAnimation("neutral");
    }

    public void StartAnimation(string emotion)
    {
        Texture2D[] targetFrames = null;
        bool shouldLoop = false;

        // Select the appropriate frames and loop setting based on emotion
        switch (emotion.ToLower())
        {
            case "neutral":
                targetFrames = neutralFrames;
                shouldLoop = loopNeutralAnimation;
                break;
            /*
            case "happy":
                targetFrames = happyFrames;
                shouldLoop = loopHappyAnimation;
                break;
            case "sad":
                targetFrames = sadFrames;
                shouldLoop = loopSadAnimation;
                break;
            case "angry":
                targetFrames = angryFrames;
                shouldLoop = loopAngryAnimation;
                break;
            case "scared":
                targetFrames = scaredFrames;
                shouldLoop = loopScaredAnimation;
                break;
            case "surprised":
                targetFrames = surprisedFrames;
                shouldLoop = loopSurprisedAnimation;
                break;
            */
            default:
                Debug.LogWarning($"Animation for emotion {emotion} not implemented yet");
                return;
        }

        if (targetFrames == null || targetFrames.Length == 0)
        {
            Debug.LogWarning($"No frames loaded for {emotion} animation!");
            return;
        }

        if (!isPlaying)
        {
            isPlaying = true;
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateSequence(targetFrames, shouldLoop));
        }
    }

    public void StopAnimation()
    {
        isPlaying = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator AnimateSequence(Texture2D[] frames, bool loop)
    {
        currentFrame = 0;
        
        do
        {
            animatedMaterial.mainTexture = frames[currentFrame];
            yield return new WaitForSeconds(frameInterval);
            
            currentFrame++;
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    isPlaying = false;
                    break;
                }
            }
        }
        while (isPlaying);
    }

    // Future animation control methods
    /*
    public void PlayHappyAnimation() => StartAnimation("happy");
    public void PlaySadAnimation() => StartAnimation("sad");
    public void PlayAngryAnimation() => StartAnimation("angry");
    public void PlayScaredAnimation() => StartAnimation("scared");
    public void PlaySurprisedAnimation() => StartAnimation("surprised");

    // Optional: Event callback for when non-looping animations finish
    public delegate void AnimationCompleteHandler(string emotion);
    public event AnimationCompleteHandler OnAnimationComplete;
    */

    public Material GetAnimatedMaterial()
    {
        return animatedMaterial;
    }

    public void SetAlpha(float alpha)
    {
        if (animatedMaterial != null)
        {
            Color color = animatedMaterial.color;
            color.a = alpha;
            animatedMaterial.color = color;
        }
    }

    private void OnDestroy()
    {
        if (animatedMaterial != null)
        {
            Destroy(animatedMaterial);
        }
    }
} 