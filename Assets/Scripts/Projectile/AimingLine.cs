using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingLine : MonoBehaviour
{
    LineRenderer lr;
    Vector3[] points = new Vector3[2];
    [SerializeField]
    FireProjectile fp;

    private void Awake ()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        int layerMask = LayerMask.GetMask("Enemies", "Obstructions");

        RaycastHit2D objectHit = Physics2D.Raycast(transform.position, transform.up, 30, layerMask);

        points[0] = transform.position;
        if(objectHit)
        {
            points[1] = objectHit.point;
        }
        else
        {
            points[1] = transform.position + transform.up * 30;
        }

        lr.SetPositions(points);

        lr.enabled = fp.CanFire();
    }
}
