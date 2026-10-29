
using UnityEngine;

public class AbsorbMagic : MonoBehaviour
{
    [SerializeField] PlayerControler player;
    [SerializeField] int ManaCost = 1;
    [SerializeField] int ParryManaRefund = 4;
    [SerializeField] float ParryWindow = 0.5f;
    [SerializeField] float Cooldown = 1.5f;
    [SerializeField] KeyCode ParryKey = KeyCode.Q;
    [SerializeField] Color EffectColour = Color.yellow;
    [SerializeField] GameObject barrierPrefab;
    GameObject barrierVisual;
    public bool IsAbsorbing { get; private set; }
    float parryTimer;
    float cooldownTimer;

    void Awake()
    {
        player = GetComponent<PlayerControler>();

        barrierVisual = Instantiate(barrierPrefab, transform);
        barrierVisual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        barrierVisual.transform.localRotation = Quaternion.identity;
        barrierVisual.transform.localScale = Vector3.one * 1.5f;

        foreach (Renderer r in barrierVisual.GetComponentsInChildren<Renderer>(true))
            r.material.color = EffectColour;

        barrierVisual.SetActive(false);
    }

    void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(ParryKey) && !IsAbsorbing && cooldownTimer <= 0f && player.TrySpendMana(ManaCost))
        {
            IsAbsorbing = true;
            parryTimer = ParryWindow;
            barrierVisual.SetActive(true);
        }

        if (!IsAbsorbing) return;
        parryTimer -= Time.deltaTime;
        if (parryTimer <= 0f) EndParry();
    }

    public bool TryParry()
    {
        if (!IsAbsorbing) return false;
        player.RefundMana(ParryManaRefund);
        EndParry();
        return true;
    }

    void EndParry()
    {
        IsAbsorbing = false;
        barrierVisual.SetActive(false);
        cooldownTimer = Cooldown;
    }
}
