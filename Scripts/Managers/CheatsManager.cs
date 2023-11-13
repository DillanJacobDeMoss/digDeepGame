using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CheatsManager : MonoBehaviour
{
    [Header("Activate Cheats")]
    public bool cheatsEnabled;

    [Header("Cheats Canvas")]
    public GameObject cheatsCanvas;

    #region Upgrade Vars
    [Header("Upgrades")]
    public Button up_fuelEff_button;
    public TMP_Text up_fuelEff_text;

    public Button up_pickaxe_button;
    public TMP_Text up_pickaxe_text;

    public Button up_moveSpd_button;
    public TMP_Text up_moveSpd_text;

    public Button up_medkit_button;
    public TMP_Text up_medkit_text;

    public Button up_reload_button;
    public TMP_Text up_reload_text;

    public Button up_magSize_button;
    public TMP_Text up_magSize_text;

    public Button up_fireRate_button;
    public TMP_Text up_fireRate_text;

    public Button up_inventory_button;
    public TMP_Text up_inventory_text;

    public Button up_dodge_button;
    public TMP_Text up_dodge_text;

    public Button up_health_button;
    public TMP_Text up_health_text;

    public Button up_drillHP_button;
    public TMP_Text up_drillHP_text;

    public Button up_repair_button;
    public TMP_Text up_repair_text;
    #endregion

    [Header("Biome Jumps")]
    public GameObject biomeJumpsMenu;
    public GameObject biomeJumpsMessage;
    private bool biomeJumpCoolingDown;
    private float biomeJumpCooldownTimer;

    [Header("Toggles")]
    public Sprite box_unchecked;
    public Sprite box_checked;
    public Image checkboxGod_player;
    private bool godmodePlayer = false;
    public Image checkboxGod_drill;
    private bool godmodeDrill = false;
    public Image checkboxSpawn_enemies;
    private bool spawnEnemies = true;
    public Image checkboxConsume_fuel;
    private bool consumeFuel = true;
    public Image checkboxStop_drill;
    private bool stopDrill = false;

    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private World world;
    private Drill drill;

    // Start is called before the first frame update
    void Start()
    {
        cheatsCanvas.SetActive(false);
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        upgradeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<UpgradeManager>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        drill = GameObject.FindGameObjectWithTag("Drill").GetComponent<Drill>();
    }

    // Update is called once per frame
    void Update()
    {

        //Toggle Cheats Menu
        if (cheatsEnabled && Input.GetKeyDown(KeyCode.K))
        {
            if (cheatsCanvas.activeSelf) { closeCheatsMenu(); }
            else { openCheatsMenu(); }
        }
        if (cheatsEnabled && Input.GetKeyDown(KeyCode.Escape) && cheatsCanvas.activeSelf) { closeCheatsMenu(); }

        //Biome Jump Cooldown
        if (biomeJumpCoolingDown)
        {
            biomeJumpCooldownTimer -= Time.deltaTime;
            if(biomeJumpCooldownTimer <= 0f)
            {
                biomeJumpCoolingDown = false;
                revealBiomeJumps();
            }
        }

    }

    public void updateUpgradesDisplay()
    {

        //Fuel Efficiency
        if(upgradeManager.getFuelUpgrades() >= upgradeManager.getFuelMax())
        {
            up_fuelEff_text.text = "Max";
            up_fuelEff_text.color = Color.red;
            up_fuelEff_button.interactable = false;
        }
        else
        {
            up_fuelEff_text.text = upgradeManager.getFuelUpgrades().ToString();
        }


        //Pickaxe Upgrades
        if(upgradeManager.getMineUpgrades() >= upgradeManager.getMineMax())
        {
            up_pickaxe_text.text = "Max";
            up_pickaxe_text.color = Color.red;
            up_pickaxe_button.interactable = false;
        }
        else
        {
            up_pickaxe_text.text = upgradeManager.getMineUpgrades().ToString();
        }


        //Move Speed Upgrades
        if(upgradeManager.getMoveSpeedUpgrades() >= upgradeManager.getMoveSpeedMax())
        {
            up_moveSpd_text.text = "Max";
            up_moveSpd_text.color = Color.red;
            up_moveSpd_button.interactable = false;
        }
        else
        {
            up_moveSpd_text.text = upgradeManager.getMoveSpeedUpgrades().ToString();
        }


        //Medkit Upgrades
        if(upgradeManager.getMedkitUpgrades() >= upgradeManager.getMedkitMax())
        {
            up_medkit_text.text = "Max";
            up_medkit_text.color = Color.red;
            up_medkit_button.interactable = false;
        }
        else
        {
            up_medkit_text.text = upgradeManager.getMedkitUpgrades().ToString();
        }


        //Reload Speed Upgrades
        if(upgradeManager.getReloadSpeed() >= upgradeManager.getReloadMax())
        {
            up_reload_text.text = "Max";
            up_reload_text.color = Color.red;
            up_reload_button.interactable = false;
        }
        else
        {
            up_reload_text.text = upgradeManager.getReloadSpeed().ToString();
        }


        //Mag Size Upgrades
        if (upgradeManager.getMagSize() >= upgradeManager.getMagMax())
        {
            up_magSize_text.text = "Max";
            up_magSize_text.color = Color.red;
            up_magSize_button.interactable = false;
        }
        else
        {
            up_magSize_text.text = upgradeManager.getMagSize().ToString();
        }


        //Fire Rate Upgrades
        if(upgradeManager.getFireRateUpgrades() >= upgradeManager.getFireRateMax())
        {
            up_fireRate_text.text = "Max";
            up_fireRate_text.color = Color.red;
            up_fireRate_button.interactable = false;
        }
        else
        {
            up_fireRate_text.text = upgradeManager.getFireRateUpgrades().ToString();
        }


        //Inventory Upgrades
        if (upgradeManager.getInventoryUpgrades() >= upgradeManager.getInventoryMax())
        {
            up_inventory_text.text = "Max";
            up_inventory_text.color = Color.red;
            up_inventory_button.interactable = false;
        }
        else
        {
            up_inventory_text.text = upgradeManager.getInventoryUpgrades().ToString();
        }

        //Dodge Cooldown
        if(upgradeManager.getDodgeCooldownUpgrades() >= upgradeManager.getDodgeCooldownMax())
        {
            up_dodge_text.text = "Max";
            up_dodge_text.color = Color.red;
            up_dodge_button.interactable = false;
        }
        else
        {
            up_dodge_text.text = upgradeManager.getDodgeCooldownUpgrades().ToString();
        }

        //Health
        up_health_text.text = gameManager.getPlayerHealthMax().ToString();

        //Drill HP
        up_drillHP_text.text = gameManager.getDrillHealthMax().ToString();

        //Repair
        up_repair_text.text = (2 + (upgradeManager.getRepairUpgrades() * 2)).ToString();
    }

    public void updateTogglesDisplay()
    {
        checkboxGod_player.sprite = godmodePlayer ? box_checked : box_unchecked;
        checkboxGod_drill.sprite = godmodeDrill ? box_checked : box_unchecked;
        checkboxSpawn_enemies.sprite = spawnEnemies ? box_checked : box_unchecked;
        checkboxConsume_fuel.sprite = consumeFuel ? box_checked : box_unchecked;
        checkboxStop_drill.sprite = stopDrill ? box_checked : box_unchecked;
    }

    #region Biome Jumps
    public void hideBiomeJumps()
    {
        biomeJumpsMenu.SetActive(false);
        biomeJumpsMessage.SetActive(true);
        biomeJumpCoolingDown = true;
        biomeJumpCooldownTimer = 5f;
    }

    private void revealBiomeJumps()
    {
        biomeJumpsMenu.SetActive(true);
        biomeJumpsMessage.SetActive(false);
    }

    public void doBiomeJump(int biomeCode)
    {
        gameManager.setDifficulty(biomeCode * 5);
        gameManager.updateBiome();
        gameManager.jumpToNextMilestone();
        world.reloadChunks();
    }
    #endregion

    private void openCheatsMenu()
    {
        Time.timeScale = 0;
        cheatsCanvas.SetActive(true);
        updateUpgradesDisplay();
        updateTogglesDisplay();
    }

    private void closeCheatsMenu()
    {
        cheatsCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    #region Toggles
    public void toggleGodmodePlayer() { godmodePlayer = !godmodePlayer; }
    public void toggleGodmodeDrill() { godmodeDrill = !godmodeDrill; }
    public void toggleSpawnEnemies() { spawnEnemies = !spawnEnemies; }
    public void toggleConsumeFuel() { consumeFuel = !consumeFuel; }
    public void toggleStopDrill()
    {
        stopDrill = !stopDrill;
        drill.updateSpeed();
    }
    #endregion

    #region Getters
    public bool getGodmodePlayer() { return godmodePlayer; }
    public bool getGodmodeDrill() { return godmodeDrill; }
    public bool getSpawnEnemies() { return spawnEnemies; }
    public bool getConsumeFuel() { return consumeFuel; }
    public bool getStopDrill() { return stopDrill; }
    #endregion
}
