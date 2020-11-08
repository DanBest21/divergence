using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField]
    private Camera mainCamera = null;

    private Vector2 input;

    [SerializeField]
    private float speed = 3;

    [SerializeField]
    private float collisionRadius = 1;

    [SerializeField]
    private float skinWidth = 0.01f;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"), 
            Input.GetAxisRaw("Vertical")
            );

        if(input.sqrMagnitude > 1)
        {
            input = input.normalized;
        }

        Vector3 velocity = input * speed;

        Move(velocity);

        UpdateLookRotation();
    }

    private void Move(Vector2 velocity)
    {
        Vector2 motion = velocity * Time.deltaTime;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, collisionRadius, motion, motion.magnitude);

        if(hit)
        {
            float a = velocity.x / hit.normal.x;
            float b = velocity.y / hit.normal.y;

            float free = hit.distance - skinWidth;
            float slide = motion.magnitude - free;

            motion = motion.normalized;

            motion = motion * free + (Vector2)Vector3.Project(motion * slide, Vector2.Perpendicular(hit.normal));
        }


        transform.position = transform.position + new Vector3(motion.x, motion.y);


    }

    void UpdateLookRotation ()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        float angle = Vector2.SignedAngle(Vector2.up, mousePos - transform.position);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
