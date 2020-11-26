using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// <summary>
// Simple 2D field of view system using raycasting and meshes.
// Adapated from Sebastian Lague's Field of View visualisation series.
// </summary>
public class FieldOfView : MonoBehaviour
{
    [SerializeField]
    private float viewRadius;
    [SerializeField]
    [Range(0, 360)]
    private float viewAngle;

    [SerializeField]
    private LayerMask interestPoints;
    [SerializeField]
    private LayerMask obstructions;

    [SerializeField]
    private List<Transform> visiblePoints = new List<Transform>();

    [SerializeField]
    private float meshResolution;
    [SerializeField]
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    [SerializeField]
    private int edgeDetectionIterations;
    [SerializeField]
    private float edgeDetectionThreshold;

    public float ViewRadius() { return viewRadius; }

    public float ViewAngle() { return viewAngle; }

    public LayerMask InterestPoints() { return interestPoints; }

    public LayerMask Obstructions() { return obstructions; }

    public List<Transform> VisiblePoints() { return visiblePoints; }

    public float MeshResolution() { return meshResolution; }

    public MeshFilter ViewMeshFilter() { return viewMeshFilter; }

    public int EdgeDetectionIterations() { return edgeDetectionIterations; }

    public float EdgeDetectionThreshold() { return edgeDetectionThreshold; }

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        StartCoroutine("FindPointsCoroutine", 0.2f);
    }

    void LateUpdate()
    {
        EnemyController enemyController;
        bool isEnemy = transform.TryGetComponent<EnemyController>(out enemyController);

        if (isEnemy && enemyController.isDead())
        {
            viewMesh.Clear();
        }
        else
        {
            DrawFOV();
        }
    }

    IEnumerator FindPointsCoroutine(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisiblePoints();
        }
    }

    void FindVisiblePoints()
    {
        visiblePoints.Clear();

        Collider2D[] pointsInCircle = Physics2D.OverlapCircleAll(transform.position, viewRadius, interestPoints);
            
        foreach (Collider2D point in pointsInCircle)
        {
            Vector3 directionToPoint = (point.transform.position - transform.position).normalized;

            if (Vector2.Angle(transform.up, directionToPoint) <= viewAngle / 2)
            {
                float distanceToPoint = Vector3.Distance(transform.position, point.transform.position);

                if (!Physics2D.Raycast(transform.position, directionToPoint, distanceToPoint, obstructions))
                {
                    visiblePoints.Add(point.transform);
                }
            }
        }
    }

    void DrawFOV()
    {
        int numberOfRays = Mathf.RoundToInt(viewAngle * meshResolution);
        float rayAngleInterval = viewAngle / numberOfRays;

        List<Vector2> viewPoints = new List<Vector2>();
        RayCastDetails oldRayCast = new RayCastDetails();

        for (int i = 0; i <= numberOfRays; i++)
        {
            float angle = -transform.eulerAngles.z - (viewAngle / 2) + (rayAngleInterval * i);

            RayCastDetails newRayCast = RayCast(angle);

            bool edgeDetectionThresholdMet = Mathf.Abs(oldRayCast.Distance - newRayCast.Distance) > edgeDetectionThreshold;

            if (i != 0 && ((edgeDetectionThresholdMet && oldRayCast.ObstructionHit && newRayCast.ObstructionHit) || 
                oldRayCast.ObstructionHit != newRayCast.ObstructionHit))
            {
                (Vector3 minPoint, Vector3 maxPoint) = FindEdge(oldRayCast, newRayCast);

                if (minPoint != Vector3.zero) 
                { 
                    viewPoints.Add(minPoint); 
                }
                if (maxPoint != Vector3.zero) 
                { 
                    viewPoints.Add(maxPoint); 
                }                
            }

            viewPoints.Add(newRayCast.Point);
            oldRayCast = newRayCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        Vector2[] uv = new Vector2[vertices.Length];

        float maxX = 0f;
        float maxY = 0f;

        vertices[0] = Vector2.zero;
        uv[0] = Vector2.zero;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            vertices[i + 1].z = 0;

            maxX = Mathf.Max(maxX, vertices[i + 1].x);
            maxY = Mathf.Max(maxY, vertices[i + 1].y);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        for (int i = 0; i < vertexCount - 1; i++)
        {
            uv[i + 1] = new Vector2(vertices[i + 1].x / maxX, vertices[i + 1].y / maxY);
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.uv = uv;
        viewMesh.RecalculateNormals();
    }

    (Vector3, Vector3) FindEdge(RayCastDetails minRay, RayCastDetails maxRay)
    {
        float minAngle = minRay.Angle;
        float maxAngle = maxRay.Angle;

        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeDetectionIterations; i++)
        {
            float midAngle = (minAngle + maxAngle) / 2;
            RayCastDetails midRayCast = RayCast(midAngle);

            bool edgeDetectionThresholdMet = Mathf.Abs(minRay.Distance - maxRay.Distance) > edgeDetectionThreshold;

            if (!edgeDetectionThresholdMet && midRayCast.ObstructionHit == minRay.ObstructionHit)
            {
                minAngle = midAngle;
                minPoint = midRayCast.Point;
            }
            else
            {
                maxAngle = midAngle;
                maxPoint = midRayCast.Point;
            }
        }

        return (minPoint, maxPoint);
    }

    RayCastDetails RayCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        RaycastHit2D objectHit = Physics2D.Raycast(transform.position, direction, viewRadius, obstructions);

        if (objectHit)
        {
            return new RayCastDetails(true, objectHit.point, objectHit.distance, globalAngle);
        }
        else 
        {
            return new RayCastDetails(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirectionFromAngle(float angle, bool isGlobal)    
    {
        if (!isGlobal)
        {
            angle -= transform.eulerAngles.z;
        }
        
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0);
    }

    public struct RayCastDetails
    {
        public bool ObstructionHit { get; }
        public Vector3 Point { get; }
        public float Distance { get; }
        public float Angle { get; }

        public RayCastDetails(bool obstructionHit, Vector3 point, float distance, float angle)
        {
            ObstructionHit = obstructionHit;
            Point = point;
            Distance = distance;
            Angle = angle;
        }
    }
}
