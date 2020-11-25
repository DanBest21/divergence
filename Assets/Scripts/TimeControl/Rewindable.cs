using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Rewindable<T>
{
    CircleStack<State> log;

    private struct State
    {
        public float time;
        public T value;

        public State (float time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }

    /// <summary>
    /// Must also be started with .Start()
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="value"></param>
    public Rewindable (T value, int capacity = 32)
    {
        log = new CircleStack<State>(capacity);
        log.Add(new State(0, value));
    }

    private void CleanLog ()
    {
        float time = TimeManager.Instance.CurrentTime;

        while(log.GetLast().time > time && log.Size > 1)
        {
            log.RemoveLast();
        }
    }

    public T Get ()
    {
        CleanLog();
        return log.GetLast().value;
    }

    public float GetTime ()
    {
        CleanLog();
        return log.GetLast().time;
    }

    public float GetPrevTime ()
    {
        CleanLog();
        if(log.Size > 2)
        {
            return log.GetRecent(1).time;
        }
        else
        {
            return log.GetLast().time;
        }        
    }

    public T GetPrev ()
    {
        CleanLog();
        if(log.Size > 2)
        {
            return log.GetRecent(1).value;
        }
        else
        {
            return log.GetLast().value;
        }
    }



    public void Set (T value)
    {
        if(log.GetLast().value.Equals(value))
        {
            return;
        }

        log.Add(new State(TimeManager.Instance.CurrentTime, value));


        if(log.GetNextOverwrite(out State overwrite))
        {
            if(TimeManager.Instance.CurrentTime - overwrite.time < TimeManager.Instance.MaxLogTime)
            {
                log.Resize();
                Debug.Log("Resized");
            }
        }
    }
}

