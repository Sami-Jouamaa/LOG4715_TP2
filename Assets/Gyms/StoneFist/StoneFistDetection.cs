
using UnityEngine;
using System.Collections.Generic;

public class StoneFistDetection : MonoBehaviour
{
    public StoneFistTrapController trapController;

    private readonly Collider[] overlappingColliders = new Collider[32];
    private readonly HashSet<Transform> detectedProjectiles = new HashSet<Transform>();
    private readonly HashSet<Transform> overlappingProjectiles = new HashSet<Transform>();
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
        DetectProjectile(other);
    }

    private void OnTriggerStay(Collider other)
    {
        DetectPlayer(other);
        DetectProjectile(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Transform projectile = FindProjectile(other.transform);
        if (projectile != null)
            detectedProjectiles.Remove(projectile);
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
        overlappingProjectiles.Clear();
        for (int i = 0; i < colliderCount; i++)
        {
            Collider overlappingCollider = overlappingColliders[i];
            PlayerControler player = overlappingCollider.GetComponentInParent<PlayerControler>();

            if (player != null)
            {
                playerInside = true;
                DetectPlayer(player);
            }

            Transform projectile = FindProjectile(overlappingCollider.transform);
            if (projectile != null)
            {
                overlappingProjectiles.Add(projectile);
                DetectProjectile(projectile);
            }
        }

        if (!playerInside)
            detectedPlayer = null;

        detectedProjectiles.RemoveWhere(
            projectile => projectile == null || !overlappingProjectiles.Contains(projectile));
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

    private void DetectProjectile(Collider other)
    {
        Transform projectile = FindProjectile(other.transform);
        DetectProjectile(projectile);
    }

    private void DetectProjectile(Transform projectile)
    {
        if (projectile == null || !detectedProjectiles.Add(projectile))
            return;

        Debug.Log("PROJECTILE DETECTED! Activating stone fist");
        if (trapController != null)
            trapController.TriggerAttack(projectile);
    }

    private Transform FindProjectile(Transform candidate)
    {
        for (Transform current = candidate; current != null; current = current.parent)
        {
            if (current.CompareTag("Projectile"))
                return current;
        }

        return null;
    }

    private void HandlePlayerRespawned(PlayerControler player)
    {
        if (player == detectedPlayer)
            detectedPlayer = null;
    }
}
