using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public int itemType;

    public Sprite[] itemSprites;

    private AudioSource audioSource;

    public AudioClip sfx_pickup; //Sound when item is placed in player's inventory
    public AudioClip sfx_drillPlat; //Sound when item is placed on the drill platform

    private bool grabable = false;
    private bool grabbed = false;
    private bool activeInPlayspace = false;

    private bool pickupAnimActive = false;
    private float pickupAnim_timer = 0f;

    private Tile occupiedTile;

    private float summonTimer = 0f; //Timer that holds back interactability after being summoned into playspace

    private InputSystem controls;
    private GameManager gameManager;
    private Inventory inventory;
    private AudioManager audioManager;

    void Start()
    {
        controls = GameObject.FindGameObjectWithTag("GameManager").GetComponent<InputSystem>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        inventory = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Inventory>();
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (activeInPlayspace)
        {
            if(gameManager.getPlayerLocationNode() == occupiedTile && summonTimer <= 0)
            {
                grabable = true;
            }
            else
            {
                grabable = false;
            }
        }
        else
        {
            grabable = false;
        }

        //Grabbing Item

        /*
         *  TO TURN OFF/ON AUTO PICKUP ADD/REMOVE "&& controls.interact()" to the following if statement
         */
        if (grabable && !grabbed)
        {
            grabbed = inventory.insertItem(this);
            if (grabbed)
            {
                setActive(false);
                occupiedTile.setContainsItem(false);
                audioSource.volume = 0.2f * audioManager.getMixedSfx();
                audioSource.clip = sfx_pickup;
                audioSource.Play();
                playAnimation();
            }
        }

        if (pickupAnimActive)
        {
            pickupAnim_timer -= Time.deltaTime;
            if(pickupAnim_timer <= 0f)
            {
                pickupAnimActive = false;
                grabbed = false;
                teleport(new Vector2(transform.position.x, -20f));
                GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Ground");
            }
        }
    }

    private void FixedUpdate()
    {
        if (activeInPlayspace && summonTimer > 0)
        {
            summonTimer -= Time.fixedDeltaTime;
        }
    }

    private void collectItem()
    {
        Debug.Log("Collected " + itemType);

        audioSource.volume = 1f * audioManager.getMixedSfx();
        audioSource.clip = sfx_drillPlat;
        audioSource.Play();

        setSummonTimer(0f);

        switch (itemType)
        {
            case ConstantLibrary.I_FUEL:
                gameManager.changeFuel(25);
                gameManager.incrementScore_minerals(1);
                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_FUEL, transform.position);
                break;

            case ConstantLibrary.I_GOLD:
                gameManager.incrementGold(10);
                gameManager.incrementScore_minerals(5);
                if (gameManager.getFuel() >= 100)
                {
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD10_X2, transform.position);
                }
                else
                {
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD10, transform.position);
                }
                break;

            case ConstantLibrary.I_AMATHYST:
                gameManager.incrementGold(30);
                gameManager.incrementScore_minerals(10);
                if (gameManager.getFuel() >= 100)
                {
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD30_X2, transform.position);
                }
                else
                {
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD30, transform.position);
                }
                break;

            case ConstantLibrary.I_DIAMOND:
                gameManager.incrementGold(10);
                gameManager.incrementScore_minerals(100);
                if(gameManager.getFuel() >= 100)
                {
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_POINTS_X2, transform.position);
                    Vector3 offset = new Vector3(3f, -1f, 0f);
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD10_X2, transform.position - offset);
                }
                else
                {
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_POINTS, transform.position);
                    Vector3 offset = new Vector3(3f, -1f, 0f);
                    gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD10, transform.position - offset);
                }
                break;

            case ConstantLibrary.I_ASSORTMENT:
                Vector3[] popupLocations = {
                    new Vector3(transform.position.x + 2, transform.position.y + 2, 0f),
                    new Vector3(transform.position.x - 2, transform.position.y + 1, 0f),
                    new Vector3(transform.position.x - 2, transform.position.y - 1, 0f),
                    new Vector3(transform.position.x + 2, transform.position.y - 2, 0f),
                };
                int randomItem;

                for(int i = 0; i < 4; i++)
                {
                    randomItem = Random.Range(0, 4);
                    switch (randomItem)
                    {
                        //fuel
                        case 0:
                            gameManager.changeFuel(25);
                            gameManager.incrementScore_minerals(1);
                            gameManager.getPopup().spawnPopup(ConstantLibrary.POP_FUEL, popupLocations[i]);
                            break;

                        //gold
                        case 1:
                            gameManager.incrementGold(10);
                            gameManager.incrementScore_minerals(5);
                            if (gameManager.getFuel() >= 100)
                            {
                                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD10_X2, popupLocations[i]);
                            }
                            else
                            {
                                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD10, popupLocations[i]);
                            }
                            break;

                        //amathyst
                        case 2:
                            gameManager.incrementGold(30);
                            gameManager.incrementScore_minerals(10);
                            if (gameManager.getFuel() >= 100)
                            {
                                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD30_X2, popupLocations[i]);
                            }
                            else
                            {
                                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_GOLD30, popupLocations[i]);
                            }
                            break;

                        //diamond
                        case 3:
                            gameManager.incrementScore_minerals(100);
                            if (gameManager.getFuel() >= 100)
                            {
                                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_POINTS_X2, popupLocations[i]);
                            }
                            else
                            {
                                gameManager.getPopup().spawnPopup(ConstantLibrary.POP_POINTS, popupLocations[i]);
                            }
                            break;
                    }
                }
                break;
        }

        teleport(new Vector2(transform.position.x, -20f));
    }

    public void setActive(bool active)
    {
        activeInPlayspace = active;
        GetComponent<Animator>().SetBool("active", active);
    }

    public void setSummonTimer(float time)
    {
        summonTimer = time;
    }

    public void setGrabbed(bool grab)
    {
        grabbed = grab;
    }

    public void teleport(Vector2 location)
    {
        transform.position = location;
    }

    public void setOccupiedTile(Tile tile)
    {
        occupiedTile = tile;
    }

    public Tile getOccupiedTile()
    {
        return occupiedTile;
    }


    private void OnTriggerEnter2D(Collider2D col)
    { 
        if(col.tag == "drillPlatform" && activeInPlayspace)
        {
            activeInPlayspace = false;
            occupiedTile.setContainsItem(false);
            collectItem();
        }
    }

    public bool getActiveInPlayspace()
    {
        return activeInPlayspace;
    }

    public void setItemProperties(int itemCode)
    {
        itemType = itemCode;
        GetComponent<SpriteRenderer>().sprite = itemSprites[itemType];
    }

    private void playAnimation()
    {
        GetComponent<Animator>().SetTrigger("pickedUp");
        pickupAnimActive = true;
        pickupAnim_timer = 1f;
        GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("HUD");
    }
}
