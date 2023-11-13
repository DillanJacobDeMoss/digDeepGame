using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{

    private bool paused = false;

    [Header("Menu Pages")]
    public GameObject pauseCanvas;
    public GameObject mainPage;
    public GameObject quitPage;
    public GameObject settingsPage;
    public GameObject audioPage;
    public GameObject controlsPage;

    private UpgradeManager upgradeManager;
    private AudioManager audioManager;
    private CheatsManager cheatsManager;

    // Start is called before the first frame update
    void Start()
    {
        upgradeManager = GetComponent<UpgradeManager>();
        audioManager = GetComponent<AudioManager>();
        cheatsManager = GameObject.FindGameObjectWithTag("cheats").GetComponent<CheatsManager>();

        //initialize pause menu state
        disablePages();
        pageChange_main();
        pauseCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //toggle Pause on escape key
        if (Input.GetKeyDown(KeyCode.Escape) && !cheatsManager.cheatsCanvas.activeSelf)
        {
            Debug.Log("escape Pressed");
            if(upgradeManager.getPendingLevelUps() <= 0)
            {
                togglePause();
            }
        }
    }

    public void togglePause()
    {
        paused = !paused;
        if (paused) { pause(); }
        else { unPause(); }
    }

    private void pause()
    {
        Time.timeScale = 0;
        pauseCanvas.SetActive(true);
    }

    private void unPause()
    {
        Time.timeScale = 1f;
        pageChange_main();
        pauseCanvas.SetActive(false);
    }

    public bool getPaused()
    {
        return paused;
    }

    public void disablePages()
    {
        mainPage.SetActive(false);
        quitPage.SetActive(false);
        settingsPage.SetActive(false);
        audioPage.SetActive(false);
        audioManager.setViewingSliders(false);
        controlsPage.SetActive(false);
    }

    //Page Changes
    public void pageChange_main()
    {
        disablePages();
        mainPage.SetActive(true);
    }

    public void pageChange_quit()
    {
        disablePages();
        quitPage.SetActive(true);
    }

    public void pageChange_settings()
    {
        disablePages();
        settingsPage.SetActive(true);
    }

    public void pageChange_audio()
    {
        disablePages();
        audioPage.SetActive(true);
    }

    public void pageChange_controls()
    {
        disablePages();
        controlsPage.SetActive(true);
    }
}
