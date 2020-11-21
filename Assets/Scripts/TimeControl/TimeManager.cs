﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField]
    private float maxLogTime = 30;
    public float MaxLogTime { get { return maxLogTime; } }

    public float CurrentTime { get; private set; }

    public float Flow { get; set; } = 1;

    private UnityEvent onLateUpdateEvent = new UnityEvent();

    private void Awake ()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Time Managers Detected! Only one should exist per scene.");
        }
    }

    public void AddLateUpdateListener (UnityAction action)
    {
        onLateUpdateEvent.AddListener(action);
    }

    private void LateUpdate ()
    {
        onLateUpdateEvent.Invoke();
    }

    private void Update ()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(CurrentTime > 1)
            {
                Flow = -2;
                CurrentTime += Time.deltaTime * Flow;
            }
            else
            {
                Flow = 0;
            }
            
        }
        else
        {
            Flow = 1;
            CurrentTime += Time.deltaTime * Flow;
        }
    }

    private void OnDestroy ()
    {
        Instance = null;
    }
}
