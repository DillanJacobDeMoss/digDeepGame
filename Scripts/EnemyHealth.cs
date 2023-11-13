using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private int health = 0;

    //let the parent know it took damage (to trigger anims)
    private bool damaged = false;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeHealth(int amount)
    {
        health += amount;
        if(amount < 0)
        {
            damaged = true;
        }
    }

    public int getHealth()
    {
        return health;
    }

    public void setDamaged(bool set)
    {
        damaged = set;
    }

    public bool getDamaged()
    {
        return damaged;
    }
}
