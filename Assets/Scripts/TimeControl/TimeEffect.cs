using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(PostProcessVolume))]
public class TimeEffect : MonoBehaviour
{
    private PostProcessVolume volume;

    private float velocity = 0;

    [SerializeField]
    private float dampTime = 0.1f;

    private void Awake ()
    {
        volume = GetComponent<PostProcessVolume>();
    }

    void Update()
    {
        float target = 0;
        if(TimeManager.Instance.Flow < 0)
        {
            target = 1;
        }

        volume.weight = Mathf.SmoothDamp(volume.weight, target, ref velocity, dampTime);
    }
}
