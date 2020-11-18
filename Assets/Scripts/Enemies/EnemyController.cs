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
        Partrolling = 2,
        Pursuing = 3,
    }

    [SerializeField]
    private Mode defaultMode = Mode.Stationary;

    private Mode mode;

    [SerializeField]
    private float stationaryAngle = 0;

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
            case Mode.Partrolling:
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

    } 

    void PatrolUpdate ()
    {

    }

    void PursuitUpdate ()
    {
        if(path == null || path[path.Length - 1] != pursuitTarget)
        {
            UpdatePath();
        }

        if(pathIndex < path.Length)
        {
            FollowPath();
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

    private void FollowPath ()
    {
        Vector2 target = path[pathIndex];
        Vector2 maxMotion = target - (Vector2)transform.position;
        Vector2 motion = maxMotion.normalized * Mathf.Min(pursuitSpeed * Time.deltaTime, maxMotion.magnitude);

        characterController.Move(motion);

        if((target - (Vector2)transform.position).sqrMagnitude < 0.01f)
        {
            pathIndex++;
        }
    }

    private void UpdatePath ()
    {
        if(pursuitTarget == Vector2.positiveInfinity)
        {
            return;
        }
        path = NavGrid.Instance.GetPath(transform.position, pursuitTarget);
        pathIndex = 1;
        destinationReachedTime = Mathf.Infinity;
    }

    private void RotateToFaceMotion ()
    {       
        if(!transformRewind.IsEmpty())
        {
            transform.up = transform.position - (Vector3)transformRewind.GetLastLogPoint();
        }
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
