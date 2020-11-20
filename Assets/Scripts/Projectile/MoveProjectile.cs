using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProjectile : MonoBehaviour
{
    private Vector3 startPoint;
    private Vector3 destinationPoint;
    private Vector2 direction;
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

    public void Setup(Vector2 direction, Mesh mesh)
    {
        startPoint = transform.position;
        this.direction = direction;
        this.mesh = mesh;

        hasStopped = false;
    }

    void LateUpdate()
    {
        if (!hasStopped)
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
            transform.position = objectHit.point - (direction * mesh.bounds.size.y * 0.8f);

            hasStopped = true;
        }
        else
        {
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        }

        destinationPoint = transform.position;
    }
}
