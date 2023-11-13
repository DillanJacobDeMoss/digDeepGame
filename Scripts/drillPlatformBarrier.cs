using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drillPlatformBarrier : MonoBehaviour
{

    private LayerMask holeMask;
    private Vector2 holeChecKSize = new Vector2(0.1f, 0.1f);
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        holeMask = LayerMask.GetMask("hole");
        player = GameObject.FindGameObjectWithTag("player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Physics2D.OverlapBox(transform.position, holeChecKSize, 0, holeMask) && player.getStandingOnDrillPlatform() && !player.getDodging())
        {
            GetComponent<BoxCollider2D>().enabled = true;
        }
        else
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
