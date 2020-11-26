using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField]
    private float collisionRadius = 1;

    [SerializeField]
    private float skinWidth = 0.01f;

    [SerializeField]
    private LayerMask collisionMask;

    public void Move (Vector2 motion)
    {

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, collisionRadius, motion, motion.magnitude, collisionMask.value);

        if(hit)
        {
            float free = hit.distance - skinWidth;
            float slide = motion.magnitude - free;

            motion = motion.normalized;

            Vector3 freeMotion = motion * free;
            Vector2 slideMotion = Vector3.Project(motion * slide, Vector2.Perpendicular(hit.normal));

            RaycastHit2D hit2 = Physics2D.CircleCast(transform.position + freeMotion, collisionRadius, slideMotion, slideMotion.magnitude, collisionMask.value);

            motion = (Vector2)freeMotion + slideMotion.normalized * Mathf.Max(((hit2 ? hit2.distance : slideMotion.magnitude) - skinWidth), 0);
        }


        transform.position = transform.position + new Vector3(motion.x, motion.y);
    }

    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
        Gizmos.DrawWireSphere(transform.position, collisionRadius + skinWidth);
    }
}
