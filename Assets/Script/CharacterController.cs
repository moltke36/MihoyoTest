using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CharacterController : MonoBehaviour
{

    public float speed;
    public float MoveX, MoveZ;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InputTick();
        Movement();
    }

    void InputTick()
    {
        MoveX = Input.GetAxis("Horizontal");
        MoveZ = Input.GetAxis("Vertical");
    }


    void Movement()
    {
        float xDeltaPos = MoveX * speed * Time.deltaTime;
        float zDeltaPos = MoveZ * speed * Time.deltaTime;

        animator.SetFloat("MovementInput", Mathf.Max(Mathf.Abs(MoveX), Mathf.Abs(MoveZ)));
        //print(animator.GetInteger("Direction"));

        transform.Translate(new Vector3(xDeltaPos, 0, zDeltaPos));
    }
}
