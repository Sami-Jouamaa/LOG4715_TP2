using UnityEngine;

public class StoneFistKill : MonoBehaviour
{
    [Header("Impact sound")]
    [Tooltip("Layers qui déclenchent le son d'impact (sol, murs...). Le joueur est géré via son tag.")]
    public LayerMask impactLayers = ~0;

    [Tooltip("Vitesse relative minimale pour jouer l'impact sur le sol. " +
             "Doit être > patrolSpeed et < attackSpeed du contrôleur.")]
    public float minGroundImpactSpeed = 5f;

    [Tooltip("Délai minimal entre deux sons d'impact (évite le spam si le poing rebondit).")]
    public float impactCooldown = 0.25f;

    private float lastImpactTime = -999f;

    private void OnCollisionEnter(Collision collision)
    {
        // Impact sur le joueur : toujours un son + dégâts.
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayImpactSound(collision);

            PlayerControler player =
                collision.gameObject.GetComponent<PlayerControler>();
            if (player != null)
            {
                Debug.Log("STONE FIST HIT PLAYER!");
                player.TakeDamage(1);
            }
            return;
        }

        // Impact sur le sol : seulement si le poing frappe assez fort
        // (pas pendant la patrouille lente).
        bool inImpactLayer =
            (impactLayers.value & (1 << collision.gameObject.layer)) != 0;

        if (inImpactLayer && collision.relativeVelocity.magnitude >= minGroundImpactSpeed)
            PlayImpactSound(collision);
    }

    private void PlayImpactSound(Collision collision)
    {
        if (Time.time - lastImpactTime < impactCooldown)
            return;
        lastImpactTime = Time.time;

        if (AudioManager.Instance == null)
            return;

        Vector3 point = collision.contactCount > 0
            ? collision.GetContact(0).point
            : transform.position;

        // Son 3D positionné au point de contact.
        AudioManager.Instance.PlayAt("StoneFist", point);
    }
}