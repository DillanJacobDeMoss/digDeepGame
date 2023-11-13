using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        checkExistingData();
    }

    public void checkExistingData()
    {
        checkExistingScores();
        checkTutorialViewed();
        checkExistingSoundSettings();
    }

    public void deleteExistingData()
    {
        resetScoreboard();
        ES3.Save("tutorialViewed", false, "saveData.dat");
    }

    private void checkExistingScores()
    {
        //Score Entry for First Place
        if (!ES3.KeyExists("score_first", "saveData.dat"))
        {
            ES3.Save("score_first", 0, "saveData.dat");
        }
        if (!ES3.KeyExists("name_first", "saveData.dat"))
        {
            ES3.Save("name_first", "___", "saveData.dat");
        }

        //Score Entry for Second Place
        if (!ES3.KeyExists("score_second", "saveData.dat"))
        {
            ES3.Save("score_second", 0, "saveData.dat");
        }
        if (!ES3.KeyExists("name_second", "saveData.dat"))
        {
            ES3.Save("name_second", "___", "saveData.dat");
        }

        //Score Entry for Third Place
        if (!ES3.KeyExists("score_third", "saveData.dat"))
        {
            ES3.Save("score_third", 0, "saveData.dat");
        }
        if (!ES3.KeyExists("name_third", "saveData.dat"))
        {
            ES3.Save("name_third", "___", "saveData.dat");
        }
    }

    public bool checkTutorialViewed()
    {
        if(!ES3.KeyExists("tutorialViewed", "saveData.dat"))
        {
            ES3.Save("tutorialViewed", false, "saveData.dat");
        }

        return ES3.Load<bool>("tutorialViewed", "saveData.dat");
    }

    public void checkExistingSoundSettings()
    {
        //check for master volume value (default 100%)
        if(!ES3.KeyExists("volumeMaster", "saveData.dat"))
        {
            ES3.Save<float>("volumeMaster", 1.0f, "saveData.dat");
        }

        //check for music volume value (default 50%)
        if (!ES3.KeyExists("volumeMusic", "saveData.dat"))
        {
            ES3.Save<float>("volumeMusic", 1.0f, "saveData.dat");
        }

        //check for sfx volume value (default 50%)
        if (!ES3.KeyExists("volumeSfx", "saveData.dat"))
        {
            ES3.Save<float>("volumeSfx", 1.0f, "saveData.dat");
        }

    }

    private void resetScoreboard()
    {
        ES3.Save("score_first", 0, "saveData.dat");
        ES3.Save("name_first", "___", "saveData.dat");
        ES3.Save("score_second", 0, "saveData.dat");
        ES3.Save("name_second", "___", "saveData.dat");
        ES3.Save("score_third", 0, "saveData.dat");
        ES3.Save("name_third", "___", "saveData.dat");
    }

    public void updateHighScores(int totalScore, string name)
    {

        int scoreRank = compareToHighScores(totalScore);

        switch (scoreRank)
        {
            case 1:
                //new Third becomes old Second
                ES3.Save("score_third", ES3.Load("score_second", "saveData.dat"), "saveData.dat");
                ES3.Save("name_third", ES3.Load("name_second", "saveData.dat"), "saveData.dat");

                //new Second becomes old First
                ES3.Save("score_second", ES3.Load("score_first", "saveData.dat"), "saveData.dat");
                ES3.Save("name_second", ES3.Load("name_first", "saveData.dat"), "saveData.dat");

                //new highscore in first place
                ES3.Save("score_first", totalScore, "saveData.dat");
                ES3.Save("name_first", name, "saveData.dat");

                break;

            case 2:
                //new Third becomes old Second
                ES3.Save("score_third", ES3.Load("score_second", "saveData.dat"), "saveData.dat");
                ES3.Save("name_third", ES3.Load("name_second", "saveData.dat"), "saveData.dat");

                //new Second
                ES3.Save("score_second", totalScore, "saveData.dat");
                ES3.Save("name_second", name, "saveData.dat");

                break;

            case 3:
                //new third
                ES3.Save("score_third", totalScore, "saveData.dat");
                ES3.Save("name_third", name, "saveData.dat");

                break;
        }

    }

    public int compareToHighScores(int totalScore)
    {
        //Check how this new score compares to previos records
        int scoreRank = 0;

        if (totalScore > ES3.Load<int>("score_first", "saveData.dat"))
        {
            scoreRank = 1;
        }
        else if (totalScore > ES3.Load<int>("score_second", "saveData.dat"))
        {
            scoreRank = 2;
        }
        else if (totalScore > ES3.Load<int>("score_third", "saveData.dat"))
        {
            scoreRank = 3;
        }

        return scoreRank;
    }
}
