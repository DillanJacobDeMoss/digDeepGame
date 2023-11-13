using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMneu : MonoBehaviour
{
    [Header("High Score Names")]
    public TMP_Text name1;
    public TMP_Text name2;
    public TMP_Text name3;

    [Header("High Scores")]
    public TMP_Text score1;
    public TMP_Text score2;
    public TMP_Text score3;

    [Header("Delete Scores Menu")]
    public GameObject deleteScoresConfirmationScreen;

    [Header("Load Screen")]
    public GameObject loadScreenCanvas;

    private const int SCENE_MAINMENU = 0;
    private const int SCENE_GAME = 1;

    [Header("How To Play")]
    public GameObject howToPlayCanvas;
    public Image howToPlayPage;
    public Sprite[] howToPlayPages;
    public TMP_Text howToPlayPageNumber_text;
    private int howToPlayPageNumber = 0;

    [Header("Settings Pages")]
    public GameObject page_settings;
    public GameObject page_audio;

    private SaveData saveData;

    public Image tutorialArrow;

    private bool changingAudio = false; //only check for changes in auido when viewing sliders
    private float audioStartDelay = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //Load Saved Data
        saveData = GetComponent<SaveData>();
        saveData.checkExistingData();

        //Populate Scoreboard
        loadScoreboard();

        //Tutorial Arrow
        if(saveData.checkTutorialViewed())
        {
            tutorialArrow.color = new Vector4(1f, 1f, 1f, 0f);
        }
        else
        {
            tutorialArrow.color = new Vector4(1f, 1f, 1f, 1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //start music
        if (!GetComponent<AudioSource>().isPlaying)
        {
            audioStartDelay -= Time.deltaTime;
            if(audioStartDelay <= 0)
            {
                GetComponent<AudioSource>().volume = 0.5f * GetComponent<AudioManager>().getMixedMusic();
                GetComponent<AudioSource>().Play();
            }
        }

        //Modify Music Volume
        if (changingAudio)
        {
            GetComponent<AudioSource>().volume = 0.5f * GetComponent<AudioManager>().getMixedMusic();
        }
    }

    private void randomizeScoreboard()
    {
        name1.text = "AAA";
        name2.text = "BBB";
        name3.text = "CCC";

        score1.text = (Random.Range(1000000, 9999999) * 10).ToString();
        score2.text = (Random.Range(100000,  999999)  * 10).ToString();
        score3.text = (Random.Range(10000,   99999)   * 10).ToString();
    }

    private void loadScoreboard()
    {
        name1.text = ES3.Load<string>("name_first", "saveData.dat");
        name2.text = ES3.Load<string>("name_second", "saveData.dat");
        name3.text = ES3.Load<string>("name_third", "saveData.dat");

        int scoreFirst = ES3.Load<int>("score_first", "saveData.dat");
        int scoreSecond = ES3.Load<int>("score_second", "saveData.dat");
        int scoreThird = ES3.Load<int>("score_third", "saveData.dat");

        score1.text = scoreFirst.ToString();
        score2.text = scoreSecond.ToString();
        score3.text = scoreThird.ToString();
    }

    public void startGame()
    {
        loadScreenCanvas.SetActive(true);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SCENE_GAME);
    }

    public void openDeleteScoresConfirmation()
    {
        deleteScoresConfirmationScreen.SetActive(true);
    }

    public void rejectDeleteScores()
    {
        deleteScoresConfirmationScreen.SetActive(false);
    }

    public void confirmDeleteScores()
    {
        saveData.deleteExistingData();
        loadScoreboard();
        deleteScoresConfirmationScreen.SetActive(false);
        tutorialArrow.color = new Vector4(1f, 1f, 1f, 1f);
    }

    public void changeHowToPlayPage(int amount)
    {
        ES3.Save("tutorialViewed", true, "saveData.dat");
        tutorialArrow.color = new Vector4(1f, 1f, 1f, 0f);
        howToPlayPageNumber += amount;
        if(howToPlayPageNumber > 3)
        {
            howToPlayPageNumber = 0;
        }
        updateHowToPlayPage();
    }

    private void updateHowToPlayPage()
    {
        howToPlayPageNumber_text.text = howToPlayPageNumber.ToString();
        if (howToPlayPageNumber == 0)
        {
            howToPlayCanvas.SetActive(false);
        }
        else
        {
            howToPlayCanvas.SetActive(true);
            howToPlayPage.sprite = howToPlayPages[howToPlayPageNumber -1];
        }
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void settingsPageChange_main()
    {
        disableSettingsPages();
        page_settings.SetActive(true);
    }

    public void settingsPageChange_audio()
    {
        disableSettingsPages();
        page_audio.SetActive(true);
        changingAudio = true;
    }

    public void disableSettingsPages()
    {
        changingAudio = false;
        page_audio.SetActive(false);
        page_settings.SetActive(false);
    }
}
