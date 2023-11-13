using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    public Eyeball[] eyePool;
    private int eyePointer = 0;

    private GameManager gameManager;
    private CheatsManager cheats;
    public World world;

    private float spawnTimer_eyeball;
    private float spawnFrequency_eyeball;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        cheats = GameObject.FindGameObjectWithTag("cheats").GetComponent<CheatsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        #region Eyeball Spawning
        if(spawnTimer_eyeball > 0 && cheats.getSpawnEnemies())
        {
            spawnTimer_eyeball -= Time.deltaTime;
        }

        if(spawnTimer_eyeball <= 0)
        {
            summonEyes();
            spawnFrequency_eyeball = Random.Range(20, 30);
            spawnTimer_eyeball = spawnFrequency_eyeball;
        }
        #endregion
    }

    private void summonEyes()
    {
        int summonLevel = gameManager.getDifficulty() / 2;
        //Max Summon Level is 16
        if(summonLevel >= 16)
        {
            summonLevel = 16;
        }

        //Debug.Log("Spawning Eyes at Summon Level: " + summonLevel);

        switch (summonLevel)
        {
            case 0:
                //Do Nothing
                break;

            case 1:
                spawnEyes(1, 0, 1);                     //1 L1
                break;

            case 2:
                spawnEyes(Random.Range(2, 4), 0, 1);    //2-3 L1
                break;

            case 3:
                spawnEyes(Random.Range(3, 6), 0, 1);    //3-5 L1
                break;

            case 4:
                spawnEyes(Random.Range(1, 3), 0, 1);    //1-2 L1
                spawnEyes(1, 1, 2);                     //1 L2
                break;

            case 5:
                spawnEyes(Random.Range(1, 3), 1, 2);    //1-2 L2
                spawnEyes(1, 0, 1);                     //1 L1
                break;

            case 6:
                spawnEyes(Random.Range(2, 4), 1, 2);    //2-3 L2
                spawnEyes(1, 0, 1);                     //1 L1
                break;

            case 7:
                spawnEyes(3, 1, 2);                     //3 L2
                spawnEyes(Random.Range(1, 3), 0, 1);    //1-2 L1
                break;

            case 8:
                spawnEyes(1, 2, 3);                     //1 L3
                spawnEyes(1, 1, 2);                     //1 L2
                spawnEyes(1, 0, 1);                     //1 L1
                break;

            case 9:
                spawnEyes(Random.Range(1, 3), 2, 3);    //1-2 L3
                spawnEyes(Random.Range(2, 4), 0, 2);    //2-3 L1-L2
                break;

            case 10:
                spawnEyes(Random.Range(2, 4), 2, 3);    //2-3 L3
                spawnEyes(2, 0, 2);                     //2 L1-L2
                break;

            case 11:
                spawnEyes(3, 2, 3);                     //3 L3
                spawnEyes(2, 0, 2);                     //2 L1-L2
                break;

            case 12:
                spawnEyes(1, 3, 4);                     //1 L4
                spawnEyes(1, 1, 3);                     //1 L2-L3
                spawnEyes(Random.Range(1,3), 0, 1);     //1-2 L1
                break;

            case 13:
                spawnEyes(Random.Range(1, 3), 3, 4);    //1-2 L4
                spawnEyes(1, 2, 3);                     //1 L3
                spawnEyes(Random.Range(1, 3), 0, 2);    //1-2 L1-L2
                break;

            case 14:
                spawnEyes(Random.Range(2, 4), 3, 4);    //2-3 L4
                spawnEyes(1, 2, 3);                     //1 L3
                spawnEyes(1, 0, 2);                     //1 L1-L2
                break;

            case 15:
                spawnEyes(Random.Range(3, 5), 3, 4);    //3-4 L4
                spawnEyes(1, 0, 3);                     //1 L1-L3
                break;

            case 16:
                spawnEyes(5, 3, 4);                     //5 L4
                break;
        }
    }

    private void spawnEyes(int amount, int typeRangeLow, int typeRangeHigh)
    {
        //Keep Track of the used vertical positions so you dont spawn them in the same lane
        List<int> usedVerticalPos = new List<int>();

        for(int i = 0; i < amount; i++)
        {
            //Determine Enemy Level
            int type = Random.Range(typeRangeLow, typeRangeHigh);

            //Determine Enemy Spawn Position
            int horizontalOffset = Random.Range(0, 5);
            int verticalPos = Random.Range(2, -3);
            while (usedVerticalPos.Contains(verticalPos))
            {
                verticalPos = Random.Range(2, -3);
            }
            usedVerticalPos.Add(verticalPos);
            Vector3 spawnPosition = new Vector3(world.columns[world.getTail()].getPosX() + horizontalOffset, (float)verticalPos, 0);

            //grab eyeball from pool
            Eyeball eyeball = eyePool[eyePointer];
            eyePointer++;
            if(eyePointer >= eyePool.Length)
            {
                eyePointer = 0;
            }

            //spawn eye
            eyeball.spawnEye(type, spawnPosition);
        }
    }
}
