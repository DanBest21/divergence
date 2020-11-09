using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class EnemyController : MonoBehaviour
{
    private CharacterController2D characterController;

    private enum Mode
    {
        Dead = 0,
        Stationary = 1,
        Partrolling = 2,
        Pursuing = 3,
    }

    [SerializeField]
    private Mode mode = Mode.Stationary;

    [SerializeField]
    private float stationaryAngle = 0;

    [SerializeField]
    private List<Vector2> patrolRoute = new List<Vector2>();

    private int pathIndex = 0;
    private Vector3 persuitTarget;
    private Vector2[] path;

    private void Awake ()
    {
        characterController = GetComponent<CharacterController2D>();
    }

    private void Update ()
    {
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
                PersuitUpdate();
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

    void PersuitUpdate ()
    {
        //DEBUG ONLY REALLY BAD CODE
        persuitTarget = FindObjectOfType<PlayerMovement>().transform.position;

        path = NavGrid.Instance.GetPath(transform.position, persuitTarget);
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
