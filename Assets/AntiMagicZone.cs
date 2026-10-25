using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AntiMagicZone : MonoBehaviour

{
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

    private void onDisable()
    {
        if (player != null && contacts > 0)
        {
            player.ExitAntiMagicZone();
            contacts = 0;
        }
    }

    private void OnDrawGizmos()
    {
        var c = GetComponent<SphereCollider>();
        Gizmos.color = new Color(1f, 0.2f, 0.8f, 0.6f);
        Gizmos.DrawWireSphere(transform.position + c.center, c.radius * transform.lossyScale.x);
    }
}