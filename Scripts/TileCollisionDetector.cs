using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCollisionDetector : MonoBehaviour
{

    public Tile parentTile;


    private void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.tag)
        {
            //Tile hit by laser
            case "laser":
                switch (parentTile.tileType)
                {
                    //make vines shootable later
                    case ConstantLibrary.T_VINE:
                        break;

                    case ConstantLibrary.T_EXPLOSIVE:
                        parentTile.changeMineHP(-1);
                        break;
                }
                break;
        }
    }

}
