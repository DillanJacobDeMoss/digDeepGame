using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drill : MonoBehaviour
{
    [Header ("The Speed that the Drill Moves")]
    public float drillSpeed;

    [Header("The Tip of the Drill (To Update World Cols)")]
    public Transform drillTip;

    [Header("The World Object")]
    public World world;

    [Header("Drill Particle Systems")]
    public ParticleSystem[] drillParticles;
    public ParticleSystem overDriveParticls;

    [Header("Drill Chassis Sprites")]
    public SpriteRenderer chassisRenderer;
    public Sprite[] chassisSprites;

    [Header("Drill Chassis Smoke")]
    public ParticleSystem[] particles_drillDMG;

    [Header("Drill Fuel Gauge")]
    public SpriteRenderer fuelGauge;
    public GameObject fuelGaugeNeedle;

    [Header("Drill HP Display")]
    public SpriteRenderer hp_background;
    public SpriteRenderer hp_ones;
    public SpriteRenderer hp_tens;
    public SpriteRenderer hp_hundreads;
    public Sprite[] numbers_HP;

    [Header("Medkit Display")]
    public Animator medkitAnim;
    public SpriteRenderer medkit_ones;
    public SpriteRenderer medkit_tens;
    public SpriteRenderer medkit_mins;
    public SpriteRenderer medkit_colon;
    public Sprite[] numbers_medkit;
    public AudioSource medkitAudio;
    private float medkit_cooldownTimer;
    private bool medkit_ready;

    [Header("Sounds")]
    public AudioSource engineSounds;
    public AudioClip sound_engine;
    public AudioClip sound_engine_overdrive;
    public AudioSource startupSounds;
    public AudioClip sound_startDrill;
    public AudioClip sound_startOverdrive;
    public AudioSource warningSounds;

    [Header("Animations")]
    public Animator a_drillHeadPulse;
    public Animator a_drillHeadSpin;
    public Animator a_drillChassis;
    public Animator a_drillPlatform;
    public SpriteRenderer s_redLights1;
    public SpriteRenderer s_redLights2;
    public SpriteRenderer s_drillChassis;

    private bool inOverdrive;
    private Rigidbody2D drillBody;
    private Vector2 drillVelocity = new Vector2(0f, 0f);

    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private AudioManager audioManager;
    private CheatsManager cheats;

    private int tilesUntilSkip = 4;

    void Start()
    {
        drillBody = GetComponent<Rigidbody2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        upgradeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<UpgradeManager>();
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();
        cheats = GameObject.FindGameObjectWithTag("cheats").GetComponent<CheatsManager>();
        
        updateSpeed();
    }

    void Update()
    {
        
        //Update The world as the drill moves
        if(drillTip.position.x + 1 > world.getHeadPos() && world.worldInit)
        {
            world.nextColumn();
            consumeFuel();
            gameManager.incrementDrillDepth();
        }

        //update the medkit display timer
        if (!medkit_ready)
        {
            medkit_cooldownTimer -= Time.deltaTime;
            if(medkit_cooldownTimer <= 0)
            {
                medkit_ready = true;
            }
            updateMedkitDisplay();
        }
    }

    private void consumeFuel()
    {
        if (!cheats.getConsumeFuel()) { return; }
        if(upgradeManager.getFuelUpgrades() > 0)
        {
            if(tilesUntilSkip <= 0)
            {
                Debug.Log("Skipped Consuming Fuel becuase of Upgrade!");
                tilesUntilSkip = 5 - upgradeManager.getFuelUpgrades();
            }
            else
            {
                gameManager.changeFuel(-1);
                tilesUntilSkip--;
            }
        }
        else
        {
            gameManager.changeFuel(-1);
        }
    }

    public void setSpeed(float newSpeed)
    {
        drillSpeed = newSpeed;
        updateSpeed();
    }

    public void overDrive(bool set)
    {
        inOverdrive = set;
        updateSpeed();
        if (inOverdrive)
        {
            if (!overDriveParticls.isPlaying)
            {
                overDriveParticls.Play();
                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_OVERDRIVE, transform.position);
                engineSounds.clip = sound_engine_overdrive;
                engineSounds.volume = 0.6f * audioManager.getMixedSfx();
                engineSounds.Play();
                startupSounds.clip = sound_startOverdrive;
                startupSounds.volume = 0.6f * audioManager.getMixedSfx();
                startupSounds.Play();
            }
        }
        else
        {
            overDriveParticls.Stop();
            engineSounds.clip = sound_engine;
            engineSounds.volume = 0.6f * audioManager.getMixedSfx();
            engineSounds.Play();
        }

        //spriteSwap drillhead in overdrive
        a_drillHeadSpin.SetBool("drillHead_overdrive", inOverdrive);
    }

    public float getSpeed()
    {
        return drillSpeed;
    }

    public float getTrueSpeed()
    {
        return drillBody.velocity.x;
    }

    public float getDrillBodyVelocity()
    {
        return drillBody.velocity.x;
    }

    public void updateSpeed()
    {
        if (inOverdrive)
        {
            drillVelocity.Set(drillSpeed * 2, 0f);
        }
        else
        {
            drillVelocity.Set(drillSpeed, 0f);
        }

        if (cheats.getStopDrill()) { drillVelocity.Set(0f, 0f); }

        drillBody.velocity = drillVelocity;
    }

    public void stopDrillParticles()
    {
        for(int i = 0; i < drillParticles.Length; i++)
        {
            drillParticles[i].Stop();
            engineSounds.Stop();
        }
    }

    public void startDrillParticles()
    {
        for (int i = 0; i < drillParticles.Length; i++)
        {
            drillParticles[i].Play();
            startupSounds.clip = sound_startDrill;
            startupSounds.volume = 0.6f * audioManager.getMixedSfx();
            startupSounds.Play();
        }
    }

    public void drillWarningLightsActive(bool state)
    {
        s_redLights1.enabled = state;
        s_redLights2.enabled = state;
    }

    public void drillAnimationActive(bool state)
    {
        a_drillHeadPulse.SetBool("drillHead_on", state);
        a_drillHeadSpin.SetBool("drillHead_on", state);
        a_drillChassis.SetBool("drillOn", state);
        a_drillPlatform.SetBool("platformTreads_on", state);
    }

    public void chassisConditionCheck()
    {
        float drillCondition = (float)gameManager.getDrillHealth() / gameManager.getDrillHealthMax();

        //Full HP
        if(drillCondition >= 1.0f)
        {
            chassisRenderer.sprite = chassisSprites[0];
            particles_drillDMG[0].Stop();
            particles_drillDMG[1].Stop();
            particles_drillDMG[2].Stop();
        }
        //100%-75%
        else if(drillCondition < 1f && drillCondition >= 0.75f)
        {
            chassisRenderer.sprite = chassisSprites[1];
            particles_drillDMG[0].Stop();
            particles_drillDMG[1].Stop();
            particles_drillDMG[2].Stop();
        }
        //75%-50%
        else if(drillCondition < 0.75f && drillCondition >= 0.5f)
        {
            chassisRenderer.sprite = chassisSprites[2];
            particles_drillDMG[0].Stop();
            particles_drillDMG[1].Stop();
            particles_drillDMG[2].Stop();
        }
        //50%-25%
        else if(drillCondition < 0.5f && drillCondition >= 0.25f)
        {
            chassisRenderer.sprite = chassisSprites[3];

            //Turn on North Chassis Smoke
            if (!particles_drillDMG[0].isPlaying)
            {
                particles_drillDMG[0].Play();
            }

            //North Chassis sparks and south chassis smoke not active yet
            particles_drillDMG[1].Stop();
            particles_drillDMG[2].Stop();
        }
        //Less than 25%
        else
        {
            chassisRenderer.sprite = chassisSprites[4];

            //Activate all DMG particles 
            if (!particles_drillDMG[0].isPlaying)
            {
                particles_drillDMG[0].Play();
            }
            if (!particles_drillDMG[1].isPlaying)
            {
                particles_drillDMG[1].Play();
            }
            if (!particles_drillDMG[2].isPlaying)
            {
                particles_drillDMG[2].Play();
            }
        }
    }

    public void updateDrillHealthDisplay()
    {
        float drillCondition = (float)gameManager.getDrillHealth() / gameManager.getDrillHealthMax();
        if(drillCondition <= 0) { drillCondition = 0; }

        //Background Color
        float rCode = 0.7f - 0.45f * drillCondition;
        float gCode = 0.25f + 0.2f * drillCondition;
        float bCode = 0.25f * drillCondition;
        hp_background.color = new Vector4(rCode, gCode, bCode, 1f);

        //Set Digital Display
        int drillHealthPercent = Mathf.FloorToInt(drillCondition * 100);
        int onesPlace = drillHealthPercent % 10;
        int tensPlace = drillHealthPercent / 10;
        hp_ones.sprite = numbers_HP[onesPlace];
        if(tensPlace == 0)
        {
            hp_tens.enabled = false;
        }
        else if(tensPlace < 10)
        {
            hp_tens.enabled = true;
            hp_tens.sprite = numbers_HP[tensPlace];
            hp_hundreads.enabled = false;
        }
        else
        {
            hp_tens.sprite = numbers_HP[0];
            hp_hundreads.enabled = true;
            hp_hundreads.sprite = numbers_HP[1];
        }
    }

    public void updateDrillFuelGauge()
    {
        float fuelPortion = (float)gameManager.getFuel() / 100;
        float overDrivePortion = fuelPortion - 1f;
        if(overDrivePortion < 0) { overDrivePortion = 0f; }
        if(fuelPortion > 1f) { fuelPortion = 1f; }

        if(overDrivePortion <= 0)
        {
            //Non-overdrive fuel gauge coloring
            if(fuelPortion >= 0.5f)
            {
                fuelGauge.color = new Vector4(1f, 1f, 0.8f, 1f);
            }
            else
            {
                float gCode = 0.5f + fuelPortion;
                float bCode = 0.3f + fuelPortion;
                fuelGauge.color = new Vector4(1f, gCode, bCode, 1f);
            }

            //fuel needle angle & color
            float needleAngle = 180 - fuelPortion * 180;
            fuelGaugeNeedle.GetComponent<Transform>().eulerAngles = new Vector3(0f, 0f, needleAngle);
            fuelGaugeNeedle.GetComponent<SpriteRenderer>().color = new Vector4(1f, 0f, 0f, 1f);
        }
        else
        {
            //overdrive fuel gauge purple-ish
            fuelGauge.color = new Vector4(1f, 0.25f, 0.9f, 1f);

            //fuel needle angle & color
            float needleAngle = 180 - overDrivePortion * 180;
            fuelGaugeNeedle.GetComponent<Transform>().eulerAngles = new Vector3(0f, 0f, needleAngle);
            fuelGaugeNeedle.GetComponent<SpriteRenderer>().color = new Vector4(0f, 1f, 0f, 1f);
        }
        
    }

    public void updateMedkitDisplay()
    {
        //background anim
        medkitAnim.SetBool("medkitReady", medkit_ready);

        //number visibility
        medkit_colon.enabled = !medkit_ready;
        medkit_ones.enabled = !medkit_ready;
        medkit_tens.enabled = !medkit_ready;
        medkit_mins.enabled = !medkit_ready;

        //update numbers
        if (!medkit_ready)
        {
            int totalSec = Mathf.FloorToInt(medkit_cooldownTimer);
            int mins = totalSec / 60;
            int tens = (totalSec % 60) / 10;
            int ones = (totalSec % 60) % 10;

            medkit_ones.sprite = numbers_medkit[ones];
            medkit_tens.sprite = numbers_medkit[tens];
            medkit_mins.sprite = numbers_medkit[mins];
        }
    }

    public void consumeMedkit()
    {
        medkit_cooldownTimer = 180f - 30f * upgradeManager.getMedkitUpgrades();
        medkit_ready = false;
        gameManager.changePlayerHealth(gameManager.getPlayerHealthMax() - gameManager.getPlayerHealth(), ConstantLibrary.DMGSRC_HEAL);
        medkitAudio.volume = 1f * audioManager.getMixedSfx();
        medkitAudio.Play();
        Popup popup = gameManager.getPopup();
        popup.spawnPopup(ConstantLibrary.POP_HEALED, transform.position);
        updateMedkitDisplay();
    }

    public bool getMedkitReady()
    {
        return medkit_ready;
    }

    public void playWarningSounds()
    {
        warningSounds.volume = 0.3f* audioManager.getMixedSfx();
        warningSounds.Play();
    }

    public bool getInOverdrive() { return inOverdrive; }
}
