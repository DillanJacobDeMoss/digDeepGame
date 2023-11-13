using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{

    public Tile[] tiles;
    private World world;

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.FindGameObjectWithTag("World").GetComponent<World>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void shiftColumn(int amount)
    {
        GetComponent<Transform>().position = new Vector3(getPosX() + amount, 0f, 0f);
    }

    public float getPosX()
    {
        return GetComponent<Transform>().position.x;
    }

    public void renderDrillPath()
    {
        tiles[10].setTreadMarks(true);
        tiles[14].setTreadMarks(true);
        for (int i = 10; i < 15; i++)
        {
            if (!tiles[i].checkTileForTypes(tiles[i], ConstantLibrary.typeCheck_drillPathImmune))
            {
                if(tiles[i].tileType == ConstantLibrary.T_VINE)
                {
                    tiles[i].changeTile(ConstantLibrary.T_VINE_GROWING);
                }
                else if(world.getCurrentBiome() == ConstantLibrary.BIO_ICE)
                {
                    tiles[i].changeTile(ConstantLibrary.T_ICE);
                }
                else
                {
                    tiles[i].changeTile(ConstantLibrary.T_GROUND);
                }
            }
        }

        //update all tiles touching drillpath
        for(int i = 9; i < 16; i++)
        {
            tiles[i].updateTile();
        }
    }

    public void loadColumn(int[,] columnData, int column)
    {
        tiles[10].setTreadMarks(false);
        tiles[14].setTreadMarks(false);

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].changeTile(columnData[column,i]);
            tiles[i].setContainsItem(false);
        }
    }
}
