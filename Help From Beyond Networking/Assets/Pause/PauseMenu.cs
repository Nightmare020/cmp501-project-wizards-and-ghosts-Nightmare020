
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class PauseMenu : MonoBehaviour
{
    private MyInputManager _ghostInputManager, _wizardInputManager;
    public CanvasGroup pauseMenu;
    public bool isPaused;
    [SerializeField] private List<Image> buttonImages;
    [SerializeField] private List<Sprite> pressedSprites, normalSprites;

    private int selectedIndex = 0;

    void Start()
    {
        pauseMenu.alpha = 0;
        selectedIndex = 0;
        HighLightButtons();
    }

    void Update()
    {
        if (Time.frameCount % 3 == 0)
        {
            _wizardInputManager = GetWizardInputs();
            _ghostInputManager = GetGhostInputs();
        }

        if (isPaused && _wizardInputManager && _ghostInputManager)
        {
            if (_wizardInputManager.NavigationRight() || _ghostInputManager.NavigationRight())
            {
                SelectNext();
                HighLightButtons();
            }
            else if (_wizardInputManager.NavigationLeft() || _ghostInputManager.NavigationLeft())
            {
                SelectPrev();
                HighLightButtons();
            }
            else if (_wizardInputManager.NavigationSelect() || _ghostInputManager.NavigationSelect())
            {
                if (selectedIndex == 0)
                {
                    ResumeGame();
                }
                else if (selectedIndex == 1)
                {
                    ResetTheGame();
                }
                else if (selectedIndex == 2)
                {
                    GoToMainMenu();
                }
                else if (selectedIndex == 3)
                {
                    QuitGame();
                }
            }
            else if (_wizardInputManager.NavigationPause() || _ghostInputManager.NavigationPause())
            {
                ResumeGame();
            }
        }
    }

    private void HighLightButtons()
    {
        for (int i = 0; i < buttonImages.Count; i++)
        {
            if (selectedIndex == i)
            {
                buttonImages[i].sprite = pressedSprites[i];
            }
            else
            {
                buttonImages[i].sprite = normalSprites[i];
            }
        }
    }

    void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % buttonImages.Count;
    }

    void SelectPrev()
    {
        selectedIndex = (selectedIndex - 1) < 0 ? buttonImages.Count - 1 : (selectedIndex - 1);
    }

    private MyInputManager GetGhostInputs()
    {
        if (!_ghostInputManager)
        {
            GameObject ghostObj = GameObject.FindGameObjectWithTag("ActiveGhost");
            MyInputManager inputManager = null;
            if (ghostObj)
            {
                inputManager = ghostObj.GetComponent<MyInputManager>();
            }

            return inputManager;
        }

        return _ghostInputManager;
    }

    private MyInputManager GetWizardInputs()
    {
        if (!_wizardInputManager)
        {
            GameObject wizardObj = GameObject.FindGameObjectWithTag("ActiveWizard");
            MyInputManager inputManager = null;
            if (wizardObj)
            {
                inputManager = wizardObj.GetComponent<MyInputManager>();
            }

            return inputManager;
        }

        return _wizardInputManager;
    }

    public void PauseGame(MyInputManager myInputManager, bool isGhost)
    {
        if (_ghostInputManager)
        {
            _ghostInputManager.SetInputMap(CurrentInputState.UiNavigation);
        }

        if (_wizardInputManager)
        {
            _wizardInputManager.SetInputMap(CurrentInputState.UiNavigation);
        }

        if (isGhost)
        {
            _ghostInputManager = myInputManager;
        }
        else
        {
            _wizardInputManager = myInputManager;
        }

        pauseMenu.alpha = 1;
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (_ghostInputManager)
        {
            _ghostInputManager.SetInputMap(CurrentInputState.Ghost);
        }

        if (_wizardInputManager)
        {
            _wizardInputManager.SetInputMap(CurrentInputState.Wizard);
        }

        pauseMenu.alpha = 0;
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ResetTheGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        MySceneLoader.LoadMainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}