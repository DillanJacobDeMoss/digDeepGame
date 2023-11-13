using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    //Array of column game objects that make up the playable worldspace
    public Column[] columns;

    //Indices of the leftmost & rightmost column gameObjects
    private int tail;
    private int head;

    /**
     * Keeps track of the number of chunks traversed. When this number gets sufficiently high (150)
     * this means the playspace has moved very far to the right of the unity scene and must be
     * recentered to avoid floating point inaccuracies.
     */
    private int chunksTraversed = 0;

    //The biome the player is in. Changes as the player advances.
    private int currentBiome = 0;

    //Ash particles that begin spawning in the Lava Biome
    private ParticleSystem particles_ash;

    //The number of chunks that need to generate before the next railway spawns
    private int chunksToNextRail;

    private GameManager gameManager;

    //Int arrays that the columns refer to in order to generate new tiles
    private int[,] chunk1;
    private int[,] chunk2;

    //The chunk that is edited as the chunk generation algortim runs
    private int[,] generatingChunk;

    private bool headChunkIs1;
    private int currentChunkColumn;

    /**
     * Dictionary containing all the different world generation formations
     * @key string
     *  The name of the formation
     *  
     * @value 2D int array
     *  The array of ints representing the tileTypes within the formation
     */
    Dictionary<string, int[,]> formations;

    private float loadTimer = 1f;
    public bool worldInit = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        particles_ash = GameObject.FindGameObjectWithTag("Particles_Ash").GetComponent<ParticleSystem>();
        formations = new Dictionary<string, int[,]>();
        chunksToNextRail = Random.Range(1, 4);
    }

    private void FixedUpdate()
    {
        if (!worldInit)
        {
            loadTimer -= Time.deltaTime;
            if(loadTimer <= 0)
            {
                worldInit = true;
                worldInitialization();
                gameManager.removeLoadScreen();
            }
        }
    }

    /**
     * Initializes all variables needed for procedural world generation
     */
    private void worldInitialization()
    {
        //TILE INITIALIZATION
        generatingChunk = new int[columns.Length, 25];
        initializeGraph();
        initializeFormations();
        generateStartChunk();
        forceUniqueTileSizes();

        //SET UP CHUNK GENERATION
        tail = 0; //The leftmost column of tile objects initially refers to the 0th column in the 
        head = columns.Length - 1;

        chunk1 = new int[columns.Length, 25];
        chunk2 = new int[columns.Length, 25];

        chunk1 = generateChunk();
        chunk2 = generateChunk();

        headChunkIs1 = true;
        currentChunkColumn = 0;

        initializeShading();
    }

    /**
     * Initializes the 2D arrays representing the world formations that are placed in the
     * generating chunk during the chunk generation step.
     */
    private void initializeFormations()
    {
        int ___ = ConstantLibrary.T_GROUND;
        int GEO = ConstantLibrary.T_GEODE;
        int GOR = ConstantLibrary.T_GEODE_ORE;
        int HOL = ConstantLibrary.T_HOLE;

        int[,] formation_geode = new int[,]
            {
                {-1,     -1,    ___,    ___,    ___,    ___,    ___,    -1,     -1 },
                {-1,    ___,    ___,    GEO,    GEO,    GEO,    ___,    ___,    -1 },
                {___,   ___,    GEO,    GEO,    GEO,    GEO,    GEO,    ___,    ___},
                {___,   GEO,    GEO,    GOR,    GOR,    GOR,    GEO,    GEO,    ___},
                {___,   GEO,    GEO,    GOR,    GOR,    GOR,    GEO,    GEO,    ___},
                {___,   GEO,    GEO,    GOR,    GOR,    GOR,    GEO,    GEO,    ___},
                {___,   ___,    GEO,    GEO,    GEO,    GEO,    GEO,    ___,    ___},
                {-1,    ___,    ___,    GEO,    GEO,    GEO,    ___,    ___,    -1 },
                {-1,    -1,     ___,    ___,    ___,    ___,    ___,    -1,     -1 }
            };
        formations.Add("geode", formation_geode);

        int[,] formation_cave_circleSmall = new int[,]
        {
            {-1,    ___,    ___,    ___,    -1  },
            {___,   ___,    ___,    ___,    ___ },
            {___,   ___,    ___,    ___,    ___ },
            {___,   ___,    ___,    ___,    ___ },
            {-1,    ___,    ___,    ___,    -1  }
        };
        formations.Add("cave_circleSmall", formation_cave_circleSmall);

        int[,] formation_cave_circleBig = new int[,]
        {
            {-1,    -1,     ___,    ___,    ___,    -1,     -1  },
            {-1,    ___,    ___,    ___,    ___,    ___,    -1  },
            {___,   ___,    ___,    ___,    ___,    ___,    ___ },
            {___,   ___,    ___,    ___,    ___,    ___,    ___ },
            {___,   ___,    ___,    ___,    ___,    ___,    ___ },
            {-1,    ___,    ___,    ___,    ___,    ___,    -1  },
            {-1,    -1,     ___,    ___,    ___,    ___,    -1  }
        };
        formations.Add("cave_circleBig", formation_cave_circleBig);

        int[,] formation_cave_snake = new int[,]
        {
            {-1,    -1,     -1 ,    -1 ,    ___,    ___},
            {-1 ,   -1 ,    ___,    ___,    ___,    ___},
            {-1 ,   ___,    ___,    ___,    ___,    ___},
            {-1 ,   ___,    ___,    ___,    ___,    -1 },
            {___,   ___,    ___,    ___,    -1 ,    -1 },
            {___,   ___,    ___,    ___,    -1 ,    -1 },
            {___,   ___,    ___,    -1 ,    -1 ,    -1 }
        };
        formations.Add("cave_snake", formation_cave_snake);

        int[,] formation_hole_snake = new int[,]
        {
            {HOL,   HOL,    -1 ,    -1 ,    -1 ,    -1 ,    HOL,    HOL,    -1 ,    -1,      -1},
            {HOL,   HOL,    -1 ,    -1 ,    -1 ,    HOL,    HOL,    HOL,    HOL,    HOL,      -1},
            {HOL,   HOL,    HOL,    HOL,    HOL,    HOL,    HOL,    HOL,    HOL,    HOL,     HOL},
            {-1 ,   HOL,    HOL,    HOL,    HOL,    HOL,    HOL,    HOL,    HOL,    HOL,     HOL},
            {-1 ,   -1 ,    HOL,    HOL,    HOL,    HOL,    -1 ,    -1 ,    -1 ,    -1,      HOL},
        };
        formations.Add("hole_snake", formation_hole_snake);

        formations.Add("cave_rand1", createRandomizedCaveFormation());
        formations.Add("cave_rand2", createRandomizedCaveFormation());
        formations.Add("cave_rand3", createRandomizedCaveFormation());

        formations.Add("hole_rand1", createRandDrillpathHazard(ConstantLibrary.T_HOLE));
        formations.Add("hole_rand2", createRandDrillpathHazard(ConstantLibrary.T_HOLE));
        formations.Add("hole_rand3", createRandDrillpathHazard(ConstantLibrary.T_HOLE));

        formations.Add("hole_randSmall1", createRandNonDrillpathHazard(ConstantLibrary.T_HOLE));
        formations.Add("hole_randSmall2", createRandNonDrillpathHazard(ConstantLibrary.T_HOLE));
        formations.Add("hole_randSmall3", createRandNonDrillpathHazard(ConstantLibrary.T_HOLE));

        formations.Add("water_rand1", createRandDrillpathHazard(ConstantLibrary.T_WATER));
        formations.Add("water_rand2", createRandDrillpathHazard(ConstantLibrary.T_WATER));
        formations.Add("water_rand3", createRandDrillpathHazard(ConstantLibrary.T_WATER));

        formations.Add("water_randSmall1", createRandNonDrillpathHazard(ConstantLibrary.T_WATER));
        formations.Add("water_randSmall2", createRandNonDrillpathHazard(ConstantLibrary.T_WATER));
        formations.Add("water_randSmall3", createRandNonDrillpathHazard(ConstantLibrary.T_WATER));

        formations.Add("lava_rand1", createRandDrillpathHazard(ConstantLibrary.T_LAVA));
        formations.Add("lava_rand2", createRandDrillpathHazard(ConstantLibrary.T_LAVA));
        formations.Add("lava_rand3", createRandDrillpathHazard(ConstantLibrary.T_LAVA));

        formations.Add("lava_randSmall1", createRandNonDrillpathHazard(ConstantLibrary.T_LAVA));
        formations.Add("lava_randSmall2", createRandNonDrillpathHazard(ConstantLibrary.T_LAVA));
        formations.Add("lava_randSmall3", createRandNonDrillpathHazard(ConstantLibrary.T_LAVA));
    }

    /**
     * Creates a cave formation to be placed in the world.
     * Only called during the world's initialize formations step. 
     * 
     * @return
     *  2D int array representing the formation generated
     */
    private int[,] createRandomizedCaveFormation()
    {
        int colCount = Random.Range(5, 10);
        int rowCount = Random.Range(5, 10);
        int[,] cave = new int[colCount, rowCount];

        int holeStart = 0;
        int holeEnd = colCount-1;

        //Top Half
        for(int j = rowCount/2; j > 0; j--)
        {
            for(int i = 0; i < colCount; i++)
            {
                if(i >= holeStart && i <= holeEnd)
                {
                    cave[i, j] = ConstantLibrary.T_GROUND;
                }
                else
                {
                    cave[i, j] = -1;
                }
            }
            holeStart += Random.Range(0, 3);
            holeEnd -= Random.Range(0, 3);
        }

        holeStart = 0;
        holeEnd = colCount - 1;

        for (int j = rowCount / 2; j < rowCount; j++)
        {
            for (int i = 0; i < colCount; i++)
            {
                if (i >= holeStart && i <= holeEnd)
                {
                    cave[i, j] = ConstantLibrary.T_GROUND;
                }
                else
                {
                    cave[i, j] = -1;
                }
            }
            holeStart += Random.Range(0, 3);
            holeEnd -= Random.Range(0, 3);
        }

        return cave;
    }

    /**
     * Creates a level hazard that is intended to be placed somwhere ON the drill's path.
     * Only called during the world's initialize formations step.
     * 
     * @param int hazardType
     *  The type of level hazard to be generated (holes/lava/etc..)
     * 
     * @return 
     *  2D int array representing the formation generated
     */
    private int[,] createRandDrillpathHazard(int hazardType)
    {
        int colCount = Random.Range(5, 8);
        int rowCount = Random.Range(12, 17);
        int[,] hole = new int[colCount, rowCount];

        int holeWidth = Random.Range(4,8);
        int holeTop = rowCount/2 - holeWidth/2;
        int holeBot = holeTop + holeWidth;
        int changeAmount;

        for (int i = 0; i < colCount; i++)
        {
            for(int j = 0; j < rowCount; j++)
            {
                if(j < holeTop || j >= holeBot)
                {
                    hole[i, j] = -1;
                }
                else
                {
                    hole[i, j] = hazardType;
                }
            }



            //expand before midpoint, contract after midpoint
            if(i < colCount / 2)
            {
                changeAmount = Random.Range(1, 5);
                holeWidth += changeAmount;
                holeTop -= Random.Range(-1,3);
                holeBot = holeTop + holeWidth;
                if(holeWidth >= rowCount)
                {
                    holeWidth = rowCount;
                }

            }
            else
            {
                changeAmount = Random.Range(1, 5);
                holeWidth -= changeAmount;
                holeTop += Random.Range(-1, 3);
                holeBot = holeTop + holeWidth;
            }
        }

        return hole;
    }

    /**
     * Creates a level hazard that is intended to be placed somwhere OFF the drill's path.
     * Only called during the world's initialize formations step.
     * 
     * @param int hazardType
     *  The type of level hazard to be generated (holes/lava/etc..)
     * 
     * @return 
     *  2D int array representing the formation generated
     */
    private int[,] createRandNonDrillpathHazard(int hazardType)
    {
        int colCount = Random.Range(5, 12);
        int rowCount = Random.Range(5, 12);
        int[,] hole = new int[colCount, rowCount];

        int holeWidth = Random.Range(4, 8);
        int holeTop = rowCount / 2 - holeWidth / 2;
        int holeBot = holeTop + holeWidth;
        int changeAmount;

        for (int i = 0; i < colCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                if (j < holeTop || j >= holeBot)
                {
                    hole[i, j] = -1;
                }
                else
                {
                    hole[i, j] = hazardType;
                }
            }



            //expand before midpoint, contract after midpoint
            if (i < colCount / 2)
            {
                changeAmount = Random.Range(1, 5);
                holeWidth += changeAmount;
                holeTop -= Random.Range(-1, 3);
                holeBot = holeTop + holeWidth;
                if (holeWidth >= rowCount)
                {
                    holeWidth = rowCount;
                }

            }
            else
            {
                changeAmount = Random.Range(1, 5);
                holeWidth -= changeAmount;
                holeTop += Random.Range(-1, 3);
                holeBot = holeTop + holeWidth;
            }
        }

        return hole;
    }

    //Generates the starting chunk (special cave)
    private void generateStartChunk()
    {
        //Fill Chunk
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < columns[0].tiles.Length; j++)
            {
                generatingChunk[i, j] = ConstantLibrary.T_WALL;
            }
        }

        //Poke Initial Hall
        int hallLeft = Random.Range(columns.Length - 38, columns.Length - 30);
        int hallRight = columns.Length - 7;
        for(int i = hallLeft; i < hallRight; i++)
        {
            for(int j = 10; j < 15; j++)
            {
                generatingChunk[i, j] = ConstantLibrary.T_GROUND;
            }
        }

        //Make hall irregular
        int hallLeftChiselStart = Random.Range(10, 12);
        for (int i = hallLeftChiselStart; i < hallLeftChiselStart+3; i++)
        {
            generatingChunk[hallLeft-1, i] = ConstantLibrary.T_GROUND;
        }

        int hallTopChiselLayers = Random.Range(3, 7);
        int leftBound = hallLeft;
        int rightBound = hallRight;
        int chiselRow = 9;
        while(hallTopChiselLayers > 0)
        {
            leftBound = leftBound + Random.Range(2, 6);
            rightBound = rightBound - Random.Range(2, 6);
            if (leftBound >= columns.Length || rightBound < 0)
            {
                break;
            }
            for (int i = leftBound; i < rightBound; i++)
            {
                generatingChunk[i, chiselRow] = ConstantLibrary.T_GROUND;
            }

            chiselRow--;
            hallTopChiselLayers--;
        }

        int hallBotChiselLayers = Random.Range(3, 7);
        leftBound = hallLeft;
        rightBound = hallRight;
        chiselRow = 15;
        while (hallBotChiselLayers > 0)
        {
            leftBound = leftBound + Random.Range(2, 6);
            rightBound = rightBound - Random.Range(2, 6);
            if(leftBound >= columns.Length || rightBound < 0)
            {
                break;
            }
            for (int i = leftBound; i < rightBound; i++)
            {
                generatingChunk[i, chiselRow] = ConstantLibrary.T_GROUND;
            }

            chiselRow++;
            hallTopChiselLayers--;
        }


        //Add Fuel
        generateFuel();

        //Overwrite Tiles
        for(int i = 0; i < columns.Length; i++)
        {
            columns[i].loadColumn(generatingChunk, i);
        }
    }

    //World generation algorithm that creates a new chunk
    private int[,] generateChunk()
    {
        int cols = columns.Length;
        int rows = 25;

        #region Fill Chunk
        for(int i = 0; i < cols; i++)
        {
            for(int j = 0; j < rows; j++)
            {
                generatingChunk[i, j] = ConstantLibrary.T_WALL;
            }
        }
        #endregion

        switch (gameManager.getWorldgenType())
        {
            case ConstantLibrary.WORLDTYPE_NORMAL:
                worldgen_normal();
                break;

            case ConstantLibrary.WORLDTYPE_BOSS:
                worldgen_boss();
                break;
        }

        return generatingChunk;
    }

    private void worldgen_normal()
    {
        generateCaves();
        generateBiomeHazard();
        generateHoles();
        generateGeode();
        generateRails();
        generateGold();
        generateDiamond();
        generateFuel();
        iceReplace();
        generateLavaFlow();
        generateBossTeasers();
    }

    private void worldgen_boss()
    {
        switch (gameManager.getCurrentBoss())
        {
            case ConstantLibrary.BOSS_TEST:
                generateCaves();
                generateBiomeHazard();
                generateHoles();
                generateFuel();
                iceReplace();
                generateLavaFlow();
                break;

            case ConstantLibrary.BOSS_SPIDER:
                break;

            case ConstantLibrary.BOSS_BEE:
                break;

            case ConstantLibrary.BOSS_FLYTRAP:
                break;

            case ConstantLibrary.BOSS_WORM:
                break;

            case ConstantLibrary.BOSS_SKEL:
                break;
        }
    }

    private void generateBossTeasers()
    {
        //generate Boss Teaser Terrain in the milestone before the boss
        if(gameManager.getDifficulty() % 5 == 4)
        {
            switch (gameManager.getCurrentBoss())
            {
                //Insert Teaser Formation Stuff Based on upcoming boss
            }
        }
    }

    //Starts the process of loading the next column of the world (called by drill)
    public void nextColumn()
    {
        //Bring the tail of the world to the head of the world
        tailToFront();

        //Generates NEW tile adjacencies after columns have moved
        updateGraph();

        //Open up a path that the drill "created"
        generateDrillPath();

        //Change the tiles to match the preset
        sendColumnData();
    }

    //Moves the leftmost column object to the frontmost side of the world
    private void tailToFront()
    {
        //Physically move the leftmost column object to the rightmost side of the world
        columns[tail].shiftColumn(columns.Length);

        //The old tail is now the new head
        head = tail;

        //The new tail is the next column
        tail++;
        if (tail >= columns.Length)
        {
            tail = 0;
            chunksTraversed++;
            if(chunksTraversed >= 150)
            {
                gameManager.resetGamespaceToOrigin();
                chunksTraversed = 0;
            }
        }
    }

    //updates the tiles in the drill's path as it drives through the world
    private void generateDrillPath()
    {
        //Column around the base of the drillhead
        int drilledColumn = head - 7;
        if(drilledColumn < 0)
        {
            drilledColumn = columns.Length + drilledColumn;
        }

        columns[drilledColumn].renderDrillPath();
    }

    /**
     * Updates the column of tiles that was just shifted from the tail of the world to the head
     * of the world with data from the current chunk column
     */
    public void sendColumnData()
    {
        //Identify the column left of the head (to update its shading)
        int leftOfHead = head - 1;
        if (leftOfHead < 0)
        {
            leftOfHead = columns.Length - 1;
        }

        //Send column data from the correct chunk
        if (headChunkIs1)
        {
            columns[head].loadColumn(chunk1, currentChunkColumn);
        }
        else
        {
            columns[head].loadColumn(chunk2, currentChunkColumn);
        }

        //Update Left of Head's shading
        for (int i = 0; i < columns[tail].tiles.Length; i++)
        {
            columns[leftOfHead].tiles[i].updateTile();
        }

        //advance chunk column pointer & check if we need to generate the next chunk
        currentChunkColumn++;
        if(currentChunkColumn >= columns.Length)
        {
            if (headChunkIs1)
            {
                chunk1 = generateChunk();
            }
            else
            {
                chunk2 = generateChunk();
            }

            headChunkIs1 = !headChunkIs1;
            currentChunkColumn = 0;
        }
    }

    #region PUBLIC DEV FUNCTIONS
    //ONLY USED FOR DEV TESTING //////
    //re-generates both chunks
    public void reloadChunks()
    {
        chunk1 = generateChunk();
        chunk2 = generateChunk();
    }
    public void reloadChunksSafe()
    {
        if (!headChunkIs1)
        {
            chunk1 = generateChunk();
        }
        else
        {
            chunk2 = generateChunk();
        }
    }
    //////////////////////////////////
    #endregion

    //Places a random assortment of caves in the newly generating chunk
    private void generateCaves()
    {
        int rootCountTop = Random.Range(8, 16);
        int rootCountBot = Random.Range(8, 16);

        #region Generate Top Caves
        while (rootCountTop > 0)
        {
            //find a location for the root
            int spawnCol = Random.Range(0, columns.Length);
            int spawnRow = Random.Range(1, 10);

            //if valid root
            if (generatingChunk[spawnCol, spawnRow] == ConstantLibrary.T_WALL)
            {
                //pick a shape
                int shape = Random.Range(0, 6);
                switch (shape)
                {
                    case 0:
                        generateFormation(formations["cave_snake"], spawnCol, spawnRow);
                        break;

                    case 1:
                        generateFormation(formations["cave_circleBig"], spawnCol, spawnRow);
                        break;

                    case 2:
                        generateFormation(formations["cave_rand1"], spawnCol, spawnRow);
                        break;

                    case 3:
                        generateFormation(formations["cave_rand2"], spawnCol, spawnRow);
                        break;

                    case 4:
                        generateFormation(formations["cave_circleSmall"], spawnCol, spawnRow);
                        break;

                    case 5:
                        generateFormation(formations["cave_rand3"], spawnCol, spawnRow);
                        break;

                    default:
                        generateFormation(formations["cave_circleSmall"], spawnCol, spawnRow);
                        break;
                }
            }

            rootCountTop--;
        }
        #endregion

        #region Generate Bottom Caves
        while (rootCountBot > 0)
        {
            //find a location for the root
            int spawnCol = Random.Range(0, columns.Length);
            int spawnRow = Random.Range(14, 24);

            //if valid root
            if (generatingChunk[spawnCol, spawnRow] == ConstantLibrary.T_WALL)
            {
                //pick a shape
                int shape = Random.Range(0, 6);
                switch (shape)
                {
                    case 0:
                        generateFormation(formations["cave_snake"], spawnCol, spawnRow);
                        break;

                    case 1:
                        generateFormation(formations["cave_circleBig"], spawnCol, spawnRow);
                        break;

                    case 2:
                        generateFormation(formations["cave_rand1"], spawnCol, spawnRow);
                        break;

                    case 3:
                        generateFormation(formations["cave_rand2"], spawnCol, spawnRow);
                        break;

                    case 4:
                        generateFormation(formations["cave_circleSmall"], spawnCol, spawnRow);
                        break;

                    case 5:
                        generateFormation(formations["cave_rand3"], spawnCol, spawnRow);
                        break;

                    default:
                        generateFormation(formations["cave_circleSmall"], spawnCol, spawnRow);
                        break;
                }
            }

            rootCountBot--;
        }
        #endregion
    }

    //Places gold in the newly generating chunk
    private void generateGold()
    {
        int oreCount = Random.Range(3, 9); //Amount of gold ore to spawn in this chunk
        int attempts;   //Limits the number of times a single ore can attempt to spawn
        int spawnSide;  //Decides weather to spawn gold in the top or bottom of the world
        int spawnCol;   //The column to spawn the gold in
        int spawnRow;   //The row to spawn the gold in

        while (oreCount > 0)
        {
            attempts = 0;

            spawnSide = Random.Range(0, 2);

            while(attempts < 20)
            {
                if (spawnSide == 0)
                {
                    spawnCol = Random.Range(0, columns.Length);
                    spawnRow = Random.Range(1, 7);
                }
                else
                {
                    spawnCol = Random.Range(0, columns.Length);
                    spawnRow = Random.Range(18, 24);
                }

                if (validateTileSpawn(ConstantLibrary.T_ORE_GOLD, spawnCol, spawnRow))
                {
                    generatingChunk[spawnCol, spawnRow] = ConstantLibrary.T_ORE_GOLD;
                    break;
                }

                attempts++;
            }

            oreCount--;
        }
    }

    //Places fuel in the newly generating chunk
    private void generateFuel()
    {
        int oreCount = Random.Range(8, 17); //Amount of fuel ore to spawn in this chunk
        int attempts;   //Limits the number of times a single ore can attempt to spawn
        int spawnSide;  //Decides weather to spawn in the top or bottom of the world
        int spawnCol;   //The column to spawn the gold in
        int spawnRow;   //The row to spawn the gold in

        while (oreCount > 0)
        {
            attempts = 0;

            spawnSide = Random.Range(0, 2);

            while (attempts < 30)
            {
                if (spawnSide == 0)
                {
                    spawnCol = Random.Range(0, columns.Length);
                    spawnRow = Random.Range(4, 10);
                }
                else
                {
                    spawnCol = Random.Range(0, columns.Length);
                    spawnRow = Random.Range(15, 19);
                }

                if (validateTileSpawn(ConstantLibrary.T_ORE_FUEL, spawnCol, spawnRow))
                {
                    generatingChunk[spawnCol, spawnRow] = ConstantLibrary.T_ORE_FUEL;
                    break;
                }

                attempts++;
            }

            oreCount--;
        }
    }

    //Places diamonds in the newly generating chunk
    private void generateDiamond()
    {
        int diamondCountTop = 1;
        int diamondCountBot = 1;

        //Generate diamond in the Top
        int attempts = 0;
        while (diamondCountTop > 0 && attempts < 50)
        {
            int spawnCol = Random.Range(5, columns.Length-5);
            int spawnRow = Random.Range(1, 4);

            if (validateTileSpawn(ConstantLibrary.T_ORE_DIAMOND, spawnCol, spawnRow))
            {
                generatingChunk[spawnCol, spawnRow] = ConstantLibrary.T_ORE_DIAMOND;
                diamondCountTop--;
            }
            attempts++;
        }

        //Generate diamond in the Bottom
        attempts = 0;
        while (diamondCountBot > 0 && attempts < 50)
        {
            int spawnCol = Random.Range(5, columns.Length-5);
            int spawnRow = Random.Range(21, 24);

            if (validateTileSpawn(ConstantLibrary.T_ORE_DIAMOND, spawnCol, spawnRow))
            {
                generatingChunk[spawnCol, spawnRow] = ConstantLibrary.T_ORE_DIAMOND;
                diamondCountBot--;
            }
            attempts++;
        }
    }

    /**
     * Checks if input location meets the tile spawn criterion.
     * 
     * @param int type
     *  The tileType of the tile requiring validation
     * 
     * @param int col
     *  The column index of the location to be validated
     *  
     * @param int row
     *  The row index of the location to be validated 
     *  
     * @return
     *  True if the proposed spawn location is valid
     *  False if the location is invalid
     */
    private bool validateTileSpawn(int type, int col, int row)
    {
        //Handle validation based on input type
        switch (type)
        {
            case ConstantLibrary.T_ORE_FUEL:
                //Make sure the spawn location is next to open ground & replacing a wall
                if (generatingChunk[col, row] == ConstantLibrary.T_WALL)
                {
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (validateChunkBound(col + i, row + j))
                            {
                                if (generatingChunk[col + i, row + j] == ConstantLibrary.T_GROUND ||
                                    generatingChunk[col + i, row + j] == ConstantLibrary.T_RAIL)
                                {
                                    return true; //adjacent ground tile found
                                }
                            }
                        }
                    }
                }
                return false; //Too deep in a wall (no adjacent ground)

            case ConstantLibrary.T_EXPLOSIVE:
                //Make sure the spawn location is next to open ground (only cardinal directions) & replacing a wall
                if (generatingChunk[col, row] == ConstantLibrary.T_WALL)
                {
                    if (validateChunkBound(col + 1,row))
                    {
                        if(generatingChunk[col + 1, row] == ConstantLibrary.T_GROUND ||
                           generatingChunk[col + 1, row] == ConstantLibrary.T_RAIL)
                        {
                            return true;
                        }
                    }
                    if (validateChunkBound(col - 1, row))
                    {
                        if (generatingChunk[col - 1, row] == ConstantLibrary.T_GROUND ||
                            generatingChunk[col - 1, row] == ConstantLibrary.T_RAIL)
                        {
                            return true;
                        }
                    }
                    if (validateChunkBound(col, row + 1))
                    {
                        if (generatingChunk[col, row + 1] == ConstantLibrary.T_GROUND ||
                            generatingChunk[col, row + 1] == ConstantLibrary.T_RAIL)
                        {
                            return true;
                        }
                    }
                    if (validateChunkBound(col, row - 1))
                    {
                        if (generatingChunk[col, row - 1] == ConstantLibrary.T_GROUND ||
                            generatingChunk[col, row - 1] == ConstantLibrary.T_RAIL)
                        {
                            return true;
                        }
                    }
                }
                return false; //Too deep in a wall (no adjacent ground)

            case ConstantLibrary.T_ORE_GOLD:
                //Make sure gold spawns within 2 tiles of ground (not too deep in a sea of stone) & replacing a wall
                if (generatingChunk[col, row] == ConstantLibrary.T_WALL)
                {
                    for (int i = -2; i < 3; i++)
                    {
                        for (int j = -2; j < 3; j++)
                        {
                            if (validateChunkBound(col + i, row + j))
                            {
                                if (generatingChunk[col + i, row + j] == ConstantLibrary.T_GROUND ||
                                    generatingChunk[col + i, row + j] == ConstantLibrary.T_RAIL)
                                {
                                    return true; //Close by ground tile found
                                }
                            }
                        }
                    }
                }
                return false; //Too deep in a wall (no close by ground)

            case ConstantLibrary.T_ORE_DIAMOND:
                //Diamond must have at least 2 tiles of stone surrounding it in all directions & replacing a wall
                if (generatingChunk[col, row] == ConstantLibrary.T_WALL)
                {
                    for (int i = -2; i < 3; i++)
                    {
                        for (int j = -2; j < 3; j++)
                        {
                            if (validateChunkBound(col + i, row + j))
                            {
                                if (generatingChunk[col + i, row + j] == ConstantLibrary.T_GROUND)
                                {
                                    return false; //Close by ground was found (not buried deep enough)
                                }
                            }
                        }
                    }
                }
                else
                {
                    return false; //must be replacing a wall tile
                }
                break;

            case ConstantLibrary.T_RAIL:
                for(int i = 0; i < ConstantLibrary.typeCheck_railSlots.Length; i++)
                {
                    if(generatingChunk[col,row] == ConstantLibrary.typeCheck_railSlots[i])
                    {
                        return true; //location in question can be replaced by a rail
                    }
                }
                return false; //location in question is occupied by a type that can't be replaced by a rail

            case ConstantLibrary.T_MINECART:
                if(row == 0 ||
                   row == columns[0].tiles.Length -1 ||
                   (row >= ConstantLibrary.DRILLPATH_TOP && row <= ConstantLibrary.DRILLPATH_BOT))
                {
                    return false; //Minecart cant be on the top of the world, bottom of the world, or Drillpath
                }
                break;

            default:
                return false; //No recognized tileType input
        }

        return true; //No complications with spawning criteria found
    }

    /**
     * Places geodes in the generating chunk. Geodes are hard to break into but contain
     * the most valuable resources.
     */
    private void generateGeode()
    {
        //Only spawn geodes after difficulty 4
        if (gameManager.getDifficulty() < 4)
        {
            return;
        }

        int geodeCountTop = Random.Range(0, 2);
        int geodeCountBot = Random.Range(0, 2);

        //Only allow 1 geode per chunk before difficulty 10
        if(geodeCountTop == geodeCountBot && gameManager.getDifficulty() < 10) { return; }

        //If difficulty high enough guarantee 2 geodes
        if(gameManager.getDifficulty() >= 30)
        {
            geodeCountBot = 1;
            geodeCountTop = 1;
        }

        //Form Geode Top
        while (geodeCountTop > 0)
        {
            int rootCol = Random.Range(4, columns.Length - 5);
            int rootRow = Random.Range(3, 7);

            generateFormation(formations["geode"], rootCol, rootRow);

            geodeCountTop--;
        }

        //Form Geode Bot
        while (geodeCountBot > 0)
        {
            int rootCol = Random.Range(4, columns.Length - 5);
            int rootRow = Random.Range(18, 22);

            generateFormation(formations["geode"], rootCol, rootRow);

            geodeCountBot--;
        }
    }

    /**
     * Verifies that the generating chunk index being referenced is not out of bounds.
     * used when generating formations. 
     * (overwriting values in the generating chunk)
     * 
     * @param int i
     *  The column index to be overwritten
     * @param int j
     *  The row index to be overwritten
     *  
     * @return
     *  True if both the row and column are valid indices.
     *  False if the either the row or column are invalid indices.
     */
    private bool validateChunkBound(int col, int row)
    {
        if (col < 0 || col >= columns.Length) { return false; }
        if (row < 0 || row >= columns[0].tiles.Length) { return false; }
        return true;
    }

    /**
     * Places holes in the generating chunk.
     * Hole tiles cannot be traversed on foot but the player can ride the drill across them
     * or walk across any wooden plank tiles that spawn across them.
     */
    private void generateHoles()
    {
        int generate = 0;
        int spawnCol;
        int spawnRow;
        int formationCode;

        if(gameManager.getDifficulty() >= 3)
        {
            generate = Random.Range(0, 2);
        }

        //Drillpath Hole
        if(generate == 1)
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(9, 15);
            formationCode = Random.Range(0, 4);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["hole_snake"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["hole_rand1"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["hole_rand2"], spawnCol, spawnRow);
                    break;

                case 3:
                    generateFormation(formations["hole_rand3"], spawnCol, spawnRow);
                    break;
            }
        }

        //Top OR Bottom Extra Hole (Always picks one or the other)
        generate = Random.Range(0, 2);
        int makeBridge = Random.Range(0, 2); //50% chance to make a bridge across the hole
        int bridgeRow;
        if (generate == 1)
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(3, 9);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["hole_randSmall1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["hole_randSmall2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["hole_randSmall3"], spawnCol, spawnRow);
                    break;
            }

            if(makeBridge == 1)
            {
                bridgeRow = spawnRow + Random.Range(-1, 2);
                for(int i = 0; i < columns.Length; i++)
                {
                    if(generatingChunk[i,bridgeRow] == ConstantLibrary.T_HOLE)
                    {
                        generatingChunk[i, bridgeRow] = ConstantLibrary.T_BRIDGE;
                    }
                }
            }
        }
        else
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(15, 22);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["hole_randSmall1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["hole_randSmall2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["hole_randSmall3"], spawnCol, spawnRow);
                    break;
            }

            if (makeBridge == 1)
            {
                bridgeRow = spawnRow + Random.Range(-1, 2);
                for (int i = 0; i < columns.Length; i++)
                {
                    if (generatingChunk[i, bridgeRow] == ConstantLibrary.T_HOLE)
                    {
                        generatingChunk[i, bridgeRow] = ConstantLibrary.T_BRIDGE;
                    }
                }
            }
        }
    }

    /**
     * Overwrites values in a subsection of the generating chunk array with values specified
     * by the input formation array.
     * (places a formation such as a hole or lava pit in the generating chunk)
     * 
     * @param 2D int Array formation
     *  The 2D array representing the values that will be used to overwrite the subsection
     *  of the generating chunk. This array is always much smaller than the generating chunk array.
     * @param int rootCol
     *  The column index of the generating chunk array that represents the center of where the
     *  formation will be placed.
     * @param int rootRow
     *  The row index of the generating chunk array that represents the center of where the
     *  formation will be placed. 
     */
    private void generateFormation(int[,] formation, int rootCol, int rootRow)
    {
        int colOffset = formation.GetLength(0) / 2;
        int colBound = colOffset + (formation.GetLength(0) % 2);
        int rowOffset = formation.GetLength(1) / 2;
        int rowBound = rowOffset + (formation.GetLength(1) % 2);

        for(int i = -colOffset; i < colBound; i++)
        {
            for(int j = -rowOffset; j < rowBound; j++)
            {
                if(validateChunkBound(rootCol + i, rootRow + j) && formation[i + colOffset, j + rowOffset] != -1)
                {
                    generatingChunk[rootCol + i, rootRow + j] = formation[i + colOffset, j + rowOffset];
                }
            }
        }
    }

    /**
     * Creates a railway in the generating chunk with a chance to spawn a minecart
     * that has a random assortment of valuable resources.
     */
    private void generateRails()
    {
        //Check Spawn conditions
        if(gameManager.getDifficulty() < 2) { return; }
        if(chunksToNextRail > 0)
        {
            chunksToNextRail--;
            return;
        }

        //Decide how many chunks until next rail
        chunksToNextRail = Random.Range(1, 5);

        //Create rail system with 3-5 nodes (coordinates that MUST be crossed by rails)
        int nodeCount = Random.Range(3, 6);
        //Empty array of tuples that will contain the nodes (coordinates)
        var nodes = new (int col, int row)[nodeCount];

        //Start the railway somewhere on the Top Left or Bottom left of the chunk
        nodes[0].col = Random.Range(0, 10);
        nodes[0].row = Random.Range(0, 2) == 1 ? 0 : columns[0].tiles.Length - 1;

        //End the railway somewhere on the Top Right or Bottom right of the chunk
        nodes[nodeCount - 1].col = Random.Range(columns.Length - 10, columns.Length);
        nodes[nodeCount - 1].row = Random.Range(0, 2) == 1 ? 0 : columns[0].tiles.Length - 1;

        //Create intermediate Nodes
        switch (nodeCount)
        {
            case 3:
                nodes[1].col = Random.Range((columns.Length / 2) - 5, (columns.Length / 2) + 5);
                nodes[1].row = Random.Range((columns[0].tiles.Length / 2) - 2, (columns[0].tiles.Length / 2) + 2);
                break;

            case 4:
                nodes[1].col = Random.Range(10, columns.Length / 2);
                nodes[1].row = nodes[0].row == 0 ? Random.Range(1, (columns[0].tiles.Length / 2)) : Random.Range((columns[0].tiles.Length / 2), columns[0].tiles.Length - 1);
                nodes[2].col = Random.Range(columns.Length / 2, columns.Length - 10);
                nodes[2].row = nodes[nodeCount - 1].row == 0 ? Random.Range(1, (columns[0].tiles.Length / 2)) : Random.Range((columns[0].tiles.Length / 2), columns[0].tiles.Length - 1);
                break;

            case 5:
                nodes[1].col = Random.Range(10, (columns.Length / 2) - 5);
                nodes[1].row = nodes[0].row == 0 ? Random.Range(1, (columns[0].tiles.Length / 2) - 2) : Random.Range(columns[0].tiles.Length - 2, (columns[0].tiles.Length / 2) + 2);
                nodes[2].col = Random.Range((columns.Length / 2) - 5, (columns.Length / 2) + 5);
                nodes[2].row = Random.Range((columns[0].tiles.Length / 2) - 2, (columns[0].tiles.Length / 2) + 2);
                nodes[3].col = Random.Range((columns.Length / 2) + 5, columns.Length - 10);
                nodes[3].row = nodes[nodeCount - 1].row == 0 ? Random.Range(1, (columns[0].tiles.Length / 2) - 2) : Random.Range(columns[0].tiles.Length - 2, (columns[0].tiles.Length / 2) + 2);
                break;
        }

        //Print Nodes for Debug
        Debug.Log("Railway Coordinates - (" + nodeCount + ") Nodes");
        for (int i = 0; i < nodeCount; i++)
        {
            Debug.Log("node " + (i + 1) + " :(" + nodes[i].col + "," + nodes[i].row + ")");
        }

        //Initialize empty tuple list of valid spawn locations for the minecart (col,row)
        List<(int, int)> minecartSpawns = new List<(int, int)>();

        //Vars for node connection
        bool shiftHorizontally;
        int horizontalDist;
        int verticalDist;
        int currentCol;
        int currentRow;

        //Connect Nodes
        for (int i = 0; i < nodeCount - 1; i++)
        {
            //Calculate Distance between 2 nodes
            horizontalDist = nodes[i + 1].col - nodes[i].col;
            verticalDist = nodes[i + 1].row - nodes[i].row;

            //Set Current col/row
            currentCol = nodes[i].col;
            currentRow = nodes[i].row;

            //connect nodes with rails
            while (horizontalDist != 0 || verticalDist != 0)
            {
                //attempt to place rail
                if (validateTileSpawn(ConstantLibrary.T_RAIL, currentCol, currentRow))
                {
                    generatingChunk[currentCol, currentRow] = ConstantLibrary.T_RAIL;
                    if (validateTileSpawn(ConstantLibrary.T_MINECART, currentCol, currentRow))
                    {
                        minecartSpawns.Add((currentCol, currentRow));
                    }
                }

                //decide direction to shift. Shift horizontally FIRST when connecting last 2 nodes
                if (i + 1 == nodeCount - 1)
                {
                    //shift horizontally until vertically aligned with next node
                    shiftHorizontally = horizontalDist != 0 ? true : false;
                }
                else
                {
                    //shift vertically until horizontally aligned with next node
                    shiftHorizontally = verticalDist == 0 ? true : false;
                }

                //Move position
                if (shiftHorizontally)
                {
                    //shift horizontally and recalculate distance (always moving right)
                    currentCol += 1;
                    horizontalDist -= 1;
                }
                else
                {
                    //shift vertically and recalculate distance (Can be up OR down, sign dependant)
                    currentRow += System.Math.Sign(verticalDist);
                    verticalDist -= System.Math.Sign(verticalDist);
                }
            }
        }
        //attempt to place Final rail
        if (validateTileSpawn(ConstantLibrary.T_RAIL, nodes[nodeCount - 1].col, nodes[nodeCount - 1].row))
        {
            generatingChunk[nodes[nodeCount - 1].col, nodes[nodeCount - 1].row] = ConstantLibrary.T_RAIL;
        }


        //Place Minecart
        if (minecartSpawns.Count > 0)
        {
            int ind = Random.Range(0, minecartSpawns.Count);
            generatingChunk[minecartSpawns[ind].Item1, minecartSpawns[ind].Item2] = ConstantLibrary.T_MINECART;
        }
    }

    /**
     * Creates a level hazard in the generating chunk based on the biome that the player
     * is currently in.
     */
    private void generateBiomeHazard()
    {
        switch (currentBiome)
        {
            //TESTING WATER AND VINES
            case ConstantLibrary.BIO_DEFAULT:
                generateWater();
                break;

            case ConstantLibrary.BIO_GREEN:
                generateVines();
                generateWater();
                break;

            case ConstantLibrary.BIO_ICE_TRANS:
                generateWater();
                break;

            case ConstantLibrary.BIO_ICE:
                generateWater();
                break;

            case ConstantLibrary.BIO_LAVA:
                generateLava();
                generateExplosiveTiles();
                break;
        }
    }

    /**
     * Creates pools of water in the generating chunk.
     * traversing through water slows the player.
     */
    private void generateWater()
    {
        int generate = 0;
        int spawnCol;
        int spawnRow;
        int formationCode;

        if (gameManager.getDifficulty() >= 3)
        {
            generate = Random.Range(0, 2);
        }

        //Drillpath Water
        if (generate == 1)
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(9, 15);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["water_rand1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["water_rand2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["water_rand3"], spawnCol, spawnRow);
                    break;
            }
        }

        //Top OR Bottom Extra Water (Always picks one or the other)
        generate = Random.Range(0, 2);
        if (generate == 1)
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(3, 9);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["water_randSmall1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["water_randSmall2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["water_randSmall3"], spawnCol, spawnRow);
                    break;
            }
        }
        else
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(15, 22);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["water_randSmall1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["water_randSmall2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["water_randSmall3"], spawnCol, spawnRow);
                    break;
            }
        }
    }

    /**
     * Creates lava pits in the generating chunk.
     * Traversing through lava slows and damages the player.
     */
    private void generateLava()
    {
        int generate = 0;
        int spawnCol;
        int spawnRow;
        int formationCode;

        if (gameManager.getDifficulty() >= 3)
        {
            generate = Random.Range(0, 2);
        }

        //Drillpath Lava
        if (generate == 1)
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(9, 15);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["lava_rand1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["lava_rand2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["lava_rand3"], spawnCol, spawnRow);
                    break;
            }
        }

        //Top OR Bottom Extra lava (Always picks one or the other)
        generate = Random.Range(0, 2);
        if (generate == 1)
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(3, 9);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["lava_randSmall1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["lava_randSmall2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["lava_randSmall3"], spawnCol, spawnRow);
                    break;
            }
        }
        else
        {
            spawnCol = Random.Range(10, columns.Length - 10);
            spawnRow = Random.Range(15, 22);
            formationCode = Random.Range(0, 3);
            switch (formationCode)
            {
                case 0:
                    generateFormation(formations["lava_randSmall1"], spawnCol, spawnRow);
                    break;

                case 1:
                    generateFormation(formations["lava_randSmall2"], spawnCol, spawnRow);
                    break;

                case 2:
                    generateFormation(formations["lava_randSmall3"], spawnCol, spawnRow);
                    break;
            }
        }
    }

    /**
     *  Creates flowing lines of lava in the generating chunk.
     *  Lava Flow lines have rocky spots that make it safe to cross.
     */
    private void generateLavaFlow()
    {
        //early return if not in lava biome
        if(currentBiome != ConstantLibrary.BIO_LAVA) { return; }
        int lavaRow = 0;
        int lavaCol = 0;
        int tilesUntilTurn = 0;
        int bridgeCount = 0;
        int bridgeSpawnIndex = 0;
        int attempts = 0;
        bool drillpathBridgePlaced = false;

        //Generate 3 flowing lava streams per chunk
        for(int i = 0; i < 3; i++)
        {
            //All lava flow starts at the top of the screen
            lavaRow = 0;
            attempts = 0;

            //create start point
            switch (i)
            {
                //leftmost stream start position
                case 0:
                    lavaCol = Random.Range(5, 15);
                    break;

                //middle stream start position
                case 1:
                    lavaCol = Random.Range(columns.Length/2 - 5, columns.Length/2 +5);
                    break;

                //rightmost stream start position
                case 2:
                    lavaCol = Random.Range(columns.Length - 15, columns.Length - 5);
                    break;
            }

            //Initialize empty list of valid bridge spawns
            List<(int, int)> bridgeSpawns = new List<(int, int)>();
            List<(int, int)> bridgeSpawns_drillpath = new List<(int, int)>();

            //Init num of tiles before a turn in the stream can occur
            tilesUntilTurn = Random.Range(3, 6);

            //Init num of bridges to spawn
            bridgeCount = Random.Range(1, 4);
            drillpathBridgePlaced = false;

            //place tiles until flow hits the bottom of the screen or a hole
            while (lavaRow < columns[0].tiles.Length && generatingChunk[lavaCol, lavaRow] != ConstantLibrary.T_HOLE && attempts < 30)
            {
                //Check if this flow tile should actually be a lava pool tile
                if(lavaFlowMergeChecker(lavaCol, lavaRow))
                {
                    generatingChunk[lavaCol, lavaRow] = ConstantLibrary.T_LAVA;
                }
                else
                {
                    //lava flow tile successfully created. Potential lava bridge spawn location.
                    generatingChunk[lavaCol, lavaRow] = ConstantLibrary.T_LAVA_FLOW;
                    if(lavaRow > 9 && lavaRow < 15)
                    {
                        bridgeSpawns_drillpath.Add((lavaCol, lavaRow));
                    }
                    else
                    {
                        bridgeSpawns.Add((lavaCol, lavaRow));
                    }
                }

                //Spread Lava
                if (tilesUntilTurn > 0)
                {
                    //No turn
                    lavaRow++;
                    tilesUntilTurn--;
                }
                else
                {
                    //possible turn
                    tilesUntilTurn = Random.Range(3, 6);
                    int decideDirection = Random.Range(1, 4);
                    switch (decideDirection)
                    {
                        case 1:
                            //no turn
                            lavaRow++;
                            break;

                        case 2:
                            if(lavaFlowValidateTurn(lavaCol, lavaRow, lavaCol + 1))
                            {
                                lavaCol++;
                            }
                            else
                            {
                                lavaRow++;
                            }
                            break;

                        case 3:
                            if (lavaFlowValidateTurn(lavaCol, lavaRow, lavaCol - 1))
                            {
                                lavaCol--;
                            }
                            else
                            {
                                lavaRow++;
                            }
                            break;
                    }
                }

                //Lava Flow Generation Maxed at 30 loops to prevent infinite lopping
                attempts++;
            }

            //place drillpath bridge (if able)
            while(!drillpathBridgePlaced && bridgeSpawns_drillpath.Count > 0)
            {
                bridgeSpawnIndex = Random.Range(0, bridgeSpawns_drillpath.Count);
                if(validateLavaBridgeSpawn(bridgeSpawns_drillpath[bridgeSpawnIndex].Item1, bridgeSpawns_drillpath[bridgeSpawnIndex].Item2))
                {
                    generatingChunk[bridgeSpawns_drillpath[bridgeSpawnIndex].Item1, bridgeSpawns_drillpath[bridgeSpawnIndex].Item2] = ConstantLibrary.T_LAVA_FLOW_BRIDGE;
                    drillpathBridgePlaced = true;
                }
            }

            //Place bridge(s)
            while(bridgeCount > 0 && bridgeSpawns.Count > 0)
            {
                bridgeSpawnIndex = Random.Range(0, bridgeSpawns.Count);
                if(validateLavaBridgeSpawn(bridgeSpawns[bridgeSpawnIndex].Item1, bridgeSpawns[bridgeSpawnIndex].Item2))
                {
                    generatingChunk[bridgeSpawns[bridgeSpawnIndex].Item1, bridgeSpawns[bridgeSpawnIndex].Item2] = ConstantLibrary.T_LAVA_FLOW_BRIDGE;
                    bridgeCount--;
                }
                bridgeSpawns.RemoveAt(bridgeSpawnIndex);
            }
        }
    }

    //Places explosive tiles in the generating chunk
    private void generateExplosiveTiles()
    {
        int oreCount = Random.Range(4, 9); //Amount of explosive ore to spawn in this chunk
        int attempts;   //Limits the number of times a single ore can attempt to spawn
        int spawnSide;  //Decides weather to spawn in the top or bottom of the world
        int spawnCol;   //The column to spawn the gold in
        int spawnRow;   //The row to spawn the gold in

        while (oreCount > 0)
        {
            attempts = 0;

            spawnSide = Random.Range(0, 2);

            while (attempts < 30)
            {
                if (spawnSide == 0)
                {
                    spawnCol = Random.Range(0, columns.Length);
                    spawnRow = Random.Range(4, 10);
                }
                else
                {
                    spawnCol = Random.Range(0, columns.Length);
                    spawnRow = Random.Range(15, 19);
                }

                if (validateTileSpawn(ConstantLibrary.T_EXPLOSIVE, spawnCol, spawnRow))
                {
                    generatingChunk[spawnCol, spawnRow] = ConstantLibrary.T_EXPLOSIVE;
                    break;
                }

                attempts++;
            }

            oreCount--;
        }
    }

    /**
     * Checks if the specified location is a valid spot to spawn a
     * Lava Flow Bridge
     */
    private bool validateLavaBridgeSpawn(int lavaCol, int lavaRow)
    {
        //check tile to the left
        if (validateChunkBound(lavaCol - 1, lavaRow))
        {
            if (generatingChunk[lavaCol - 1, lavaRow] == ConstantLibrary.T_LAVA || generatingChunk[lavaCol - 1, lavaRow] == ConstantLibrary.T_LAVA_FLOW)
            {
                return false;
            }
        }

        //check tile to the right
        if (validateChunkBound(lavaCol + 1, lavaRow))
        {
            if (generatingChunk[lavaCol + 1, lavaRow] == ConstantLibrary.T_LAVA || generatingChunk[lavaCol + 1, lavaRow] == ConstantLibrary.T_LAVA_FLOW)
            {
                return false;
            }
        }

        //valid bridge spawn
        return true;
    }

    /**
     * Checks tiles to the left and right of a generating flow tile
     * to see if it should be converted to a lava pool tile
     */
    private bool lavaFlowMergeChecker(int lavaCol, int lavaRow)
    {
        //check tile to the left
        if(validateChunkBound(lavaCol - 1, lavaRow))
        {
            if(generatingChunk[lavaCol - 1, lavaRow] == ConstantLibrary.T_LAVA)
            {
                return true;
            }
        }

        //check tile to the right
        if (validateChunkBound(lavaCol + 1, lavaRow))
        {
            if (generatingChunk[lavaCol + 1, lavaRow] == ConstantLibrary.T_LAVA)
            {
                return true;
            }
        }

        //No lava pool tiles to the left or right
        return false;
    }

    /**
     * Checks if tile the Lava flow wants to turn to is valid
     */
    private bool lavaFlowValidateTurn(int curCol, int row, int nextCol)
    {
        //return false early if potential turn is out of bounds
        if (!validateChunkBound(nextCol, row)) { return false; }
        
        //check the tile to be overwritten
        int overwriteTile = generatingChunk[nextCol, row];
        if(overwriteTile == ConstantLibrary.T_HOLE || overwriteTile == ConstantLibrary.T_LAVA)
        {
            return false;
        }

        //Dont allow turns if the tile below OR above is a lava pit tile
        if (validateChunkBound(curCol, row + 1))
        {
            if(generatingChunk[curCol, row+1] == ConstantLibrary.T_LAVA)
            {
                return false;
            }
        }
        if (validateChunkBound(curCol, row - 1))
        {
            if (generatingChunk[curCol, row - 1] == ConstantLibrary.T_LAVA)
            {
                return false;
            }
        }

        return true;
    }

    /*
     * Starts/Stops the ash particles from playing
     * Plays if true, Stops if false
     */

    public void playAshParticles(bool playParticles)
    {
        if (playParticles) { particles_ash.Play(); }
        else { particles_ash.Stop(); }
    }

    /**
     * Creates Vines in the generating chunk
     */
    private void generateVines()
    {
        //TODO: Write an algo that determines spawn locations, primary/secondary locations,
        // and how many vines per chunk

        //TEMPORARILY INITIALIZED TO THESE VALUES (FIX LATER)
        int vineRow = 0;
        int vineCol = 0;
        int vineLength = 0;
        int primaryDir = ConstantLibrary.NORTH;
        int secondaryDir = ConstantLibrary.SOUTH;

        for (int vineNum = 0; vineNum < 4; vineNum++)
        {

            switch (vineNum)
            {
                case 0:
                    vineCol = Random.Range(0, 18);
                    secondaryDir = ConstantLibrary.EAST;
                    if(Random.Range(0,2) == 0)
                    {
                        vineRow = Random.Range(0, 7);
                        primaryDir = ConstantLibrary.SOUTH;
                    }
                    else
                    {
                        vineRow = Random.Range(18, columns[0].tiles.Length);
                        primaryDir = ConstantLibrary.NORTH;
                    }
                    break;

                case 1:
                    vineCol = Random.Range(18, 36);
                    secondaryDir = Random.Range(0, 2) == 0 ? ConstantLibrary.EAST : ConstantLibrary.WEST;
                    if (Random.Range(0, 2) == 0)
                    {
                        vineRow = Random.Range(0, 7);
                        primaryDir = ConstantLibrary.SOUTH;
                    }
                    else
                    {
                        vineRow = Random.Range(18, columns[0].tiles.Length);
                        primaryDir = ConstantLibrary.NORTH;
                    }
                    break;

                case 2:
                    vineCol = Random.Range(36, columns.Length);
                    secondaryDir = ConstantLibrary.WEST;
                    if (Random.Range(0, 2) == 0)
                    {
                        vineRow = Random.Range(0, 7);
                        primaryDir = ConstantLibrary.SOUTH;
                    }
                    else
                    {
                        vineRow = Random.Range(18, columns[0].tiles.Length);
                        primaryDir = ConstantLibrary.NORTH;
                    }
                    break;

                case 3:
                    vineRow = Random.Range(10, 16);
                    secondaryDir = Random.Range(0, 2) == 0 ? ConstantLibrary.SOUTH : ConstantLibrary.NORTH;
                    if(Random.Range(0,2) == 0)
                    {
                        vineCol = Random.Range(10, 20);
                        primaryDir = ConstantLibrary.EAST;
                    }
                    else
                    {
                        vineCol = Random.Range(30, 40);
                        primaryDir = ConstantLibrary.WEST;
                    }
                    break;
            }

            vineLength = Random.Range(20, 30);

            // build vine by placing tiles until the specified length is fulfilled
            // or the vine has grown out of the chunk bounds
            while (vineLength > 0 && validateChunkBound(vineCol, vineRow))
            {
                generatingChunk[vineCol, vineRow] = ConstantLibrary.T_VINE;
                vineLength--;
                (int, int) newCoord = spreadVine(vineRow, vineCol, primaryDir, secondaryDir);
                vineRow = newCoord.Item1;
                vineCol = newCoord.Item2;
            }
        }

    }

    /**
     * Finds the next row,col coordinate to spawn a vine based on current coodinate
     * and specified directions [Helper function for generateVines()]
     * 
     * @param int row,col
     *  The row and column coordinates of the current vine tile
     * @param int primaryDir
     *  The primary direction that we would like the vine to grow in
     *  (Constant representing 1 of 4 cardinal directions)
     * @param int secondaryDir
     *  The secondary direction that we would like the vine to grow in
     *  which adds a bit of variety to the primary direction
     *  (Constant representing 1 of 4 cardinal directions)
     *  
     *  @return (int,int)
     *   Tuple holding the (row,column) of the new vine tile
     */
    private (int, int) spreadVine(int vineRow, int vineCol, int primaryDir, int secondaryDir)
    {
        //Pick spread direction with 80% bias towards primary direction
        int chosenDir = (Random.Range(0, 10) < 8) ? primaryDir : secondaryDir;

        switch (chosenDir)
        {
            case ConstantLibrary.NORTH:
                return (vineRow - 1, vineCol);

            case ConstantLibrary.EAST:
                return (vineRow, vineCol + 1);

            case ConstantLibrary.SOUTH:
                return (vineRow + 1, vineCol);

            case ConstantLibrary.WEST:
                return (vineRow, vineCol - 1);
        }

        Debug.LogError("spreadVine() chosenDir was invalid");
        return (vineRow, vineCol);
    }

    /**
     * Replaces the regular ground tiles with icy floor tiles in the generating chunk
     * if the player is currently in the ice biome.
     */
    private void iceReplace()
    {
        if(currentBiome == ConstantLibrary.BIO_ICE)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                for(int j = 0; j < columns[0].tiles.Length; j++)
                {
                    if(generatingChunk[i, j] == ConstantLibrary.T_GROUND)
                    {
                        generatingChunk[i, j] = ConstantLibrary.T_ICE;
                    }
                }
            }
        }
    }

    /**
     * Loop through all tile objects in the scene and assign their 4 neighbors. This occurs once
     * in the world initialization step, then the neighbors are updated each time the tail column
     * of tiles becomes the new head column of tiles in the updateGraph() method.
     */
    private void initializeGraph()
    {
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < columns[0].tiles.Length; j++)
            {
                #region Assign Up Neighbors
                if(j <= 0)
                {
                    columns[i].tiles[j].setNeighborUp(null);
                }
                else
                {
                    columns[i].tiles[j].setNeighborUp(columns[i].tiles[j-1]);
                }
                #endregion

                #region Assign Down Neighbors
                if (j + 1 >= columns[0].tiles.Length)
                {
                    columns[i].tiles[j].setNeighborDown(null);
                }
                else
                {
                    columns[i].tiles[j].setNeighborDown(columns[i].tiles[j+1]);
                }
                #endregion

                #region Assign Right Neighbors
                if(i+1 >= columns.Length)
                {
                    columns[i].tiles[j].setNeighborRight(null);
                }
                else
                {
                    columns[i].tiles[j].setNeighborRight(columns[i + 1].tiles[j]);
                }
                #endregion

                #region Assign Left Neighbors
                if (i == 0)
                {
                    columns[i].tiles[j].setNeighborLeft(null);
                }
                else
                {
                    columns[i].tiles[j].setNeighborLeft(columns[i - 1].tiles[j]);
                }
                #endregion
            }
        }
    }

    /**
     * Loop through all tile objects in the scene and call the updateTileShadeLayer()
     * method on each of them. This is called once during the world initialization step.
     * Tile shade layers are updated on an individual basis after this.
     */
    private void initializeShading()
    {
        for (int i = 0; i < columns.Length; i++)
        {
            for (int j = 0; j < columns[0].tiles.Length; j++)
            {
                columns[i].tiles[j].updateTile();
            }
        }
    }

    /**
     * Loop though all tiles and ensure that every tile sprite has a size different from ALL of its
     * neighbors to 2 decimal points. This is needed because the layer that each tile sprite is
     * rendered on is a function of its size as well as its tileType. In general, larger tiles are
     * rendered on top of smaller tiles. When two tiles are the same type AND the same size, this
     * causes weird artifacting. This method is ONLY called once during the world initialization
     * step. There is no need to change the sizes of the tiles once they are all unique from their
     * neighbors.
     */
    private void forceUniqueTileSizes()
    {
        //Loops though all tiles and ensures that every tile has a size different from its neighbor to 2 decimal points
        for (int i = 1; i < columns.Length - 1; i++)
        {
            for (int j = 1; j < columns[0].tiles.Length - 1; j++)
            {
                while (neighborIdenticalSize(i-1, i , i+1, j-1, j, j + 1))
                {
                    columns[i].tiles[j].randomizeSpriteSize();
                    columns[i].tiles[j].changeTile(columns[i].tiles[j].tileType);
                }
            }
        }

        //Handle case first column
        for(int j = 1; j < columns[0].tiles.Length - 1; j++)
        {
            while (neighborIdenticalSize(columns.Length -1, 0, 1, j - 1, j, j + 1))
            {
                columns[0].tiles[j].randomizeSpriteSize();
                columns[0].tiles[j].changeTile(columns[0].tiles[j].tileType);
            }
        }

        //Handle case last column
        for (int j = 1; j < columns[0].tiles.Length - 1; j++)
        {
            while (neighborIdenticalSize(columns.Length - 2, columns.Length - 1, 0, j - 1, j, j + 1))
            {
                columns[columns.Length - 1].tiles[j].randomizeSpriteSize();
                columns[columns.Length - 1].tiles[j].changeTile(columns[columns.Length - 1].tiles[j].tileType);
            }
        }
    }

    /**
     * Checks if any of neighboring tile sprites have an identical size to the tile in question.
     * Since sprites are attatched to the tile objects, the array referenced is the column array
     * and NOT the chunk arrays.
     * 
     * @param int leftCol
     *  The columns array index of the column left of the tile in question
     * @param int midCol
     *  The columns array index of the tile in question
     * @param int rightCol
     *  The columns array of the column right of the tile in question
     * @param int topRow
     *  The tiles array index of the row above the tile in question
     * @param int midRow
     *  The tiles array index of the tile in question
     * @param int botRow
     *  The tiles array index of the row below the tile in question
     *  
     * @return bool
     *  True if at least 1 neighboring tile in any of the 8 cardinal directions has an equal sprite
     *  size as the tile in question.
     *  False if none of the neighboring tiles have an equal sprite size as the tile in question.
     */
    private bool neighborIdenticalSize(int leftCol, int midCol, int rightCol, int topRow, int midRow, int botRow)
    {
        //Size of the tile in question
        int tileSize = (int)(columns[midCol].tiles[midRow].getSpriteSize() * 100);

        //check NW tile
        if (tileSize == (int)(columns[leftCol].tiles[topRow].getSpriteSize() * 100)) { return true; }

        //check N tile
        if (tileSize == (int)(columns[midCol].tiles[topRow].getSpriteSize() * 100)) { return true; }

        //check NE tile
        if (tileSize == (int)(columns[rightCol].tiles[topRow].getSpriteSize() * 100)) { return true; }

        //check W tile
        if (tileSize == (int)(columns[leftCol].tiles[midRow].getSpriteSize() * 100)) { return true; }

        //check E tile
        if (tileSize == (int)(columns[rightCol].tiles[midRow].getSpriteSize() * 100)) { return true; }

        //check SW tile
        if (tileSize == (int)(columns[leftCol].tiles[botRow].getSpriteSize() * 100)) { return true; }

        //check S tile
        if (tileSize == (int)(columns[midCol].tiles[botRow].getSpriteSize() * 100)) { return true; }

        //check SE tile
        if (tileSize == (int)(columns[rightCol].tiles[botRow].getSpriteSize() * 100)) { return true; }

        //No tiles of identical size
        return false;
    }

    /*
     * Fix the node-edge relationship of the graph when the leftmost column (tail) of tile objects
     * is shifted to the rightmost column (head) of the world.
     */
    private void updateGraph()
    {
        //Identify the column left of the new head. The right-nighbors of tiles in this column
        //used to be null since it used to be the front (rightmost) column of the world.
        int leftOfHead = head - 1;
        if (leftOfHead < 0)
        {
            leftOfHead = columns.Length - 1;
        }

        //Update the columns
        for (int i = 0; i < columns[tail].tiles.Length; i++)
        {
            //Nullify Left of Tail
            columns[tail].tiles[i].setNeighborLeft(null);

            //Nullify Right of head
            columns[head].tiles[i].setNeighborRight(null);

            //Assign head's left neighbors
            columns[head].tiles[i].setNeighborLeft(columns[leftOfHead].tiles[i]);

            //Assign left of head's right neighbors
            columns[leftOfHead].tiles[i].setNeighborRight(columns[head].tiles[i]);
        }
    }

    /**
     * Loop through all tile objects in the scene and run the recalculateMineHP method on each.
     * Used when the player obtains a Mining upgrade that reduces the amount of pickaxe hits it
     * takes to break a tile.
     */
    public void recalculateTileHealth()
    {
        for(int i = 0; i < columns.Length; i++)
        {
            for(int j = 0; j < columns[0].tiles.Length; j++)
            {
                columns[i].tiles[j].recalculateMineHP();
            }
        }
    }

    //Getters & Setters
    public int getHead()
    {
        return head;
    }

    public float getHeadPos()
    {
        return columns[head].getPosX() + 0.5f;
    }

    public int getTail()
    {
        return tail;
    }

    public void setCurrerntBiome(int biomeNumber)
    {
        currentBiome = biomeNumber;
    }

    public int getCurrentBiome()
    {
        return currentBiome;
    }
}
