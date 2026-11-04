using UnityEngine;

public class TelekinesisSpell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerControler player;
    [SerializeField] private Camera aimCamera;

    [Header("Spell Settings")]
    [SerializeField] private string interactableTag = "Telekinesable";
    [SerializeField] private KeyCode activationKey = KeyCode.T;
    [SerializeField] private int manaCost = 1;
    [SerializeField] private int manaPerDrain = 1;
    [SerializeField] private float manaDrainInterval = 1f;
    [SerializeField] private float hoverDuration = 6f;
    [SerializeField] private float raycastDistance = 100f;

    private Rigidbody controlledBody;
    private Collider[] controlledColliders;
    private readonly RaycastHit[] movementHits = new RaycastHit[16];
    private int playerLayerMask;
    private Plane movementPlane;
    private bool isHolding;
    private bool isHovering;
    private float dropTime;
    private float nextManaDrainTime;

    private void Awake()
    {
        if (player == null)
            player = GetComponentInParent<PlayerControler>();

        if (aimCamera == null)
            aimCamera = Camera.main;
    }

    private void Update()
    {
        if (isHolding && Input.GetKeyUp(activationKey))
        {
            isHolding = false;
            isHovering = true;
            dropTime = Time.time + hoverDuration;
        }

        if (isHovering && Time.time >= dropTime)
        {
            DropObject();
            return;
        }

        if (controlledBody != null && Time.time >= nextManaDrainTime)
        {
            if (!player.TrySpendMana(manaPerDrain))
            {
                DropObject();
                return;
            }

            nextManaDrainTime = Time.time + Mathf.Max(0.01f, manaDrainInterval);
        }

        if (controlledBody == null && Input.GetKeyDown(activationKey))
            TryPickUpObject();
    }

    private void FixedUpdate()
    {
        if (!isHolding || controlledBody == null || !Input.GetKey(activationKey))
            return;

        Ray mouseRay = aimCamera.ScreenPointToRay(Input.mousePosition);
        if (movementPlane.Raycast(mouseRay, out float distance))
            MoveObjectWithoutPushingPlayer(mouseRay.GetPoint(distance));
    }

    private void TryPickUpObject()
    {
        if (player == null)
        {
            Debug.LogError("TelekinesisSpell: Assign a PlayerControler.", this);
            return;
        }

        if (aimCamera == null)
        {
            Debug.LogError("TelekinesisSpell: Assign the gameplay camera.", this);
            return;
        }

        Ray mouseRay = aimCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(mouseRay, out RaycastHit hit, raycastDistance))
        {
            Debug.Log("Telekinesis: no object under the mouse.", this);
            return;
        }

        Transform interactable = FindInteractable(hit.collider.transform);
        if (interactable == null)
        {
            Debug.Log("Telekinesis: aim at an object tagged " + interactableTag + ".", hit.collider);
            return;
        }

        if (!player.TrySpendMana(manaCost))
        {
            Debug.Log("Telekinesis: not enough mana (" + player.CurrentMana +
                " available, " + manaCost + " required).", this);
            return;
        }

        Rigidbody body = interactable.GetComponent<Rigidbody>();
        if (body == null)
            body = interactable.GetComponentInParent<Rigidbody>();
        if (body == null)
            body = interactable.gameObject.AddComponent<Rigidbody>();

        controlledBody = body;
        controlledBody.useGravity = false;
        controlledBody.isKinematic = true;
        controlledColliders = controlledBody.GetComponentsInChildren<Collider>();
        playerLayerMask = 0;
        foreach (Collider playerCollider in player.GetComponentsInChildren<Collider>())
            playerLayerMask |= 1 << playerCollider.gameObject.layer;

        movementPlane = new Plane(aimCamera.transform.forward, controlledBody.position);
        isHolding = true;
        isHovering = false;
        nextManaDrainTime = Time.time + Mathf.Max(0.01f, manaDrainInterval);
    }

    private void MoveObjectWithoutPushingPlayer(Vector3 targetPosition)
    {
        Vector3 movement = targetPosition - controlledBody.position;
        float movementDistance = movement.magnitude;
        if (movementDistance <= 0f)
            return;

        Vector3 direction = movement / movementDistance;
        float allowedDistance = movementDistance;

        foreach (Collider objectCollider in controlledColliders)
        {
            if (objectCollider == null || !objectCollider.enabled ||
                objectCollider.isTrigger || playerLayerMask == 0)
                continue;

            Bounds bounds = objectCollider.bounds;
            int hitCount = Physics.BoxCastNonAlloc(
                bounds.center,
                bounds.extents,
                direction,
                movementHits,
                Quaternion.identity,
                movementDistance,
                playerLayerMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (movementHits[i].collider.GetComponentInParent<PlayerControler>() != player)
                    continue;

                allowedDistance = Mathf.Min(
                    allowedDistance, Mathf.Max(0f, movementHits[i].distance - 0.01f));
            }
        }

        controlledBody.MovePosition(
            controlledBody.position + direction * allowedDistance);
    }

    private Transform FindInteractable(Transform colliderTransform)
    {
        for (Transform current = colliderTransform; current != null; current = current.parent)
        {
            if (current.CompareTag(interactableTag))
                return current;
        }

        return null;
    }

    private void DropObject()
    {
        if (controlledBody != null)
        {
            controlledBody.isKinematic = false;
            controlledBody.useGravity = true;
        }

        controlledBody = null;
        controlledColliders = null;
        isHovering = false;
    }

    private void OnDisable()
    {
        if (controlledBody != null)
            DropObject();
    }
}
