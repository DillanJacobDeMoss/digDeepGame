using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Tile : MonoBehaviour
{
    public bool onDrillpath; //True when the tile lies in rows 10-14 (inclusive)

    [Header ("Assign 'Collision'")]
    public BoxCollider2D tileCollisionStandard;
    public BoxCollider2D tileCollisionRhombus;

    [Header("Assign From 'Sprite'")]
    public SpriteRenderer s_Base;       //Sprite layer for the base color of tiles
    public SpriteRenderer s_Mod;        //Spirte layer for tile mods like ores and treads
    public SpriteRenderer s_Shade;      //Sprite layer for tile shading
    public Animator anim;               //Keyframe Animation for tile shaking when hit
    public Animator a_Base;             //Base Layer frame animation for things like water
    public Animator a_Edge;             //Edge Layer frame animation for things like water
    private Animator a_Master;          //Master animator to snyc up frame animations
    public Transform spriteTransform;
    private TileSprites spriteLoader;
    private Vector3 spriteSize;

    [Header("Lights")]
    public Light2D light_top;
    public Light2D light_top2;
    public ShadowCaster2D tileShadows;
    private bool facingFlashlight;
    private bool isShadowCaster;

    [Header("Sprite Vars")]
    public int tileType;        //Determines what kind of tile this is
    private bool addTreadMarks;
    private int baseLayer;      //Layer ints used to assign sprite sorting layer

    [Header("Sprite Containers")]
    private SpriteContainer shadeLayerSprites;
    private SpriteContainer tileBaseSprites;
    private SpriteContainer tileModSprites;
    private SpriteContainer holeEdgeSprites;
    private SpriteContainer holeShaderSprites;
    private SpriteContainer railSprites;
    private SpriteContainer vineSprites;
    private SpriteContainer mineshaftSprites;
    private SpriteContainer mineshaftSprites2;

    //Neighboring Tiles for BFS pathfinding and shade layer rendering
    private Tile tileUp;
    private Tile tileDown;
    private Tile tileLeft;
    private Tile tileRight;

    // Used to save random decsions made like tile variants at the moment of tile
    // creation so that they do not get overwritten when a tile updates
    // -1 indicated NO seed
    private int seed1 = -1;
    private int seed2 = -1;

    #region Base Layer Constants
    private const int BIOME0_WALL_START = 0;
    private const int BIOME0_GROUND_START = 6;
    private const int BIOME1_WALL_START = 12;
    private const int BIOME1_GROUND_START = 18;
    private const int BIOME2_WALL_START = 24;
    private const int BIOME2_GROUND_START = 30;
    private const int BIOME3_WALL_START = 33;
    private const int BIOME3_GROUND_START = 39;
    private const int GEODE_START = 45;
    #endregion

    #region Mod Layer Constants
    private const int MOD_EMPTY = 0;
    private const int MOD_TREAD = 1;
    private const int MOD_FUEL = 2;
    private const int MOD_GOLD = 3;
    private const int MOD_AMATHYST = 4;
    private const int MOD_DIAMOND = 5;
    private const int MOD_BRIDGE = 6;
    private const int MOD_RAIL_START = 7; //RAILS occupy slots 7-9
    private const int MOD_MINECART = 10;
    private const int MOD_WATER_START = 11; //WAter occupies slots 11-13
    private const int MOD_ICE_START = 14; //Ice occupies slots 14-16
    private const int MOD_LAVA_START = 17; //Lava occupies slots 17-19
    #endregion

    private int mineHP; //HP of mineable tiles
    private bool constantMineHP; //Determines if the tile is affected by pickaxe upgrades

    private float vineGrowTimer = 0f; //keeps track of time until a vine regrows

    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private World world;

    private bool containsItem = false;

    private Transform drillPlatform;

    #region Pathfinding
    private bool visited = false;
    private Tile previousTile = null;
    #endregion

    void Start()
    {
        spriteLoader = GameObject.FindGameObjectWithTag("SpriteContainer").GetComponent<TileSprites>();
        shadeLayerSprites = GameObject.FindGameObjectWithTag("SpriteContainer_Shading").GetComponent<SpriteContainer>();
        tileModSprites = GameObject.FindGameObjectWithTag("SpriteContainer_TileMods").GetComponent<SpriteContainer>();
        tileBaseSprites = GameObject.FindGameObjectWithTag("SpriteContainer_Tiles").GetComponent<SpriteContainer>();
        holeEdgeSprites = GameObject.FindGameObjectWithTag("SpriteContainer_HoleEdgeShading").GetComponent<SpriteContainer>();
        holeShaderSprites = GameObject.FindGameObjectWithTag("SpriteContainer_HoleShading").GetComponent<SpriteContainer>();
        railSprites = GameObject.FindGameObjectWithTag("SpriteContainer_Rails").GetComponent<SpriteContainer>();
        mineshaftSprites = GameObject.FindGameObjectWithTag("SpriteContainer_Mineshafts").GetComponent<SpriteContainer>();
        mineshaftSprites2 = GameObject.FindGameObjectWithTag("SpriteContainer_Mineshafts2").GetComponent<SpriteContainer>();
        vineSprites = GameObject.FindGameObjectWithTag("SpriteContainer_Vines").GetComponent<SpriteContainer>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        upgradeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<UpgradeManager>();
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
        drillPlatform = GameObject.FindGameObjectWithTag("drillPlatform").GetComponent<Transform>();

        randomizeSpriteSize();
        changeTile(tileType);

        a_Base.gameObject.SetActive(false);
        a_Edge.gameObject.SetActive(false);
        a_Master = GameObject.FindGameObjectWithTag("Tile_MasterAnim").GetComponent<Animator>();
    }

    private void Update()
    {
        if(tileType == ConstantLibrary.T_VINE_GROWING)
        {
            if(!onDrillpath || transform.position.x < (drillPlatform.position.x - 5.5f))
            {
                a_Base.SetBool("vineGrowing", true);
                vineGrowTimer -= Time.deltaTime;
                if(vineGrowTimer <= 0)
                {
                    changeTile(ConstantLibrary.T_VINE);
                    a_Base.SetBool("vineGrowing", false);
                    updateTileShadeLayer();
                    updateNeighbors();
                    vineGrowTimer = ConstantLibrary.vineRegrowthTime;
                }
            }
        }
        
        //cast shadows
        tileShadows.enabled = isShadowCaster && facingFlashlight;
    }

    public void setTreadMarks(bool set)
    {
        addTreadMarks = set;
    }
    
    public void changeTile(int newType)
    {
        resetTileValues();
        tileType = newType;

        switch (tileType)
        {
            case ConstantLibrary.T_GROUND:
                setCollision(ConstantLibrary.T_GROUND);
                determineGroundSprite();
                if (addTreadMarks) { s_Mod.sprite = tileModSprites.getSprite(MOD_TREAD); }
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Ground"));
                break;

            case ConstantLibrary.T_WALL:
                setCollision(ConstantLibrary.T_WALL);
                determineWallSprite();
                updateSortingOrder(baseLayer);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                //Seed1 used for mineshaft wall beam variants (50% to have beam)
                seed1 = Random.Range(0, 2);
                break;

            case ConstantLibrary.T_ORE_GOLD:
                setCollision(ConstantLibrary.T_WALL);
                determineWallSprite();
                determineLightInfo();
                calculateMineHP(1);
                s_Mod.sprite = tileModSprites.getSprite(MOD_GOLD);
                updateSortingOrder(baseLayer + 350);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;

            case ConstantLibrary.T_ORE_FUEL:
                setCollision(ConstantLibrary.T_WALL);
                determineWallSprite();
                determineLightInfo();
                calculateMineHP(1);
                s_Mod.sprite = tileModSprites.getSprite(MOD_FUEL);
                updateSortingOrder(baseLayer + 360);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;

            case ConstantLibrary.T_GEODE:
                setCollision(ConstantLibrary.T_WALL);
                calculateMineHP(8);
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(GEODE_START, GEODE_START + 3));
                updateSortingOrder(baseLayer);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;

            case ConstantLibrary.T_GEODE_ORE:
                setCollision(ConstantLibrary.T_WALL);
                determineGeodeOre();
                determineLightInfo();
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;

            case ConstantLibrary.T_ORE_DIAMOND:
                setCollision(ConstantLibrary.T_WALL);
                determineWallSprite();
                determineLightInfo();
                calculateMineHP(2);
                s_Mod.sprite = tileModSprites.getSprite(MOD_DIAMOND);
                updateSortingOrder(baseLayer + 370);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;

            case ConstantLibrary.T_HOLE:
                setCollision(ConstantLibrary.T_HOLE);
                determineGroundSprite();
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Holes"));
                //seed1 used to determine if hole tile should have a stalagmite (10% chance)
                seed1 = Random.Range(0, 10);
                break;

            case ConstantLibrary.T_BRIDGE:
                setCollision(ConstantLibrary.T_GROUND);
                determineGroundSprite();
                s_Mod.sprite = tileModSprites.getSprite(MOD_BRIDGE);
                updateSortingOrder(baseLayer - 1000);
                //The black backdrop is in the hole layer but the bridge should be ground
                updateSortingLayer(SortingLayer.NameToID("Holes"));
                s_Mod.sortingLayerID = SortingLayer.NameToID("Ground");
                break;

            case ConstantLibrary.T_RAIL:
                setCollision(ConstantLibrary.T_GROUND);
                determineGroundSprite();
                determineRailSprite();
                updateSortingOrder(baseLayer);
                updateSortingLayer(SortingLayer.NameToID("Ground"));
                //seed1 used to determine rail variant (base index +0, +1, or +2)
                seed1 = Random.Range(0, 3);
                //seed2 used to determine mineshaft beam chance [assuming appropriate conditions] (50%)
                seed2 = Random.Range(0, 2);
                break;

            case ConstantLibrary.T_MINECART:
                setCollision(ConstantLibrary.T_WALL);
                calculateMineHP(1);
                determineLightInfo();
                s_Mod.sprite = tileModSprites.getSprite(MOD_MINECART);
                updateSortingOrder(baseLayer + 380);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;

            case ConstantLibrary.T_WATER:
                setCollision(ConstantLibrary.T_WATER);
                playAnimations();
                s_Mod.sprite = tileModSprites.getSprite(Random.Range(MOD_WATER_START, MOD_WATER_START + 3));
                determineGroundSprite();
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Liquids"));
                break;

            case ConstantLibrary.T_ICE:
                setCollision(ConstantLibrary.T_ICE);
                determineGroundSprite();
                s_Base.sprite = tileModSprites.getSprite(Random.Range(MOD_ICE_START, MOD_ICE_START + 3));
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Ground"));
                break;

            case ConstantLibrary.T_LAVA:
                setCollision(ConstantLibrary.T_LAVA);
                playAnimations();
                determineLightInfo();
                s_Mod.sprite = tileModSprites.getSprite(Random.Range(MOD_LAVA_START, MOD_LAVA_START + 3));
                determineGroundSprite();
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Liquids"));
                break;

            case ConstantLibrary.T_VINE:
                setCollision(ConstantLibrary.T_WALL);
                determineGroundSprite();
                s_Mod.sprite = vineSprites.getSprite(getNeighborCode(ConstantLibrary.typeCheck_vines, false));
                constantMineHP = true;
                mineHP = 3;
                tileCollisionStandard.enabled = false;
                tileCollisionRhombus.enabled = true;
                updateSortingOrder(baseLayer);
                updateSortingLayer(SortingLayer.NameToID("Ground"));
                break;

            case ConstantLibrary.T_VINE_GROWING:
                setCollision(ConstantLibrary.T_GROUND);
                updateSortingOrder(baseLayer - 1000);
                vineGrowTimer = ConstantLibrary.vineRegrowthTime;
                a_Base.gameObject.SetActive(true);
                a_Base.SetInteger("tileType", tileType);
                updateSortingLayer(SortingLayer.NameToID("Ground"));
                break;

            case ConstantLibrary.T_LAVA_FLOW:
                setCollision(ConstantLibrary.T_LAVA);
                determineGroundSprite();
                determineLightInfo();
                a_Base.gameObject.SetActive(true);
                a_Base.SetInteger("tileType", tileType);
                a_Base.Play(0, -1, a_Master.GetCurrentAnimatorStateInfo(0).normalizedTime);
                tileCollisionStandard.size = new Vector2(0.6f, 0.6f);
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Liquids"));
                break;

            case ConstantLibrary.T_LAVA_FLOW_BRIDGE:
                setCollision(ConstantLibrary.T_GROUND);
                determineGroundSprite();
                determineLightInfo();
                a_Base.gameObject.SetActive(true);
                a_Base.SetInteger("tileType", tileType);
                a_Base.Play(0, -1, a_Master.GetCurrentAnimatorStateInfo(0).normalizedTime);
                updateSortingOrder(baseLayer - 1000);
                updateSortingLayer(SortingLayer.NameToID("Liquids"));
                break;

            case ConstantLibrary.T_EXPLOSIVE:
                setCollision(ConstantLibrary.T_WALL);
                determineWallSprite();
                determineLightInfo();
                calculateMineHP(1);
                a_Base.gameObject.SetActive(true);
                a_Base.SetInteger("tileType", tileType);
                updateSortingOrder(baseLayer);
                updateSortingLayer(SortingLayer.NameToID("Walls"));
                break;
        }
    }

    public void updateTile()
    {
        updateTileShadeLayer();
        updateTileModLayer();
        checkShadowCasting();
    }

    private void updateTileModLayer()
    {
        switch (tileType)
        {
            case ConstantLibrary.T_WALL:
                int shaftCode = 0;
                int[] mineshaftCheck = { ConstantLibrary.T_RAIL };
                shaftCode = getNeighborCode(mineshaftCheck, false);
                if (seed1 == 0)
                {
                    s_Mod.sprite = mineshaftSprites.getSprite(shaftCode);
                }
                else
                {
                    s_Mod.sprite = mineshaftSprites2.getSprite(shaftCode);
                }
                break;

            case ConstantLibrary.T_RAIL:
                //50% chance to spawn a beam
                if (seed2 == 0) { return; }
                int beamCode = 0;
                int[] beamCheck = { ConstantLibrary.T_WALL };
                beamCode = getNeighborCode(beamCheck, false);
                switch (beamCode)
                {
                    //beam for vertical shaft
                    case 5:
                        s_Mod.sprite = railSprites.getSprite(22 + seed1);
                        s_Mod.sortingLayerID = SortingLayer.NameToID("Foreground");
                        break;

                    //bream for horizontal shaft
                    case 10:
                        s_Mod.sprite = railSprites.getSprite(25 + seed1);
                        s_Mod.sortingLayerID = SortingLayer.NameToID("Foreground");
                        break;

                    //not a code that would produce a beam
                    default:
                        s_Mod.sprite = tileModSprites.getSprite(MOD_EMPTY);
                        break;
                }

                break;
        }
    }

    private void updateTileShadeLayer()
    {
        //used to send the tile to the correct shading algorithm

        switch (tileType)
        {
            case ConstantLibrary.T_GROUND:
                updateHoleShading();
                break;

            case ConstantLibrary.T_WALL:
                updateWallShading();
                break;

            case ConstantLibrary.T_ORE_GOLD:
                updateWallShading();
                break;

            case ConstantLibrary.T_ORE_FUEL:
                updateWallShading();
                break;

            case ConstantLibrary.T_GEODE:
                updateWallShading();
                break;

            case ConstantLibrary.T_GEODE_ORE:
                updateWallShading();
                break;

            case ConstantLibrary.T_ORE_AMATHYST:
                updateWallShading();
                break;

            case ConstantLibrary.T_ORE_DIAMOND:
                updateWallShading();
                break;

            case ConstantLibrary.T_HOLE:
                updateHoleShading();
                break;

            case ConstantLibrary.T_BRIDGE:
                updateHoleShading();
                break;

            case ConstantLibrary.T_RAIL:
                determineRailSprite();
                break;

            case ConstantLibrary.T_WATER:
                updateWaterAnims();
                break;

            case ConstantLibrary.T_ICE:
                updateHoleShading();
                break;

            case ConstantLibrary.T_LAVA:
                updateLavaAnims();
                break;

            case ConstantLibrary.T_LAVA_FLOW:
                determineLavaFlowSprite();
                break;

            case ConstantLibrary.T_VINE:
                s_Mod.sprite = vineSprites.getSprite(getNeighborCode(ConstantLibrary.typeCheck_vines, false));
                break;

            case ConstantLibrary.T_VINE_GROWING:
                break;
        }
    }

    private void updateWallShading()
    {
        int edgeCode = 0;
        edgeCode = getNeighborCode(ConstantLibrary.typeCheck_nonWalls, true);
        s_Shade.sprite = shadeLayerSprites.getSprite(edgeCode);
    }

    private void resetTileValues()
    {
        //light
        light_top.enabled = false;
        light_top2.enabled = false;
        tileShadows.enabled = false;

        //sprites
        a_Base.gameObject.SetActive(false);
        a_Edge.gameObject.SetActive(false);
        s_Shade.sprite = tileModSprites.getSprite(MOD_EMPTY);
        s_Mod.sprite = tileModSprites.getSprite(MOD_EMPTY);
        seed1 = -1;
        seed2 = -1;

        //Health
        constantMineHP = false;

        //Colliders
        tileCollisionStandard.enabled = true;
        tileCollisionStandard.size = new Vector2(1f, 1f);
        tileCollisionRhombus.enabled = false;
    }

    private void updateHoleShading()
    {
        int edgeCode = 0;

        //Shade the edges of the ground next to holes
        if(tileType == ConstantLibrary.T_GROUND || tileType == ConstantLibrary.T_ICE)
        {
            int[] holeCheck = { ConstantLibrary.T_HOLE };
            edgeCode = getNeighborCode(holeCheck, false);
            s_Shade.sprite = holeEdgeSprites.getSprite(edgeCode);
        }

        //Shade the outskirts of a hole to create the dropoff effect
        else
        {
            int[] holeTypes = { ConstantLibrary.T_HOLE, ConstantLibrary.T_BRIDGE };
            edgeCode = getNeighborCode(holeTypes, true);
            if(edgeCode > 0)
            {
                s_Shade.sprite = holeShaderSprites.getSprite(edgeCode);
            }
            else
            {
                if(seed1 == 0)
                {
                    switch (world.getCurrentBiome())
                    {
                        case ConstantLibrary.BIO_DEFAULT:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(16,19));
                            break;

                        case ConstantLibrary.BIO_GREEN_TRANS:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(16, 22));
                            break;

                        case ConstantLibrary.BIO_GREEN:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(19, 22));
                            break;

                        case ConstantLibrary.BIO_ICE_TRANS:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(19, 25));
                            break;

                        case ConstantLibrary.BIO_ICE:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(22, 25));
                            break;

                        case ConstantLibrary.BIO_LAVA_TRANS:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(22, 28));
                            break;

                        case ConstantLibrary.BIO_LAVA:
                            s_Shade.sprite = holeShaderSprites.getSprite(Random.Range(25, 28));
                            break;
                    }
                }
                else
                {
                    s_Shade.sprite = holeShaderSprites.getSprite(edgeCode);
                }
            }
        }
    }

    private void updateWaterAnims()
    {
        int wakeEdgeCode = 0;
        int flowEdgeCode = 0;

        //Check for adjacent holes for water to flow to
        int[] flowCheck = { ConstantLibrary.T_HOLE };
        flowEdgeCode = getNeighborCode(flowCheck, false);

        //Check for adjacent water to set the wake
        int[] wakeTypes = { ConstantLibrary.T_HOLE, ConstantLibrary.T_WATER };
        wakeEdgeCode = getNeighborCode(wakeTypes, false);

        a_Base.SetInteger("edgeCode", flowEdgeCode);
        a_Edge.SetInteger("edgeCode", wakeEdgeCode);
    }

    private void updateLavaAnims()
    {
        int wakeEdgeCode = 0;
        int flowEdgeCode = 0;

        //Check for adjacent holes for water to flow to
        int[] flowCheck = { ConstantLibrary.T_HOLE };
        flowEdgeCode = getNeighborCode(flowCheck, false);

        //Check for adjacent Lava to set the wake
        int[] wakeTypes = { ConstantLibrary.T_HOLE, ConstantLibrary.T_LAVA, ConstantLibrary.T_LAVA_FLOW, ConstantLibrary.T_LAVA_FLOW_BRIDGE };
        wakeEdgeCode = getNeighborCode(wakeTypes, false);

        a_Base.SetInteger("edgeCode", flowEdgeCode);
        a_Edge.SetInteger("edgeCode", wakeEdgeCode);
    }

    public bool checkTileForType(Tile tile, int type)
    {
        if(tile != null)
        {
            if(tile.tileType == type)
            {
                return true;
            }
        }
        return false;
    }

    public bool checkTileForTypes(Tile tile, int[] types)
    {
        if(tile != null)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (tile.tileType == types[i])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void updateNeighbors()
    {
        if (tileUp != null)
        {
            tileUp.updateTile();
        }
        if (tileDown != null)
        {
            tileDown.updateTile();
        }
        if (tileLeft != null)
        {
            tileLeft.updateTile();
        }
        if (tileRight != null)
        {
            tileRight.updateTile();
        }
    }

    private void setCollision(int colType)
    {
        switch (colType)
        {
            case ConstantLibrary.T_WALL:
                tileCollisionStandard.gameObject.layer = LayerMask.NameToLayer("wall");
                tileCollisionStandard.gameObject.tag = "tileCollision";
                tileCollisionStandard.enabled = true;
                tileCollisionStandard.isTrigger = false;
                break;

            case ConstantLibrary.T_GROUND:
                tileCollisionStandard.gameObject.layer = LayerMask.NameToLayer("ground");
                tileCollisionStandard.enabled = false;
                tileCollisionStandard.gameObject.tag = "Untagged";
                break;

            case ConstantLibrary.T_HOLE:
                tileCollisionStandard.gameObject.layer = LayerMask.NameToLayer("hole");
                tileCollisionStandard.gameObject.tag = "Untagged";
                tileCollisionStandard.enabled = true;
                tileCollisionStandard.isTrigger = false;
                break;

            case ConstantLibrary.T_WATER:
                tileCollisionStandard.gameObject.layer = LayerMask.NameToLayer("water");
                tileCollisionStandard.gameObject.tag = "Untagged";
                tileCollisionStandard.enabled = true;
                tileCollisionStandard.isTrigger = true;
                break;

            case ConstantLibrary.T_ICE:
                tileCollisionStandard.gameObject.layer = LayerMask.NameToLayer("ice");
                tileCollisionStandard.gameObject.tag = "Untagged";
                tileCollisionStandard.enabled = true;
                tileCollisionStandard.isTrigger = true;
                break;

            case ConstantLibrary.T_LAVA:
                tileCollisionStandard.gameObject.layer = LayerMask.NameToLayer("lava");
                tileCollisionStandard.gameObject.tag = "Untagged";
                tileCollisionStandard.enabled = true;
                tileCollisionStandard.isTrigger = true;
                break;
        }
    }

    private void determineWallSprite()
    {
        //used for transition biome 50/50 descion
        float transitionChoice = Random.Range(0, 1f);

        switch (world.getCurrentBiome())
        {

            case ConstantLibrary.BIO_DEFAULT:
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME0_WALL_START, BIOME0_WALL_START + 6));
                calculateMineHP(3);
                break;

            case ConstantLibrary.BIO_GREEN_TRANS:
                if(transitionChoice > gameManager.getMilestoneProgress())
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME0_WALL_START, BIOME0_WALL_START + 6));
                    calculateMineHP(3);
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_WALL_START, BIOME1_WALL_START + 6));
                    calculateMineHP(4);
                }
                break;

            case ConstantLibrary.BIO_GREEN:
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_WALL_START, BIOME1_WALL_START + 6));
                calculateMineHP(4);
                break;

            case ConstantLibrary.BIO_ICE_TRANS:
                if (transitionChoice > gameManager.getMilestoneProgress())
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_WALL_START, BIOME1_WALL_START + 6));
                    calculateMineHP(4);
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME2_WALL_START, BIOME2_WALL_START + 6));
                    calculateMineHP(5);
                }
                break;

            case ConstantLibrary.BIO_ICE:
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME2_WALL_START, BIOME2_WALL_START + 6));
                calculateMineHP(5);
                break;

            case ConstantLibrary.BIO_LAVA_TRANS:
                if (transitionChoice > gameManager.getMilestoneProgress())
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME2_WALL_START, BIOME2_WALL_START + 6));
                    calculateMineHP(5);
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME3_WALL_START, BIOME3_WALL_START + 6));
                    calculateMineHP(6);
                }
                break;

            case ConstantLibrary.BIO_LAVA:
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME3_WALL_START, BIOME3_WALL_START + 6));
                calculateMineHP(6);
                break;
        }
    }

    private int getNeighborCode(int[] typeCheck, bool invertCheck)
    {
        /* neighborCode is a value that determines which neighebors are present using binary
         * encoding to assign a unique int to each possible neighbor combination
         * 
         *          -Left   Neighbor is place value 1
         *          -Top    Neighbor is place value 2
         *          -Right  Neighbor is place value 4
         *          -Bottom Neighbor is place value 8
         * Example:
         *      if the top and right neighbors are present (are a wall type) then the neighborCode is
         *      2 + 4 = 6
        */
        int neighborCode = 0;

        //check that neighbors DO NOT have the specified types
        if (invertCheck)
        {
            neighborCode = checkTileForTypes(tileLeft, typeCheck) ? neighborCode : neighborCode + 1;
            neighborCode = checkTileForTypes(tileUp, typeCheck) ? neighborCode : neighborCode + 2;
            neighborCode = checkTileForTypes(tileRight, typeCheck) ? neighborCode : neighborCode + 4;
            neighborCode = checkTileForTypes(tileDown, typeCheck) ? neighborCode : neighborCode + 8;
        }

        //check that the neighbors DO have the specified types
        else
        {
            neighborCode = checkTileForTypes(tileLeft, typeCheck) ? neighborCode + 1 : neighborCode;
            neighborCode = checkTileForTypes(tileUp, typeCheck) ? neighborCode + 2 : neighborCode;
            neighborCode = checkTileForTypes(tileRight, typeCheck) ? neighborCode + 4 : neighborCode;
            neighborCode = checkTileForTypes(tileDown, typeCheck) ? neighborCode + 8 : neighborCode;
        }

        return neighborCode;
    }

    private void determineLightInfo()
    {
        checkShadowCasting();

        switch (tileType)
        {
            case ConstantLibrary.T_ORE_FUEL:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                light_top2.enabled = true;
                light_top2.color = new Vector4(1f, 0.5f, 0f, 1f);
                break;

            case ConstantLibrary.T_ORE_GOLD:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                light_top2.enabled = true;
                light_top2.color = new Vector4(1f, 1f, 0f, 1f);
                break;

            case ConstantLibrary.T_ORE_DIAMOND:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                light_top2.enabled = true;
                light_top2.color = new Vector4(0f, 1f, 1f, 1f);
                break;

            case ConstantLibrary.T_ORE_AMATHYST:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                light_top2.enabled = true;
                light_top2.color = new Vector4(1f, 0f, 1f, 1f);
                break;

            case ConstantLibrary.T_MINECART:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                light_top2.enabled = true;
                light_top2.color = new Vector4(1f, 1f, 1f, 1f);
                break;

            case ConstantLibrary.T_LAVA:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                break;

            case ConstantLibrary.T_LAVA_FLOW:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                break;

            case ConstantLibrary.T_LAVA_FLOW_BRIDGE:
                light_top.enabled = true;
                light_top.color = new Vector4(1f, 1f, 1f, 1f);
                break;

            case ConstantLibrary.T_EXPLOSIVE:
                light_top2.enabled = true;
                light_top2.color = new Vector4(1f, 0f, 0f, 1f);
                break;
        }
    }

    private void checkShadowCasting()
    {
        //Check if it is the type of tile that casts shadows
        if(checkTileForTypes(this, ConstantLibrary.typeCheck_shadowCasters))
        {
            //If the tile is surroundded by other shadow casters then turn off shadow casting
            //tileShadows.enabled = getNeighborCode(ConstantLibrary.typeCheck_shadowCasters, false) != 15;
            isShadowCaster = getNeighborCode(ConstantLibrary.typeCheck_shadowCasters, false) != 15;
        }
        else
        {
            //Does not cast shadows
            //tileShadows.enabled = false;
            isShadowCaster = false;
        }
    }

    private void determineGroundSprite()
    {
        float transitionChoice = Random.Range(0, 1f);

        switch (world.getCurrentBiome())
        {
            case ConstantLibrary.BIO_DEFAULT:
                //90% chance for basic floor, 10% chance for variant
                if(Random.Range(0,10) < 9)
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME0_GROUND_START, BIOME0_GROUND_START + 3));
                }
                else
                {
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME0_GROUND_START + 3, BIOME0_GROUND_START + 6));
                }
                break;

            case ConstantLibrary.BIO_GREEN_TRANS:
                if(transitionChoice > gameManager.getMilestoneProgress())
                {
                    if (Random.Range(0, 10) < 9)
                    {
                        s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME0_GROUND_START, BIOME0_GROUND_START + 3));
                    }
                    else
                    {
                        s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME0_GROUND_START + 3, BIOME0_GROUND_START + 6));
                    }
                }
                else
                {
                    if (Random.Range(0, 10) < 9)
                    {
                        s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_GROUND_START, BIOME1_GROUND_START + 3));
                    }
                    else
                    {
                        s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_GROUND_START + 3, BIOME1_GROUND_START + 6));
                    }
                }
                break;

            case ConstantLibrary.BIO_GREEN:
                if (Random.Range(0, 10) < 9)
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_GROUND_START, BIOME1_GROUND_START + 3));
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_GROUND_START + 3, BIOME1_GROUND_START + 6));
                }
                break;

            case ConstantLibrary.BIO_ICE_TRANS:
                if (transitionChoice > gameManager.getMilestoneProgress())
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME1_GROUND_START, BIOME1_GROUND_START + 6));
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME2_GROUND_START, BIOME2_GROUND_START + 3));
                }
                break;

            case ConstantLibrary.BIO_ICE:
                s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME2_GROUND_START, BIOME2_GROUND_START + 3));
                break;

            case ConstantLibrary.BIO_LAVA_TRANS:
                if (transitionChoice > gameManager.getMilestoneProgress())
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME2_GROUND_START, BIOME2_GROUND_START + 3));
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME3_GROUND_START, BIOME3_GROUND_START + 3));
                }
                break;

            case ConstantLibrary.BIO_LAVA:
                if (Random.Range(0, 10) < 9)
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME3_GROUND_START, BIOME3_GROUND_START + 3));
                }
                else
                {
                    s_Base.sprite = tileBaseSprites.getSprite(Random.Range(BIOME3_GROUND_START + 3, BIOME3_GROUND_START + 6));
                }
                break;
        }
    }

    private void determineRailSprite()
    {
        int edgeCode = 0;
        int[] railcheck = { ConstantLibrary.T_RAIL };
        edgeCode = getNeighborCode(railcheck, false);

        switch (edgeCode)
        {
            //Horizontal Variants
            case 5:
                s_Shade.sprite = railSprites.getSprite(16 + seed1);
                break;

            //Vertical Variants
            case 10:
                s_Shade.sprite = railSprites.getSprite(19 + seed1);
                break;

            //Just use edgeCode index (Error PNG's for invalid indices)
            default:
                s_Shade.sprite = railSprites.getSprite(edgeCode);
                break;
        }
    }

    private void determineLavaFlowSprite()
    {
        /*
                 * EDGE CODES FOR LAVA FLOW:
                 *  0 = Straight Line Down
                 *  1 = Merge to pool at bottom
                 *  2 = Merge to pool at top
                 *  3 = Bend touching bottom & right
                 *  4 = Bend touching top & right
                 *  5 = Bend touching bottom & left
                 *  6 = Bend touching top & left
                 */
        int edgeCode = 0;

        if (checkTileForType(tileUp, ConstantLibrary.T_LAVA))
        {
            edgeCode = 2;
        }

        if (checkTileForType(tileDown, ConstantLibrary.T_LAVA) || checkTileForType(tileDown, ConstantLibrary.T_HOLE))
        {
            edgeCode = 1;
        }

        if (checkTileForTypes(tileUp, ConstantLibrary.typeCheck_lavaFlow) && checkTileForTypes(tileRight, ConstantLibrary.typeCheck_lavaFlow))
        {
            edgeCode = 4;
        }

        if (checkTileForTypes(tileUp, ConstantLibrary.typeCheck_lavaFlow) && checkTileForTypes(tileLeft, ConstantLibrary.typeCheck_lavaFlow))
        {
            edgeCode = 6;
        }

        if (checkTileForTypes(tileDown, ConstantLibrary.typeCheck_lavaFlow) && checkTileForTypes(tileRight, ConstantLibrary.typeCheck_lavaFlow))
        {
            edgeCode = 3;
        }

        if (checkTileForTypes(tileDown, ConstantLibrary.typeCheck_lavaFlow) && checkTileForTypes(tileLeft, ConstantLibrary.typeCheck_lavaFlow))
        {
            edgeCode = 5;
        }

        a_Base.SetInteger("edgeCode", edgeCode);
    }

    private void determineGeodeOre()
    {
        int oreChoice = Random.Range(0, 10);
        if(oreChoice < 5)
        {
            tileType = ConstantLibrary.T_ORE_AMATHYST;
            calculateMineHP(2);
            s_Base.sprite = tileBaseSprites.getSprite(Random.Range(GEODE_START, GEODE_START + 3));
            s_Mod.sprite = tileModSprites.getSprite(MOD_AMATHYST);
            updateSortingOrder(baseLayer + 370);
            return;
        }
        else if(oreChoice < 8)
        {
            tileType = ConstantLibrary.T_ORE_DIAMOND;
            calculateMineHP(3);
            s_Base.sprite = tileBaseSprites.getSprite(Random.Range(GEODE_START, GEODE_START + 3));
            s_Mod.sprite = tileModSprites.getSprite(MOD_DIAMOND);
            updateSortingOrder(baseLayer + 370);
            return;
        }
        else
        {
            tileType = ConstantLibrary.T_ORE_GOLD;
            calculateMineHP(2);
            s_Base.sprite = tileBaseSprites.getSprite(Random.Range(GEODE_START, GEODE_START + 3));
            s_Mod.sprite = tileModSprites.getSprite(MOD_GOLD);
            updateSortingOrder(baseLayer + 350);
        }
    }

    private void calculateMineHP(int baseHealth)
    {
        mineHP = baseHealth - upgradeManager.getMineUpgrades();
        if(mineHP <= 0)
        {
            mineHP = 1;
        }
    }

    public void recalculateMineHP()
    {
        if(mineHP > 1 && !constantMineHP)
        {
            mineHP = mineHP - 1;
        }
    }

    public void randomizeSpriteSize()
    {
        float randomScale = Random.Range(1f, 1.3f);
        spriteSize = new Vector3(randomScale, randomScale, 1f);
        spriteTransform.localScale = spriteSize;

        baseLayer = ((int)(randomScale * 100)) * 10;
    }

    public float getSpriteSize()
    {
        return spriteSize.x;
    }

    private void updateSortingOrder(int baseVal)
    {
        s_Base.sortingOrder  = baseVal;
        s_Shade.sortingOrder = baseVal + 1;
        s_Mod.sortingOrder   = baseVal + 2;
        a_Base.GetComponent<SpriteRenderer>().sortingOrder = baseVal + 3;
        a_Edge.GetComponent<SpriteRenderer>().sortingOrder = baseVal + 4;
    }

    private void updateSortingLayer(int layer)
    {
        s_Base.sortingLayerID = layer;
        s_Shade.sortingLayerID = layer;
        s_Mod.sortingLayerID = layer;
        a_Base.GetComponent<SpriteRenderer>().sortingLayerID = layer;
        a_Edge.GetComponent<SpriteRenderer>().sortingLayerID = layer;
    }

    #region Graph Methods
    public void setNeighborUp(Tile up)
    {
        tileUp = up;
    }

    public Tile getNeighborUp()
    {
        return tileUp;
    }

    public void setNeighborDown(Tile down)
    {
        tileDown = down;
    }

    public Tile getNeighborDown()
    {
        return tileDown;
    }

    public void setNeighborLeft(Tile left)
    {
        tileLeft = left;
    }

    public Tile getNeighborLeft()
    {
        return tileLeft;
    }

    public void setNeighborRight(Tile right)
    {
        tileRight = right;
    }

    public Tile getNeighborRight()
    {
        return tileRight;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D col)
    {
        //explosion
        if (col.tag == "Explosion")
        {
            //Debug.Log("Tile hit by explosion!");
            changeMineHP(-100);
        }

        //flashlight
        if(col.tag == "flashlight")
        {
            facingFlashlight = true;
        }
        else
        {
            facingFlashlight = false;
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        //assign player locator
        if (col.tag == "playerLocator")
        {
            //Debug.Log("Player Locator checked");
            gameManager.checkPlayerLocationNode(this);
        }
    }

    public void changeMineHP(int HPamount)
    {
        mineHP += HPamount;
        if(mineHP <= 0)
        {
            switch (tileType)
            {
                case ConstantLibrary.T_WALL:
                    placeGround();
                    break;

                case ConstantLibrary.T_ORE_FUEL:
                    spawnItem(gameManager.getItem(ConstantLibrary.I_FUEL));
                    placeGround();
                    break;

                case ConstantLibrary.T_ORE_GOLD:
                    spawnItem(gameManager.getItem(ConstantLibrary.I_GOLD));
                    placeGround();
                    break;

                case ConstantLibrary.T_GEODE:
                    placeGround();
                    break;

                case ConstantLibrary.T_ORE_AMATHYST:
                    spawnItem(gameManager.getItem(ConstantLibrary.I_AMATHYST));
                    placeGround();
                    break;

                case ConstantLibrary.T_ORE_DIAMOND:
                    spawnItem(gameManager.getItem(ConstantLibrary.I_DIAMOND));
                    placeGround();
                    break;

                case ConstantLibrary.T_MINECART:
                    spawnItem(gameManager.getItem(ConstantLibrary.I_ASSORTMENT));
                    placeGround();
                    break;

                case ConstantLibrary.T_VINE:
                    changeTile(ConstantLibrary.T_VINE_GROWING);
                    break;

                case ConstantLibrary.T_EXPLOSIVE:
                    placeGround();
                    gameManager.getExplosion().spawnExplosion(transform.position);
                    break;
            }
            updateTile();
            updateNeighbors();
        }
        else
        {
            if (!checkTileForTypes(this, ConstantLibrary.tileNoShakeList))
            {
                anim.SetTrigger("tileHit");
            }
        }
    }

    private void placeGround()
    {
        if(world.getCurrentBiome() == ConstantLibrary.BIO_ICE)
        {
            changeTile(ConstantLibrary.T_ICE);
        }
        else
        {
            changeTile(ConstantLibrary.T_GROUND);
        }
    }

    public int getMineHP()
    {
        return mineHP;
    }

    public void setContainsItem(bool set)
    {
        containsItem = set;
    }

    public bool getContainsItem()
    {
        return containsItem;
    }

    private void spawnItem(Item item)
    {
        item.setOccupiedTile(this);
        item.teleport(transform.position);
        item.setActive(true);
        containsItem = true;
    }

    private void playAnimations()
    {
        if (gameManager.enableAnimatedTiles)
        {
            //Activate Animators
            a_Base.gameObject.SetActive(true);
            a_Edge.gameObject.SetActive(true);

            //Update Animator info
            a_Edge.SetInteger("tileType", tileType);
            a_Base.SetInteger("tileType", tileType);

            //Sync Animations with master animator tile
            a_Edge.Play(0, -1, a_Master.GetCurrentAnimatorStateInfo(0).normalizedTime);
            a_Base.Play(0, -1, a_Master.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
    }

    #region Pathfinding
    public bool isValidPathTile()
    {
        if (!visited)
        {
            //Add more checks as more types of ground are developed
            if (tileType == ConstantLibrary.T_GROUND)
            {
                return true;
            }
        }

        return false;
    }

    public void setVisited(bool set)
    {
        visited = set;
    }

    public void setPrevious(Tile tile)
    {
        previousTile = tile;
    }

    public Tile getPrevious()
    {
        return previousTile;
    }

    #endregion
}
