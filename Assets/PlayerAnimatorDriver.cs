using UnityEngine;

public class PlayerAnimatorDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;        // Animator du MANNEQUIN
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float speedSmoothing = 12f;

    [Header("Lecture seule (pour tester)")]
    [SerializeField] private float debugSpeed;

    Vector3 lastPos;
    float smoothSpeed;

    static readonly int Speed      = Animator.StringToHash("Speed");
    static readonly int VelocityY  = Animator.StringToHash("VelocityY");
    static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    static readonly int Cast       = Animator.StringToHash("Cast");
    static readonly int Punch      = Animator.StringToHash("Punch");
    static readonly int Hurt       = Animator.StringToHash("Hurt");
    static readonly int Dead       = Animator.StringToHash("Dead");
    static readonly int Victory    = Animator.StringToHash("Victory");

    void Awake()
    {
        if (bodyCollider == null) bodyCollider = GetComponent<Collider>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        if (animator == null || Time.deltaTime <= 0f) return;

        // Vitesse réelle calculée à partir du déplacement, quelle que soit la méthode de mouvement
        Vector3 delta = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;

        float horizontal = new Vector3(delta.x, 0f, delta.z).magnitude;
        smoothSpeed = Mathf.Lerp(smoothSpeed, horizontal, speedSmoothing * Time.deltaTime);
        debugSpeed = smoothSpeed;

        animator.SetFloat(Speed, smoothSpeed);
        animator.SetFloat(VelocityY, delta.y);
        animator.SetBool(IsGrounded, CheckGrounded());
    }

    bool CheckGrounded()
    {
        Bounds b = bodyCollider.bounds;
        return Physics.Raycast(b.center, Vector3.down, b.extents.y + 0.1f,
                               groundMask, QueryTriggerInteraction.Ignore);
    }

    public void PlayCast()  => animator.SetTrigger(Cast);
    public void PlayPunch() => animator.SetTrigger(Punch);
    public void PlayHurt()  => animator.SetTrigger(Hurt);
    public void PlayDeath() => animator.SetBool(Dead, true);
    public void SetVictory(bool on) => animator.SetBool(Victory, on);
}