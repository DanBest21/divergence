using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformLogger : MonoBehaviour
{
    private CircleStack<State> log = new CircleStack<State>(512);
    [SerializeField]
    private float maxErrorDistance = 0.01f;


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

        float deltaTime = newState.time - a.time;
        float t = (b.time - a.time) / deltaTime;

        Vector2 idealPos = Vector2.Lerp(a.position, newState.position, t);

        float errorSqr = Vector2.SqrMagnitude(idealPos - b.position);

        if(errorSqr < maxErrorDistance * maxErrorDistance)
        {
            log.RemoveLast();
        }

        log.Add(newState);


    }

    private void OnDrawGizmosSelected ()
    {
        for(int i = 0; i < log.Size; i++)
        {
            Gizmos.DrawSphere(log.GetRecent(i).position, 0.1f);
        }
    }





}
