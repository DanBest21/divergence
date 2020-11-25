using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


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

    private Rewindable<Mode> mode;

    [SerializeField]
    private Vector2 stationaryForward = Vector2.up;
    [SerializeField]
    private Vector2 stationaryTarget = Vector2.zero;

    [SerializeField]
    private List<Vector2> patrolRoute = new List<Vector2>();
    private Rewindable<int> patrolIndex = new Rewindable<int>(0, 30);

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

    [SerializeField]
    private Material aliveMaterial;
    [SerializeField]
    private Material deadMaterial;

    private float stationaryTolerance = 1;



    private void Awake ()
    {
        characterController = GetComponent<CharacterController2D>();
        transformRewind = GetComponent<TransformRewind>();
        mode = new Rewindable<Mode>(defaultMode);
    }

    private void Update ()
    {
        //DEBUG ONLY REALLY BAD CODE
        pursuitTarget = FindObjectOfType<PlayerMovement>().transform.position;
        if(TimeManager.Instance.Flow < 0)
        {
            path = null;
        }

        switch (mode.Get())
        {
            case Mode.Dead:
                transform.GetComponent<MeshRenderer>().material = deadMaterial;
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
        transform.GetComponent<MeshRenderer>().material = aliveMaterial;

        if (Vector2.Distance(transform.position, stationaryTarget) > stationaryTolerance 
            && TimeManager.Instance.Flow > 0)
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
        transform.GetComponent<MeshRenderer>().material = aliveMaterial;

        if (patrolRoute == null || patrolRoute.Count < 2)
        {
            Debug.LogError("Invalid patrol route found. Please assign at least 2 points to enemy with patrol job");
            return;
        }

        int pi = patrolIndex.Get();

        Vector2 target = patrolRoute[pi];


        if(TimeManager.Instance.Flow > 0)
        {
            if(((Vector2)transform.position - target).sqrMagnitude < 0.01f)
            {
                patrolIndex.Set((pi + 1) % patrolRoute.Count);
                path = null;
            }
            if(path == null)
            { 
                target = patrolRoute[patrolIndex.Get()];
                UpdatePath(target);
            }

            FollowPath(patrolSpeed);
        }

        RotateToFaceMotion();
    }

    void PursuitUpdate ()
    {
        transform.GetComponent<MeshRenderer>().material = aliveMaterial;

        if (TimeManager.Instance.Flow > 0)
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
                mode.Set(defaultMode);
            }
        }

        RotateToFaceMotion();
    }

    private void FollowPath (float speed)
    {
        Vector2 target = path[pathIndex];
        Vector2 maxMotion = target - (Vector2)transform.position;
        Vector2 motion = maxMotion.normalized
            * Mathf.Min(pursuitSpeed * Time.deltaTime * TimeManager.Instance.Flow, maxMotion.magnitude);

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

    public void Kill ()
    {
        mode.Set(Mode.Dead);
    }

    public bool isDead()
    {
        return mode.Get() == Mode.Dead;
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

#if UNITY_EDITOR
        if(mode != null && mode.Get() == Mode.Patrolling)
        {
            Handles.color = Color.white;
            Handles.Label(transform.position + Vector3.up, patrolIndex.Get().ToString());
        }
#endif
    }
}
