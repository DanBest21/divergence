using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformRewind : MonoBehaviour
{
    private CircleStack<State> log = new CircleStack<State>(512);
    [SerializeField]
    private float maxErrorDistance = 0.01f;
    [SerializeField]
    private float maxErrorVelocity = 0.01f;

    [SerializeField]
    private bool ghostMode = false;
    [SerializeField]
    private Transform ghost = null;

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

    private void LateUpdate ()
    {
        Log();

        if(TimeManager.Instance.Flow < 0)
        {
            float time = TimeManager.Instance.CurrentTime;

            while(log.GetLast().time >= time && log.Size > 1)
            {
                log.RemoveLast();
            }

            Vector2 target = log.GetLast().position;
            float t = Time.deltaTime * Mathf.Abs(TimeManager.Instance.Flow) / (time - log.GetLast().time);

            if(!ghostMode)
            {
                transform.position = Vector2.Lerp(transform.position, target, t);
            }
            else
            {
                ghost.position = Vector2.Lerp(ghost.position, target, t);
            }
            
        }
        else if(ghost != null)
        {
            ghost.position = transform.position;
        }
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
                Debug.Log("Resized");
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

    public bool IsEmpty ()
    {
        return log.IsEmpty();
    }

    public Vector2 GetLastLogPoint ()
    {
        return log.GetLast().position;
    }

    public float GetRecentLogTime (int i)
    {
        return log.GetRecent(Mathf.Clamp(i, 0, log.Size - 1)).time;
    }

    public Vector2 GetLastMotion (out float deltaTime)
    {
        Vector2 currentPos = log.GetLast().position;
        Vector2 prev = log.GetRecent(Mathf.Min(log.Size - 1, 1)).position;
        int i = (Mathf.Min(log.Size, 2));
        while((currentPos - prev).sqrMagnitude < 0.01f && i < log.Size - 1)
        {
            prev = log.GetRecent(i).position;
            i++;
        }
        deltaTime = log.GetLast().time - log.GetRecent(i - 1).time;
        return (currentPos - prev).normalized;
    }

    public Vector2 GetLastMotion ()
    {
        return GetLastMotion(out float ignored);
    }

    public Vector2 GetSmoothedForward (Vector2 newPosition, float smoothTime)
    {
        if(log.Size == 0) return Vector2.zero;
        if(log.Size < 3) return (newPosition - log.GetLast().position);

        Vector2 simple = GetLastMotion(out float simpleDT);

        if(simpleDT > smoothTime)
        {
            return simple;
        }

        Vector2 mid = log.GetRecent(1).position;
        Vector2 prev = log.GetRecent(2).position;

        Vector2 a = mid - prev;
        Vector2 b = newPosition - mid;

        float angleA = Vector2.SignedAngle(Vector2.right, a);
        float angleB = Vector2.SignedAngle(Vector2.right, b);

        float deltaTime = TimeManager.Instance.CurrentTime - log.GetRecent(1).time;
        float t = Mathf.Clamp01(deltaTime / smoothTime);

        float tSmooth = t * t * (3f - 2f * t);
      
        float radian = Mathf.LerpAngle(angleA, angleB, tSmooth) * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
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
