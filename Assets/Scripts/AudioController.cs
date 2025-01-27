using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource qooboSpeaker;
    [SerializeField] private AudioClip[] beepSounds;  // Drag your beep sound files here

    public void PlaySound(string emotion)
    {
        int index = 0;
        string[] emotionArray = {"happy", "sad", "scared", "surprised", "angry", "peep"};

        for (int i = 0; i < emotionArray.Length; i++)
        {
            if (emotionArray[i] == emotion)
            {
                index = i;
            }
        }   

        if (qooboSpeaker != null && beepSounds != null && index < beepSounds.Length && beepSounds[index] != null)
        {
            qooboSpeaker.clip = beepSounds[index];
            qooboSpeaker.Play();
        }
    }

    // Convenience methods
    public void PlayHappySound() => PlaySound("happy");
    public void PlaySadSound() => PlaySound("sad");
    public void PlayScaredSound() => PlaySound("scared");
    public void PlaySurprisedSound() => PlaySound("surprised");
    public void PlayAngrySound() => PlaySound("angry");
    public void PlayPeepSound() => PlaySound("peep");
} 