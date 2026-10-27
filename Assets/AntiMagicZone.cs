using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AntiMagicZone : MonoBehaviour

{
    [Header("Visuals")]
    [SerializeField] private Color zoneColor = new Color(0.8f, 0.8f, 0.8f, 0.35f); //light grey
    [SerializeField] private bool showVisual = true;
    private PlayerControler player;
    private int contacts;

    private void Reset()
    {
        SphereCollider collider = GetComponent<SphereCollider>(); //var
        collider.isTrigger = true;
        collider.radius = 5f;
    }

    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponent<PlayerControler>();
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
        var c = GetComponent<SphereCollider>();
        Gizmos.color = zoneColor;
        Color wire = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position + c.center, c.radius * transform.lossyScale.x);
        Gizmos.color = wire;
        Gizmos.DrawWireSphere(transform.position + c.center, c.radius * transform.lossyScale.x);
    }
}