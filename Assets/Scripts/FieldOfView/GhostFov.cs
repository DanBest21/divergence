using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostFov : MonoBehaviour
{
    FieldOfView fov;

    private void Awake ()
    {
        fov = GetComponent<FieldOfView>();
    }

    void Update()
    {
        if(TimeManager.Instance.CurrentTime < 0)
        {
            fov.enabled = false;
            fov.ViewMeshFilter().gameObject.SetActive(false);
        }
        else
        {
            fov.enabled = true;
            fov.ViewMeshFilter().gameObject.SetActive(true);
        }
    }
}
