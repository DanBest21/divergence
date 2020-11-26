using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraScale : MonoBehaviour
{
    public Camera cam;
    public RectTransform rectTransform;

    private void Update ()
    {
        if(cam != null && rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2((float)cam.pixelWidth / cam.pixelHeight * cam.orthographicSize * 2, cam.orthographicSize * 2);
        }
    }
}
