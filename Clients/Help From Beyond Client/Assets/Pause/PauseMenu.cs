
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class PauseMenu : MonoBehaviour
{
    private MyInputManager _ghostInputManager, _wizardInputManager;
    public CanvasGroup pauseMenu;
    public GameObject tutorialMenu;
    public GameObject GameOverPanel;
    public GameObject GamePointsPanel;
    public GameObject GameTimePanel;

    [SerializeField] private TMP_Text _totalPoints;
    [SerializeField] private TMP_Text _totalTime;

    public bool isPaused;
    private bool showingTutorials;
    private bool resumeEnabled = true;

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

        if (isPaused)
        {
            if (_wizardInputManager && _wizardInputManager.NavigationRight() 
                || _ghostInputManager && _ghostInputManager.NavigationRight())
            {
                SelectNext();
                HighLightButtons();
            }
            else if (_wizardInputManager && _wizardInputManager.NavigationLeft() 
                || _ghostInputManager && _ghostInputManager.NavigationLeft())
            {
                SelectPrev();
                HighLightButtons();
            }
            else if (_wizardInputManager && _wizardInputManager.NavigationSelect() 
                || _ghostInputManager && _ghostInputManager.NavigationSelect())
            {
                if (selectedIndex == 0 && resumeEnabled)
                {
                    ResumeGame();
                }
                else if (selectedIndex == 1)
                {
                    ShowTutorial();
                }
                else if (selectedIndex == 2)
                {
                    QuitGame();
                }
            }
            else if (_wizardInputManager && _wizardInputManager.NavigationPause() 
                || _ghostInputManager && _ghostInputManager.NavigationPause())
            {
                if (showingTutorials)
                {
                    HideTutorial();
                }
                else if (resumeEnabled)
                {
                    ResumeGame();
                }
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

        ShowPauseUI();

        // Notify server
        NetworkPauseManager.Instance?.SetPauseStateServerRpc(true);
    }

    public void ResumeGame()
    {
        if (_ghostInputManager)
        {
            _ghostInputManager.SetInputMap(CurrentInputState.Ghost);
        }

        if (_wizardInputManager)
        {
            _wizardInputManager.SetInputMap(CurrentInputState.Ghost);
        }

        HidePauseUI();

        // Notify server
        NetworkPauseManager.Instance?.SetPauseStateServerRpc(false);
    }

    public void ShowPauseUI()
    {
        pauseMenu.alpha = 1;
        isPaused = true;
    }

    public void HidePauseUI()
    {
        pauseMenu.alpha = 0;
        isPaused = false;
    }

    public void ShowTutorial()
    {
        Debug.Log("Show tutorial");
        showingTutorials = true;
        tutorialMenu.SetActive(true);
    }

    public void HideTutorial()
    {
        Debug.Log("Hide tutorial");
        showingTutorials = false;
        tutorialMenu.SetActive(false);
    }

    public void ShowGameOver()
    {
        Debug.Log("Show game over");
        GameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        Debug.Log("Hide tutorial");
        GameOverPanel.SetActive(false);
    }

    public void ShowTotalPoints()
    {
        Debug.Log("Show tutorial");
        GamePointsPanel.SetActive(true);
    }

    public void HideTotalPoints()
    {
        Debug.Log("Hide tutorial");
        GamePointsPanel.SetActive(false);
    }

    public void ShowTotalTime()
    {
        Debug.Log("Show tutorial");
        GameTimePanel.SetActive(true);
    }

    public void HideTotalTime()
    {
        Debug.Log("Hide tutorial");
        GameTimePanel.SetActive(false);
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowGameOverStuff(int totalPoints, int totalTimeMinutes, int totalTimeSeconds)
    {
        ShowPauseUI(); // makes sure the pause menu canvas is visible

        GameOverPanel.SetActive(true);
        GamePointsPanel.SetActive(true);
        GameTimePanel.SetActive(true);

        resumeEnabled = false;

        // Dim the resume button visually
        if (buttonImages.Count > 0 && buttonImages[0] != null)
        {
            var color = buttonImages[0].color;
            color.a = 0.5f;
            buttonImages[0].color = color;
        }

        if (_totalPoints != null)
            _totalPoints.text = $"{totalPoints} PTS";

        if (_totalTime != null)
            _totalTime.text = $"{totalTimeMinutes} : {totalTimeSeconds}";
    }
}