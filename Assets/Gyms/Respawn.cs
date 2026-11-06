using UnityEngine;

public class Respawn : MonoBehaviour
{
    public bool isActive = false;
    Rigidbody _rb;
    PlayerControler player;
    [SerializeField] GameObject fireVFX;

    void Start()
    {
        fireVFX.SetActive(false);
        _rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerControler>();
        if (player == null || isActive) return;
        isActive = true;
        player.respawnPosition.z = _rb.transform.position.z;
        fireVFX.SetActive(true);
    }
}
