using UnityEngine;

public class StoneFistTrapController : MonoBehaviour
{
    private const float PointReachDistance = 0.05f;

    [Header("References")]
    public Rigidbody stoneFist;
    public Transform[] railPoints;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float attackSpeed = 15f;
    public float rotationSpeed = 8f;

    [Header("Audio")]
    [Tooltip("AudioSource enfant du stone fist, clip StoneScrapping, Spatial Blend = 3D. " +
             "Jouée en boucle pendant la patrouille (idle), coupée pendant l'attaque.")]
    public AudioSource scrapingSource;

    private int currentPoint;
    private int targetPoint = 1;
    private int patrolDirection = 1;
    private int pursuitPoint;

    private bool pursuing;

    void Start()
    {
        if (stoneFist == null || railPoints == null || railPoints.Length < 2)
        {
            Debug.LogError("Assign the fist Rigidbody and at least two rail points!");
            enabled = false;
            return;
        }

        stoneFist.position = railPoints[0].position;
        stoneFist.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (scrapingSource != null)
        {
            scrapingSource.loop = true;
            scrapingSource.playOnAwake = false;
            UpdateScrapingSound();
        }
        else
        {
            Debug.LogWarning("Assign scrapingSource (AudioSource with StoneScrapping) for the idle sound.", this);
        }
    }

    public void TriggerAttack(Transform player)
    {
        if (player == null)
            return;

        pursuitPoint = FindClosestPoint(player.position);
        SetPursuing(true);
        targetPoint = FindFirstPursuitPoint();

        Debug.Log("Stone fist attack started!");
    }

    void FixedUpdate()
    {
        if (stoneFist == null || railPoints == null || railPoints.Length < 2)
            return;

        Vector3 target = railPoints[targetPoint].position;
        Vector3 direction = target - stoneFist.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stoneFist.MoveRotation(Quaternion.Slerp(
                stoneFist.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

        float speed = pursuing ? attackSpeed : patrolSpeed;
        stoneFist.MovePosition(Vector3.MoveTowards(
            stoneFist.position, target, speed * Time.fixedDeltaTime));

        if (Vector3.Distance(stoneFist.position, target) < PointReachDistance)
            ArriveAtTarget();
    }

    private void ArriveAtTarget()
    {
        currentPoint = targetPoint;

        if (pursuing && currentPoint == pursuitPoint)
        {
            SetPursuing(false);
            SetNextPatrolPoint();
        }
        else if (pursuing)
        {
            SetNextPursuitPoint();
        }
        else
        {
            SetNextPatrolPoint();
        }
    }

    // Centralise le changement d'état pour synchroniser le son idle.
    private void SetPursuing(bool value)
    {
        if (pursuing == value)
            return;

        pursuing = value;
        UpdateScrapingSound();
    }

    // Idle (patrouille) = StoneScrapping en boucle ; attaque = silence (l'impact prend le relais).
    private void UpdateScrapingSound()
    {
        if (scrapingSource == null)
            return;

        if (pursuing)
        {
            if (scrapingSource.isPlaying)
                scrapingSource.Stop();
        }
        else if (!scrapingSource.isPlaying)
        {
            scrapingSource.Play();
        }
    }

    private int FindFirstPursuitPoint()
    {
        if (IsAtPoint(currentPoint))
            return currentPoint;

        if (!IsValidPoint(targetPoint))
            return FindClosestPoint(stoneFist.position);

        float distanceFromCurrent = Vector3.Distance(
            stoneFist.position, railPoints[currentPoint].position) +
            GetRailDistance(currentPoint, pursuitPoint);
        float distanceFromTarget = Vector3.Distance(
            stoneFist.position, railPoints[targetPoint].position) +
            GetRailDistance(targetPoint, pursuitPoint);

        return distanceFromCurrent <= distanceFromTarget
            ? currentPoint
            : targetPoint;
    }

    private float GetRailDistance(int fromPoint, int toPoint)
    {
        float distance = 0f;
        int direction = toPoint > fromPoint ? 1 : -1;

        while (fromPoint != toPoint)
        {
            int nextPoint = fromPoint + direction;
            distance += Vector3.Distance(
                railPoints[fromPoint].position, railPoints[nextPoint].position);
            fromPoint = nextPoint;
        }

        return distance;
    }

    private void SetNextPursuitPoint()
    {
        targetPoint = currentPoint + (pursuitPoint > currentPoint ? 1 : -1);
    }

    private void SetNextPatrolPoint()
    {
        if (currentPoint == railPoints.Length - 1)
            patrolDirection = -1;
        else if (currentPoint == 0)
            patrolDirection = 1;

        targetPoint = currentPoint + patrolDirection;
    }

    private int FindClosestPoint(Vector3 position)
    {
        int closestPoint = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < railPoints.Length; i++)
        {
            float distance = (position - railPoints[i].position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = i;
            }
        }

        return closestPoint;
    }

    private bool IsAtPoint(int point)
    {
        return Vector3.Distance(stoneFist.position, railPoints[point].position) <
            PointReachDistance;
    }

    private bool IsValidPoint(int point)
    {
        return point >= 0 && point < railPoints.Length;
    }
}