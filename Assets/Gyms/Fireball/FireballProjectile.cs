
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;

    [Header("Visual Rotation")]
    [SerializeField] private Transform visual;
    [SerializeField] private float rotationOffset = 0f;

    private Vector3 direction;
    private bool launched;

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

    private void Update()
    {
        if (!launched)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }
}
