
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifetime = 3f;

    [Header("Visual Rotation")]
    [SerializeField] private Transform visual;
    [SerializeField] private float rotationOffset = 0f;

    private Vector3 direction;
    private bool launched;
    private Rigidbody body;

    public void Launch(Vector3 launchDirection)
    {
        direction = launchDirection.normalized;
        direction.x = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            Destroy(gameObject);
            return;
        }

        direction.Normalize();
        gameObject.tag = "Projectile";
        body = GetComponent<Rigidbody>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        SphereCollider projectileCollider = GetComponent<SphereCollider>();
        if (projectileCollider == null)
            projectileCollider = gameObject.AddComponent<SphereCollider>();
        projectileCollider.isTrigger = true;
        projectileCollider.radius = 0.5f;

        if (visual == null)
            visual = transform;

        float angle = Mathf.Atan2(
            direction.y,
            direction.z
        ) * Mathf.Rad2Deg;

        visual.localRotation = Quaternion.Euler(
            0f,
            90f,
            -angle + rotationOffset
        );

        launched = true;
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!launched)
            return;

        body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
    }
}
