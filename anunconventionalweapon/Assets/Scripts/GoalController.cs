using System.Collections;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject.Find("MenuController").GetComponent<MenuController>().MenuTransition(MenuController.MenuState.Win);
            other.GetComponent<PlayerController>().EndGame();
        }
    }
}