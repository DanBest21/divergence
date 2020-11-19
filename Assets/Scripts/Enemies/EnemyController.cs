using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(TransformRewind))]
public class EnemyController : MonoBehaviour
{
    private CharacterController2D characterController;
    private TransformRewind transformRewind;

    private enum Mode
    {
        Dead = 0,
        Stationary = 1,
        Patrolling = 2,
        Pursuing = 3,
    }

    [SerializeField]
    private Mode defaultMode = Mode.Stationary;

    private Mode mode;

    [SerializeField]
    private Vector2 stationaryForward = Vector2.up;
    [SerializeField]
    private Vector2 stationaryTarget = Vector2.zero;

    [SerializeField]
    private List<Vector2> patrolRoute = new List<Vector2>();
    private int patrolIndex = 0;

    private int pathIndex = 1;
    private Vector2 pursuitTarget = Vector2.positiveInfinity;
    private Vector2[] path;
    private float destinationReachedTime = Mathf.Infinity;
    [SerializeField]
    private float destinationWaitTime = 1.5f;

    [SerializeField]
    private float patrolSpeed = 3;
    [SerializeField]
    private float pursuitSpeed = 5;

    private float stationaryTolerance = 1;



    private void Awake ()
    {
        characterController = GetComponent<CharacterController2D>();
        transformRewind = GetComponent<TransformRewind>();
        mode = defaultMode;
    }

    private void Update ()
    {
        //DEBUG ONLY REALLY BAD CODE
        pursuitTarget = FindObjectOfType<PlayerMovement>().transform.position;


        switch(mode)
        {
            case Mode.Dead:
                break;
            case Mode.Stationary:
                StationaryUpdate();
                break;
            case Mode.Patrolling:
                PatrolUpdate();
                break;
            case Mode.Pursuing:
                PursuitUpdate();
                break;
            default:
                break;
        }
    }

    void StationaryUpdate ()
    {
        if(Vector2.Distance(transform.position, stationaryTarget) > stationaryTolerance)
        {
            if(path == null)
            {
                UpdatePath(stationaryTarget);
            }
            FollowPath(patrolSpeed);
            RotateToFaceMotion();
        }
        else
        {
            Vector2 up = transformRewind.GetSmoothedForward((Vector2)transform.position + stationaryForward, 0.3f); ;
            if(up == Vector2.zero)
            {
                up = stationaryForward;
            }

            transform.up = up;
        }
    } 

    void PatrolUpdate ()
    {
        if(patrolRoute == null || patrolRoute.Count < 2)
        {
            Debug.LogError("Invalid patrol route found. Please assign at least 2 points to enemy with patrol job");
            return;
        }

        Vector2 target = patrolRoute[patrolIndex];

        if(Vector2.Distance(transform.position, target) < 0.01f || path == null)
        {
            patrolIndex = (patrolIndex + 1) % patrolRoute.Count;
            target = patrolRoute[patrolIndex];
            UpdatePath(target);
        }

        FollowPath(patrolSpeed);

        RotateToFaceMotion();
    }

    void PursuitUpdate ()
    {
        if((path == null || path[path.Length - 1] != pursuitTarget) && pursuitTarget != Vector2.positiveInfinity)
        {
            UpdatePath(pursuitTarget);
        }

        if(pathIndex < path.Length)
        {
            FollowPath(pursuitSpeed);
        }
        else if(TimeManager.Instance.CurrentTime < destinationReachedTime)
        {
            destinationReachedTime = TimeManager.Instance.CurrentTime;
        }
        else if(TimeManager.Instance.CurrentTime > destinationReachedTime + destinationWaitTime)
        {
            mode = defaultMode;
        }

        RotateToFaceMotion();
    }

    private void FollowPath (float speed)
    {
        Vector2 target = path[pathIndex];
        Vector2 maxMotion = target - (Vector2)transform.position;
        Vector2 motion = maxMotion.normalized * Mathf.Min(pursuitSpeed * Time.deltaTime, maxMotion.magnitude);

        characterController.Move(motion);

        if((target - (Vector2)transform.position).sqrMagnitude < 0.01f)
        {
            pathIndex++;

            if(pathIndex == path.Length)
            {
                path = null;
                pathIndex = 0;
            }
        }
    }

    private void UpdatePath (Vector2 target)
    {
        path = NavGrid.Instance.GetPath(transform.position, target);
        pathIndex = 1;
        destinationReachedTime = Mathf.Infinity;
    }



    private void RotateToFaceMotion ()
    {
        transform.up = transformRewind.GetSmoothedForward(transform.position, 0.1f);
    }


    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.blue;
        foreach(Vector2 point in patrolRoute)
        {
            Gizmos.DrawSphere(point, 0.5f);
        }

        Gizmos.color = Color.red;
        if(path != null && path.Length > 2)
        {
            for(int i = 1; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
}
