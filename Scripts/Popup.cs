using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{

    public Sprite[] popups;

    public void spawnPopup(int popupCode, Vector3 spawnLocation)
    {
        GetComponent<SpriteRenderer>().sprite = popups[popupCode];
        transform.position = spawnLocation;
        GetComponent<Animator>().SetTrigger("spawn");
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 2f);
    }

    public void stopMovement()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
    }
}
