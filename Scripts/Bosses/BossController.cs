using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{

    //Health
    private int health;
    private int healthMax;

    //Scripts
    private GameManager gameManager;
    private Drill drill;

    private bool active = false;

    [Header("Transforms")]
    public Transform bossSpawn;

    [Header("Hurtbox")]
    public BossHurtbox hurtbox;

    [Header("Art Assets")]
    public GameObject artTest;

    void Start()
    {
        //Acquire Scripts
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        drill = GameObject.FindGameObjectWithTag("Drill").GetComponent<Drill>();
    }

    // Update is called once per frame
    void Update()
    {
        //values to check while the boss is active
        if (active)
        {
            updateSpeed();
        }
    }

    public void activateBoss()
    {
        int currentBoss = gameManager.getCurrentBoss();

        switch (currentBoss)
        {
            case ConstantLibrary.BOSS_TEST:
                //Set HP
                health = 100;
                healthMax = 100;
                gameManager.updateBossHealthbar(health, healthMax);
                //Teleport to spawn point
                transform.position = bossSpawn.position;
                break;
        }


        active = true;
    }

    private void checkHealth()
    {
        if(health <= 0)
        {
            death();
        }
    }

    private void death()
    {
        active = false;
        gameManager.endBossBattle();
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
    }

    public void changeHealth(int amount)
    {
        health += amount;
        if (health > healthMax) { health = healthMax; }
        checkHealth();
        gameManager.updateBossHealthbar(health, healthMax);
    }

    private void updateSpeed()
    {
        float speed = 0f;

        if (drill.getInOverdrive())
        {
            speed = drill.getTrueSpeed() + 0.25f;
        }
        else
        {
            speed = drill.getTrueSpeed() + 0.5f;
        }

        GetComponent<Rigidbody2D>().velocity = new Vector2(speed, 0f);
    }
}
