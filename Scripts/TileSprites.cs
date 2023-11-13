using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSprites : MonoBehaviour
{

    public Sprite[] spriteArray;

    public Sprite getSprite(int index)
    {
        if(index > spriteArray.Length - 1)
        {
            return spriteArray[0];
        }
        else
        {
            return spriteArray[index];
        }
    }
}
