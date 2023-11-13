using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTest : MonoBehaviour
{

    //Scripts
    private GameManager gameManager;
    private Drill drill;

    [Header("Hurtbox")]
    public BossHurtbox hurtbox;

    private bool active = false; //true when the boss is active

    // Start is called before the first frame update
    void Start()
    {
        //Acquire Scripts
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        drill = GameObject.FindGameObjectWithTag("Drill").GetComponent<Drill>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            updateSpeed();
            checkDeath();
        }
    }

    public void activateBoss()
    {
        //update boss bar
        gameManager.updateBossHealthbar(hurtbox.getHealth(), hurtbox.getHealthMax());

        //Teleport to spawn point
        transform.position = new Vector3(drill.transform.position.x - 39, 0, 0);

        //Init Health
        hurtbox.initializeHealth();

        active = true;
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

    private void checkDeath()
    {
        if(hurtbox.getHealth() <= 0)
        {
            active = false;
            gameManager.endBossBattle();
            GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        }
    }
}
