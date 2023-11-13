using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    private GameManager gameManager;
    private Inventory inventory;
    private PauseMenu pauseMenu;

    public Camera camera;
    private Vector2 mousePos;

    #region Input Booleans
    private bool input_up;
    private bool input_down;
    private bool input_left;
    private bool input_right;
    private bool input_interact;
    private bool input_mining;
    private bool input_shooting;
    private bool input_reload;
    private bool input_dodge;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        inventory = GetComponent<Inventory>();
        pauseMenu = GetComponent<PauseMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        #region PC controls
        if (gameManager.pc && !pauseMenu.getPaused())
        {
            //Up
            input_up = (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S));

            //Down
            input_down = (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W));

            //Left
            input_left = (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D));

            //Right
            input_right = (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A));

            //Mining
            input_mining = (Input.GetMouseButton(0) && !Input.GetMouseButton(1));

            //Shooting
            input_shooting = (Input.GetMouseButton(1) && !Input.GetMouseButton(0));

            //Reload
            input_reload = (Input.GetKeyDown(KeyCode.R));

            //Dodge
            input_dodge = (Input.GetKeyDown(KeyCode.LeftShift));

            //Interact
            input_interact = (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E));

            //Inventory Num Key Selection
            if (Input.GetKeyDown(KeyCode.Alpha1)) { inventory.setFoucusedSlot(0); }

            if (Input.GetKeyDown(KeyCode.Alpha2)) { inventory.setFoucusedSlot(1); }

            if (Input.GetKeyDown(KeyCode.Alpha3)) { inventory.setFoucusedSlot(2); }

            if (Input.GetKeyDown(KeyCode.Alpha4)) { inventory.setFoucusedSlot(3); }

            //Inventory Scrolling
            if (Input.mouseScrollDelta.y != 0) { inventory.scrollFocusedSlot(-1 * (int)Input.mouseScrollDelta.y); }

            //Tracking Mouse Position
            mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        }

        #endregion

        #region Mobile controls

        #endregion
    }

    public bool up() { return input_up; }

    public bool down() { return input_down; }

    public bool left() { return input_left; }

    public bool right() { return input_right; }

    public bool mining() { return input_mining; }

    public bool shooting() { return input_shooting; }

    public bool reload() { return input_reload; }

    public bool dodge() { return input_dodge; }

    public bool interact() { return input_interact; }

    public Vector2 getMousePos() { return mousePos; }
}
