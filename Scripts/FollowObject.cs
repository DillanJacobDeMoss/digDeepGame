using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{

    public GameObject objectToFollow;
    public float offsetVertical;
    public float offsetHorizontal;

    private Transform objectTransform;


    // Start is called before the first frame update
    void Start()
    {
        objectTransform = objectToFollow.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 position = new Vector2(objectTransform.position.x + offsetHorizontal, objectTransform.position.y + offsetVertical);
        transform.position = position;
    }
}
