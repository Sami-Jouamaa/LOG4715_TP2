using UnityEngine;

/// <summary>
/// Sort Cloudburst : dash horizontal uniquement (axe Z), coûte du mana,
/// distance équivalente à celle d'un saut normal.
/// À placer sur le joueur, à côté de PlayerControler.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CloudBurst : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private PlayerControler player;

    [Header("Coût et entrée")]
    [Tooltip("Mana consommé par dash.")]
    [SerializeField] private int manaCost = 1;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private string horizontalAxis = "Horizontal";

    [Header("Dash")]
    [Tooltip("Axe monde du déplacement horizontal (le joueur se déplace sur Z).")]
    [SerializeField] private Vector3 moveAxis = Vector3.forward;
    [Tooltip("Distance parcourue (m). Clic droit sur le composant > 'Aligner la distance sur un saut normal'.")]
    [SerializeField] private float dashDistance = 5.7f;
    [Tooltip("Durée du dash (s). Vitesse = distance / durée.")]
    [SerializeField] private float dashDuration = 0.2f;
    [Tooltip("Délai minimal entre deux dashs (s).")]
    [SerializeField] private float cooldown = 0.25f;
    [Tooltip("Coupe la gravité pendant le dash pour qu'il reste strictement horizontal.")]
    [SerializeField] private bool disableGravityDuringDash = true;

    [Header("Rétroaction visuelle")]
    [Tooltip("Créé automatiquement si vide.")]
    [SerializeField] private TrailRenderer trail;
    [Tooltip("Optionnel : particules jouées au début du dash.")]
    [SerializeField] private ParticleSystem cloudBurstFx;
    [SerializeField] private Color trailColor = new Color(0.8f, 0.9f, 1f, 0.8f);

    public bool IsDashing { get; private set; }
    public float CooldownRemaining => Mathf.Max(0f, cooldownEnd - Time.time);


    private Rigidbody rb;
    private float dashEnd;
    private float cooldownEnd;
    private float dir = 1f;
    private bool previousGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (player == null) player = GetComponent<PlayerControler>();

        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.4f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = trailColor;
            trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        }
        trail.emitting = false;
    }

    private void Update()
    {
        // Retient la dernière direction demandée (même logique que le flip du joueur).
        float h = Input.GetAxisRaw(horizontalAxis);
        if (Mathf.Abs(h) > 0.1f && !IsDashing) dir = Mathf.Sign(h);

        if (Input.GetKeyDown(dashKey)) TryDash();
    }

    private void FixedUpdate()
    {
        if (!IsDashing) return;
        if (Time.time >= dashEnd) { EndDash(); return; }

        rb.linearVelocity = moveAxis.normalized * (dir * dashDistance / dashDuration);
    }

    private void TryDash()
    {
        if (IsDashing || Time.time < cooldownEnd) return;
        if (!player.TrySpendMana(manaCost)) return; // pas assez de mana : refusé
        IsDashing = true;
        dashEnd = Time.time + dashDuration;

        previousGravity = rb.useGravity;
        if (disableGravityDuringDash) rb.useGravity = false;

        // PlayerControler écrase linearVelocity.z à chaque Update : on le suspend le temps du dash.
        player.enabled = false;
        trail.Clear();
        trail.emitting = true;
        if (cloudBurstFx != null) cloudBurstFx.Play();
    }

    private void EndDash()
    {
        IsDashing = false;
        cooldownEnd = Time.time + cooldown;

        rb.useGravity = previousGravity;
        rb.linearVelocity = Vector3.zero;
        player.enabled = true;
        trail.emitting = false;
    }

    /// <summary>
    /// Distance d'un saut normal = vitesse de marche * temps de vol.
    /// Le saut est une impulsion : v0 = JumpForce / masse, temps de vol = 2 * v0 / g.
    /// </summary>
    [ContextMenu("Aligner la distance sur un saut normal")]
    private void MatchJumpDistance()
    {
        if (player == null) player = GetComponent<PlayerControler>();
        var body = GetComponent<Rigidbody>();
        float g = Mathf.Abs(Physics.gravity.y);
        float v0 = player.JumpForceValue / body.mass;
        dashDistance = player.MoveSpeedValue * (2f * v0 / g);
        Debug.Log($"Cloudburst : dashDistance = {dashDistance:F2} m");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 a = transform.position + Vector3.up;
        Vector3 axis = moveAxis.normalized;
        Gizmos.DrawLine(a, a + axis * dashDistance);
        Gizmos.DrawLine(a, a - axis * dashDistance);
    }
}