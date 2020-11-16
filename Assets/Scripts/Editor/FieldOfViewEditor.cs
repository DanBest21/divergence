using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireDisc(fov.transform.position, Vector3.forward, fov.ViewRadius());

        Vector3 initialAngle = fov.CalculateAngle(-fov.ViewAngle() / 2, false);
        Vector3 endAngle = fov.CalculateAngle(fov.ViewAngle() / 2, false);
        Handles.DrawLine(fov.transform.position, fov.transform.position + initialAngle * fov.ViewRadius());
        Handles.DrawLine(fov.transform.position, fov.transform.position + endAngle * fov.ViewRadius());

        Handles.color = Color.green;

        foreach (Transform point in fov.VisiblePoints())
        {
            Handles.DrawLine(fov.transform.position, point.position);
        }
    }
}
