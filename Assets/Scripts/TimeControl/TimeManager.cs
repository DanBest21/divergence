using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField]
    private float maxLogTime = 30;
    public float MaxLogTime { get { return maxLogTime; } }

    public float CurrentTime { get; private set; }

    public float Flow { get; set; } = 1;

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

    private void Update ()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            Flow = -2;
        }
        else
        {
            Flow = 1;
        }
        CurrentTime += Time.deltaTime * Flow;

        CurrentTime = Mathf.Max(CurrentTime, 0.01f);
    }

    private void OnDestroy ()
    {
        Instance = null;
    }
}
