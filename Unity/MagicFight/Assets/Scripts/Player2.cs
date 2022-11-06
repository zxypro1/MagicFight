using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.HealthSystemCM;

public class Player2 : MonoBehaviour
{
    public bool isDead = false;
    private GameObject shield;
    private GameObject fireball; // player1 exp
    private GameObject fireball2; // player1 ava
    private GameObject fireball_other; // player1 oppsite
    private Animator anim;
    public int waitTime = 1; // shield show time (second)
    [SerializeField] private GameObject getHealthSystemGameObject;
    HealthSystem healthSystem;

    void ResetAnimation()
    {
        anim.SetBool("isLookUp", false);
        anim.SetBool("isRun", false);
        anim.SetBool("isJump", false);
    }

    public void block()
    {
        if (fireball_other.activeSelf)
        {
            fireball_other.GetComponent<fireballs>().isblocked = true;
        }
    }

    public void die()
    {
        anim.SetTrigger("die");
    }

    public void idle()
    {
        anim.SetTrigger("idle");
        isDead = false;
    }


    /// <summary>
    /// Demo usage of fireballs. Only for test.
    /// Disable this after testing
    /// </summary>
    public void test()
    {
        anim.SetTrigger("idle");
        fireball.GetComponent<fireballs>().onInit();
        fireball.SetActive(true);
        fireball.transform.position = transform.position;
        fireball.transform.position += getInitPosition();
        fireball2.SetActive(true);
        fireball2.transform.position = transform.position;
        fireball2.transform.position += getInitPosition();
        //pro();
        //block();
        //fireball.transform.rotation = Quaternion.Euler(0, 30, 30);
    }

    /// <summary>
    /// Set init position of fireballs.
    /// </summary>
    /// <returns>Vector3</returns>
    private Vector3 getInitPosition()
    {
        return new Vector3(-2.5f, 2f, 0);
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
        healthSystem.Heal(20);
    }

    /// <summary>
    /// Spell Avada Kedavra
    /// </summary>
    public void avad()
    {
        Debug.Log("AVA!");
        if (healthSystem.GetHealth() == 100f)
        {
            anim.SetTrigger("attack");
            fireball2.SetActive(true);
            fireball2.transform.position = transform.position;
            fireball2.transform.position += getInitPosition();
            healthSystem.Damage(100);
        }
    }

    /// <summary>
    /// Spell Protego
    /// </summary>
    public void pro()
    {
        Debug.Log("PRO!");
        if (healthSystem.GetHealth() >= 20f && shield.activeSelf == false)
        {
            anim.SetTrigger("attack");
            shield.SetActive(true);
            healthSystem.Damage(20);
        }
        //Invoke("closeShield", waitTime);
    }

    private void closeShield()
    {
        Debug.Log("shield close");
        shield.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        getHealthSystemGameObject = gameObject;
        HealthSystem.TryGetHealthSystem(getHealthSystemGameObject, out healthSystem, true);


        shield = transform.GetChild(1).gameObject;
        shield.SetActive(false);
        anim = GetComponent<Animator>();
        fireball = GameObject.Find("fireball-e-2");
        fireball2 = GameObject.Find("fireball-a-2");
        fireball_other = GameObject.Find("fireball-e");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.name == "fireball-e")
        {
            Debug.Log("is trigger!");
            Debug.Log(collision.name);
            if (shield.activeSelf == false)
            {
                isDead = true;
                anim.SetTrigger("die");
            } else
            {
                shield.SetActive(false);
            }
        } else if (collision.name == "fireball-a")
        {
            isDead = true;
            anim.SetTrigger("die");
        }
    }
}
