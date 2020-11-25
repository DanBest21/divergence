using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProjectile : MonoBehaviour
{
    private Vector3 startPoint;
    private Vector3 destinationPoint;
    private Vector2 direction;
    private FireProjectile fireProjectile;
    private float timeStarted = float.MaxValue;
    private float timeLanded = float.MaxValue;
    private float timeRewind = 0;
    private bool hasStopped = true;

    private Mesh mesh;

    [SerializeField]
    [Range(1, 100)]
    private float speed;

    public Vector3 StartPoint() { return startPoint; }

    public Vector3 DestinationPoint() { return destinationPoint; }

    public bool HasStopped() { return hasStopped; }

    public Mesh Mesh() { return mesh; }

    public float Speed() { return speed; }

    public void Setup(Vector2 direction, Mesh mesh, FireProjectile fireProjectile)
    {
        startPoint = transform.position;
        this.direction = direction;
        this.mesh = mesh;
        this.fireProjectile = fireProjectile;

        timeStarted = TimeManager.Instance.CurrentTime;
        hasStopped = false;
    }

    void LateUpdate()
    {
        bool timeRewinding = TimeManager.Instance.Flow < 0;

        if (timeRewinding) // && TimeManager.Instance.CurrentTime <= timeLanded)
        {
            timeRewind = Mathf.Max(timeRewind, TimeManager.Instance.CurrentTime);
            RewindProjectile();
        }
        else if (!hasStopped)
        {
            DrawProjectile();
        }
    }

    void DrawProjectile()
    {
        int layerMask = LayerMask.GetMask("Enemies", "Obstructions");
        
        RaycastHit2D objectHit = Physics2D.Raycast(transform.position, direction, speed * Time.deltaTime, layerMask);

        if (objectHit)
        {            
            if ((LayerMask.GetMask("Enemies") & 1 << objectHit.transform.gameObject.layer) != 0)
            {
                objectHit.transform.gameObject.GetComponent<EnemyController>().Kill();
            }
            
            transform.position = objectHit.point - (direction * mesh.bounds.size.y * 0.8f);

            timeLanded = TimeManager.Instance.CurrentTime;
            hasStopped = true;
        }
        else
        {
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }

        destinationPoint = transform.position;
    }

    void RewindProjectile()
    {
        // Vector3 rewindPosition = transform.position + (Vector3)direction * speed * TimeManager.Instance.Flow * Time.deltaTime;
        float rewindSpeed = Vector3.Distance(destinationPoint, startPoint) / (timeRewind - timeStarted);
        Vector3 rewindPosition = transform.position + (Vector3)direction * rewindSpeed * TimeManager.Instance.Flow * Time.deltaTime;
        float rewindDistance = Vector3.Distance(transform.position, rewindPosition);
        float distanceToTarget = Vector3.Distance(transform.position, startPoint);

        if (distanceToTarget < rewindDistance)
        {
            fireProjectile.Pickup();
        }
        else
        {
            transform.position = rewindPosition;
        }
    }
}
