using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    [Header("Inventory Slot Items")]
    //Inventory Management
    public Item slot_empty;
    public Item slot_locked;
    private Item[] inventory;
    private int unlockedSlots = 0;
    private int focusedSlot = 0;
    private bool touchingItem;

    private InputSystem controls;
    private GameManager gameManager;

    [Header("Sprites for Inventory HUD")]
    public Sprite[] itemSprites;
    public Sprite focusuedSlotIcon;
    public Sprite unFocusedSlotIcon;
    public Sprite blank;

    //Sprite Codes for the Invetory Script's Sprite array 
    private const int S_FUEL = 0;
    private const int S_GOLD = 1;
    private const int S_AMATHYST = 2;
    private const int S_DIAMOND = 3;
    private const int S_ASSORTMENT = 4;

    [Header("Inventory UI Elements")]
    public GameObject[] slots;
    public Image[] itemIcons;


    // Start is called before the first frame update
    void Start()
    {
        inventory = new Item[] { slot_empty, slot_locked, slot_locked, slot_locked };
        controls = GameObject.FindGameObjectWithTag("GameManager").GetComponent<InputSystem>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

        //ITEM COLLECTION AND DROPPING
        if(gameManager.getPlayerLocationNode() != null)
        {
            if (gameManager.getPlayerLocationNode().getContainsItem())
            {
                touchingItem = true;
            }
            else
            {
                touchingItem = false;
            }
        }
        else
        {
            touchingItem = false;
        }

        if(!touchingItem && controls.interact())
        {
            dropItem();
        }



        //MOUSE & KEYBOARD INVENTORY SCROLLING
    }

    //returns true if item was successfully added to inventory
    public bool insertItem(Item item)
    {
        for(int i = 0; i < inventory.Length; i++)
        {
            if(inventory[i].itemType == ConstantLibrary.I_EMPTY)
            {
                inventory[i] = item;
                updateInventoryUI();
                //Debug.Log("Added " + item.itemType + " to slot " + i);
                return true;
            }
        }
        Debug.Log("Inventory Full");
        return false;
    }

    public void expandInventory()
    {
        for(int i = 0; i < inventory.Length; i++)
        {
            if(inventory[i].itemType == ConstantLibrary.I_LOCKED)
            {
                inventory[i] = slot_empty;
                slots[i].GetComponent<Image>().sprite = unFocusedSlotIcon;
                slots[i].GetComponent<Button>().enabled = true;
                unlockedSlots++;
                return;
            }
        }
        Debug.Log("Expand Failed: Inventory Maximized");
    }

    public void dropItem()
    {

        Item item = inventory[focusedSlot];

        if(item.itemType != ConstantLibrary.I_EMPTY)
        {

            inventory[focusedSlot] = slot_empty;
            
            //Shift Inventory
            for (int i = focusedSlot; i+1 < inventory.Length; i++)
            {
                if(inventory[i+1].itemType != ConstantLibrary.I_LOCKED)
                {
                    inventory[i] = inventory[i + 1];
                    inventory[i + 1] = slot_empty;
                }
            }

            updateInventoryUI();


            //Move new item to playspace and update it
            Item newItem = gameManager.getItem(item.itemType);
            newItem.teleport(gameManager.getPlayerLocationNode().transform.position);
            newItem.setOccupiedTile(gameManager.getPlayerLocationNode());
            gameManager.getPlayerLocationNode().setContainsItem(true);
            newItem.setSummonTimer(1f);
            newItem.setActive(true);
            newItem.setGrabbed(false);
        }
    }

    private void updateInventoryUI()
    {
        for(int i = 0; i < inventory.Length; i++)
        {
            int itemType = inventory[i].itemType;
            switch (itemType)
            {
                case ConstantLibrary.I_EMPTY:
                    itemIcons[i].sprite = blank;
                    break;

                case ConstantLibrary.I_FUEL:
                    itemIcons[i].sprite = itemSprites[S_FUEL];
                    break;

                case ConstantLibrary.I_GOLD:
                    itemIcons[i].sprite = itemSprites[S_GOLD];
                    break;

                case ConstantLibrary.I_AMATHYST:
                    itemIcons[i].sprite = itemSprites[S_AMATHYST];
                    break;

                case ConstantLibrary.I_DIAMOND:
                    itemIcons[i].sprite = itemSprites[S_DIAMOND];
                    break;

                case ConstantLibrary.I_ASSORTMENT:
                    itemIcons[i].sprite = itemSprites[S_ASSORTMENT];
                    break;
            }
        }
    }

    public void setFoucusedSlot(int slot)
    {
        //catch invalid swaps
        if(slot > unlockedSlots)
        {
            return;
        }

        focusedSlot = slot;
        updateInventoryFocus();
    }

    public void scrollFocusedSlot(int direction)
    {
        focusedSlot += direction;
        if(focusedSlot > unlockedSlots)
        {
            focusedSlot = 0;
        }
        if(focusedSlot < 0)
        {
            focusedSlot = unlockedSlots;
        }

        updateInventoryFocus();
    }

    private void updateInventoryFocus()
    {
        for(int i = 0; i < inventory.Length; i++)
        {
            //exit once you hit a locked slot
            if(inventory[i].itemType == ConstantLibrary.I_LOCKED)
            {
                return;
            }

            if(i == focusedSlot)
            {
                slots[i].GetComponent<Image>().sprite = focusuedSlotIcon;
            }
            else
            {
                slots[i].GetComponent<Image>().sprite = unFocusedSlotIcon;
            }
        }
    }
}
