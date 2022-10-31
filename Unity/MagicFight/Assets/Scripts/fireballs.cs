using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireballs : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //transform.Rotate(new Vector3(0, 0, 20.0f), Space.Self);
        //transform.position += transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (name == "fireball-e" || name == "fireball-a")
        {
            transform.position += new Vector3(0.01f, 0, 0);
        }
        if (name == "fireball-e-2" || name == "fireball-a-2")
        {
            transform.position += new Vector3(-0.01f, 0, 0);
        }
    }
}
