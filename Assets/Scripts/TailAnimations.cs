using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailAnimations : MonoBehaviour
{

    private Animator tailAnimator;
    private SceneController sceneController;
    // Start is called before the first frame update
    void Start()
    {
        tailAnimator = GetComponent<Animator>();
        sceneController = FindObjectOfType<SceneController>();
    }

    public void PlayTailAnimation(string animationName)
    {

        if (tailAnimator == null){
             Debug.LogError("TailAnimator not found");
             return;    
        }

        switch (animationName.ToLower())
        {
            case "happy":
                PlayHorizontalTail();
                break;
            case "angry":
                PlayBristleTail();
                break;
            case "sad":
                PlayDroopTail();
                break;
            case "scared":
                PlaySleepyTail();
                break;
            case "surprised":
                PlaySurprisedTail();
                break;
            case "wakeup":
                PlayWakeupTail();
                break;
            default:
                Debug.LogWarning($"Unknown animation: {animationName}");
                break;
        }
    }

    [ContextMenu("Play Horizontal Tail")]
    private void PlayHorizontalTail()
    {
        tailAnimator.SetTrigger("TrHorizontalWag");
    }       

    [ContextMenu("Play Bristle Tail")]
    private void PlayBristleTail()
    {
        tailAnimator.SetTrigger("TrBristle");
    }

    [ContextMenu("Play Droop Tail")]
    private void PlayDroopTail()
    {
        tailAnimator.SetTrigger("TrDroop");
    }

    [ContextMenu("Play Sleepy Tail")]
    private void PlaySleepyTail()
    {
        tailAnimator.SetTrigger("TrSleepy");
    }   

    [ContextMenu("Play Surprised Tail")]
    private void PlaySurprisedTail()
    {
        tailAnimator.SetTrigger("TrSurprised");
    }

    [ContextMenu("Play Wakeup Tail")]
    private void PlayWakeupTail()
    {
        tailAnimator.SetTrigger("TrWakeup");
    }        

    [ContextMenu("Stop Tail Animation")]
    public void StopTailAnimation()
    {
        tailAnimator.SetTrigger("TrStopAnimation");
    }      



}
