using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    private bool active;
    private float activeTime = 0.2f;
    private float activeTimer = 0f;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AudioManager>();
    }

    private void Update()
    {
        if (active)
        {
            activeTimer -= Time.deltaTime;
            if(activeTimer <= 0)
            {
                active = false;
                GetComponent<CircleCollider2D>().enabled = false;
                activeTimer = activeTime;
            }
        }
    }

    public void spawnExplosion(Vector3 location)
    {
        GetComponent<Transform>().position = location;
        GetComponent<ParticleSystem>().Play();
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<AudioSource>().volume = 1f * audioManager.getMixedSfx();
        GetComponent<AudioSource>().Play();
        activeTimer = activeTime;
        active = true;
    }
}
