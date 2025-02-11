using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class LevelFinish : MonoBehaviour
{
    public bool wizardCompleted = false;
    public bool ghostCompleted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("levelfinish");
        
        if (!wizardCompleted)
        {
            if (other.CompareTag("Wizard"))
            {
                print("levelfinishw");
                wizardCompleted = true;
                other.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
                CheckLevelComplete();
            }
        }
        
        if (!ghostCompleted)
        {
            if (other.CompareTag("Ghost"))
            {
                print("levelfinishg");
                ghostCompleted = true;
                other.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
                CheckLevelComplete();
            }
        }
        
    }

    private void CheckLevelComplete()
    {
        if (wizardCompleted && ghostCompleted) MySceneLoader.LoadMainMenu();
    }
}
