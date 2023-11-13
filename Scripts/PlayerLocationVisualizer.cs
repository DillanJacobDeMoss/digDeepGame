using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocationVisualizer : MonoBehaviour
{

    public GameManager gameManager;
    private bool toolActive = false;

    // Update is called once per frame
    void Update()
    {
        //Toggle Tool
        if (Input.GetKeyDown(KeyCode.H))
        {
            toolActive = !toolActive;
            GetComponent<SpriteRenderer>().enabled = toolActive;
        }

        if (!gameManager.loadScreen.activeSelf && toolActive)
        {
        GetComponent<Transform>().position = gameManager.getPlayerLocationNode().transform.position;
        }
    }
}
