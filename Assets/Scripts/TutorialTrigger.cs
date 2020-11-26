using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField]
    int id = 0;

    private void OnTriggerEnter2D (Collider2D collision)
    {
        Debug.Log("Trigger hit! " + id);
        TutorialScript.Instance.Trigger(GetComponent<BoxCollider2D>(), id);
    }
}
