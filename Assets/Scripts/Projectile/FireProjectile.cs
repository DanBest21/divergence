using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProjectile : MonoBehaviour
{  
    [SerializeField]
    private GameObject projectile;
    private GameObject firedProjectile;
    private MoveProjectile moveProjectile;

    [SerializeField]
    private bool canFire = true;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    [Range(0f, 1f)]
    private float pickupRange;

    [SerializeField]
    private LayerMask pickup;

    public GameObject Projectile() { return projectile; }

    public bool CanFire() { return canFire; }

    public MeshFilter MeshFilter() { return meshFilter; }

    public float PickupRange() { return pickupRange; }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && canFire && TimeManager.Instance.Flow > 0)
        {
            canFire = false;

            Vector3 mousePos = transform.GetComponent<PlayerMovement>().MainCamera().ScreenToWorldPoint(Input.mousePosition);

            float angle = Vector2.SignedAngle(Vector2.up, mousePos - transform.position);

            firedProjectile = Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, angle));

            projectile.SetActive(false);

            Vector2 direction = ((Vector2)(mousePos - transform.position)).normalized;

            moveProjectile = firedProjectile.GetComponent<MoveProjectile>();
            moveProjectile.Setup(direction, meshFilter.mesh, this);
        }

        if (!canFire && moveProjectile.HasStopped())
        {
            if (Physics2D.Raycast(transform.position, (moveProjectile.transform.position - transform.position).normalized, pickupRange, pickup))
            {
                Destroy(firedProjectile);
                projectile.SetActive(true);
                canFire = true;
            }
        }
    }
}
