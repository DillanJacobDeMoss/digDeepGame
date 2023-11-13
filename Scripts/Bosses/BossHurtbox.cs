using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHurtbox : MonoBehaviour
{

    private int health;
    public int healthMax;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "laser")
        {
            changeHealth(-1);
        }
    }

    public void changeHealth(int amount)
    {
        //change HP
        health += amount;
        if(health > healthMax) { health = healthMax; }

        //Update Healthbar
        gameManager.updateBossHealthbar(health, healthMax);
    }

    public void initializeHealth()
    {
        //Set Health
        health = healthMax;

        //Update Healthbar
        gameManager.updateBossHealthbar(health, healthMax);
    }

    public int getHealth() { return health; }
    public int getHealthMax() { return healthMax; }
}
