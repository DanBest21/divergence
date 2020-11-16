using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public float ViewRadius() { return viewRadius; }

    public float ViewAngle() { return viewAngle; }

    public LayerMask InterestPoints() { return interestPoints; }

    public LayerMask Obstructions() { return obstructions; }

    public List<Transform> VisiblePoints() { return visiblePoints; }

    void Start()
    {
        StartCoroutine("FindPointsCoroutine", 0.2f);
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

    public Vector3 CalculateAngle(float angle, bool isGlobal)    
    {
        if (!isGlobal)
        {
            angle -= transform.eulerAngles.z;
        }
        
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0);
    }
}
