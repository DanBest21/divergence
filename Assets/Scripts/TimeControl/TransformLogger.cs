using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLogger : MonoBehaviour
{
    private CircleStack<State> log = new CircleStack<State>(512);
    [SerializeField]
    private float maxErrorDistance = 0.01f;
    [SerializeField]
    private float maxErrorVelocity = 0.01f;

    private float lastDeltaTime = 1;

    private struct State
    {
        public float time;
        public Vector2 position;

        public State (float time, Vector2 position)
        {
            this.time = time;
            this.position = position;
        }
    }

   

    private void Update ()
    {
        Log();
       
        lastDeltaTime = Time.deltaTime;
    
    }

    private void Log ()
    {
        State newState = new State(TimeManager.Instance.CurrentTime, transform.position);

        if(log.Size < 2)
        {
            log.Add(newState);
            return;
        }

        if(log.GetNextOverwrite(out State overwrite))
        {
            if(newState.time - overwrite.time < TimeManager.Instance.MaxLogTime)
            {
                log.Resize();
            }
        }

        State a = log.GetRecent(1);

        State b = log.GetLast();

        if(GetErrorDistance(a.position, b.position, newState.position) < maxErrorDistance
            && GetErrorVelocity(a, b, newState) < maxErrorVelocity) 
        {
            log.RemoveLast();
        }

        log.Add(newState);


    }

    float GetErrorDistance (Vector2 a, Vector2 b, Vector2 c)
    {
        return Vector3.Cross(c - a, b - a).magnitude;
    }

    float GetErrorVelocity (State a, State b, State c)
    {
        return Vector2.Distance(GetVelocity(a, b), GetVelocity(a, c));
    }

    Vector2 GetVelocity (State a, State b)
    {
        return (b.position - a.position) / (b.time - a.time);
    }

    private void OnDrawGizmosSelected ()
    {
        for(int i = 0; i < log.Size; i++)
        {
            Gizmos.DrawSphere(log.GetRecent(i).position, 0.1f);
        }
    }





}
