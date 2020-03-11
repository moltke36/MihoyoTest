using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Character : MonoBehaviour
{
    public float MaxSpeed = 8.0f;

    public float _targetHorizontalSpeed; // In meters/second
    public float _horizontalSpeed; // In meters/second

    // TODO: Move to private
    public Animator animator;
    public CharacterController characterController;

    public Vector2 characterRotation; // X (Pitch), Y (Yaw)
    public Vector3 OSmovementInput;
    public Vector3 LastInput;
    public Vector2 CameraInput;
    public Vector2 RawMoveInput;

    public AnimatorStateInfo CurrentStateInfo;
    public AnimatorStateInfo LastStateInfo;

    public float comboTimer;

    private bool HasMovementInput;
    public bool canMove = false;

    public bool AttackInput;
    public int ComboIndex = 0;
    public bool IsAttack = false;
    public bool canCombo = true;

    public Vector2 controlRotation;

    public Vector3 Velocity => characterController.velocity;
    public Vector3 HorizontalVelocity => new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z);

    // Start is called before the first frame update
    void Start()
    {
        //Velocity = characterController.velocity;
        //HorizontalVelocity = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z);
        HasMovementInput = false;
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        InputTick();
        UpdateAction();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        Movement();
    }

    public void InputTick()
    {
        RawMoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        AttackInput = Input.GetButtonDown("Fire1");

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
        if (canMove)
        { 
            Vector3 movement = _horizontalSpeed * GetMovementDirection();
            characterController.Move(movement * Time.deltaTime);
            Vector3 HorizontalMovement = new Vector3(movement.x, 0.0f, movement.z);
            // Rotate
            OrientToTargetRotation(HorizontalMovement);
        }
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
        if (Input.GetMouseButtonDown(0))
            {
              ComboStarter();
            }
    }

    void ComboStarter()
    {

        if (canCombo)
        {
            ComboIndex++;
        }

        if (ComboIndex == 1)
        {
            animator.SetInteger("condition", 4);
            canMove = false;
        }

        ComboIndex = Mathf.Clamp(ComboIndex, 0, 3);

        Debug.Log(ComboIndex);
    }

    public void ComboCheck()
    {
        canCombo = false;
        Debug.Log(canCombo);
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01") && ComboIndex == 1 )
        {
            animator.SetInteger("condition", 0);
            canCombo = true;
            canMove = true;
            ComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01") && ComboIndex >= 2)
        {
            animator.SetInteger("condition", 5);
            canCombo = true;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack02") && ComboIndex == 2)
        {
            animator.SetInteger("condition", 0);
            canCombo = true;
            canMove = true;
            ComboIndex = 0;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack02") && ComboIndex >= 3)
        {
            animator.SetInteger("condition", 6);
            canCombo = true;
        }

        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack03") && ComboIndex >= 3)
        {
            animator.SetInteger("condition", 0);
            canCombo = true;
            canMove = true;
            ComboIndex = 0;
        }
    }

    
}
