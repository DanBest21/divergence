using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindMusic : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip normalTrack;
    [SerializeField]
    private AudioClip rewindTrack;

    private bool isRewinding = false;

    // Update is called once per frame
    void Update()
    {
        if (TimeManager.Instance.Flow < 0)
        {
            isRewinding = true;
            audioSource.clip = rewindTrack;
            audioSource.time = (rewindTrack.length - TimeManager.Instance.CurrentTime) % rewindTrack.length;
            audioSource.Play();
        }
        else if (isRewinding)
        {
            isRewinding = false;
            audioSource.clip = normalTrack;
            audioSource.time = TimeManager.Instance.CurrentTime % normalTrack.length;
            audioSource.Play();
        }
    }
}
