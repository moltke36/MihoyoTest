using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Woodman : MonoBehaviour
{
    public GameObject model;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void takeDamage(Character damager)
    {

        var targetRotation = Quaternion.LookRotation(damager.transform.position - transform.position);
        transform.rotation = targetRotation;
        animator.SetTrigger("GitHit");
    }


}
