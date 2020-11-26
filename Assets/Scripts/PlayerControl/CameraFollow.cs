using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Main { get; private set; }

    [SerializeField]
    private Transform target = null;

    [SerializeField]
    private float smoothTime = 1f;

    private Vector3 velocity = Vector3.zero;

    private Vector2 kickOffset = Vector2.zero;
    private Vector2 kickVelocity = Vector2.zero;

    [SerializeField]
    private float kickSpeed = 1;

    [SerializeField]
    private float kickSlowdown = 0.2f;
    [SerializeField]
    private float kickReturn = 0.4f;

    private void Awake ()
    {
        Main = this;
    }

    public void Kick(Vector2 direction)
    {
        kickVelocity = direction.normalized * kickSpeed;
    }

    void LateUpdate()
    {
        Vector3 targetPos = target.position;
        targetPos.z = transform.position.z;

        transform.position -= (Vector3)kickOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        kickOffset += kickVelocity;
        kickVelocity = Vector2.MoveTowards(kickVelocity, Vector2.zero, kickSlowdown * Time.deltaTime);
        kickOffset = Vector2.MoveTowards(kickOffset, Vector2.zero, kickReturn * Time.deltaTime);

        transform.position += (Vector3)kickOffset;
    }
}
