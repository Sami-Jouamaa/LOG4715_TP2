using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        PlayerControler player = collision.collider.GetComponentInParent<PlayerControler>();
        if (player != null) player.KillByFall();
    }
}