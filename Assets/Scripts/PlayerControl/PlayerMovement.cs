using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController2D characterController;

    public bool IsAlive { get; private set; } = true;

    [SerializeField]
    private Camera mainCamera = null;

    private Vector2 input;

    [SerializeField]
    private float speed = 3;

    public Camera MainCamera() { return mainCamera; }

    private void Awake ()
    {
        characterController = GetComponent<CharacterController2D>();
    }

    void Update()
    {
        if(!IsAlive)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
            return;
        }

        input = new Vector2(
            Input.GetAxisRaw("Horizontal"), 
            Input.GetAxisRaw("Vertical")
            );

        if(input.sqrMagnitude > 1)
        {
            input = input.normalized;
        }

        Vector3 velocity = input * speed;

        characterController.Move(velocity * Time.deltaTime);

        UpdateLookRotation();
    }

    void UpdateLookRotation ()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        float angle = Vector2.SignedAngle(Vector2.up, mousePos - transform.position);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Kill ()
    {
        IsAlive = false;
    }
}
