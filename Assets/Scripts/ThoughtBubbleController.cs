using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ThoughtBubbleController : MonoBehaviour
{
    [Header("Bubble Components")]
    [SerializeField] private GameObject mainBubble;
    [SerializeField] private GameObject smallBubble1;
    [SerializeField] private GameObject smallBubble2;
    [SerializeField] private Image thoughtImage;
    [SerializeField] private CanvasGroup mainBubbleGroup;
    [SerializeField] private CanvasGroup smallBubble1Group;
    [SerializeField] private CanvasGroup smallBubble2Group;

    [Header("Animation Settings")]
    [SerializeField] private float bubbleAppearDelay = 0.3f;
    [SerializeField] private float bubbleFadeDuration = 0.5f;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatAmount = 0.1f;
    
    [Header("Thought Images")]
    [SerializeField] private Sprite[] thoughtSprites; // Array of different thought images/emojis

    private Transform cameraRig;
    private Vector3[] initialPositions;
    private bool isVisible = false;
    private Coroutine floatingCoroutine;

    void Start()
    {
        // Find the camera rig - adjust the path based on your hierarchy
        cameraRig = GameObject.Find("Camera Rig").transform;
        
        // Store initial positions for floating animation
        initialPositions = new Vector3[3];
        initialPositions[0] = mainBubble.transform.localPosition;
        initialPositions[1] = smallBubble1.transform.localPosition;
        initialPositions[2] = smallBubble2.transform.localPosition;

        // Initially hide all bubbles
        SetBubblesVisible(false);
    }

    void LateUpdate()
    {
        if (cameraRig != null)
        {
            // Make the canvas face the camera
            transform.LookAt(cameraRig.position);
            transform.Rotate(0, 180, 0); // Correct the rotation
        }
    }

    public void ShowThought(string emotion)
    {
        // Find the appropriate sprite based on emotion
        Sprite thoughtSprite = null;
        for (int i = 0; i < thoughtSprites.Length; i++)
        {
            if (thoughtSprites[i].name.ToLower().Contains(emotion.ToLower()))
            {
                thoughtSprite = thoughtSprites[i];
                break;
            }
        }

        if (thoughtSprite != null)
        {
            thoughtImage.sprite = thoughtSprite;
            ShowBubbleSequence();
        }
    }

    private void ShowBubbleSequence()
    {
        if (!isVisible)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateBubbleSequence());
            if (floatingCoroutine != null)
                StopCoroutine(floatingCoroutine);
            floatingCoroutine = StartCoroutine(FloatBubbles());
            isVisible = true;
        }
    }

    public void HideThought()
    {
        if (isVisible)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutBubbles());
            isVisible = false;
        }
    }

    private IEnumerator AnimateBubbleSequence()
    {
        // Reset all bubbles
        SetBubblesVisible(false);

        // Animate small bubble 1
        smallBubble1.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(smallBubble1Group, 0f, 1f, bubbleFadeDuration));
        yield return new WaitForSeconds(bubbleAppearDelay);

        // Animate small bubble 2
        smallBubble2.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(smallBubble2Group, 0f, 1f, bubbleFadeDuration));
        yield return new WaitForSeconds(bubbleAppearDelay);

        // Animate main bubble
        mainBubble.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(mainBubbleGroup, 0f, 1f, bubbleFadeDuration));
    }

    private IEnumerator FadeOutBubbles()
    {
        // Fade out all bubbles simultaneously
        StartCoroutine(FadeCanvasGroup(mainBubbleGroup, 1f, 0f, bubbleFadeDuration));
        StartCoroutine(FadeCanvasGroup(smallBubble1Group, 1f, 0f, bubbleFadeDuration));
        StartCoroutine(FadeCanvasGroup(smallBubble2Group, 1f, 0f, bubbleFadeDuration));

        yield return new WaitForSeconds(bubbleFadeDuration);
        SetBubblesVisible(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        group.alpha = end;
    }

    private IEnumerator FloatBubbles()
    {
        float[] timeOffsets = { 0f, 0.33f, 0.66f }; // Different starting points for each bubble
        
        while (true)
        {
            float time = Time.time * floatSpeed;
            
            // Float main bubble
            mainBubble.transform.localPosition = initialPositions[0] + new Vector3(
                Mathf.Sin(time + timeOffsets[0]) * floatAmount,
                Mathf.Cos(time + timeOffsets[0]) * floatAmount,
                0f
            );
            
            // Float small bubbles
            smallBubble1.transform.localPosition = initialPositions[1] + new Vector3(
                Mathf.Sin(time + timeOffsets[1]) * floatAmount * 0.7f,
                Mathf.Cos(time + timeOffsets[1]) * floatAmount * 0.7f,
                0f
            );
            
            smallBubble2.transform.localPosition = initialPositions[2] + new Vector3(
                Mathf.Sin(time + timeOffsets[2]) * floatAmount * 0.5f,
                Mathf.Cos(time + timeOffsets[2]) * floatAmount * 0.5f,
                0f
            );
            
            yield return null;
        }
    }

    private void SetBubblesVisible(bool visible)
    {
        mainBubble.SetActive(visible);
        smallBubble1.SetActive(visible);
        smallBubble2.SetActive(visible);
    }

    // Convenience methods for showing specific thoughts
    public void ShowHappyThought() => ShowThought("happy");
    public void ShowSadThought() => ShowThought("sad");
    public void ShowAngryThought() => ShowThought("angry");
    public void ShowSurprisedThought() => ShowThought("surprised");
    public void ShowScaredThought() => ShowThought("scared");
} 