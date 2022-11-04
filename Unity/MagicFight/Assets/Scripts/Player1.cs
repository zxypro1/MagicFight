using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Player1 : MonoBehaviour
{
    public bool isDead = false;
    private GameObject shield;
    private GameObject fireball; // player1 exp
    private GameObject fireball2; // player1 ava
    private GameObject fireball_other; // player1 oppsite
    private Animator anim;
    public int waitTime = 1; // shield show time (second)

    void ResetAnimation()
    {
        anim.SetBool("isLookUp", false);
        anim.SetBool("isRun", false);
        anim.SetBool("isJump", false);
    }

    public void die()
    {
        anim.SetTrigger("die");
    }

    public void idle()
    {
        anim.SetTrigger("idle");
    }

    public void block()
    {
        if (fireball_other.activeSelf)
        {
            fireball_other.GetComponent<fireballs>().isblocked = true;
        }
    }

    /// <summary>
    /// Demo usage of fireballs. Only for test.
    /// Disable this after testing
    /// </summary>
    void test()
    {
        anim.SetTrigger("idle");
        fireball.SetActive(true);
        fireball.transform.position = transform.position;
        fireball.transform.position += getInitPosition();
        fireball2.SetActive(true);
        fireball2.transform.position = transform.position;
        fireball2.transform.position += getInitPosition();
        //pro();
        block();
        //fireball.transform.rotation = Quaternion.Euler(0, 30, 30);
    }

    /// <summary>
    /// Set init position of fireballs.
    /// </summary>
    /// <returns>Vector3</returns>
    private Vector3 getInitPosition()
    {
        return new Vector3(2.5f, 2.5f, 0);
    }

    /// <summary>
    /// Spell Expelliamas
    /// </summary>
    public void exps()
    {
        anim.SetTrigger("attack");
        fireball.GetComponent<fireballs>().onInit();
        fireball.SetActive(true);
        fireball.transform.position = transform.position;
        fireball.transform.position += getInitPosition();
        Debug.Log("EXPS!");
    }

    /// <summary>
    /// Spell Avada Kedavra
    /// </summary>
    public void avad()
    {
        anim.SetTrigger("attack");
        fireball2.SetActive(true);
        fireball2.transform.position = transform.position;
        fireball2.transform.position += getInitPosition();
        Debug.Log("AVA!");
    }

    /// <summary>
    /// Spell Protego
    /// </summary>
    public void pro()
    {
        Debug.Log("PRO!");
        anim.SetTrigger("attack");
        shield.SetActive(true);
        Invoke("closeShield", waitTime);
    }

    private void closeShield()
    {
        Debug.Log("shield close");
        shield.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        shield = transform.GetChild(1).gameObject;
        shield.SetActive(false);
        anim = GetComponent<Animator>();
        fireball = GameObject.Find("fireball-e");
        fireball2 = GameObject.Find("fireball-a");
        fireball_other = GameObject.Find("fireball-e-2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.name == "fireball-e" || collision.name == "fireball-a")
        {
            Debug.Log("is trigger!");
            Debug.Log(collision.name);
            if (shield.activeSelf == false)
            {
                isDead = true;
                anim.SetTrigger("die");
            }
        }
    }
}
