using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ConjureSlime : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private PlayerControler player;

    [Header("Coût et entrée")]
    [SerializeField] private int manaCost = 3;
    [SerializeField] private KeyCode activateKey = KeyCode.C;
    [SerializeField] private KeyCode conjureKey = KeyCode.E;
    [SerializeField] private bool exitAfterPlace = true;
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private float cooldown = 0.3f;

    [Header("Placement")]
    [SerializeField] private Vector3 moveAxis = Vector3.forward;
    [SerializeField] private float placeRange = 2.5f;
    [SerializeField] private float aimHeight = 0.8f;
    [SerializeField] private float floorLookAhead = 1.2f;
    [SerializeField] private LayerMask placeableSurfaces = ~0;

    [Header("Slime")]
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private float slimeSize = 1.5f;
    [SerializeField] private float slimeThickness = 0.15f;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private int maxActiveSlimes = 2;
    [SerializeField] private Color slimeColor = new Color(0.35f, 0.9f, 0.3f, 0.85f);

    [Header("Contact avec le slime")]
    [SerializeField] private bool stickyWalls = true;
    [SerializeField] private float wallSlideSpeed = 1.5f;
    [SerializeField] private float regenDelay = 1.5f;

    [Header("Rétroaction visuelle")]
    [SerializeField] private bool showPreview = true;
    [SerializeField] private Color previewOk = new Color(0.35f, 0.9f, 0.3f, 0.35f);
    [SerializeField] private Color previewNoMana = new Color(0.9f, 0.3f, 0.3f, 0.35f);


    public bool IsAiming { get; private set; }
    private Rigidbody rb;
    private float dir = 1f;
    private float cooldownEnd;
    private float regenTimer;
    private readonly List<SlimePatch> active = new List<SlimePatch>();
    private readonly HashSet<SlimePatch> touching = new HashSet<SlimePatch>();
    private GameObject preview;
    private Material previewMat;
    private static Mesh sphereMesh;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (player == null) player = GetComponent<PlayerControler>();
        if (showPreview) BuildPreview();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw(horizontalAxis);
        if (Mathf.Abs(h) > 0.1f) dir = Mathf.Sign(h);

        if (Input.GetKeyDown(activateKey)) IsAiming = !IsAiming;

        bool canPlace = false;
        Vector3 point = Vector3.zero, normal = Vector3.up;
        if (IsAiming) canPlace = TryGetPlacement(out point, out normal);
        UpdatePreview(canPlace, point, normal);

        if (IsAiming &&Input.GetKeyDown(conjureKey) && canPlace)
        {
            if (Conjure(point, normal) && exitAfterPlace) IsAiming = false;

        }

        RegenWhileTouching();
    }

    private void FixedUpdate()
    {
        if (!stickyWalls || touching.Count == 0) return;

        bool onWall = false;
        foreach (var s in touching) if (s != null && s.IsWall) onWall = true;

        if (onWall && rb.linearVelocity.y < -wallSlideSpeed)
        {
            Vector3 v = rb.linearVelocity;
            v.y = -wallSlideSpeed;
            rb.linearVelocity = v;
        }
    }

    private bool TryGetPlacement(out Vector3 point, out Vector3 normal)
    {
        Vector3 axis = moveAxis.normalized * dir;
        Vector3 origin = transform.position + Vector3.up * aimHeight;
        bool preferFloor = Input.GetAxisRaw(verticalAxis) < -0.5f;

        RaycastHit hit;
        if (!preferFloor && FirstValidHit(origin, axis, placeRange, out hit))
        {
            point = hit.point; normal = hit.normal; return true;
        }

        Vector3 ahead = origin + axis * floorLookAhead;
        if (FirstValidHit(ahead, Vector3.down, aimHeight + 1f, out hit))
        {
            point = hit.point; normal = hit.normal; return true;
        }

        point = Vector3.zero; normal = Vector3.up;
        return false;
    }

    private bool FirstValidHit(Vector3 origin, Vector3 direction, float distance, out RaycastHit result)
    {
        var hits = Physics.RaycastAll(origin, direction, distance, placeableSurfaces, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            if (h.collider.GetComponentInParent<PlayerControler>() != null) continue;
            if (h.collider.GetComponentInParent<SlimePatch>() != null) continue;
            result = h;
            return true;
        }
        result = default;
        return false;
    }

       private bool Conjure(Vector3 point, Vector3 normal)
    {
        if (Time.time < cooldownEnd) return false;
        if (player.Current_Mana < manaCost) return false; // pas assez de mana : refusé
 
        player.Current_Mana -= manaCost;
        cooldownEnd = Time.time + cooldown;
 
        Vector3 pos = point + normal * (slimeThickness * 0.5f - 0.01f);
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
 
        GameObject go;
        if (slimePrefab != null)
        {
            go = Instantiate(slimePrefab, pos, rot);
            if (go.GetComponentInChildren<Collider>() == null) go.AddComponent<BoxCollider>();
        }
        else
        {
            go = new GameObject("SlimePatch");
            go.transform.SetPositionAndRotation(pos, rot);
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(slimeSize, slimeThickness, slimeSize);
            BuildVisual(go.transform, slimeColor);
        }
 
        var patch = go.GetComponent<SlimePatch>();
        if (patch == null) patch = go.AddComponent<SlimePatch>();
        patch.Init(this, player, normal.y < 0.5f, slimeColor, lifetime);
 
        active.RemoveAll(s => s == null);
        active.Add(patch);
        while (active.Count > maxActiveSlimes)
        {
            active[0].Dismiss();
            active.RemoveAt(0);
        }
        return true;
    }

    public void NotifyEnter(SlimePatch patch)
    {
        touching.Add(patch);
        player.SetGrounded(true); // le saut est réinitialisé
    }

    public void NotifyExit(SlimePatch patch)
    {
        touching.Remove(patch);
        if (touching.Count == 0 && !player.IsStandingOnGround())
            player.SetGrounded(false);
    }

// Régénération de mana sur un SLime 
    private void RegenWhileTouching()
    {
        touching.RemoveWhere(s => s == null);

        // Au sol, la régénération de PlayerControler s'applique déjà.
        if (touching.Count > 0 && !player.IsGrounded) //&& player.Current_Mana < player.Max_Mana)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
            {
                regenTimer = 0f;
                player.Current_Mana++;
            }
        }
        else regenTimer = 0f;
    }

    private Renderer BuildVisual(Transform parent, Color color)
    {
        if (sphereMesh == null)
        {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(tmp);
        }

        var visual = new GameObject("Visual");
        visual.transform.SetParent(parent, false);
        visual.transform.localScale = new Vector3(slimeSize, slimeThickness * 1.6f, slimeSize);
        visual.AddComponent<MeshFilter>().sharedMesh = sphereMesh;

        var r = visual.AddComponent<MeshRenderer>();
        r.material = new Material(Shader.Find("Sprites/Default")) { color = color };
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return r;
    }

    private void BuildPreview()
    {
        preview = new GameObject("SlimePreview");
        var r = BuildVisual(preview.transform, previewOk);
        previewMat = r.material;
        preview.SetActive(false);
    }

    private void UpdatePreview(bool valid, Vector3 point, Vector3 normal)
    {
        if (preview == null) return;
        preview.SetActive(valid);
        if (!valid) return;

        preview.transform.SetPositionAndRotation(
            point + normal * (slimeThickness * 0.5f - 0.01f),
            Quaternion.FromToRotation(Vector3.up, normal));
        previewMat.color = player.Current_Mana >= manaCost ? previewOk : previewNoMana;
    }

    private void OnDestroy()
    {
        if (preview != null) Destroy(preview);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 o = transform.position + Vector3.up * aimHeight;
        Vector3 axis = moveAxis.normalized;
        Gizmos.DrawLine(o, o + axis * placeRange);
        Gizmos.DrawLine(o, o - axis * placeRange);
    }
}
