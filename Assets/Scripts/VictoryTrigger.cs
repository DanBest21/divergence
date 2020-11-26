using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VictoryTrigger : MonoBehaviour
{
    [SerializeField]
    RectTransform victoryUI;
    [SerializeField]
    private PostProcessVolume volume;

    private bool won = false;
    private float weightVelocity = 0;


    private void OnTriggerEnter2D (Collider2D collision)
    {
        won = true;
        Debug.Log("Win");
        Debug.Log(won);
    }

    private void Update ()
    {
        if(!won)
        {
            volume.weight = 0;
            victoryUI.gameObject.SetActive(false);
        }
        else
        {
            volume.weight = Mathf.SmoothDamp(volume.weight, 1, ref weightVelocity, 1);
            victoryUI.gameObject.SetActive(true);
        }
    }
}
