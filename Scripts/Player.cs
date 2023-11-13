using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{

    public float moveSpeed;

    public ParticleSystem mineParticles;
    public Animator mineAnims;
    public ParticleSystem breakParticles;

    private Rigidbody2D body;
    private Transform playerTransform;
    private InputSystem controls;
    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private World world;
    private CheatsManager cheats;

    #region Drill Platform Vars
    private bool standingOnDrill;
    private Vector2 drillCheckSize = new Vector2(0.5f, 0.5f);
    private LayerMask layerMask_drillPlatform;
    private LayerMask layerMask_drill;
    private LayerMask layerMask_medkit;
    private Drill drill;
    #endregion

    #region Special Tile Vars
    private bool standingInWater;
    private bool standingOnIce;
    private bool standingInLava;
    private LayerMask layerMask_water;
    private LayerMask layerMask_ice;
    private LayerMask layerMask_lava;
    private LayerMask layerMask_hole;
    #endregion

    //Movement Vars
    private float velx;
    private float vely;
    private Vector2 bodyVelocity = new Vector2(0f,0f);
    private float accelerationUpdateTimer;
    private float accelX;
    private float accelY;

    //Status Vars
    private bool frozen = false;
    private int frozenMeter = 0;
    private float frozenMeter_updateTimer;
    private float frozenMeter_statusDecayTimer;
    private float iceDamageTimer;
    private bool onFire = false;
    private float fireDamageTimer;
    private float fireDecayTimer;

    //Wall detection (to zero out momentum while sliding on ice)
    private bool touchingWallNorth;
    private bool touchingWallSouth;
    private bool touchingWallEast;
    private bool touchingWallWest;

    //Mining Vars
    private LayerMask layerMask_wall;
    private bool mineOnCooldown = false;
    private float mineCooldownTimer = 0f;

    //Shooting Vars
    private bool shootOnCooldown = false;
    private float shootCooldownTimer = 0f;
    private int magSize = 4;
    private int ammo = 4;
    private bool reloading;
    private float reloadTimer = 1.5f;
    private float reloadbarHideTimer = 0f;
    [Header("Reload")]
    public SpriteRenderer reloadBar;
    public GameObject reloadPin;

    //Dodge Vars
    private bool dodging;               //true when in dodge anim
    private float dodgeTimer;           //time to complete dodge
    private float dodgeBuffer = 0f;     //time it takes to process a fall after a dodge
    private bool dodgeReady;            //true when dodge not on cooldown
    private float dodgeCooldownTimer;   //Tracks cooldown time of dodge
    private Vector2 dodgeDirection;     //Movement vector stays the same throughout the dodge
    [Header("Dodge")]
    public Image dodgeCircle;
    public SpriteRenderer dodgeBar;
    public Animator dodgeBarAnim;

    //Falling Vars
    private bool falling;           //The player fell in a hole
    private float fallingTimer;     //The length of the falling anim
    [Header("Falling")]
    public Transform respawnPoint;  //The place the player teleprts to after a fall

    //Anims
    private Animator playerAnims;

    //Audio
    [Header("Audio")]
    public AudioClip[] sounds_mining;
    public AudioClip[] sounds_status;
    public AudioClip[] sounds_reload;
    public AudioSource audioSource_repair;
    public AudioSource audioSource_status;
    public AudioSource audioSource_mine;
    public AudioSource audioSource_rubble;
    public AudioSource audioSource_reload;
    public AudioSource audioSource_fall;
    public AudioSource audioSource_roll;
    private AudioManager audioManager;

    [Header("Particle Systems")]
    public ParticleSystem particles_footprints;
    public ParticleSystem particles_wading;
    public ParticleSystem particles_frozen;
    public ParticleSystem particles_onFire;
    public ParticleSystem particles_dodge;


    /* Needed so player-hole collision can be turned on and off
     * so players can pass thru the hole tiles when on the drill Plat
     */ 
    private int layerMaskINT_player;
    private int layerMaskINT_hole;




    // Start is called before the first frame update
    void Start()
    {
        //Components
        body = GetComponent<Rigidbody2D>();
        playerAnims = GetComponent<Animator>();
        playerTransform = GetComponent<Transform>();

        //Scripts
        controls = GameObject.FindGameObjectWithTag("GameManager").GetComponent<InputSystem>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        upgradeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<UpgradeManager>();
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();
        drill = GameObject.FindGameObjectWithTag("Drill").GetComponent<Drill>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        cheats = GameObject.FindGameObjectWithTag("cheats").GetComponent<CheatsManager>();

        //particles
        mineParticles.Stop();
        breakParticles.Stop();

        //Layer Masks
        layerMask_drillPlatform = LayerMask.GetMask("drillPlatform");
        layerMask_drill = LayerMask.GetMask("drill");
        layerMask_wall = LayerMask.GetMask("wall");
        layerMask_water = LayerMask.GetMask("water");
        layerMask_ice = LayerMask.GetMask("ice");
        layerMask_lava = LayerMask.GetMask("lava");
        layerMask_hole = LayerMask.GetMask("hole");
        layerMaskINT_player = LayerMask.NameToLayer("player");
        layerMaskINT_hole = LayerMask.NameToLayer("hole");
        layerMask_medkit = LayerMask.GetMask("medkit");
    }

    // Update is called once per frame
    void Update()
    {
        #region Detecting Ground Types
        //Drill Platform Detection
        standingOnDrill = Physics2D.OverlapBox(playerTransform.position, drillCheckSize, 0, layerMask_drillPlatform);

        //Water Detection
        standingInWater = Physics2D.OverlapBox(playerTransform.position, drillCheckSize, 0, layerMask_water) && !standingOnDrill && !dodging;

        //Ice Detection
        standingOnIce = Physics2D.OverlapBox(playerTransform.position, drillCheckSize, 0, layerMask_ice) && !standingOnDrill;

        //Lava Detection
        standingInLava = Physics2D.OverlapBox(playerTransform.position, drillCheckSize, 0, layerMask_lava) && !standingOnDrill && !dodging;
        #endregion

        checkWallCollisions();

        #region Movement

        //Ice Movement
        if (standingOnIce && !standingOnDrill && !standingInWater)
        {

            //assign acceleration values
            if (controls.up())
            {
                accelY = ConstantLibrary.iceAccelerationRate;
            }
            else if (controls.down())
            {
                accelY = -ConstantLibrary.iceAccelerationRate;
            }
            else
            {
                accelY = 0f;
            }

            if (controls.left())
            {
                accelX = -ConstantLibrary.iceAccelerationRate;
            }
            else if (controls.right())
            {
                accelX = ConstantLibrary.iceAccelerationRate;
            }
            else
            {
                accelX = 0f;
            }
        }
        //(Regular Movement if not on ice) Assign Velocity Based on Player Input
        else
        {
            if (controls.up()) 
            { 
                vely = moveSpeed; 
            }
            else if (controls.down()) 
            { 
                vely = -moveSpeed; 
            }
            else 
            {
                vely = 0f; 
            }

            if (controls.left()) 
            {
                velx = -moveSpeed; 
            }
            else if (controls.right()) 
            {
                velx = moveSpeed; 
            }
            else {
                velx = 0; 
            }
        }

        //Modify Speed in Liquids
        if((standingInLava || standingInWater) && !standingOnDrill)
        {
            velx = velx / 2;
            vely = vely / 2;

            //divide again if frozen
            if (frozen)
            {
                velx = velx / 2;
                vely = vely / 2;
            }
        }

        //Set Animations & Movement Particles
        if(Mathf.Abs(velx) > 0 || Mathf.Abs(vely) > 0)
        {
            playerAnims.SetBool("walking", true);

            //Water Wading Particles
            if (standingInWater)
            {
                particles_wading.emissionRate = 10f;
                particles_footprints.emissionRate = 0f;
            }
            //Footprints
            else
            {
                particles_wading.emissionRate = 0f;
                particles_footprints.emissionRate = 4f;
            }
        }
        else
        {
            playerAnims.SetBool("walking", false);

            //water wading particles
            if (standingInWater)
            {
                particles_wading.emissionRate = 1.5f;
                particles_footprints.emissionRate = 0f;
            }
            //footprints
            else
            {
                particles_wading.emissionRate = 0f;
                particles_footprints.emissionRate = 0f;
            }
        }

        //Stop Particles once the player stands on the drill
        if (standingOnDrill)
        {
            particles_wading.emissionRate = 0f;
            particles_footprints.emissionRate = 0f;
        }


        //Diagonal Movement 
        if(Mathf.Abs(velx) == Mathf.Abs(vely) && !standingOnIce)
        {
            velx = velx * (1 / Mathf.Sqrt(2));
            vely = vely * (1 / Mathf.Sqrt(2));
        }

        if (standingOnDrill) { velx = velx + drill.getDrillBodyVelocity(); }

        //Kill momentum when hitting walls 
        resetVelocityOnWallCollision();

        //dodge (player must be moving in SOME direction)
        if (controls.dodge() && dodgeReady && (velx != 0 || vely != 0)) { startDodge(); }

        if (dodging) { processDodge(); }

        if (falling)
        {
            velx = 0f;
            vely = 0f;
        }

        //apply velocity
        bodyVelocity.Set(velx, vely);
        body.velocity = bodyVelocity;
        #endregion


        #region Offscreen
        if(transform.position.x < drill.transform.position.x - 36.5)
        {
            if (!gameManager.getOffscreen())
            {
                gameManager.setOffscreen(true);
            }
        }
        else
        {
            if (gameManager.getOffscreen())
            {
                gameManager.setOffscreen(false);
            }
        }
        #endregion


        //orient player
        if (gameManager.pc)
        {
            GetComponent<Transform>().right = controls.getMousePos() - body.position;
        }
        if (dodging)
        {
            GetComponent<Transform>().right = dodgeDirection;
        }

        #region Mining
        if (controls.mining() && !dodging)
        {
            if (!mineOnCooldown)
            {
                mineOnCooldown = true;
                mineCooldownTimer = 0.5f;
                mine();
            }
        }
        #endregion

        #region Shooting
        if (controls.shooting() && !dodging)
        {
            if (!shootOnCooldown && ammo > 0 && !reloading)
            {
                shootOnCooldown = true;
                shootCooldownTimer = 1f - upgradeManager.getFireRateReduction();
                shoot();
                changeAmmo(-1);
            }
            //Empty Fire Sound
            else if(!shootOnCooldown && !reloading && ammo <= 0)
            {
                shootOnCooldown = true;
                shootCooldownTimer = 1f - upgradeManager.getFireRateReduction();
                audioSource_reload.clip = sounds_reload[2];
                audioSource_reload.volume = 0.2f * audioManager.getMixedSfx();
                audioSource_reload.pitch = 2f;
                audioSource_reload.Play();
            }
        }
        #endregion

        #region Reloading
        if(controls.reload() && !reloading && ammo < magSize)
        {
            //Start Timer
            reloadTimer = ConstantLibrary.defaultReloadTime - (0.35f * upgradeManager.getReloadSpeed());
            reloading = true;

            //Play Anim
            reloadBar.color = new Vector4(1f, 1f, 1f, 1f);
            reloadPin.GetComponent<Animator>().enabled = true;
            reloadPin.GetComponent<Animator>().speed = 1f / reloadTimer;
            reloadPin.GetComponent<Animator>().SetTrigger("reload");

            //Play Sounds
            audioSource_reload.volume = audioManager.getMixedSfx();
            audioSource_reload.pitch = 1f;
            audioSource_reload.clip = sounds_reload[0];
            audioSource_reload.Play();
        }
        if (reloading)
        {
            reloadTimer -= Time.deltaTime;
            if(reloadTimer <= 0f)
            {
                //Stop Reload & fill mag
                reloading = false;
                changeAmmo(magSize);

                //Hide bar & pin
                reloadBar.color = new Vector4(1f, 1f, 1f, 0f);
                reloadPin.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 0f);
                reloadPin.GetComponent<Animator>().enabled = false;

                //Play Sounds
                audioSource_reload.volume = 0.5f * audioManager.getMixedSfx();
                audioSource_reload.pitch = 1f;
                audioSource_reload.clip = sounds_reload[1];
                audioSource_reload.Play();
            }
        }
        //Hide Reload Bar Info
        if (reloadbarHideTimer > 0) { reloadbarHideTimer -= Time.deltaTime; }
        if(reloadbarHideTimer <= 0 && !reloading && ammo > 0)
        {
            //hide bar info
            reloadBar.color = new Vector4(1f, 1f, 1f, 0f);
            reloadPin.GetComponent<SpriteRenderer>().color = new Vector4(1f, 0f, 0f, 0f);
        }
        #endregion

        //Hole Collision
        Physics2D.IgnoreLayerCollision(layerMaskINT_player, layerMaskINT_hole, (standingOnDrill || dodging));
    }

    private void FixedUpdate()
    {
        #region Ice Movement
        if (standingOnIce)
        {
            //update velocity on intervals
            accelerationUpdateTimer -= Time.deltaTime;
            if(accelerationUpdateTimer <= 0)
            {
                accelerationUpdateTimer = 0.05f;

                //add acceleration to velocity on the x-axis
                if (frozen) { velx += accelX / 2; }
                else        { velx += accelX; }

                //cap speed
                if(Mathf.Abs(velx) > moveSpeed/2 && frozen)
                {
                    velx = moveSpeed/2 * Mathf.Sign(velx);
                }
                if(Mathf.Abs(velx) > moveSpeed)
                {
                    velx = moveSpeed * Mathf.Sign(velx);
                }

                //add acceleration to velocity on the y-axis
                if (frozen) { vely += accelY / 2; }
                else        { vely += accelY; }

                //cap speed
                if(Mathf.Abs(vely) > moveSpeed/2 && frozen)
                {
                    vely = moveSpeed/2 * Mathf.Sign(vely);
                }
                if(Mathf.Abs(vely) > moveSpeed)
                {
                    vely = moveSpeed * Mathf.Sign(vely);
                }
            }
        }
        #endregion

        #region Frozen Status
        if(standingInWater && world.getCurrentBiome() == ConstantLibrary.BIO_ICE)
        {
            //reset status decay timer
            frozenMeter_statusDecayTimer = ConstantLibrary.frozenMeter_statusDecayTime;

            //initialize ice buildup
            if(frozenMeter <= 0)
            {
                frozenMeter = 1;
                frozenMeter_updateTimer = ConstantLibrary.frozenMeter_updateFrequency;
            }

            //build frozen meter
            if (frozenMeter < 6)
            {
                frozenMeter_updateTimer -= Time.deltaTime;
            }
            if(frozenMeter_updateTimer <= 0)
            {
                frozenMeter++;
                frozenMeter_updateTimer = ConstantLibrary.frozenMeter_updateFrequency;
            }

            //max frozen meter
            if(frozenMeter >= 6)
            {
                if (!frozen)
                {
                    audioSource_status.clip = sounds_status[0];
                    audioSource_status.volume = 1f * audioManager.getMixedSfx();
                    audioSource_status.Play();
                }
                frozen = true;
                frozenMeter = 6;
            }
        }
        else
        {
            //begin to decay frozen status 
            if (frozen)
            {
                frozenMeter_statusDecayTimer -= Time.deltaTime;
                if(frozenMeter_statusDecayTimer <= 0)
                {
                    frozen = false;
                    frozenMeter = 5;
                    frozenMeter_updateTimer = ConstantLibrary.frozenMeter_updateFrequency;
                }
            }

            //decay frozen meter
            if(frozenMeter > 0 && frozenMeter < 6)
            {
                frozenMeter_updateTimer -= Time.deltaTime;
                if(frozenMeter_updateTimer <= 0)
                {
                    frozenMeter--;
                    frozenMeter_updateTimer = ConstantLibrary.frozenMeter_updateFrequency;
                }
            }

            //zero frozen meter
            if(frozenMeter < 0)
            {
                frozenMeter = 0;
            }
        }

        //snowflake particles & player color
        if (frozen)
        {
            if (!particles_frozen.isPlaying)
            {
                particles_frozen.Play();
            }
            GetComponent<SpriteRenderer>().color = new Vector4(0f, 0f, 1f, 1f);
        }
        else
        {
            particles_frozen.Stop();
            GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 1f);
        }
        #endregion

        #region On Fire Status
        //initiate onFire status when in lava, do not begin decay until out of lava
        if (standingInLava)
        {
            onFire = true;
            fireDecayTimer = ConstantLibrary.onFire_decayTime;
        }
        else
        {
            if (onFire)
            {
                fireDecayTimer -= Time.deltaTime;
                if(fireDecayTimer <= 0)
                {
                    onFire = false;
                    fireDecayTimer = ConstantLibrary.onFire_decayTime;
                }
            }
        }

        //Particles and player color
        if (onFire)
        {
            if (!particles_onFire.isPlaying)
            {
                particles_onFire.Play();
            }
            GetComponent<SpriteRenderer>().color = new Vector4(1f, 0f, 0f, 1f);
        }
        else
        {
            particles_onFire.Stop();
            GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 1f);
        }
        #endregion

        #region MineColldown
        if (mineOnCooldown)
        {
            mineCooldownTimer -= Time.fixedDeltaTime;
        }

        if(mineCooldownTimer <= 0)
        {
            mineOnCooldown = false;
            mineCooldownTimer = 0.5f;
        }
        #endregion

        #region ShootCooldown
        if (shootOnCooldown)
        {
            shootCooldownTimer -= Time.fixedDeltaTime;
        }

        if(shootCooldownTimer <= 0)
        {
            shootOnCooldown = false;
            shootCooldownTimer = 1f - upgradeManager.getFireRateReduction();
        }
        #endregion

        #region Status Based Damage
        //Ice tick damage
        if (frozen)
        {
            iceDamageTimer -= Time.deltaTime;
            if(iceDamageTimer <= 0)
            {
                iceDamageTimer = ConstantLibrary.frozen_damageRate;
                gameManager.changePlayerHealth(-5, ConstantLibrary.DMGSRC_ICE);
            }
        }
        else
        {
            iceDamageTimer = ConstantLibrary.frozen_damageRate;
        }

        //Fire tick damage
        if (onFire)
        {
            fireDamageTimer -= Time.deltaTime;
            if(fireDamageTimer <= 0)
            {
                fireDamageTimer = ConstantLibrary.onFire_damageRate;
                audioSource_status.clip = sounds_status[1];
                audioSource_status.volume = 1f * audioManager.getMixedSfx();
                audioSource_status.Play();
                //Take extra damage if standing in lava
                if (standingInLava)
                {
                    gameManager.changePlayerHealth(-10, ConstantLibrary.DMGSRC_FIRE);
                }
                else
                {
                    gameManager.changePlayerHealth(-5, ConstantLibrary.DMGSRC_FIRE);
                }
            }
        }
        else
        {
            fireDamageTimer = ConstantLibrary.onFire_damageRate;
        }
        #endregion

        #region DodgeCooldown
        if (!dodgeReady)
        {
            dodgeCooldownTimer -= Time.deltaTime;
            dodgeCircle.fillAmount = 1f - dodgeCooldownTimer / (ConstantLibrary.defaultDodgeCooldownTime - (1f * upgradeManager.getDodgeCooldownUpgrades()));
            if(dodgeCooldownTimer <= 0)
            {
                dodgeReady = true;
                dodgeBar.color = new Vector4(1f, 1f, 1f, 0f);
                dodgeCircle.enabled = false;
            }
        }

        if(dodgeBuffer > 0)
        {
            dodgeBuffer -= Time.deltaTime;
        }
        #endregion

        #region falling
        //Check Falling (after load screen gone)
        if (!gameManager.loadScreen.activeSelf)
        {
            if (gameManager.getPlayerLocationNode().tileType == ConstantLibrary.T_HOLE && 
                !standingOnDrill && 
                !dodging && 
                !falling && 
                dodgeBuffer <= 0) 
            { fall(); }
        }

        //Do Falling
        if (falling)
        {
            fallingTimer -= Time.deltaTime;
            if(fallingTimer <= 0)
            {
                standingOnDrill = true;
                falling = false;
                playerAnims.SetBool("falling", false);
                //lose 40% of max HP on a fall
                gameManager.changePlayerHealth(-Mathf.RoundToInt(0.4f * gameManager.getPlayerHealthMax()), ConstantLibrary.DMGSRC_FALL);
                respawn();
            }
        }
        #endregion
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Explosion" && !dodging)
        {
            Debug.Log("Player hit by explosion!");
            gameManager.changePlayerHealth(-25, ConstantLibrary.DMGSRC_PHYSICAL);
        }
    }

    #region Wall Checkers
    private bool wallCheck(Vector3 position)
    {
        if(Physics2D.OverlapBox(position, drillCheckSize, 0, layerMask_wall))
        {
            return true;
        }
        else if(Physics2D.OverlapBox(position,drillCheckSize,0,layerMask_hole) && !standingOnDrill)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void checkWallCollisions()
    {
        touchingWallEast = wallCheck(new Vector3(playerTransform.position.x + 0.2f, playerTransform.position.y, 0f));
        touchingWallNorth = wallCheck(new Vector3(playerTransform.position.x, playerTransform.position.y + 0.2f, 0f));
        touchingWallSouth = wallCheck(new Vector3(playerTransform.position.x, playerTransform.position.y - 0.2f, 0f));
        touchingWallWest = wallCheck(new Vector3(playerTransform.position.x - 0.2f, playerTransform.position.y, 0f));
    }
    #endregion

    //Applies movement in the direction player is dodging
    private void processDodge()
    {
        dodgeTimer -= Time.deltaTime;
        if(dodgeTimer <= 0)
        {
            dodging = false;
            playerAnims.SetBool("dodging", false);
            if (gameManager.getPlayerLocationNode().tileType != ConstantLibrary.T_HOLE) { particles_dodge.Play(); }
            return;
        }
        velx = dodgeDirection.x;
        vely = dodgeDirection.y;
    }

    private void startDodge()
    {
        //Start Dodge Status
        dodgeTimer = 0.2f;
        dodgeBuffer = 0.3f;
        dodgeCooldownTimer = ConstantLibrary.defaultDodgeCooldownTime - (1f * upgradeManager.getDodgeCooldownUpgrades());
        dodging = true;
        dodgeReady = false;

        //Set Velocity Vector
        Vector2 direction = new Vector2(velx * 2, vely * 2);
        dodgeDirection = direction.normalized * 15f;

        //Anims and Bars
        playerAnims.SetBool("dodging", true);
        dodgeCircle.fillAmount = 0;
        dodgeCircle.enabled = true;
        dodgeBarAnim.speed = 1f / dodgeCooldownTimer;
        dodgeBarAnim.SetTrigger("dodge");
        dodgeBar.color = new Vector4(1f, 1f, 1f, 1f);

        //Audio
        audioSource_roll.volume = audioManager.getMixedSfx();
        audioSource_roll.time = 0.15f;
        audioSource_roll.Play();

        //particles
        particles_dodge.Play();
    }

    //kill ice momentum when hitting walls
    private void resetVelocityOnWallCollision()
    {
        if (standingOnIce)
        {
            //reset velocity after hitting a wall
            //Right
            if (touchingWallEast && velx > 0)
            {
                velx = 0;
            }
            //left
            if (touchingWallWest && velx < 0)
            {
                velx = 0;
            }
            //top
            if (touchingWallNorth && vely > 0)
            {
                vely = 0;
            }
            //bottom
            if (touchingWallSouth && vely < 0)
            {
                vely = 0;
            }
        }
    }

    private void mine()
    {
        //Mining
        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, transform.right, 1f, layerMask_wall);
        if (hitWall)
        {
            Tile targetTile = hitWall.transform.parent.GetComponentInChildren<Tile>();
            targetTile.changeMineHP(-1);
            mineAnims.SetTrigger("swing");
            mine_audio(targetTile);
            mine_particles(targetTile);
        }

        //Repairing
        RaycastHit2D hitDrill = Physics2D.Raycast(transform.position, transform.right, 1f, layerMask_drill);
        if (hitDrill)
        {
            if (gameManager.getDrillHealth() < gameManager.getDrillHealthMax())
            {
                //Percentage Repair:
                //float repairAmount = (((float)(1 + upgradeManager.getRepairUpgrades())) / 10) * gameManager.getDrillHealthMax();
                //gameManager.changeDrillHealth((int)repairAmount);

                //Raw Repair
                gameManager.changeDrillHealth(2 + (upgradeManager.getRepairUpgrades() * 2));

                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_REPAIR, new Vector3(drill.transform.position.x, drill.transform.position.y + 3, drill.transform.position.z));
                audioSource_repair.volume = 0.8f * audioManager.getMixedSfx();
                audioSource_repair.Play();
            }
        }

        //Consuming medkit
        RaycastHit2D hitMedkit = Physics2D.Raycast(transform.position, transform.right, 1f, layerMask_medkit);
        if (hitMedkit && drill.getMedkitReady() && gameManager.getPlayerHealth() < gameManager.getPlayerHealthMax())
        {
            drill.consumeMedkit();
        }
    }

    private void mine_audio(Tile targetTile)
    {
        if (targetTile.tileType != ConstantLibrary.T_HOLE)
        {
            //set audi0 clip
            if (targetTile.tileType == ConstantLibrary.T_VINE)
            {
                audioSource_mine.volume = 2f * audioManager.getMixedSfx() > 1f ? 1f : 2f * audioManager.getMixedSfx();
                audioSource_mine.clip = sounds_mining[1];
            }
            else
            {
                audioSource_mine.volume = 0.2f * audioManager.getMixedSfx();
                audioSource_mine.clip = sounds_mining[0];
            }
            audioSource_mine.Play();
            
            if (targetTile.getMineHP() == 0 && targetTile.tileType != ConstantLibrary.T_VINE)
            {
                audioSource_rubble.volume = 1f * audioManager.getMixedSfx();
                audioSource_rubble.Play();
            }
        }
    }

    private void mine_particles(Tile targetTile)
    {
        if (targetTile.getMineHP() == 0)
        {
            breakParticles.Play();
        }
        else
        {
            mineParticles.Play();
        }
    }

    private void shoot()
    {
        Laser laser = gameManager.getLaser();
        laser.spawnLaser(body.position, transform.rotation, transform.right);
    }

    private void changeAmmo(int amount)
    {
        //change Ammo
        ammo += amount;
        if (ammo > magSize) { ammo = magSize; }

        updateAmmoBar();
    }

    private void updateAmmoBar()
    {
        if (ammo < magSize)
        {
            reloadBar.color = ammo == 0 ? new Vector4(1f, 0f, 0f, 1f) : new Vector4(1f, 1f, 1f, 1f);
            reloadPin.GetComponent<SpriteRenderer>().color = new Vector4(1f, 0f, 0f, 1f);
            float pinPosition = -2.7f + ((float)ammo / magSize) * 5.4f;
            reloadPin.transform.localPosition = new Vector3(pinPosition, 0f, 0f);
            reloadbarHideTimer = 1.75f;
        }
    }

    public void fall()
    {
        //start fall status
        falling = true;
        fallingTimer = 1.5f;

        //animation
        playerAnims.SetBool("falling", true);

        //sound
        audioSource_fall.volume = 0.5f * audioManager.getMixedSfx();
        audioSource_fall.time = 0.5f;
        audioSource_fall.Play();
    }

    private void respawn()
    {
        GetComponent<Transform>().position = respawnPoint.position;
    }

    public void changeMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

    public void changeMagSize(int amount)
    {
        magSize += amount;
        changeAmmo(amount);
    }

    public bool getStandingOnDrillPlatform()
    {
        return standingOnDrill;
    }

    public bool getStandingInWater()
    {
        return standingInWater;
    }

    public int getFrozenMeter()
    {
        return frozenMeter;
    }

    public bool getDodging()
    {
        return dodging;
    }
}
