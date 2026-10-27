using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] float speed = 15.0f;
    [SerializeField] float lifetime = 5.0f;
    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.linearVelocity = transform.up * speed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
