
using UnityEngine;

public class FireballSpell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerControler player;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera aimCamera;

    [Header("Spell Settings")]
    [SerializeField] private int manaCost = 1;
    [SerializeField] private float spawnOffset = 0.8f;
    [SerializeField] private KeyCode castKey = KeyCode.F;

    [Header("Fireball Recoil")]
    [SerializeField] private float recoilForce = 7f;
    [SerializeField] private float maxVerticalSpeed = 8f;

    private Rigidbody playerRb;

    private void Awake()
    {
        if (player == null)
            player = GetComponentInParent<PlayerControler>();

        if (player != null)
            playerRb = player.GetComponent<Rigidbody>();

        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(castKey))
            Cast();
    }

    public void Cast()
    {
        if (player == null || projectilePrefab == null)
            return;

        if (aimCamera == null)
        {
            Debug.LogError(
                "FireballSpell: Assign your gameplay camera."
            );
            return;
        }
        
        if (playerRb == null)
            playerRb = player.GetComponent<Rigidbody>();

        Plane gameplayPlane = new Plane(
            Vector3.right,
            player.transform.position
        );

        Ray mouseRay = aimCamera.ScreenPointToRay(
            Input.mousePosition
        );

        if (!gameplayPlane.Raycast(mouseRay, out float distance))
            return;

        Vector3 mouseWorldPosition = mouseRay.GetPoint(distance);

        Vector3 aimDirection =
            mouseWorldPosition - player.transform.position;

        aimDirection.x = 0f;

        if (aimDirection.sqrMagnitude < 0.001f)
            return;

        aimDirection.Normalize();

        Vector3 spawnPosition = spawnPoint != null
            ? spawnPoint.position
            : player.transform.position;

        spawnPosition += aimDirection * spawnOffset;

        spawnPosition.x = player.transform.position.x;

        if (projectilePrefab.GetComponent<FireballProjectile>() == null)
        {
            Debug.LogError(
                "Fireball prefab needs FireballProjectile on its root."
            );
            return;
        }

        if (!player.TrySpendMana(manaCost))
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        FireballProjectile fireball =
            projectile.GetComponent<FireballProjectile>();

        fireball.Launch(aimDirection);

        if (playerRb == null || recoilForce <= 0f)
            return;

        Vector3 recoil = -aimDirection * recoilForce;
        Vector3 velocity = playerRb.linearVelocity + recoil;

        if (Mathf.Abs(recoil.y) > 0.001f)
            velocity.y = Mathf.Clamp(
                velocity.y,
                -maxVerticalSpeed,
                maxVerticalSpeed
            );

        player.ApplyVelocityChange(velocity - playerRb.linearVelocity);
    }
}
