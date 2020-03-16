using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Woodman : MonoBehaviour
{
    public GameObject model;
    public GameObject Zombie;
    public Animator animator;
    private bool die = false;
    private bool beProtect = true;
    public bool isZombie;
    private int HitTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        beProtect = isZombie;
    }

    // Update is called once per frame
    void Update()
    {
        if (beProtect)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                this.gameObject.layer = 8;
                beProtect = false;
            }


        }
    }

    public void takeDamage(Character damager)
    {
        if (!die && !beProtect)
        {
            var targetRotation = Quaternion.LookRotation(damager.transform.position - transform.position);
            transform.rotation = targetRotation;
            if (!isZombie)
                animator.SetTrigger("GitHit");
            else
            {
                HitTime++;
                if (HitTime > 2)
                {
                    animator.SetBool("Die",true);
                    this.gameObject.layer = 0;
                    Destroy(this, 3.0f);
                    die = true;
                }
            }
        }
    }

    public void takeDamageBattou(Character damager)
    {
        if (!die && !beProtect)
        { 
            var targetRotation = Quaternion.LookRotation(damager.transform.position - transform.position);
            transform.rotation = targetRotation;
            if (!isZombie)
            {
                animator.SetTrigger("GitHit");
            }
            HitTime++;
            if (HitTime > 2)
            {
                animator.SetBool("Die", true);
                Destroy(this, 3.0f);
                if (!isZombie)
                {
                    var zombie = Instantiate(Zombie, transform.position, Quaternion.identity);
                }
                die = true;
                this.gameObject.layer = 0;
            }
        }
    }


}
