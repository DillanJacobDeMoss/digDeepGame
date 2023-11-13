using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    private Rigidbody2D body;
    private float laserSpeed = 20;
    private Vector2 velocity = new Vector2(0f, 0f);

    private string[] collisionTargets;

    private AudioSource audioSource;

    private bool activeInPlayspace;
    private float maxLifetime;

    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();
        body = GetComponent<Rigidbody2D>();
        collisionTargets = new string[] {"tileCollision", "Drill", "bossHurtbox"};
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(activeInPlayspace)
        {
            maxLifetime -= Time.fixedDeltaTime;
            if(maxLifetime <= 0f)
            {
                despawnLaser();
            }
        }
    }

    public void spawnLaser(Vector2 location, Quaternion rotation, Vector2 trajectory)
    {
        transform.position = location;
        transform.rotation = rotation;
        velocity.Set(trajectory.x * laserSpeed, trajectory.y * laserSpeed);
        body.velocity = velocity;
        maxLifetime = 5f;
        activeInPlayspace = true;
        audioSource.volume = 0.4f * audioManager.getMixedSfx();
        audioSource.Play();
    }

    public void despawnLaser()
    {
        transform.position = new Vector2(transform.position.x, -22f);
        velocity.Set(0f, 0f);
        body.velocity = velocity;
        activeInPlayspace = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        
        //despawn if laser hit any other collision targets
        for (int i = 0; i < collisionTargets.Length; i++)
        {
            //despawn laser upon hitting objects
            if(col.tag == collisionTargets[i])
            {
                despawnLaser();
            }
        }

        //Damage enemies on inpact
        if (col.tag == "enemyHitbox")
        {
            col.GetComponent<EnemyHealth>().changeHealth(-1);
            despawnLaser();
        }
    }
}
