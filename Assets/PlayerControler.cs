using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    public float MoveSpeedValue => MoveSpeed;
    public float JumpForceValue => JumpForce;

    public bool isMagicSuppressed => antiMagicZoneCount > 0;

    public bool IsGrounded => _Grounded;
    public int CurrentMana => Current_Mana;
    public bool IsFlipped => _Flipped;
    // Déclaration des constantes
    private static readonly Vector3 FlipRotation = new Vector3(0, 180, 0);
    private static readonly Vector3 CameraPosition = new Vector3(10, 1, 0);
    private static readonly Vector3 InverseCameraPosition = new Vector3(-10, 1, 0);

    // Déclaration des variables
    bool _Grounded { get; set; }
    bool _Flipped { get; set; }
    Animator _Anim { get; set; }
    Rigidbody _Rb { get; set; }
    Camera _MainCamera { get; set; }
    float horizontalInput;
    float externalHorizontalVelocity;

    int antiMagicZoneCount = 0;

    // Variables pour mécaniques
    int HP = 1;
    [SerializeField] public int Max_Mana = 1;
    [SerializeField] public int Current_Mana = 1;
    int Mana_Growth = 1;
    // À chaque 1.5 seconde, 1 mana est récupéré, seulement au sol
    float Mana_Regen_Delay = 1.5f;
    Coroutine manaRegenCoroutine;
    float ParryWindowAbsorbMagic = 0.5f;


    // Pour des escaliers et petits obstacles
    [SerializeField] float StepHeight = 0.75f;
    [SerializeField] float StepCheckDistance = 0.4f;
    [SerializeField] float StepSmooth = 5f;
    [SerializeField] float StepForwardOffset = 0.05f;
    // Valeurs exposées
    [SerializeField]
    float MoveSpeed = 5.0f;

    [SerializeField]
    float HorizontalMomentumDamping = 4f;

    [SerializeField]
    float JumpForce = 10f;

    [SerializeField]
    LayerMask WhatIsGround;

    // Awake se produit avait le Start. Il peut être bien de régler les références dans cette section.
    void Awake()
    {
        _Anim = GetComponent<Animator>();
        _Rb = GetComponent<Rigidbody>();
        _MainCamera = Camera.main;
        Current_Mana = Max_Mana;
    }

    // Utile pour régler des valeurs aux objets
    void Start()
    {
        _Grounded = false;
        _Flipped = false;
    }

    // Vérifie les entrées de commandes du joueur
    void Update()
    {
        var horizontal = Input.GetAxis("Horizontal") * MoveSpeed;
        horizontalInput = horizontal;
        _Anim.SetFloat("MoveSpeed", Mathf.Abs(horizontal));
        FlipCharacter(horizontal);
        CheckJump();
        CheckHP();
        CheckStep();

        if (_Grounded && manaRegenCoroutine == null)
        {
            manaRegenCoroutine = StartCoroutine(RegenMana());
        }
        else if (!_Grounded && manaRegenCoroutine != null)
        {
            StopCoroutine(manaRegenCoroutine);
            manaRegenCoroutine = null;
        }
    }

    void FixedUpdate()
    {
        externalHorizontalVelocity = Mathf.MoveTowards(
            externalHorizontalVelocity,
            0f,
            HorizontalMomentumDamping * Time.fixedDeltaTime
        );

        Vector3 velocity = _Rb.linearVelocity;
        velocity.z = horizontalInput + externalHorizontalVelocity;
        _Rb.linearVelocity = velocity;
    }

    public void ApplyVelocityChange(Vector3 velocityChange)
    {
        _Rb.linearVelocity += new Vector3(
            velocityChange.x,
            velocityChange.y,
            0f
        );
        externalHorizontalVelocity += velocityChange.z;
    }

    public bool TrySpendMana(int amount)
    {
        if (isMagicSuppressed || Current_Mana < amount || amount <= 0)
            return false;

        Current_Mana -= amount;
        return true;
    }
    void CheckStep()
    {
        if (!_Grounded) return;
        Vector3 direction = Vector3.forward * Mathf.Sign(_Rb.linearVelocity.z);
        Vector3 origin = transform.position + Vector3.up * StepHeight;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, StepCheckDistance, WhatIsGround)) return;

        Vector3 lowerOrigin = transform.position + Vector3.up * 0.05f;

        if (Physics.Raycast(lowerOrigin, direction, out hit, StepCheckDistance, WhatIsGround))
        {
            Vector3 upperOrigin = hit.point + Vector3.up * (StepHeight + 0.05f);
            if (!Physics.Raycast(upperOrigin, Vector3.down, out RaycastHit stepHit, StepHeight + 0.1f, WhatIsGround)) return;

            float heightDifference = stepHit.point.y - transform.position.y;

            if (heightDifference > 0.05f && heightDifference <= StepHeight)
            {
                _Rb.position += Vector3.up * heightDifference * Time.fixedDeltaTime * StepSmooth;
            }
        }
    }

    public void RefundMana(int mana)
    {
        Current_Mana = Math.Min(Current_Mana + mana, Max_Mana);
    }

    IEnumerator RegenMana()
    {
        while (_Grounded)
        {
            yield return new WaitForSeconds(Mana_Regen_Delay);
            if (_Grounded && Current_Mana < Max_Mana)
            {
                Current_Mana++;
            }
        }
        manaRegenCoroutine = null;
    }

    void CheckHP()
    {
        if (HP <= 0)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        DestroyAllProjectiles();
        Current_Mana = Max_Mana;
        HP = 1;
        // On retourne le joueur au début du niveau ou au dernier checkpoint
        _Rb.position = new Vector3(1f, 1f, 1f);
    }

    void DestroyAllProjectiles()
    {
        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("Projectile"))
        {
            Destroy(projectile);
        }
    }

    // Gère le saut du personnage, ainsi que son animation de saut
    void CheckJump()
    {
        if (_Grounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                _Rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
                _Grounded = false;
                _Anim.SetBool("Grounded", false);
                _Anim.SetBool("Jump", true);
            }
        }
    }

    public void EnterAntiMagicZone()
    {
        antiMagicZoneCount++;
    }

    public void ExitAntiMagicZone()
    {
        antiMagicZoneCount--;
    }

    public void SetGrounded(bool grounded)
    {
        _Grounded = grounded;
        _Anim.SetBool("Grounded", grounded);
    }

    public bool IsStandingOnGround()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f, WhatIsGround);

    }

    // Gère l'orientation du joueur et les ajustements de la camera
    void FlipCharacter(float horizontal)
    {
        if (horizontal < 0 && !_Flipped)
        {
            _Flipped = true;
            transform.Rotate(FlipRotation);
            _MainCamera.transform.Rotate(-FlipRotation);
            _MainCamera.transform.localPosition = InverseCameraPosition;
        }
        else if (horizontal > 0 && _Flipped)
        {
            _Flipped = false;
            transform.Rotate(-FlipRotation);
            _MainCamera.transform.Rotate(FlipRotation);
            _MainCamera.transform.localPosition = CameraPosition;
        }
    }

    // Collision avec le sol
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == 6)
        {
            AbsorbMagic parry = GetComponent<AbsorbMagic>();
            if (!parry.TryParry()) HP--;
            return;
        }

        // Ignore collisions with objects that are not ground.
        if ((WhatIsGround.value & (1 << coll.gameObject.layer)) == 0)
            return;

        // Only count a collision as grounding if the contact
        // surface is underneath the player.
        foreach (ContactPoint contact in coll.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                _Grounded = true;
                _Anim.SetBool("Grounded", true);
                _Anim.SetBool("Jump", false);
                return;
            }
        }
    }
}