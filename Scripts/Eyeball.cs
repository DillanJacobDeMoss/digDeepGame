using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eyeball : MonoBehaviour
{
    [Header ("Eye Sprites")]
    public Sprite[] eyeSprites;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Animator anims;

    [Header("Particle Systems")]
    public ParticleSystem deathParticles;
    public ParticleSystem drillHitParticles;

    [Header("Sounds")]
    public AudioClip sound_death;
    public AudioClip sound_attackDrill;

    private int eyeType = 0;

    private float moveSpeed = 0;

    private bool activeInPlayspace = false;
    private bool reachedDrill = false;

    private float drillAttackTimer;

    public EnemyHealth enemyHealth;
    private GameManager gameManager;
    private Drill drill;
    private AudioManager audioManager;
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();
        drill = GameObject.FindGameObjectWithTag("Drill").GetComponent<Drill>();
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Player>();
        anims = GetComponent<Animator>();
        deathParticles.Stop();
        drillHitParticles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        #region Movement
        //move faster than drill to catch up
        if (activeInPlayspace && !reachedDrill)
        {
            if(drill.getTrueSpeed() > 0)
            {
                //eyeballs should look like they are falling behind a little bit during overdrive
                if(drill.getSpeed() < drill.getTrueSpeed())
                {
                    moveSpeed = drill.getTrueSpeed() + eyeType + 0.5f;
                }
                else
                {
                    moveSpeed = drill.getTrueSpeed() + eyeType + 2f;
                }
            }
            if(transform.position.x >= drill.transform.position.x - 5)
            {
                reachedDrill = true;
                drillAttackTimer = 1f;
                hitDrill();
            }
        }
        //match drill speed to attack it
        if (reachedDrill)
        {
            moveSpeed = drill.getTrueSpeed();
        }
        //stop moving when inactive
        if (!activeInPlayspace)
        {
            moveSpeed = 0;
        }

        body.velocity = new Vector2(moveSpeed, 0);
        #endregion

        //Eyeball Hurt Anims
        if (enemyHealth.getDamaged())
        {
            anims.SetTrigger("hurt");
            enemyHealth.setDamaged(false);
        }

        //Eyeball Death
        if (enemyHealth.getHealth() <= 0 && activeInPlayspace)
        {
            killed();
        }
    }

    private void FixedUpdate()
    {
        if (reachedDrill)
        {
            drillAttackTimer -= Time.deltaTime;
            if(drillAttackTimer <= 0)
            {
                drillAttackTimer = 1f;
                hitDrill();
            }
        }
    }

    public void spawnEye(int type, Vector3 spawnPosition)
    {
        enemyHealth.changeHealth((type + 1) * 2);
        activeInPlayspace = true;
        teleport(spawnPosition);
        spriteRenderer.sprite = eyeSprites[type];
        eyeType = type;
        transform.localScale = new Vector3(1f + (type / (float)10), 1f + (type / (float)10), 1f);
        reachedDrill = false;
    }

    private void teleport(Vector3 position)
    {
        transform.position = position;
        reachedDrill = false;
    }

    private void killed()
    {
        activeInPlayspace = false;
        gameManager.incrementScore_kills((eyeType + 1) * 5);
        Vector3 particlePos = transform.position;
        teleport(new Vector3(transform.position.x, -30, 0));
        deathParticles.transform.position = particlePos;
        deathParticles.Play();
        GetComponent<AudioSource>().clip = sound_death;
        GetComponent<AudioSource>().volume = 0.4f * audioManager.getMixedSfx();
        GetComponent<AudioSource>().Play();
        reachedDrill = false;
    }

    private void hitDrill()
    {
        gameManager.changeDrillHealth(-((eyeType + 1) * 5));
        GetComponent<AudioSource>().clip = sound_attackDrill;
        GetComponent<AudioSource>().volume = 0.4f * audioManager.getMixedSfx();
        GetComponent<AudioSource>().Play();
        drillHitParticles.Play();
    }

    #region Getters
    public bool getActiveInPlayspace()
    {
        return activeInPlayspace;
    }

    public int getType()
    {
        return eyeType;
    }
    #endregion

    #region Setters
    public void setType(int type)
    {
        eyeType = type;
    }

    public void setActiveInPlayspace(bool set)
    {
        activeInPlayspace = set;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "player" && !player.getDodging())
        {
            gameManager.changePlayerHealth(-((eyeType + 1) * 5), ConstantLibrary.DMGSRC_PHYSICAL);
        }
    }
}
