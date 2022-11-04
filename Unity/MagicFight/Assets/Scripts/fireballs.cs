using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireballs : MonoBehaviour
{
    public float speed;
    public bool isblocked = false;
    // Start is called before the first frame update
    private Quaternion initPosition;
    void Start()
    {
        initPosition = transform.rotation;
    }

    public void onInit()
    {
        isblocked = false;
        transform.rotation = initPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * speed, Space.Self);
        if (isblocked)
        {
            transform.rotation *= Quaternion.AngleAxis(0.1f, Vector3.forward); // 绕z轴旋转
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Wizard" || collision.name == "Wizard2")
        {
            gameObject.SetActive(false);
        }
    }
}
