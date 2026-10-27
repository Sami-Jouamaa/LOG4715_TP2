using UnityEngine;

//Les zones peuvent etre spheriques ou des prismes (box collider)
public class AntiMagicZone : MonoBehaviour

{
    [Header("Visuals")]
    [SerializeField] private Color zoneColor = new Color(0.8f, 0.8f, 0.8f, 0.35f); //light grey
    [SerializeField] private bool showVisual = true;

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
            //Important : si le collider est manquant, on désactive le script pour éviter des erreurs null reference.
            Debug.LogError("AntiMagicZone requires a Collider component.");
            enabled = false;
            return;
        }
        zoneCollider.isTrigger = true;
        if (showVisual) BuildVisual();
    }

    private void BuildVisual()
    {
        PrimitiveType type;
        Vector3 center, size;

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

        var rend = visual.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Sprites/Default")) { color = zoneColor };
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponentInParent<PlayerControler>();
        if (p != null)
        {
           player = p;
           if (++contacts == 1)
           {
               player.EnterAntiMagicZone();
           }
        }
  
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerControler>() == null) return;
        if (--contacts == 0 && player != null) player.ExitAntiMagicZone();
    }

    private void OnDisable()
    {
        if (player != null && contacts > 0)
        {
            player.ExitAntiMagicZone();
            contacts = 0;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showVisual) return;


        var c = GetComponent<Collider>();
        if (c == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Color wire = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);

        if (c is BoxCollider box)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = wire;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        if (c is SphereCollider sphere)
        {
            Gizmos.color = zoneColor;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
            Gizmos.color = wire;
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
        }
    }
}