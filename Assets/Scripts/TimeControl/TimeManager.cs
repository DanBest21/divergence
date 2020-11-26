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

    public float Flow { get; set; } = 0;

    private UnityEvent onLateUpdateEvent = new UnityEvent();

    [SerializeField]
    private FireProjectile fireProjectile = null;

    [SerializeField]
    private PlayerMovement player = null;

    private float stopTimeVelocity = 0;

    private float rewindVelocity = 0;

    private bool rewind = false;

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
        if(!player.IsAlive)
        {
            Flow = Mathf.Clamp01(Mathf.SmoothDamp(Flow, 0, ref stopTimeVelocity, DeathScreen.deathTime));
            return;
        }

        if(!fireProjectile.CanFire())
        {
            if(Input.GetKeyDown(KeyCode.LeftShift))
            {
                rewind = true;
            }
        }
        else
        {
            rewind = false;
            Flow = Mathf.Max(Mathf.SmoothDamp(Flow, 1, ref rewindVelocity, 0.1f), -1);
            if(Flow > 0.9f)
            {
                Flow = 1f;
            }
        }

        if(rewind)
        {
            Flow = Mathf.Min(Mathf.SmoothDamp(Flow, -2, ref rewindVelocity, 0.2f), 0);
        }

        CurrentTime += Time.deltaTime * Flow;
    }

    private void OnDestroy ()
    {
        Instance = null;
    }
}
