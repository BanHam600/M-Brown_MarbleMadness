using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag=="Player" && collision.contacts[0].point.y > transform.position.y)
        { collision.transform.parent = transform; }

    }

    private void OnCollisionExit(Collision collision)

    {
     if(collision.transform.tag=="Player")
            { collision.transform.parent = null; }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
