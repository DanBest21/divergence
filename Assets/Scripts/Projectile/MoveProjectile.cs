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
    private AudioClip projectileHitSound;
    [SerializeField]
    private AudioClip enemyDeathSound;
    private AudioSource audioSource;

    [SerializeField]
    [Range(1, 100)]
    private float speed;

    public Vector3 StartPoint() { return startPoint; }

    public Vector3 DestinationPoint() { return destinationPoint; }

    public bool HasStopped() { return hasStopped; }

    public Mesh Mesh() { return mesh; }

    public float Speed() { return speed; }

    public bool HitEnemy { get; private set; } = false;

    [SerializeField]
    private MeshRenderer meshRenderer;

    public void Setup(Vector2 direction, Mesh mesh, FireProjectile fireProjectile)
    {
        startPoint = transform.position;
        this.direction = direction;
        this.mesh = mesh;
        this.fireProjectile = fireProjectile;

        timeStarted = TimeManager.Instance.CurrentTime;
        hasStopped = false;
        audioSource = gameObject.AddComponent<AudioSource>();
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
        int layerMask = LayerMask.GetMask("Enemies", "Obstructions", "Targets");
        
        RaycastHit2D objectHit = Physics2D.Raycast(transform.position, direction, speed * Time.deltaTime, layerMask);

        if (objectHit)
        {
            if ((LayerMask.GetMask("Enemies") & 1 << objectHit.transform.gameObject.layer) != 0)
            {
                objectHit.transform.gameObject.GetComponent<EnemyController>().Kill();
                audioSource.PlayOneShot(enemyDeathSound);
                HitEnemy = true;
                meshRenderer.enabled = false;
            }
            else
            {
                audioSource.PlayOneShot(projectileHitSound);
            }
            
            transform.position = objectHit.point + objectHit.normal / 10;

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
        float timeSinceThrow = TimeManager.Instance.CurrentTime - timeStarted;

        if(timeSinceThrow < 0)
        {
            fireProjectile.Pickup();
            return;
        }

        float maxPossibleDistance = speed * timeSinceThrow;
        float currentDistance = Vector2.Distance(startPoint, transform.position);
        float newDistance = Mathf.Min(currentDistance, maxPossibleDistance);

        if(Mathf.Abs(currentDistance - newDistance) > 0)
        {
            meshRenderer.enabled = true;
        }

        Vector3 position = startPoint + (Vector3)direction.normalized * newDistance;

        transform.position = position;
    }
}
