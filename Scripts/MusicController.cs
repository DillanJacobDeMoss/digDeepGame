using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{

    public AudioClip[] tracks;
    public AudioClip caveAmbience;
    public AudioSource musicPlayer;

    private bool gameStarted;

    private int currentTrack;
    private bool loadingTrack;
    private float trackLoadingTime;

    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();

        musicPlayer.clip = caveAmbience;
        musicPlayer.volume = 1f * audioManager.getRawMaster();
        musicPlayer.Play();
        musicPlayer.loop = true;
    }

    // Update is called once per frame
    void Update()
    {

        //Constantly Update Music Volume
        if (gameStarted)
        {
            //Music Vol
            musicPlayer.volume = 0.2f * audioManager.getMixedMusic();
        }
        else
        {
            //Cave Ambience (before Game Start)
            musicPlayer.volume = 1f * audioManager.getRawMaster();
        }

        //Check if song ended
        if (!musicPlayer.isPlaying && !loadingTrack && gameStarted)
        {
            loadingTrack = true;
            trackLoadingTime = 1f;
            selectNextTrack();
        }
    }

    public void startMusic()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            currentTrack = Random.Range(0, tracks.Length);
            musicPlayer.clip = tracks[currentTrack];
            musicPlayer.Play();
            musicPlayer.loop = false;
            musicPlayer.volume = 0.1f;
        }
    }

    private void FixedUpdate()
    {

        //load up next track
        if (loadingTrack)
        {
            trackLoadingTime -= Time.deltaTime;
            if(trackLoadingTime <= 0)
            {
                loadingTrack = false;
                musicPlayer.clip = tracks[currentTrack];
                musicPlayer.Play();
            }
        }
    }

    private void selectNextTrack()
    {
        int nextTrack = Random.Range(0, tracks.Length);
        while(nextTrack == currentTrack)
        {
            nextTrack = Random.Range(0, tracks.Length);
        }

        currentTrack = nextTrack;
    }
}
