using System;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    // time (seconds) between each shot
    [SerializeField] float fireInterval = 1.0f;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] Vector3 shootDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(Shoot), 0f, fireInterval);
    }

    void Shoot()
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, shootDirection.normalized);
        Instantiate(arrowPrefab, firePoint.position, rotation);
    }
}
