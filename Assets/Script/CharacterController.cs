using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CharacterController : MonoBehaviour
{
    
    public float speed;
    public float MoveX, MoveZ;

    public Animator animator;
    public Camera camera;
    public GameObject cameraArm;
    public float CameraRotateSpeed = 2.0f;

    public float CameraPan;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputTick();
        CameraControl();
        Movement();

    }

    void InputTick()
    {
        MoveX = Input.GetAxis("Horizontal");
        MoveZ = Input.GetAxis("Vertical");
        float MouseX = Mathf.Clamp(Input.mousePosition.x, 0.0f, Screen.width);
        CameraPan = Mathf.SmoothStep(0.0f,1.0f,Mathf.Abs(MouseX / Screen.width - 0.5f)-0.125f)
             * (MouseX / Screen.width - 0.5f) * Mathf.PI * CameraRotateSpeed * Time.deltaTime;
    }

    void CameraControl()
    {
        if (camera && cameraArm)
        {
            cameraArm.transform.RotateAround(transform.position, new Vector3(0.0f, 1.0f, 0.0f), CameraPan);
        }
    }

    void Movement()
    {

        Vector3 DeltaPos = new Vector3(MoveX, 0, 0);
        float DeltaRotate = CameraPan * 60 * Time.deltaTime;
        Vector3 newPos = transform.forward * MoveX* speed * Time.deltaTime;
        transform.Rotate(0, CameraPan * 60 * Time.deltaTime, 0);
        animator.SetFloat("MovementInput", Mathf.Max(Mathf.Abs(MoveX), Mathf.Abs(MoveZ)));
        //print(animator.GetInteger("Direction"));
        
        transform.Translate(newPos);
    }
}
