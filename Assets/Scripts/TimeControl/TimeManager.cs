using System.Collections;
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

    [SerializeField]
    private FireProjectile fireProjectile = null;

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
        if(!fireProjectile.CanFire())
        {
            if(Input.GetKeyDown(KeyCode.LeftShift))
            {
                Flow = -2;
            }
        }
        else
        {
            Flow = 1;
        }

        CurrentTime += Time.deltaTime * Flow;
    }

    private void OnDestroy ()
    {
        Instance = null;
    }
}
