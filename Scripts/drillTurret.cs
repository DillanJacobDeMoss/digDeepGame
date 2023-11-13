using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drillTurret : MonoBehaviour
{

    public Transform playerMountPosition;

    [Header("Sprites")]
    public Sprite[] turretSprites;
    /*  0 = Idle
     *  1 = Focused
     *  2 = In-use
     */
    public SpriteRenderer spriteRenderer;

    private bool inInteractRange = false;
    private Vector2 checkSize = new Vector2(0.5f, 0.5f);
    private LayerMask layerMask_player;


    private bool inUse = false;


    //Managers
    private InputSystem controls;
    private Player player;


    // Start is called before the first frame update
    void Start()
    {
        controls = GameObject.FindGameObjectWithTag("GameManager").GetComponent<InputSystem>();
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Player>();

        layerMask_player = LayerMask.GetMask("player");
    }

    // Update is called once per frame
    void Update()
    {
        if (inUse)
        {

            //ADD SHOOTING MECHANICS FROM PLAYER SCRIPT

            //Orient Turret
            GetComponent<Transform>().right = controls.getMousePos() - GetComponent<Rigidbody2D>().position;

            //check for leaving the turret
            if (controls.interact()) { exitTurret(); }

            //force plyer to the center
            player.transform.position = playerMountPosition.position;

        }
        else
        {

            //detect if player is close enough to get on the turret
            inInteractRange = Physics2D.OverlapBox(GetComponent<Transform>().position, checkSize, 0, layerMask_player);

            //Set the highlighted/un-highlighted sprites
            spriteRenderer.sprite = inInteractRange ? turretSprites[1] : turretSprites[0];

            //check for entering the turret
            if (controls.interact() && inInteractRange) { enterTurret(); }
        }
    }

    private void enterTurret()
    {
        inUse = true;

        //set sprite
        spriteRenderer.sprite = turretSprites[2];

        //Disable player movements? (or have player check if its in the turret)
    }

    private void exitTurret()
    {
        inUse = false;

        //re-enable player movements?
    }

    public bool getInUse() { return inUse; }
}
