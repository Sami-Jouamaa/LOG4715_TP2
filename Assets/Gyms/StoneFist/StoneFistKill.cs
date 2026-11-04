
using UnityEngine;

public class StoneFistKill : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        PlayerControler player =
            collision.gameObject.GetComponent<PlayerControler>();

        if (player != null)
        {
            Debug.Log("STONE FIST HIT PLAYER!");
            player.TakeDamage(1);
        }
    }
}
