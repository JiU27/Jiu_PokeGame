using UnityEngine;
using System;
using System.Collections.Generic;

public class BackgroundMusicManager : MonoBehaviour
{
    [Serializable]
    public class LandTypeMusic
    {
        public LandType landType;
        public AudioClip music;
    }

    public AudioClip StartMusic;

    public List<LandTypeMusic> landTypeMusics = new List<LandTypeMusic>();
    private AudioSource audioSource;
    private LandType currentLandType;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = StartMusic;
        audioSource.Play();
    }

    public void SetLandType(LandType newLandType)
    {
        if (newLandType != currentLandType)
        {
            currentLandType = newLandType;
            PlayMusicForLandType(currentLandType);
        }
    }

    private void PlayMusicForLandType(LandType landType)
    {
        AudioClip newClip = landTypeMusics.Find(ltm => ltm.landType == landType)?.music;

        if (newClip != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.clip = newClip;
            audioSource.Play();
            Debug.Log($"Now playing music for {landType}");
        }
        else
        {
            Debug.LogWarning($"No music found for land type: {landType}");
        }
    }
}