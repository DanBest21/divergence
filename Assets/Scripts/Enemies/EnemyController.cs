using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(TransformRewind))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(FieldOfView))]
public class EnemyController : MonoBehaviour
{
    private CharacterController2D characterController;
    private TransformRewind transformRewind;
    private MeshRenderer meshRenderer;
    private FieldOfView fov;

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

    private Vector2 stationaryForward = Vector2.up;
    [SerializeField]
    private Vector2 stationaryTarget = Vector2.zero;

    [SerializeField]
    private List<Vector2> patrolRoute = new List<Vector2>();
    private Rewindable<int> patrolIndex = new Rewindable<int>(0, 30);

    private int pathIndex = 1;
    private Rewindable<Vector2> pursuitTarget = new Rewindable<Vector2> (Vector2.positiveInfinity);
    private Vector2[] path;
    private Rewindable<float> destinationReachedTime = new Rewindable<float>(Mathf.Infinity);
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

    [SerializeField]
    private MeshRenderer fovMeshRenderer;
    [SerializeField]
    private Material normalFovMaterial;
    [SerializeField]
    private Material pursuingFovMaterial;
    [SerializeField]
    private Material inSightFovMaterial;

    [SerializeField]
    private float killDistance = 1.1f;

    private float stationaryTolerance = 1;

    private Rewindable<bool> canSeePlayer = new Rewindable<bool>(false);


    private void Awake ()
    {
        stationaryForward = ((Vector2)(transform.rotation * Vector3.up)).normalized;

        characterController = GetComponent<CharacterController2D>();
        transformRewind = GetComponent<TransformRewind>();
        meshRenderer = GetComponent<MeshRenderer>();
        fov = GetComponent<FieldOfView>();
        mode = new Rewindable<Mode>(defaultMode);

        if(defaultMode == Mode.Pursuing)
        {
            Debug.LogError("Enemy set to pursue by default - has nothing to pursue");
        }
    }

    private void Update ()
    {
        if(Mathf.Abs(TimeManager.Instance.Flow) < 0.1f)
        {
            return;
        }
        if(TimeManager.Instance.Flow < 0)
        {
            path = null;
        }
        switch(mode.Get())
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
        if(mode.Get() == Mode.Dead)
        {
            meshRenderer.material = deadMaterial;
        }
        else
        {
            if(TimeManager.Instance.Flow > 0)
            {
                CheckForPlayer();
            }
            meshRenderer.material = aliveMaterial;
        }
        UpdateFovColor();
        if(canSeePlayer.Get())
        {

            Vector2 a = pursuitTarget.GetPrev();
            Vector2 b = pursuitTarget.Get();

            float t = TimeManager.Instance.CurrentTime - pursuitTarget.GetTime();
            t /= pursuitTarget.GetTime() - pursuitTarget.GetPrevTime();
            if(float.IsNaN(t))
            {
                t = 1;
            }

            Vector2 c = Vector2.Lerp(a, b, t);

            transform.up = c - (Vector2)transform.position;
        }
    }

    void CheckForPlayer ()
    {
        List<Transform> interests = fov.VisiblePoints();

        if(interests.Count > 0)
        {
            if(Vector2.Distance(pursuitTarget.Get(), interests[0].position) > 0.25f)
            {
                pursuitTarget.Set(interests[0].position);
            }
            canSeePlayer.Set(true);
            mode.Set(Mode.Pursuing);
            if(Vector2.Distance(transform.position, interests[0].position) < killDistance)
            {
                CameraFollow.Main.Kick(interests[0].position - transform.position);
                interests[0].GetComponent<PlayerMovement>().Kill();
                this.enabled = false;
            }
            
        }
        else
        {
            canSeePlayer.Set(false);            
        }
        
    }

    void UpdateFovColor ()
    {
        if(canSeePlayer.Get())
        {
            fovMeshRenderer.material = inSightFovMaterial;
        }
        else
        {
            fovMeshRenderer.material = mode.Get() == Mode.Pursuing ? pursuingFovMaterial : normalFovMaterial;
        }        
    }

    void StationaryUpdate ()
    {

        if (Vector2.Distance(transform.position, stationaryTarget) > stationaryTolerance)
        {
            if(TimeManager.Instance.Flow > 0)
            {
                if(path == null)
                {
                    UpdatePath(stationaryTarget);
                }
                FollowPath(patrolSpeed);
            }
            RotateToFaceMotion();
        }
        else
        {
            Vector2 up = transformRewind.GetSmoothedForward((Vector2)transform.position + stationaryForward, 0.3f);
        
            if(up == Vector2.zero || TimeManager.Instance.Flow < 0)
            {
                up = stationaryForward;
            }

            transform.up = up;
        }
    } 

    void PatrolUpdate ()
    {
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
            else if (path == null && ((Vector2)transform.position - target).sqrMagnitude < 1.2f)
            {
                patrolIndex.Set((pi + 1) % patrolRoute.Count);
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
        if (TimeManager.Instance.Flow > 0)
        {
            Vector2 target = pursuitTarget.Get();
            if((path == null || path[path.Length - 1] != target) && target != Vector2.positiveInfinity)
            {
                UpdatePath(target);
            }

            if(pathIndex < path.Length)
            {
                FollowPath(pursuitSpeed);
            }
            else if(TimeManager.Instance.CurrentTime < destinationReachedTime.Get())
            {
                destinationReachedTime.Set(TimeManager.Instance.CurrentTime);
            }
            else if(TimeManager.Instance.CurrentTime > destinationReachedTime.Get() + destinationWaitTime)
            {
                mode.Set(defaultMode);
                return;
            }
        }
        if(TimeManager.Instance.CurrentTime < destinationReachedTime.Get() + destinationWaitTime)
        {
            LookAroundAtDestination();
        }
        else
        {
            RotateToFaceMotion();
        }
            
    }
    private void LookAroundAtDestination ()
    {        
        Vector2 forward = transformRewind.GetLastMotion();
        Vector2 left = Vector2.Perpendicular(forward);
        Vector2 lookDir;
        float t = (TimeManager.Instance.CurrentTime - destinationReachedTime.Get()) / destinationWaitTime;

        if(t > 0.5f)
        {
            t -= 0.5f;
            t *= 2;
        }

        if(t < 0.25f)
        {
            lookDir = Vector2.Lerp(forward, left, t * 4);
        }
        else if(t < 0.5f)
        {
            lookDir = Vector2.Lerp(left, forward, (t-0.25f) * 4);
        }
        else if(t < 0.75f)
        {
            lookDir = Vector2.Lerp(forward, -left, (t - 0.5f) * 4);
        }
        else
        {
            lookDir = Vector2.Lerp(-left, forward, (t - 0.75f) * 4);
        }

        transform.up = lookDir;            
    }

    private void FollowPath (float speed)
    {
        if(path == null || pathIndex >= path.Length)
        {
            path = null;
            return;
        }
        Vector2 target = path[pathIndex];
        Vector2 maxMotion = target - (Vector2)transform.position;
        Vector2 motion = maxMotion.normalized
            * Mathf.Min(speed * Time.deltaTime * TimeManager.Instance.Flow, maxMotion.magnitude);

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
        destinationReachedTime.Set(Mathf.Infinity);
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(stationaryTarget, 0.5f);

#if UNITY_EDITOR
        if(mode != null && mode.Get() == Mode.Patrolling)
        {
            Handles.color = Color.white;
            Handles.Label(transform.position + Vector3.up, patrolIndex.Get().ToString());
        }
#endif
    }
}
