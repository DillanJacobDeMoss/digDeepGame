using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    //The 3 volume modifiers
    private float volume_master;
    private float volume_music;
    private float volume_sfx;

    [Header("Volume Sliders")]
    public Slider slider_master;
    public Slider slider_music;
    public Slider slider_sfx;

    private bool viewingSliders = false;

    [Header("Save Data")]
    public SaveData saveData;


    // Start is called before the first frame update
    void Start()
    {
        //check for saved volume values
        saveData.checkExistingSoundSettings();

        //Grab values from save file and apply them (default to 100% master & 50% music/sfx)
        volume_master   = ES3.Load<float>("volumeMaster", "saveData.dat");
        volume_music    = ES3.Load<float>("volumeMusic", "saveData.dat");
        volume_sfx      = ES3.Load<float>("volumeSfx", "saveData.dat");
        setSliders();
    }

    public void setSliders()
    {
        slider_master.value = volume_master;
        slider_music.value = volume_music;
        slider_sfx.value = volume_sfx;
    }

    public void setVolumeFromSliders()
    {
        //Must be viewing the sliders page in order to make changes
        if (viewingSliders)
        {
            //Change Volume
            volume_master = slider_master.value;
            volume_music = slider_music.value;
            volume_sfx = slider_sfx.value;

            //Save values
            ES3.Save<float>("volumeMaster", volume_master, "saveData.dat");
            ES3.Save<float>("volumeMusic", volume_music, "saveData.dat");
            ES3.Save<float>("volumeSfx", volume_sfx, "saveData.dat");
        }
    }

    public void setViewingSliders(bool viewing)
    {
        viewingSliders = viewing;
    }


    //Getters
    public float getMixedSfx()
    {
        return volume_sfx * volume_master;
    }

    public float getMixedMusic()
    {
        return volume_music * volume_master;
    }

    public float getRawSfx()
    {
        return volume_sfx;
    }

    public float getRawMusic()
    {
        return volume_music;
    }

    public float getRawMaster()
    {
        return volume_master;
    }
}
