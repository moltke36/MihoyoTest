using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Character : MonoBehaviour
{
    public float MaxSpeed = 8.0f;

    private float _targetHorizontalSpeed; // In meters/second
    private float _horizontalSpeed; // In meters/second

    // TODO: Move to private
    public Animator animator;
    public CharacterController characterController;

    public Vector2 characterRotation; // X (Pitch), Y (Yaw)
    private Vector3 OSmovementInput;
    private Vector3 LastInput;
    private Vector2 CameraInput;
    private Vector2 RawMoveInput;

    public GameObject hitPSPrefab;

    private bool HasMovementInput;
    private bool canMove = false;

    private int ComboIndex = 0;
    private int CComboIndex = 0;
    private int nextAnimation = 0;
    private bool canCombo = true;

    private bool ishitEffects = false;
    private float hitEffectTimer = 0.0f;
    public float hitEffectSpeed = 0.0f;
    public float hitEffectMaxTime = 0.1f;

    public Collider hitboxes;
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
        //Time.timeScale = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        InputTick();
        UpdateAction();

    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        Movement();
        UpdateAnimation();
        hitEffects();
    }

    public void InputTick()
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

    void UpdateAction()
    {
        GetInput();

        
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

        float acceleration = HasMovementInput ? 25f : 25f;

        _horizontalSpeed = Mathf.MoveTowards(_horizontalSpeed, _targetHorizontalSpeed, acceleration * Time.deltaTime);
    }


    public void Movement()
    {

            Vector3 movement = _horizontalSpeed * GetMovementDirection();
            characterController.Move(movement * Time.deltaTime);
            Vector3 HorizontalMovement = new Vector3(movement.x, 0.0f, movement.z);
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

    void GetInput()
    {
        if (Input.GetButton("Cancel"))
        {
            Application.Quit();
        }

        if (Input.GetMouseButtonDown(0))
        {
            ComboStarter();
        }
        if (Input.GetMouseButtonDown(1))
        {
            CComboStarter();
        }

        if (Input.GetButtonDown("Fire3") && animator.GetInteger("condition") == 0)
            animator.SetInteger("condition", 10);

        if (Input.GetButtonUp("Fire3") && animator.GetInteger("condition") == 10)
            animator.SetInteger("condition", 0);


        ComboDetect();

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
            animator.speed = hitEffectSpeed;
            var PSeffects = Instantiate(hitPSPrefab, (c.transform.position - transform.position) / 2.0f + c.transform.position + new Vector3(0.0f, hitboxes.transform.position.y,0.0f),Quaternion.identity);
            ishitEffects = true;
            c.SendMessageUpwards("takeDamage", this);
            Destroy(PSeffects,1f);
        }
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
