using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    public float MoveSpeedValue => MoveSpeed;
    public float JumpForceValue => JumpForce;

    public bool isMagicSuppressed => antiMagicZoneCount > 0;
    
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
    float JumpForce = 10f;

    [SerializeField]
    LayerMask WhatIsGround;

    // Awake se produit avait le Start. Il peut être bien de régler les références dans cette section.
    void Awake()
    {
        _Anim = GetComponent<Animator>();
        _Rb = GetComponent<Rigidbody>();
        _MainCamera = Camera.main;
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
        HorizontalMove(horizontal);
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
        Current_Mana = Max_Mana;
        // On retourne le joueur au début du niveau
        _Rb.position = new Vector3(0f, 0f, 0f);
        HP = 1;
    }

    // Gère le mouvement horizontal
    void HorizontalMove(float horizontal)
    {
        _Rb.linearVelocity = new Vector3(_Rb.linearVelocity.x, _Rb.linearVelocity.y, horizontal);
        _Anim.SetFloat("MoveSpeed", Mathf.Abs(horizontal));
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
        // On s'assure de bien être en contact avec le sol
        if ((WhatIsGround & (1 << coll.gameObject.layer)) == 0)
            return;

        // Évite une collision avec le plafond
        if (coll.relativeVelocity.y > 0)
        {
            _Grounded = true;
            _Anim.SetBool("Grounded", _Grounded);
        }
    }
}