using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    Collider hitbox;
    public Character character;
    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] cols = Physics.OverlapBox(hitbox.bounds.center, hitbox.bounds.extents, hitbox.transform.rotation, LayerMask.GetMask("Enemy"));
        if (cols.Length > 0)
        {
            foreach (Collider c in cols)
            {
                character.SendMessageUpwards("HitresReceive", c);
            }
        }

    }
}
