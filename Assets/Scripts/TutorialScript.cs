using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{
    public static TutorialScript Instance { get; private set; }

    [SerializeField]
    FireProjectile fp;
    [SerializeField]
    GameObject aimLine;
    [SerializeField]
    GameObject handKnife;
    [SerializeField]
    PlayerMovement player;
    [SerializeField]
    Transform pickupKnife;
    [SerializeField]
    GameObject door;
    [SerializeField]
    Text text;

    bool hasKnife = false;

    readonly string throwPrompt = "Left click to throw";
    readonly string levelPrompt = "Level 01 - Tutorial";
    readonly string shiftPrompt = "Tap shift to rewind time";

    int maxID = -1;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        fp.enabled = false;
        handKnife.SetActive(false);
        aimLine.SetActive(false);
        text.text = "";
        door.SetActive(true);
        text.text = levelPrompt;
    }

    // Update is called once per frame
    void Update()
    {
        if(pickupKnife.gameObject.activeSelf && Vector3.Distance(pickupKnife.position, player.transform.position) < 2f)
        {
            fp.enabled = true;
            handKnife.SetActive(true);
            aimLine.SetActive(true);
            pickupKnife.gameObject.SetActive(false);
            hasKnife = true;
            text.text = throwPrompt;
        }

        if(maxID < 3 && fp.enabled && fp.Landed)
        {
            if(!fp.HitEnemy)
            {
                text.text = shiftPrompt;
            }
        }

        if(text.text.Equals(throwPrompt) && Input.GetKeyDown(KeyCode.Mouse0))
        if(text.text.Equals(throwPrompt) && Input.GetKeyDown(KeyCode.Mouse0))
        {
            text.text = "";
        }

        Debug.Log((int)(Time.deltaTime * 1000));
    }

    public void Trigger (BoxCollider2D trigger, int id)
    {
        maxID = Mathf.Max(maxID, id);
        if(id == 0 && text.text == levelPrompt)
        {
            text.text = "";
            
        }
        if(id == 1)
        {
            Debug.Log("Trigger1");
            text.text = "Pick up the knife";
            trigger.enabled = false;
            door.SetActive(false);
        }
        if(id == 2 && hasKnife)
        {
            text.text = shiftPrompt;
        }
        if(id == 3)
        {
            text.text = "";
        }
    }
}
