using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField]
    private Camera mainCamera = null;

    private Vector2 input;

    [SerializeField]
    private float speed = 3;

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

        transform.position = transform.position + (velocity * Time.deltaTime);

        UpdateLookRotation();
    }

    void UpdateLookRotation ()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        float angle = Vector2.SignedAngle(Vector2.up, mousePos - transform.position);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
