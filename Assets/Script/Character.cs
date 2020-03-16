using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*
时间紧迫，写的比较乱，也没有时间Refactoring。
*/
public class Character : MonoBehaviour
{
    public float MaxSpeed = 8.0f;
    public float Gravity = 8.0f;
    public float AccelerateSpeed = 25.0f;
    public float DecelerateSpeed = 25.0f;

    private float _targetHorizontalSpeed; // In meters/second
    private float _horizontalSpeed; // In meters/second

    // TODO: Move to private
    public Animator animator;
    public CharacterController characterController;

    public Vector2 characterRotation; // X (Pitch), Y (Yaw)
    private Vector3 OSmovementInput;
    private Vector3 LastInput;
    private Vector2 CameraInput;
    private Vector3 MoveOutput;
    private Vector2 RawMoveInput;
    public float battouTimer = 0.0f;
    public bool isbattou = false;
    public bool battouCD = false;
    public bool canBattouCombo = false;
    public bool isBattouCombo = false;
    public Collider BattouTarget;
    private List<Collider> BattouList;

    public GameObject hitPSPrefab;
    public GameObject SakuraPSPrefab;

    public GameObject SakuraOnWeapon;

    private bool HasMovementInput;
    private bool canMove = false;

    private int ComboIndex = 0;
    private int CComboIndex = 0;
    private int nextAnimation = 0;
    private bool canCombo = true;
    private bool isDie = false;
    private float VerticalSpeed;
    private bool ishitEffects = false;
    private float hitEffectTimer = 0.0f;
    public float hitEffectSpeed = 0.0f;
    public float hitEffectMaxTime = 0.1f;

    public Collider hitboxes;
    public Collider Skillhitbox;
    private List<Collider> Hitres;

    private Vector2 controlRotation;

    private Vector3 Velocity => characterController.velocity;
    private Vector3 HorizontalVelocity => new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z);

    // Start is called before the first frame update
    void Start()
    {
        //Velocity = characterController.velocity;
        //HorizontalVelocity = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z);
        HasMovementInput = false;
        canMove = true;
        Hitres = new List<Collider>();
        BattouList = new List<Collider>();
    //Time.timeScale = 0.1f;
}

    // Update is called once per frame
    void Update()
    {
        if (!isDie)
        {
            MoveInput();
            ActionInput();
        }
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        Movement();
        UpdateAnimation();
        hitEffects();
    }

    public void MoveInput()
    {
        RawMoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Quaternion yawRotation = Quaternion.Euler(0.0f, characterRotation.y, 0.0f);
        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;
        Vector3 NewmovementInput = (forward * RawMoveInput.y + right * RawMoveInput.x);

        if (NewmovementInput.sqrMagnitude > 1f)
        {
            NewmovementInput.Normalize();
        }

        // TODO: AxisDeadZone
        bool CurrentMoveInput = NewmovementInput.sqrMagnitude > 0.0f;
        if (HasMovementInput && !CurrentMoveInput)
        {
            LastInput = OSmovementInput;
        }

        OSmovementInput = NewmovementInput;
        HasMovementInput = CurrentMoveInput;
    }

    void UpdateAnimation()
    {
        float normHorizontalSpeed = HorizontalVelocity.magnitude / MaxSpeed;
        animator.SetFloat("HorizontalSpeed", normHorizontalSpeed);

    }

    public void UpdateSpeed()
    {
        Vector3 movementInput = OSmovementInput;
        if (movementInput.sqrMagnitude > 1.0f)
        {
            movementInput.Normalize();
        }

        if (canMove)
        {
            _targetHorizontalSpeed = movementInput.magnitude * MaxSpeed; 
        }
        else _targetHorizontalSpeed = 0.0f;

        float acceleration = HasMovementInput ? AccelerateSpeed : DecelerateSpeed;

        _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, _targetHorizontalSpeed, acceleration * Time.deltaTime);

    }


    public void Movement()
    {
        MoveOutput = _horizontalSpeed * GetMovementDirection();
        if (!characterController.isGrounded)
        {
            MoveOutput.y = -Gravity;
        }
        characterController.Move(MoveOutput * Time.deltaTime);
        Vector3 HorizontalMovement = new Vector3(MoveOutput.x, 0.0f, MoveOutput.z);
        // Rotate
        OrientToTargetRotation(HorizontalMovement);

    }

    public void SetControlRotation(Vector2 controlRotation)
    {
        // Adjust the pitch angle (X Rotation)
        float pitchAngle = controlRotation.x;
        pitchAngle %= 360.0f;
        pitchAngle = Mathf.Clamp(pitchAngle, -45f, 75f);

        // Adjust the yaw angle (Y Rotation)
        float yawAngle = controlRotation.y;
        yawAngle %= 360.0f;

        controlRotation = new Vector2(pitchAngle, yawAngle);
    }

    void OrientToTargetRotation(Vector3 horizontalMovement)
    {
        if (horizontalMovement.sqrMagnitude > 0.0f)
        {
            float rotationSpeed = Mathf.Lerp(1200f, 600f, _horizontalSpeed / _targetHorizontalSpeed);

            Quaternion targetRotation = Quaternion.LookRotation(horizontalMovement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 moveDir = HasMovementInput ? OSmovementInput : LastInput ;
        if (moveDir.sqrMagnitude > 1f)
        {
            moveDir.Normalize();
        }

        return moveDir;
    }

    void ActionInput()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            animator.SetInteger("condition", -1);
            canMove = false;
            isDie = true;
            this.enabled = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ComboStarter();
        }
        if (Input.GetMouseButtonDown(1))
        {
            CComboStarter();
        }

        if (Input.GetKeyDown(KeyCode.E) && animator.GetInteger("condition") == 0)
        {
            canMove = false;
            animator.SetInteger("condition", 10);
        }
            

        if (Input.GetKeyUp(KeyCode.E) && animator.GetInteger("condition") == 10)
        {
            animator.SetInteger("condition", 0);
            canMove = true;
        }

        if (Input.GetKey(KeyCode.F) )
        {
            if (!isbattou && !battouCD)
            {
                if (animator.GetInteger("condition") == 0)
                {
                    animator.SetInteger("condition", 11);
                    canMove = false;
                    isbattou = true;
                    battouCD = true;
                    SakuraOnWeapon.SetActive(true);
                }
            }
            else if (isbattou)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("battou"))
                {
                    animator.speed = 0.01f;
                    battouTimer += Time.deltaTime;
                }
                if (battouTimer > 2.0f)
                {
                    battou();
                }
            }
        }

        // Cancel battou
        if (Input.GetKeyUp(KeyCode.F) && isbattou)
        {
            animator.SetInteger("condition", 0);
            animator.speed = 1.0f;
            isbattou = false;
            canMove = true;
            battouTimer = 0.0f;
            SakuraOnWeapon.SetActive(false);
        }

        // Battou Combo
        if (Input.GetKeyDown(KeyCode.F) && canBattouCombo)
        {
            BattouCombo();
        }

        ComboDetect();

        // Skill Cool Down
        if (battouCD)
        {
            battouTimer += Time.deltaTime;
            if (battouTimer >= 6.0f)
            { 
                battouCD = false;
                battouTimer = 0.0f;
            }
        }
        
    }

    void battou()
    {
        isbattou = false;
        Collider[] Cols = Physics.OverlapBox(Skillhitbox.bounds.center, Skillhitbox.bounds.extents,transform.rotation, LayerMask.GetMask("Enemy"));
        foreach (Collider c in Cols)
        {
            SpawnHitEffects(c,3.0f,SakuraPSPrefab, "takeDamageBattou");
            BattouList.Add(c);
        }

        Vector3 movement = 250.0f * GetMovementDirection();
        characterController.Move(movement);

        if (Cols.Length > 0)
        {
            canBattouCombo = true;
            //animator.speed = 0.5f;
        }
        else animator.speed = 1.0f; 
    }

    void BattouCombo()
    {
        canBattouCombo = false;
        Collider[] Cols = Physics.OverlapBox(Skillhitbox.bounds.center, Skillhitbox.bounds.extents, transform.rotation, LayerMask.GetMask("Enemy"));
        if (Cols.Length > 0)
        {
            foreach (Collider c in Cols)
            {
                if (!BattouList.Contains(c))
                {
                    animator.SetTrigger("BattouComboTriger");
                    animator.speed = 2.0f;
                    BattouList.Add(c);
                    BattouTarget = c;
                    isBattouCombo = true;
                    return;
                }
            }
        }
                
        //animator.speed = 1.0f;
        //canMove = true;
    }

    public void battouEnd()
    {
        if (isBattouCombo)
        {
            isBattouCombo = false;
            canBattouCombo = true;
            animator.SetTrigger("BattouComboTriger");
            animator.speed = 2.0f;
            SpawnHitEffects(BattouTarget,3.0f,SakuraPSPrefab, "takeDamageBattou");
            Vector3 direction = (BattouTarget.transform.position - transform.position).normalized;
            Vector3 movement = ((BattouTarget.transform.position - transform.position).magnitude) * direction;
            characterController.Move(movement);
            canBattouCombo = true;
        }
        else
        {
            animator.SetInteger("condition", 0);
            canMove = true;
            battouCD = true;
            canBattouCombo = false;
            battouTimer = 0.0f;
            animator.speed = 1.0f;
            SakuraOnWeapon.SetActive(false);
            BattouList.Clear();
        }

    }

    private void ComboDetect()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01") && CComboIndex >= 1)
        {
            nextAnimation = 7;
            canCombo = false;
        }

        else if ((animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01") || animator.GetCurrentAnimatorStateInfo(0).IsName("CAttack01")) && ComboIndex >= 2)
        {
            nextAnimation = 5;
            canCombo = false;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack02") && CComboIndex >= 1)
        {
            nextAnimation = 8;
            canCombo = false;
        }

        else if ((animator.GetCurrentAnimatorStateInfo(0).IsName("Attack02") || animator.GetCurrentAnimatorStateInfo(0).IsName("CAttack02")) && ComboIndex >= 3)
        {
            nextAnimation = 6;
            canCombo = false;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack03") && CComboIndex >= 1)
        {
            nextAnimation = 9;
            canCombo = false;
        }
    }

    void ComboStarter()
    {
        if (ComboIndex == 0)
        {
            ComboIndex=1;
            animator.SetInteger("condition", 4);
            canMove = false;
            canCombo = false;
        }

        if (canCombo)
        {
            ComboIndex++;
            canCombo = false;
        }

        ComboIndex = Mathf.Clamp(ComboIndex, 0, 3);

    }

    void CComboStarter()
    {
        if (canCombo && CComboIndex == 0)
        {
            CComboIndex = 1;
            canMove = false;
            canCombo = false;
        }



        CComboIndex = Mathf.Clamp(CComboIndex, 0, 1);

    }

    public void HitboxOn()
    {
        hitboxes.gameObject.SetActive(true);
    }

    public void HitboxOff()
    {
        hitboxes.gameObject.SetActive(false);
        Hitres.Clear();
    }

    public void HitresReceive(Collider c)
    {
        if (!Hitres.Contains(c))
        {
            Hitres.Add(c);
            SpawnHitEffects(c,1, hitPSPrefab, "takeDamage");
        }
    }

    private void SpawnHitEffects(Collider c,float DeleteTime, GameObject effectsPrefab,string message)
    {
        animator.speed = hitEffectSpeed;
        var PSeffects = Instantiate(effectsPrefab, (c.transform.position - transform.position) / 2.0f + c.transform.position + new Vector3(0.0f, hitboxes.transform.position.y, 0.0f), Quaternion.identity);
        ishitEffects = true;
        c.SendMessageUpwards(message, this);
        Destroy(PSeffects, DeleteTime);
    }

    void hitEffects()
    {
        if (ishitEffects)
        {
            hitEffectTimer += Time.deltaTime;
            if (hitEffectTimer >= hitEffectMaxTime)
            {
                hitEffectTimer = 0.0f;
                animator.speed = 1.0f;
                ishitEffects = false;
            }
        }
    }

    public void ComboCheck()
    {
        canCombo = true;
    }
    
    public void AttackFinish()
    {

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01") && ComboIndex == 1 && CComboIndex == 0)
        {
            nextAnimation = 0;
            canCombo = false;
            canMove = true;
            ComboIndex = 0;
            CComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("CAttack01") && ComboIndex == 1)
        {
            nextAnimation = 0;
            canCombo = false;
            canMove = true;
            ComboIndex = 0;
            CComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack02") && ComboIndex == 2 && CComboIndex == 0)
        {
            nextAnimation = 0;
            canCombo = false;
            canMove = true;
            ComboIndex = 0;
            CComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("CAttack02") && ComboIndex == 2)
        {
            nextAnimation = 0;
            canCombo = false;
            canMove = true;
            ComboIndex = 0;
            CComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack03") && ComboIndex >= 3 && CComboIndex == 0 )
        {
            nextAnimation = 0;
            canCombo = false;
            canMove = true;
            ComboIndex = 0;
            CComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("CAttack03") )
        {
            nextAnimation = 0;
            canCombo = false;
            canMove = true;
            ComboIndex = 0;
            CComboIndex = 0;
        }

        animator.SetInteger("condition", nextAnimation);
        CComboIndex = 0;


    }

}
