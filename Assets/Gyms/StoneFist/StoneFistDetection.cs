
using UnityEngine;

public class StoneFistDetection : MonoBehaviour
{
    public StoneFistTrapController trapController;

    private readonly Collider[] overlappingColliders = new Collider[16];
    private Collider detectionZone;
    private PlayerControler detectedPlayer;

    private void Awake()
    {
        detectionZone = GetComponent<Collider>();
        if (detectionZone == null || !detectionZone.isTrigger)
        {
            Debug.LogError("Stone fist detection requires a trigger collider.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        PlayerControler.Respawned += HandlePlayerRespawned;
    }

    private void OnDisable()
    {
        PlayerControler.Respawned -= HandlePlayerRespawned;
    }

    private void OnTriggerEnter(Collider other)
    {
        DetectPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        DetectPlayer(other);
    }

    private void FixedUpdate()
    {
        Bounds bounds = detectionZone.bounds;
        int colliderCount = Physics.OverlapBoxNonAlloc(
            bounds.center,
            bounds.extents,
            overlappingColliders,
            Quaternion.identity,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        bool playerInside = false;
        for (int i = 0; i < colliderCount; i++)
        {
            PlayerControler player =
                overlappingColliders[i].GetComponentInParent<PlayerControler>();

            if (player == null)
                continue;

            playerInside = true;
            DetectPlayer(player);
            break;
        }

        if (!playerInside)
            detectedPlayer = null;
    }

    private void DetectPlayer(Collider other)
    {
        PlayerControler player = other.GetComponentInParent<PlayerControler>();
        DetectPlayer(player);
    }

    private void DetectPlayer(PlayerControler player)
    {
        if (player == null || player == detectedPlayer)
            return;

        detectedPlayer = player;
        Debug.Log("PLAYER DETECTED! Activating stone fist");

        if (trapController != null)
            trapController.TriggerAttack(player.transform);
    }

    private void HandlePlayerRespawned(PlayerControler player)
    {
        if (player == detectedPlayer)
            detectedPlayer = null;
    }
}
