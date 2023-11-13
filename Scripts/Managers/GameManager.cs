using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Platform Type (Pick ONE)")]
    public bool pc;
    public bool mobile;

    [Header("Graphics Settings")]
    public bool enableAnimatedTiles;

    [Header("Drill Object")]
    public Drill drill;
    public Animator drillAnims;
    public Animator drillTreadAnims;

    [Header("Sounds")]
    public MusicController musicPlayer;

    //track fuel
    private int fuel = 0;

    //track drill HP
    private int HP_drill = 100;
    private int HP_drill_max = 100;

    [Header("HUD - Player Healthbar")]
    public GameObject healthbar_background;
    public GameObject healthbar_fill;
    private bool healthbarFade = false;
    private float healthbarFadeTimer = 0f;
    private int HP_player = 100;
    private int HP_player_max = 100;
    private bool playerImmune = false;
    private float playerImmunityTimer = 0.5f;

    [Header("HUD - Treasure Bar")]
    public TMP_Text levelIndicator;
    public Image treasureBar;
    private UpgradeManager upgradeManager;
    private Vector3 scale_treasure = new Vector3(0f, 0f, 0f);
    private int gold = 0;
    private int playerLevel = 1;
    private int nextLevelup = 10;

    [Header("HUD - Warnings")]
    public Image vignette_warning;
    public Image vignette_ice;
    public Animator vignette_snowflakes;
    public Animator vignette_takeDamage;
    private bool warningVignetteOn; //For extremely critical state such as NO fuel
    private bool warningSignalsOn;  //For warning states such as LOW fuel
    private float warningSoundTimer = 0f;
    public GameObject signalWarningCanvas;
    public Animator signalTime_five;
    public Animator signalTime_ten;
    public GameObject fuelWarningCanvas;
    public Animator fuelTime_five;
    public Animator fuelTime_ten;

    [Header("HUD - LOAD SCREEN")]
    public GameObject loadScreen;

    [Header("Item Pool")]
    public Item[] itemPool;
    private int itemPointer = 0; //points to the index of the next item object to bring into the playspace

    [Header("Laser Pool")]
    public Laser[] laserPool;
    private int laserPointer = 0;

    [Header("Explosion Pool")]
    public Explosion[] explosionPool;
    private int explosionPointer = 0;

    [Header("Popup Pool")]
    public Popup[] popupPool;
    private int popupPointer = 0;

    [Header("HUD - Progress Bar")]
    public GameObject progressbar_parent;
    public Image progressBar;
    public Image progressBar_Icon;
    public TMP_Text milestoneCount;
    private int previousMilestone = 0;

    [Header("HUD - Boss Bar")]
    public GameObject bossBar_parent;
    public Image bossBar;
    public TMP_Text bossName;


    //Difficulty Tracking
    private int difficulty = 0;             //Current milestone #
    private int nextMilestone = 60;         //How high the drill depth must be to reach the next milestone
    private int drillDepth = 0;             //How far the drill has traveled in whole Unity units (Tiles)
    private int worldgenType = 0;           //decides how to generate chunks (Normal/bossChunks/etc..)
    private float drillMaxSpeed = 2.5f;     //The fastest the drill can move
    private bool trackDrillDepth = true;    //set to false during events like a bossfight

    //Boss info
    private int currentBoss = ConstantLibrary.BOSS_TEST;    //The boss code of the current boss
    private int nextBossMilestone = 5;                      //The difficulty (milestone #) that the next boss will be encountered at
    //The following arrays are used to randomize biome bosses (pick one from each array)
    private int[] bosses_caves = { ConstantLibrary.BOSS_SPIDER };
    private int[] bosses_lush = { ConstantLibrary.BOSS_BEE, ConstantLibrary.BOSS_FLYTRAP };
    private int[] bosses_ice = { ConstantLibrary.BOSS_WORM };
    private int[] bosses_lava = { ConstantLibrary.BOSS_SKEL };
    [Header("Bosses")]
    public BossTest bossTest;

    //Score Tracking
    private int score_minerals = 0;
    private int score_overDrive = 0;
    private int score_scrap = 0;
    private int score_kills = 0;

    private Tile playerLocationNode;
    private float distanceToCurrentLocation = 100f;

    private Queue<Enemy> pathfindingQueue;
    private bool lockPathfinding;

    #region GAME OVER VARS
    [Header("HUD - Game Over")]
    public GameObject gameOverCanvas;
    public Image gameOverIcon;
    public TMP_Text gameOverDescription;
    public Sprite[] gameOverIcons; //indexed by game over code
    private bool gameOver;

    //Score Breakdown
    public GameObject scoreBreakdownCanvas;
    public TMP_Text scoreBreakdown_total;
    public TMP_Text scoreBreakdown_drillDepth;
    public TMP_Text scoreBreakdown_overdrive;
    public TMP_Text scoreBreakdown_minerals;
    public TMP_Text scoreBreakdown_scrap;
    public TMP_Text scoreBreakdown_kills;
    private bool newHighScore = false;

    //New High Score
    public GameObject newHighscoreCanavs;
    public TMP_InputField nameEntry;

    //Game Over: Zero Fuel
    private bool fuelEmpty;
    private float fuelEmptyTimer;

    //Game Over: Off Screen
    private bool offScreen;
    private float offScreenTimer;

    //Game Over Codes
    private const int GO_FUEL_EMPTY = 0;
    private const int GO_OFF_SCREEN = 1;
    private const int GO_PLAYER_DIE = 2;
    private const int GO_DRILL_DIE = 3;
    private const int GO_QUIT = 4;
    #endregion

    private World world;
    private Player player;
    private SaveData saveData;
    private CheatsManager cheats;

    // Start is called before the first frame update
    void Start()
    {
        upgradeManager = GetComponent<UpgradeManager>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Player>();
        saveData = GetComponent<SaveData>();
        cheats = GameObject.FindGameObjectWithTag("cheats").GetComponent<CheatsManager>();

        //default location
        playerLocationNode = world.columns[0].tiles[0];

        pathfindingQueue = new Queue<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {

        #region HealthBar
        if (healthbarFade)
        {
            healthbarFadeTimer -= Time.deltaTime;
            if(healthbarFadeTimer <= 0f)
            {
                healthbarFade = false;
                healthbar_background.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 0f);
                healthbar_fill.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 0f);
            }
            else
            {
                healthbar_background.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, healthbarFadeTimer);
                healthbar_fill.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, healthbarFadeTimer);
            }
        }
        #endregion

        #region Warning Sounds
        if (warningSignalsOn)
        {
            warningSoundTimer -= Time.deltaTime;
            if(warningSoundTimer <= 0)
            {
                drill.playWarningSounds();
                warningSoundTimer = 2f;
            }
        }
        #endregion

        //location tracking
        updateDistanceToCurrentLocation();
    }

    private void FixedUpdate()
    {
        //warning timer for no fuel
        if (fuelEmpty)
        {
            fuelEmptyTimer -= Time.fixedDeltaTime;
            if(fuelEmptyTimer <= 0)
            {
                initiateGameOver(GO_FUEL_EMPTY);
            }
        }

        //Icy Blue Vignette for in ice water
        if(player.getStandingInWater() && world.getCurrentBiome() == ConstantLibrary.BIO_ICE)
        {
            vignette_ice.enabled = true;
        }
        else
        {
            vignette_ice.enabled = false;
        }

        //Icy snowflakes for in ice water
        vignette_snowflakes.SetInteger("frozenMeter", player.getFrozenMeter());

        if (offScreen)
        {
            offScreenTimer -= Time.fixedDeltaTime;
            if(offScreenTimer <= 0)
            {
                initiateGameOver(GO_OFF_SCREEN);
            }
        }
        else
        {
            offScreenTimer = 5f;
        }

        if (playerImmune)
        {
            playerImmunityTimer -= Time.deltaTime;
            if(playerImmunityTimer <= 0)
            {
                playerImmune = false;
                player.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 1f);
            }
        }
    }

    public void removeLoadScreen()
    {
        loadScreen.SetActive(false);
    }

    public void initiateGameOver(int gameOverCode)
    {
        gameOver = true;
        Time.timeScale = 0;
        gameOverCanvas.SetActive(true);

        switch (gameOverCode)
        {
            case GO_FUEL_EMPTY:
                gameOverIcon.sprite = gameOverIcons[GO_FUEL_EMPTY];
                gameOverDescription.text = "THE DRILL RAN OUT OF FUEL";
                break;

            case GO_OFF_SCREEN:
                gameOverIcon.sprite = gameOverIcons[GO_OFF_SCREEN];
                gameOverDescription.text = "SIGNAL LOST\nYOU GOT LEFT BEHIND";
                break;

            case GO_PLAYER_DIE:
                gameOverIcon.sprite = gameOverIcons[GO_PLAYER_DIE];
                gameOverDescription.text = "YOU DIED";
                break;

            case GO_DRILL_DIE:
                gameOverIcon.sprite = gameOverIcons[GO_DRILL_DIE];
                gameOverDescription.text = "THE DRILL WAS DESTROYED";
                break;

            case GO_QUIT:
                gameOverIcon.sprite = gameOverIcons[GO_QUIT];
                gameOverDescription.text = "YOU QUIT THE GAME";
                break;
        }
    }

    public void enterScoreBreakdown()
    {
        scoreBreakdownCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        scoreBreakdown_drillDepth.text = drillDepth.ToString();
        scoreBreakdown_minerals.text = score_minerals.ToString();
        scoreBreakdown_kills.text = score_kills.ToString();
        scoreBreakdown_scrap.text = score_scrap.ToString();
        scoreBreakdown_overdrive.text = score_overDrive.ToString();
        scoreBreakdown_total.text = (drillDepth + score_minerals + score_overDrive + score_scrap + score_kills).ToString();
    }

    public void enterNewHighscoreScreen()
    {
        int totalScore = drillDepth + score_minerals + score_overDrive + score_scrap + score_kills;

        //No new High Score, return to main menu
        if (saveData.compareToHighScores(totalScore) == 0)
        {
            //Load Main Menu
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
        else
        {
            newHighscoreCanavs.SetActive(true);
            scoreBreakdownCanvas.SetActive(false);
        }
    }

    public void submitNewHighscore()
    {
        if(nameEntry.text.Length != 3)
        {
            Debug.Log("Invalid Name Entry");
            return;
        }

        int totalScore = drillDepth + score_minerals + score_overDrive + score_scrap + score_kills;
        saveData.updateHighScores(totalScore, nameEntry.text);

        //Load Main Menu
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void incrementScore_minerals(int points)
    {
        if (!gameOver)
        {
            score_minerals += points;
        }
    }

    public void incrementScore_overdrive(int points)
    {
        if (!gameOver)
        {
            score_overDrive += points;
        }
    }

    public void incrementScore_scrap(int points)
    {
        if (!gameOver)
        {
            score_scrap += points;
        }
    }

    public void incrementScore_kills(int points)
    {
        if (!gameOver)
        {
            score_kills += points;
        }
    }

    public void incrementDrillDepth()
    {
        if (trackDrillDepth && !gameOver)
        {
            drillDepth++;
            if(fuel > 100)
            {
                incrementScore_overdrive(1);
            }
            checkMilestone();
        }

        updateProgressBar();
    }

    public void checkPlayerLocationNode(Tile newLocation)
    {
        //check distance to new tile
        Vector2 playerToNewNode = player.transform.position - newLocation.transform.position;
        float distanceToNewLoaction = playerToNewNode.magnitude;

        if(distanceToNewLoaction < distanceToCurrentLocation)
        {
            distanceToCurrentLocation = distanceToNewLoaction;
            playerLocationNode = newLocation;
        }
    }

    public Tile getPlayerLocationNode()
    {
        return playerLocationNode;
    }

    private void updateDistanceToCurrentLocation()
    {
        Vector2 playerToNode = player.transform.position - playerLocationNode.transform.position;
        distanceToCurrentLocation = playerToNode.magnitude;
    }

    #region Object Pools

    public Item getItem(int itemCode)
    {
        Item item = itemPool[itemPointer];
        item.setItemProperties(itemCode);
        itemPointer++;
        if(itemPointer >= itemPool.Length)
        {
            itemPointer = 0;
        }
        return item;
    }

    public Laser getLaser()
    {
        Laser laser = laserPool[laserPointer];
        laserPointer++;
        if(laserPointer >= laserPool.Length)
        {
            laserPointer = 0;
        }
        return laser;
    }

    public Explosion getExplosion()
    {
        Explosion explosion = explosionPool[explosionPointer];
        explosionPointer++;
        if (explosionPointer >= explosionPool.Length)
        {
            explosionPointer = 0;
        }
        return explosion;
    }

    public Popup getPopup()
    {
        Popup popup = popupPool[popupPointer];
        popupPointer++;
        if(popupPointer >= popupPool.Length)
        {
            popupPointer = 0;
        }
        return popup;
    }
    #endregion

    public void updateProgressBar()
    {
        //Bar Length = 110 (1/5 bar = 22)
        //Bar start = 0
        //difficulty % 5 = which milestone pin to be at

        float milestoneFrac = getMilestoneProgress();
        float iconDistance = (22f)*(milestoneFrac + (difficulty % 5));

        progressBar_Icon.rectTransform.anchoredPosition = new Vector3(iconDistance, -1f, 0f);

        milestoneCount.text = difficulty.ToString();
    }

    public float getMilestoneProgress()
    {
        float currentProgress = ((float)drillDepth - previousMilestone) / (nextMilestone - previousMilestone);
        return currentProgress;
    }

    public void changeFuel(int amount)
    {
        fuel += amount;
        if(fuel > 200)
        {
            incrementScore_overdrive(fuel - 200);
            fuel = 200;
        }
        updateFuel(); //REFACTOR THIS, ITS VERY UGLY

        drill.updateDrillFuelGauge();
    }

    private void updateFuel()
    {
        //Set drill anims according to fuel
        drill.drillAnimationActive(fuel > 0);

        if (fuel > 100)
        {
            fuelEmpty = false;

            //restart drill if player fueled it before the empty timer ran out
            if(drill.getSpeed() == 0)
            {
                drill.setSpeed(1 + ((float)difficulty / 10));
                drill.startDrillParticles();
                drillAnims.SetBool("drillOn", true);
                drillTreadAnims.SetBool("drillOn", true);
                checkWarnings();
                fuelWarningCanvas.SetActive(false);
            }

            drill.overDrive(true);
        }
        else if(fuel > 0 && fuel <= 100)
        {
            fuelEmpty = false;

            //restart drill if player fueled it before the empty timer ran out
            if (drill.getSpeed() == 0)
            {
                musicPlayer.startMusic(); //only truly happens once
                drill.setSpeed(1 + ((float)difficulty / 10));
                drill.startDrillParticles();
                drillAnims.SetBool("drillOn", true);
                drillTreadAnims.SetBool("drillOn", true);
                checkWarnings();
                fuelWarningCanvas.SetActive(false);
            }

            drill.overDrive(false);
        }
        else
        {
            drill.stopDrillParticles();
            drillAnims.SetBool("drillOn", false);
            drillTreadAnims.SetBool("drillOn", false);
            drill.setSpeed(0f);
            fuelEmptyTimer = 5.0f;
            fuelEmpty = true;
            checkWarnings();
            fuelWarningCanvas.SetActive(true);
            fuelTime_five.SetTrigger("startTimer");
            fuelTime_ten.SetTrigger("startTimer");
        }

        checkWarnings();
    }

    public int getFuel()
    {
        return fuel;
    }

    public void incrementGold(int amount)
    {
        //overdrive bonus
        if(fuel >= 100)
        {
            gold += amount;
        }
        gold += amount;

        updateTreasurebar();
    }

    private void updateTreasurebar()
    {
        int levelUps = 0;
        while(gold >= nextLevelup)
        {
            gold = gold - nextLevelup;
            playerLevel++;
            levelIndicator.text = ("  LVL: " + playerLevel.ToString());
            nextLevelup += 10;
            levelUps++;
        }
        upgradeManager.setPendingLevelups(levelUps);
        if(levelUps > 0)
        {
            upgradeManager.levelUp();
        }

        scale_treasure.Set(((float)gold) / nextLevelup, 1f, 1f);
        treasureBar.rectTransform.localScale = scale_treasure;
    }

    public void changePlayerHealth(int amount, int source)
    {
        switch (source)
        {
            case ConstantLibrary.DMGSRC_HEAL:
                HP_player += amount;
                if (HP_player > HP_player_max)
                {
                    HP_player = HP_player_max;
                }
                break;

            case ConstantLibrary.DMGSRC_PHYSICAL:
                if (cheats.getGodmodePlayer()) { return; }
                if (!playerImmune)
                {
                    vignette_takeDamage.SetTrigger("takeDamage");
                    HP_player += amount;
                    playerImmune = true;
                    playerImmunityTimer = 0.5f;
                    player.GetComponent<SpriteRenderer>().color = new Vector4(1f, 0.75f, 0.75f, 1f);
                }
                break;

            case ConstantLibrary.DMGSRC_ICE:
                if (cheats.getGodmodePlayer()) { return; }
                vignette_takeDamage.SetTrigger("takeDamage");
                HP_player += amount;
                break;

            case ConstantLibrary.DMGSRC_FIRE:
                if (cheats.getGodmodePlayer()) { return; }
                vignette_takeDamage.SetTrigger("takeDamage");
                HP_player += amount;
                break;

            case ConstantLibrary.DMGSRC_FALL:
                if (cheats.getGodmodePlayer()) { return; }
                vignette_takeDamage.SetTrigger("takeDamage");
                HP_player += amount;
                playerImmune = true;
                playerImmunityTimer = 0.5f;
                player.GetComponent<SpriteRenderer>().color = new Vector4(1f, 0.75f, 0.75f, 1f);
                break;
        }

        //Check Death
        if(HP_player <= 0)
        {
            initiateGameOver(GO_PLAYER_DIE);
        }

        updatePlayerHealthbar();
    }

    private void updatePlayerHealthbar()
    {
        float hpFraction = (float)HP_player / HP_player_max;
        healthbar_fill.GetComponent<Transform>().localScale = new Vector3(hpFraction, 1f, 1f);
        if(hpFraction >= 1f)
        {
            healthbarFade = true;
            healthbarFadeTimer = 1f;
        }
        else
        {
            healthbar_background.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 1f);
            healthbar_fill.GetComponent<SpriteRenderer>().color = new Vector4(1f, 1f, 1f, 1f);
        }

        checkWarnings();
    }

    public int getPlayerHealth()
    {
        return HP_player;
    }

    public int getPlayerHealthMax()
    {
        return HP_player_max;
    }

    public void incrementMaxPlayerHealth(int amount)
    {
        HP_player_max += amount;
        HP_player += amount;
        updatePlayerHealthbar();
    }

    public void changeDrillHealth(int amount)
    {
        if(cheats.getGodmodeDrill() && amount < 0) { return; }
        HP_drill += amount;
        if(HP_drill > HP_drill_max)
        {
            HP_drill = HP_drill_max;
        }

        if(HP_drill <= 0)
        {
            initiateGameOver(GO_DRILL_DIE);
        }

        checkWarnings();
        drill.chassisConditionCheck();
        drill.updateDrillHealthDisplay();
    }

    public int getDrillHealth()
    {
        return HP_drill;
    }

    public int getDrillHealthMax()
    {
        return HP_drill_max;
    }

    public void incrementMaxDrillHealth(int amount)
    {
        HP_drill_max += amount;
        HP_drill += amount;

        checkWarnings();
        drill.chassisConditionCheck();
        drill.updateDrillHealthDisplay();
    }

    private void checkMilestone()
    {
        if(drillDepth >= nextMilestone)
        {
            //Increment difficulty & set next milestone
            difficulty++;
            previousMilestone = nextMilestone;
            float milestoneIncrement = 60*(1 + ((float)difficulty/10));
            nextMilestone = nextMilestone + (int)milestoneIncrement;

            //Increase Drill Speed
            drill.setSpeed(1 + ((float)difficulty / 10));
            if(drill.getSpeed() >= drillMaxSpeed)
            {
                drill.setSpeed(drillMaxSpeed);
            }

            //Start Boss battle if boss milestone reached
            if(difficulty == nextBossMilestone)
            {
                startBossBattle();
                nextBossMilestone += 5;
            }
            else
            {
                //Dont update Biome when a boss battle starts
                updateBiome();
            }
        }
    }

    private void startBossBattle()
    {
        //World Gen Updates
        trackDrillDepth = false;
        worldgenType = ConstantLibrary.WORLDTYPE_BOSS;

        //UI changes
        swapProgessBossBar();
        switch (currentBoss)
        {
            case ConstantLibrary.BOSS_TEST:
                //Set Bossbar Name
                bossName.text = "TEST BOSS";
                //Activate Boss
                bossTest.activateBoss();
                break;
        }
    }

    public void endBossBattle()
    {
        //World Gen Updates
        trackDrillDepth = true;
        worldgenType = ConstantLibrary.WORLDTYPE_NORMAL;
        updateBiome();

        //UI changes
        swapProgessBossBar();

    }

    private void swapProgessBossBar()
    {
        if (trackDrillDepth)
        {
            progressbar_parent.SetActive(true);
            bossBar_parent.SetActive(false);
        }
        else
        {
            progressbar_parent.SetActive(false);
            bossBar_parent.SetActive(true);
        }
    }

    public void updateBossHealthbar(int HP, int HP_MAX)
    {
        float HP_frac = (float)HP / HP_MAX;
        bossBar.rectTransform.localScale = new Vector3(HP_frac, 1f, 1f);
    }

    public void updateBiome()
    {
        switch (difficulty)
        {
            case 0:
                world.setCurrerntBiome(ConstantLibrary.BIO_DEFAULT);
                break;

            case 5:
                world.setCurrerntBiome(ConstantLibrary.BIO_GREEN_TRANS);
                break;

            case 6:
                world.setCurrerntBiome(ConstantLibrary.BIO_GREEN);
                break;

            case 10:
                world.setCurrerntBiome(ConstantLibrary.BIO_ICE_TRANS);
                break;

            case 11:
                world.setCurrerntBiome(ConstantLibrary.BIO_ICE);
                break;

            case 15:
                world.setCurrerntBiome(ConstantLibrary.BIO_LAVA_TRANS);
                break;

            case 16:
                world.setCurrerntBiome(ConstantLibrary.BIO_LAVA);
                world.playAshParticles(true);
                break;

            default:
                //DO Nothing
                break;
        }
    }

    public void jumpToNextMilestone()
    {
        drillDepth = nextMilestone;
    }

    public int getDifficulty()
    {
        return difficulty;
    }

    public void setDifficulty(int diff)
    {
        difficulty = diff;
    }

    #region Pathfinding
    public bool pathfindingLocked()
    {
        return lockPathfinding;
    }

    public void waitToPathfind(Enemy enemy)
    {
        pathfindingQueue.Enqueue(enemy);
    }

    public void nextPathfind()
    {
        if(pathfindingQueue.Count > 0)
        {
            Enemy enemy = pathfindingQueue.Dequeue();
            enemy.validatePathfind();
        }
    }

    public void setPathfindingLock(bool set)
    {
        lockPathfinding = set;
    }

    #endregion

    #region WARNINGS
    private void checkWarnings()
    {
        //cheack warning vignette condtions
        if (fuelEmpty)
        {
            warningVignetteOn = true;
        }
        else if (offScreen)
        {
            warningVignetteOn = true;
        }
        else if((float)HP_player/HP_player_max <= 0.25f)
        {
            warningVignetteOn = true;
        }
        else if((float)HP_drill/HP_drill_max <= 0.25f)
        {
            warningVignetteOn = true;
        }
        else
        {
            warningVignetteOn = false;
        }

        //Turn on/off warning vignette
        if (warningVignetteOn)
        {
            vignette_warning.enabled = true;
        }
        else
        {
            vignette_warning.enabled = false;
        }

        bool fuelCritical = fuel <= 10;
        bool drillHPCritical = (float)HP_drill / HP_drill_max <= 0.25f;
        warningSignalsOn = (fuelCritical || drillHPCritical);
        if (!warningSignalsOn) { warningSoundTimer = 0f; }
        drill.drillWarningLightsActive(warningSignalsOn);
    }

    public void setOffscreen(bool set)
    {
        offScreen = set;
        if (offScreen)
        {
            signalWarningCanvas.SetActive(true);
            signalTime_five.SetTrigger("startTimer");
            signalTime_ten.SetTrigger("startTimer");
        }
        else
        {
            signalWarningCanvas.SetActive(false);
        }
        checkWarnings();
    }

    public bool getOffscreen()
    {
        return offScreen;
    }
    #endregion

    public void resetGamespaceToOrigin()
    {
        //Where the drill and columns are postioned at the start of the game
        float colResetValue = world.columns[world.getTail()].transform.position.x + 31;
        float newPosition = 0;
        float drillStartX = 17f;
        int tail = world.getTail();

        //Shift columns back to start
        for(int i = 0; i < world.columns.Length; i++)
        {
            newPosition = world.columns[i].transform.position.x - colResetValue;
            world.columns[i].transform.position = new Vector3(newPosition, 0, 0);
            if(i == tail)
            {
                Debug.Log("Set Tail ("+ i +") to: " + newPosition);
            }
        }

        //Shift Drill back to start
        float playerOffsetFromDrill = drill.transform.position.x - player.transform.position.x;
        drill.transform.position = new Vector3(drillStartX, 0, 0);
        player.transform.position = new Vector3(drillStartX - playerOffsetFromDrill, player.transform.position.y, 0);

        //shift active Items
        shiftActiveItems(itemPool);

        //Dont even bother shifting the lasers

        //Shift enemies
    }

    private void shiftActiveItems(Item[] pool)
    {
        for(int i = 0; i < pool.Length; i++)
        {
            if (pool[i].getActiveInPlayspace())
            {
                pool[i].transform.position = pool[i].getOccupiedTile().transform.position;
            }
        }
    }

    public int getWorldgenType() { return worldgenType; }

    public int getCurrentBoss() { return currentBoss; }

    #region Cheats
    public void cheats_levelup()
    {
        gold = nextLevelup;
        updateTreasurebar();
    }
    public void cheats_fuelDrill()
    {
        int amount = 100 - fuel;
        changeFuel(amount);
    }
    public void cheats_overdirveDrill()
    {
        int amount = 200 - fuel;
        changeFuel(amount);
    }
    public void cheats_healDrill()
    {
        int amount = HP_drill_max - HP_drill;
        changeDrillHealth(amount);
    }
    public void cheats_healPlayer()
    {
        int amount = HP_player_max - HP_player;
        changePlayerHealth(amount, ConstantLibrary.DMGSRC_HEAL);
    }
    #endregion
}
