using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteContainer : MonoBehaviour
{

    public Sprite missingTexture;
    public Sprite[] sprites;
    
    public bool validateSpriteIndex(int index)
    {
        return index >= sprites.Length ? false : true;
    }

    public Sprite getSprite(int index)
    {
        if (validateSpriteIndex(index))
        {
            return sprites[index];
        }
        else
        {
            Debug.LogError("Tried to Access Sprite Index Out of Bounds");
            return missingTexture;
        }
    }
}
