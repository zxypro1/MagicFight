using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLoop : MonoBehaviour
{
    GameObject endUI;
    GameObject player1;
    GameObject player2;
    GameObject fireball;
    GameObject fireball2;
    GameObject fireball_a;
    GameObject fireball2_a;
    // Start is called before the first frame update
    void Start()
    {
        // Time.timeScale = 0;
        endUI = GameObject.Find("endUI");
        endUI.SetActive(false);
        player1 = GameObject.Find("Wizard");
        player2 = GameObject.Find("Wizard2");
        fireball = GameObject.Find("fireball-e");
        fireball2 = GameObject.Find("fireball-a");
        fireball_a = GameObject.Find("fireball-e-2");
        fireball2_a = GameObject.Find("fireball-a-2");

        fireball.SetActive(false);
        fireball2.SetActive(false);
        fireball_a.SetActive(false);
        fireball2_a.SetActive(false);
    }

    private void closeUI()
    {
        endUI.SetActive(false);
        player2.GetComponent<Player2>().idle();
        player1.GetComponent<Player1>().idle();
    }

    // Update is called once per frame
    void Update()
    {
        if (player1.GetComponent<Player1>().isDead)
        {
            //Time.timeScale = 0;
            endUI.SetActive(true);
            endUI.transform.Find("Text").GetComponent<Text>().text = "Player2 Win!";
            Invoke("closeUI", 5);
        }
        if (player2.GetComponent<Player2>().isDead)
        {
            //Time.timeScale = 0;
            endUI.SetActive(true);
            endUI.transform.Find("Text").GetComponent<Text>().text = "Player1 Win!";
            Invoke("closeUI", 5);
        }
    }
}
