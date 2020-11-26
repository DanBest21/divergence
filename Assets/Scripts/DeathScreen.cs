using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UIElements;

[RequireComponent(typeof(PostProcessVolume))]

public class DeathScreen : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement player;
    [SerializeField]
    private PostProcessVolume volume;
    [SerializeField]
    private RectTransform deathUI;

    private float weightVelocity = 0;

    public static readonly float deathTime = 2f;

    private void Update ()
    {
        if(player.IsAlive)
        {
            volume.weight = 0;
            deathUI.gameObject.SetActive(false);
        }
        else
        {
            volume.weight = Mathf.SmoothDamp(volume.weight, 1, ref weightVelocity, deathTime);
            deathUI.gameObject.SetActive(true);
        }
    }

}
