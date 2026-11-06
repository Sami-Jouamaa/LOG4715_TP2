using UnityEngine;

// Les zones peuvent être sphériques ou des prismes (Box Collider).
public class AntiMagicZone : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Color zoneColor = new Color(0.8f, 0.8f, 0.8f, 0.35f); // light grey
    [SerializeField] private bool showVisual = true;

    [Header("Effets")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.2f;
    [SerializeField] private bool showParticles = true;
    [SerializeField] private Color particleColor = new Color(1f, 1f, 1f, 0.6f);

    private Renderer visualRenderer;
    private Material visualMaterial;
    private Collider zoneCollider;
    private PlayerControler player;
    private int contacts;

    private void Reset()
    {
        if (GetComponent<Collider>() == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(4f, 4f, 8f);
        }
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
        {
            Debug.LogError("AntiMagicZone requires a Collider component.", this);
            enabled = false;
            return;
        }
        zoneCollider.isTrigger = true;
        if (showVisual) BuildVisual();
    }

    private void Update()
    {
        if (visualMaterial == null) return;
        Color c = zoneColor;
        c.a = zoneColor.a * (1f - pulseAmount + pulseAmount * Mathf.Sin(Time.time * pulseSpeed));
        visualMaterial.color = c;
    }

    private void BuildVisual()
    {
        PrimitiveType type;
        Vector3 center, size;
        bool isBox = zoneCollider is BoxCollider;

        if (zoneCollider is BoxCollider box)
        {
            type = PrimitiveType.Cube;
            center = box.center;
            size = box.size;
        }
        else if (zoneCollider is SphereCollider sphere)
        {
            type = PrimitiveType.Sphere;
            center = sphere.center;
            size = Vector3.one * sphere.radius * 2f;
        }
        else return;

        var visual = GameObject.CreatePrimitive(type);
        visual.name = "ZoneVisual";
        Destroy(visual.GetComponent<Collider>());
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = center;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = size;

        visualRenderer = visual.GetComponent<Renderer>();
        visualMaterial = new Material(Shader.Find("Sprites/Default")) { color = zoneColor };
        visualRenderer.material = visualMaterial;
        visualRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        visualRenderer.receiveShadows = false;

        if (showParticles) BuildParticles(center, size, isBox);
    }

//Nouveau UI: Particules pour rendre ces zones speciales
    private void BuildParticles(Vector3 center, Vector3 size, bool isBox)
    {
        var go = new GameObject("ZoneParticles");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = center;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.startLifetime = 3f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = particleColor;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        float volume = size.x * size.y * size.z;
        var emission = ps.emission;
        emission.rateOverTime = Mathf.Clamp(volume * 0.4f, 8f, 60f);

        var shape = ps.shape;
        if (isBox)
        {
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = size.x * 0.5f;
        }

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.y = new ParticleSystem.MinMaxCurve(0.4f);
        vel.z = new ParticleSystem.MinMaxCurve(0f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var pr = go.GetComponent<ParticleSystemRenderer>();
        pr.material = new Material(Shader.Find("Sprites/Default"));

        ps.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponentInParent<PlayerControler>();
        if (p == null) return;
        player = p;
        if (++contacts == 1) player.EnterAntiMagicZone();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerControler>() == null) return;
        if (--contacts == 0 && player != null) player.ExitAntiMagicZone();
    }

    private void OnDisable()
    {
        if (player != null && contacts > 0) player.ExitAntiMagicZone();
        contacts = 0;
    }

    // Visible dans la Scene view seulement (pas dans le jeu).
    private void OnDrawGizmos()
    {
        if (!showVisual) return;

        var c = GetComponent<Collider>();
        if (c == null) return;

        // La matrice applique déjà position/rotation/échelle : on dessine en espace local.
        Gizmos.matrix = transform.localToWorldMatrix;
        Color wire = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);

        if (c is BoxCollider box)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = wire;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (c is SphereCollider sphere)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = wire;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}