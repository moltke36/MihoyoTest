using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float ControlRotationSensitivity = 3.0f;
    public float RotationSpeed = 0.0f;
    public Character character;
    public Quaternion pivotTargetLocalRotation;
    public Vector2 CameraInput;

    public float PositionSmoothDamp;
    public Vector3 cameraVelocity;
    // Start is called before the first frame update
    void Start()
    {
        //Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateControllerRotation();
    }

    public void FixedUpdate()
    {
        CameraInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (character)
        {
            transform.position = Vector3.SmoothDamp(transform.position, character.transform.position, ref cameraVelocity, PositionSmoothDamp);
        }

        // Y Rotation (Yaw Rotation)
        //Quaternion TargetLocalRotation = Quaternion.Euler(0.0f, character.characterRotation.y, 0.0f);

        // X Rotation (Pitch Rotation)
        pivotTargetLocalRotation = Quaternion.Euler(character.characterRotation.x, character.characterRotation.y, 0.0f);

        if (RotationSpeed > 0.0f)
        {
            //transform.localRotation = Quaternion.Slerp(transform.localRotation, TargetLocalRotation, RotationSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, pivotTargetLocalRotation, RotationSpeed * Time.deltaTime);
        }
        else
        {
            //transform.localRotation = TargetLocalRotation;
            transform.localRotation = pivotTargetLocalRotation;
        }
    }

    private void UpdateControllerRotation()
    {
        // Adjust the pitch angle (X Rotation)
        float pitchAngle = character.characterRotation.x;
        pitchAngle -= CameraInput.y * ControlRotationSensitivity;

        // Adjust the yaw angle (Y Rotation)
        float yawAngle = character.characterRotation.y;
        yawAngle += CameraInput.x * ControlRotationSensitivity;

        character.characterRotation = new Vector2(pitchAngle, yawAngle); 
    }

    void CalculateRotation(Vector2 RawInput)
    {
        //Quaternion yawRotation = Quaternion.Euler(0.0f, controlRotation.y, 0.0f);

    }
}
