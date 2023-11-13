using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public GameObject upgradeMenuCanvas;

    private int pendingLevelups = 0;

    [Header("Upgrade 1")]
    public Image icon_upgrade1;
    public TMP_Text description_upgrade1;

    [Header("Upgrade 2")]
    public Image icon_upgrade2;
    public TMP_Text description_upgrade2;

    [Header("Upgrade 3")]
    public Image icon_upgrade3;
    public TMP_Text description_upgrade3;

    [Header("Scrap")]
    public TMP_Text scrapValue;
    private int nextScrapValue = 400;

    [Header("Upgrade Icons")]
    public Sprite[] upgradeIcons;

    [Header("Upgrade Sound")]
    public AudioSource upgradeSound;

    //upgrade descriptions
    private string[] upgradeDesciptions;

    //Upgrade Options are linked to the Upgrade Codes
    private int upgradeOption1 = 0;
    private int upgradeOption2 = 0;
    private int upgradeOption3 = 0;

    private bool nextRandomizationReady = true; //used to prevent the menu from randomizing while open

    #region Upgrade Tracking

    private int miningUpgrades = 0;
    private int miningmax = 7;

    private int movespeedUpgrades = 0;
    private int movespeedMax = 5;

    private int repairUpgrades = 0;

    private float fireRateReduction = 0;
    private int fireRateUpgrades = 0;
    private int fireRateMax = 4;

    private int fuelUpgrades = 0;
    private int fuelMax = 4;

    private int inventoryUpgrades = 0;
    private int inventoryMax = 3;

    private int drillHealthUpgrades = 0;

    private int playerHealthUpgrades = 0;

    private int medkitCooldownUpgrades = 0;
    private int medkitCooldownMax = 4;

    private int reloadSpeedUpgrades = 0;
    private int reloadSpeedMax = 3;

    private int magSizeUpgrades = 0;
    private int magSizeMax = 4;

    private int dodgeCooldownUpgrades = 0;
    private int dodgeCooldownMax = 3;

    #endregion

    #region Upgrade Codes
    private const int SCRAP = -1;
    private const int MINE = 0;         
    private const int MOVESPEED = 1;    
    private const int FIRERATE = 2;     
    private const int FUELEFF = 3;      
    private const int INVENTORY = 4;    
    private const int DRILLHP = 5;      //un-capped
    private const int PLAYERHP = 6;     //un-capped
    private const int REPAIR = 7;       //un-capped
    private const int MEDKIT = 8;       
    private const int MAGSIZE = 9;      
    private const int RELOADSPEED = 10;
    private const int DODGECOOLDOWN = 11;
    private const int POWERATTACK = 12;
    #endregion

    private List<int> availableUpgrades;

    private Player player;
    private GameManager gameManager;
    private Inventory inventory;
    private World world;
    private PauseMenu pauseMenu;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        inventory = GetComponent<Inventory>();
        pauseMenu = GetComponent<PauseMenu>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Player>();
        upgradeDesciptions = new string[upgradeIcons.Length];
        populateDescriptions();
        initializeAvailableUpgrades();
    }

    //Dev Testing
    public void giveMiningAndShooting()
    {
        miningUpgrades = miningmax;
        for(int i = 0; i < miningmax; i++)
        {
            availableUpgrades.Remove(MINE);
        }
        world.recalculateTileHealth();

        fireRateUpgrades = fireRateMax;
        for(int i = 0; i < fireRateMax; i++)
        {
            availableUpgrades.Remove(FIRERATE);
        }
        fireRateReduction = 0.9f;

    }

    //TODO: USE THIS LATER FOR A MORE EFFICIENT UPGRADE MANAGER
    private void initializeAvailableUpgrades()
    {
        availableUpgrades = new List<int>();
        for(int i = 0; i < miningmax; i++)
        {
            availableUpgrades.Add(MINE);
        }
        for(int i = 0; i < movespeedMax; i++)
        {
            availableUpgrades.Add(MOVESPEED);
        }
        for(int i = 0; i < fireRateMax; i++)
        {
            availableUpgrades.Add(FIRERATE);
        }
        for(int i = 0; i < fuelMax; i++)
        {
            availableUpgrades.Add(FUELEFF);
        }
        for(int i = 0; i < inventoryMax; i++)
        {
            availableUpgrades.Add(INVENTORY);
        }
        for(int i = 0; i < medkitCooldownMax; i++)
        {
            availableUpgrades.Add(MEDKIT);
        }
        for(int i = 0; i < reloadSpeedMax; i++)
        {
            availableUpgrades.Add(RELOADSPEED);
        }
        for(int i = 0; i < magSizeMax; i++)
        {
            availableUpgrades.Add(MAGSIZE);
        }
        for(int i = 0; i < dodgeCooldownMax; i++)
        {
            availableUpgrades.Add(DODGECOOLDOWN);
        }

        for(int i = 0; i < 1; i++)
        {
            availableUpgrades.Add(DRILLHP);
            availableUpgrades.Add(PLAYERHP);
            availableUpgrades.Add(REPAIR);
        }
    }

    public void setPendingLevelups(int levelups)
    {
        pendingLevelups += levelups;
    }

    public void levelUp()
    {
        Time.timeScale = 0;
        upgradeMenuCanvas.SetActive(true);
        if (nextRandomizationReady)
        {
            randomizeUpgradesRevised();
            nextRandomizationReady = false;
        }
    }

    private void randomizeUpgradesRevised()
    {
        scrapValue.text = nextScrapValue.ToString();
        int upgrade1;
        int upgrade2;
        int upgrade3;

        //Set First Upgrade Option
        upgrade1 = availableUpgrades[Random.Range(0, availableUpgrades.Count)];

        //Set Second Upgrade Option
        upgrade2 = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
        while(upgrade2 == upgrade1)
        {
            upgrade2 = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
        }

        //Set Third Upgrade Option
        upgrade3 = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
        while (upgrade3 == upgrade2 || upgrade3 == upgrade1)
        {
            upgrade3 = availableUpgrades[Random.Range(0, availableUpgrades.Count)];
        }

        //update menu
        icon_upgrade1.sprite = upgradeIcons[upgrade1];
        description_upgrade1.text = upgradeDesciptions[upgrade1];
        upgradeOption1 = upgrade1;

        icon_upgrade2.sprite = upgradeIcons[upgrade2];
        description_upgrade2.text = upgradeDesciptions[upgrade2];
        upgradeOption2 = upgrade2;

        icon_upgrade3.sprite = upgradeIcons[upgrade3];
        description_upgrade3.text = upgradeDesciptions[upgrade3];
        upgradeOption3 = upgrade3;
    }

    public void upgradeSelected(int option)
    {
        upgradeMenuCanvas.SetActive(false);
        upgradeSound.volume = 0.6f * GetComponent<AudioManager>().getMixedSfx();
        upgradeSound.Play();
        if (!pauseMenu.getPaused())
        {
            Time.timeScale = 1f;
        }

        //Get Upgrade Code
        int upgradeCode = 0;
        switch (option)
        {
            case 1:
                upgradeCode = upgradeOption1;
                break;

            case 2:
                upgradeCode = upgradeOption2;
                break;

            case 3:
                upgradeCode = upgradeOption3;
                break;

            case 4:
                upgradeCode = SCRAP;
                break;
        }

        performUpgrade(upgradeCode);

        pendingLevelups--;
        nextRandomizationReady = true;
        if (pendingLevelups > 0)
        {
            levelUp();
        }
    }

    public void performUpgrade(int upgradeCode)
    {
        switch (upgradeCode)
        {
            case SCRAP:
                gameManager.incrementScore_scrap(nextScrapValue);
                nextScrapValue = nextScrapValue + 50;
                break;

            case MINE:
                miningUpgrades += 1;
                world.recalculateTileHealth();
                availableUpgrades.Remove(MINE);
                break;

            case MOVESPEED:
                movespeedUpgrades += 1;
                player.changeMoveSpeed(1f);
                availableUpgrades.Remove(MOVESPEED);
                break;

            case FIRERATE:
                fireRateUpgrades++;
                switch (fireRateUpgrades)
                {
                    case 1:
                        fireRateReduction = 0.45f;
                        break;

                    case 2:
                        fireRateReduction = 0.65f;
                        break;

                    case 3:
                        fireRateReduction = 0.8f;
                        break;

                    case 4:
                        fireRateReduction = 0.9f;
                        break;
                }
                availableUpgrades.Remove(FIRERATE);
                break;

            case FUELEFF:
                fuelUpgrades += 1;
                availableUpgrades.Remove(FUELEFF);
                break;

            case INVENTORY:
                inventory.expandInventory();
                inventoryUpgrades++;
                availableUpgrades.Remove(INVENTORY);
                break;

            case DRILLHP:
                gameManager.incrementMaxDrillHealth(25);
                drillHealthUpgrades++;
                break;

            case PLAYERHP:
                gameManager.incrementMaxPlayerHealth(25);
                playerHealthUpgrades++;
                break;

            case REPAIR:
                repairUpgrades++;
                break;

            case MEDKIT:
                medkitCooldownUpgrades++;
                availableUpgrades.Remove(MEDKIT);
                break;

            case RELOADSPEED:
                reloadSpeedUpgrades++;
                availableUpgrades.Remove(RELOADSPEED);
                break;

            case MAGSIZE:
                magSizeUpgrades++;
                player.changeMagSize(2);
                availableUpgrades.Remove(MAGSIZE);
                break;

            case DODGECOOLDOWN:
                dodgeCooldownUpgrades++;
                availableUpgrades.Remove(DODGECOOLDOWN);
                break;

            default:
                Debug.Log("Unknown Upgrade Code: " + upgradeCode);
                break;
        }
    }

    public void maximizeUpgrades()
    {
        //Pickaxe Upgrades
        for(int i = miningUpgrades; i < miningmax; i++)
        {
            availableUpgrades.Remove(MINE);
        }
        miningUpgrades = miningmax;
        world.recalculateTileHealth();

        //Movespeed Upgrades
        for(int i = movespeedUpgrades; i < movespeedMax; i++)
        {
            availableUpgrades.Remove(MOVESPEED);
            player.changeMoveSpeed(1f);
        }
        movespeedUpgrades = movespeedMax;

        //Firerate Upgrades
        for(int i = fireRateUpgrades; i < fireRateMax; i++)
        {
            availableUpgrades.Remove(FIRERATE);
        }
        fireRateUpgrades = fireRateMax;
        fireRateReduction = 0.9f;

        //Fuel Efficiency
        for(int i = fuelUpgrades; i < fuelMax; i++)
        {
            availableUpgrades.Remove(FUELEFF);
        }
        fuelUpgrades = fuelMax;

        //Invetory
        for(int i = inventoryUpgrades; i < inventoryMax; i++)
        {
            inventory.expandInventory();
            availableUpgrades.Remove(INVENTORY);
        }
        inventoryUpgrades = inventoryMax;

        //Medkit
        for(int i = medkitCooldownUpgrades; i < medkitCooldownMax; i++)
        {
            availableUpgrades.Remove(MEDKIT);
        }
        medkitCooldownUpgrades = medkitCooldownMax;

        //Reload Speed
        for(int i = reloadSpeedUpgrades; i < reloadSpeedMax; i++)
        {
            availableUpgrades.Remove(RELOADSPEED);
        }
        reloadSpeedUpgrades = reloadSpeedMax;

        //Magsize
        for(int i = magSizeUpgrades; i < magSizeMax; i++)
        {
            availableUpgrades.Remove(MAGSIZE);
            player.changeMagSize(2);
        }
        magSizeUpgrades = magSizeMax;

        //Dodge Cooldown
        for(int i = dodgeCooldownUpgrades; i < dodgeCooldownMax; i++)
        {
            availableUpgrades.Remove(DODGECOOLDOWN);
        }
        dodgeCooldownUpgrades = dodgeCooldownMax;
    }

    #region Getters
    //Fire Rate
    public float getFireRateReduction() { return fireRateReduction; }
    public int getFireRateUpgrades() { return fireRateUpgrades; }
    public int getFireRateMax() { return fireRateMax; }

    //Move Speed
    public float getMoveSpeedUpgrades() { return movespeedUpgrades; }
    public int getMoveSpeedMax() { return movespeedMax; }

    //Mining Upgrades
    public int getMineUpgrades() { return miningUpgrades; }
    public int getMineMax() { return miningmax; }

    //Fuel Efficiency
    public int getFuelUpgrades() { return fuelUpgrades; }
    public int getFuelMax() { return fuelMax; }

    //Drill Repair
    public int getRepairUpgrades() { return repairUpgrades; }

    //Medkit Cooldown
    public int getMedkitUpgrades() { return medkitCooldownUpgrades; }
    public int getMedkitMax() { return medkitCooldownMax; }

    //Reload Speed
    public int getReloadSpeed() { return reloadSpeedUpgrades; }
    public int getReloadMax() { return reloadSpeedMax; }

    //Mag Size
    public int getMagSize() { return magSizeUpgrades; }
    public int getMagMax() { return magSizeMax; }

    //Dodge Cooldown
    public int getDodgeCooldownUpgrades() { return dodgeCooldownUpgrades; }
    public int getDodgeCooldownMax() { return dodgeCooldownMax; }

    //Invetory
    public int getInventoryUpgrades() { return inventoryUpgrades; }
    public int getInventoryMax() { return inventoryMax; }
    #endregion

    private void populateDescriptions()
    {

        upgradeDesciptions[MINE] = "Breaking Blocks takes 1 less hit";
        upgradeDesciptions[MOVESPEED] = "Increase MoveSpeed";
        upgradeDesciptions[INVENTORY] = "Increase Inventory Slots by 1";
        upgradeDesciptions[FIRERATE] = "Increase Raygun Fire Rate";
        upgradeDesciptions[FUELEFF] = "Increase Fuel Efficiency";
        upgradeDesciptions[DRILLHP] = "Increase Drill Armor";
        upgradeDesciptions[PLAYERHP] = "Increase Health";
        upgradeDesciptions[MEDKIT] = "Reduce Health Station Cooldown";
        upgradeDesciptions[REPAIR] = "Repair the Drill faster";
        upgradeDesciptions[RELOADSPEED] = "Reduce Reload time by 0.35 sec";
        upgradeDesciptions[MAGSIZE] = "Increase Raygun Battery by 2 shots";
        upgradeDesciptions[DODGECOOLDOWN] = "Reduce Dodge Cooldown by 1 sec";


        //Ensure no Null Descriptions
        for (int i = 0; i < upgradeDesciptions.Length; i++)
        {
            if(upgradeDesciptions[i] == null)
            {
                upgradeDesciptions[i] = "";
            }
        }
    }

    public int getPendingLevelUps()
    {
        return pendingLevelups;
    }
}
