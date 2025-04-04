
using System.Collections.Generic;
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
    public bool isPaused;
    private bool showingTutorials;

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

        if (isPaused && (_wizardInputManager || _ghostInputManager))
        {
            if (_wizardInputManager.NavigationRight() /*|| _ghostInputManager.NavigationRight()*/)
            {
                SelectNext();
                HighLightButtons();
            }
            else if (_wizardInputManager.NavigationLeft() /*|| _ghostInputManager.NavigationLeft()*/)
            {
                SelectPrev();
                HighLightButtons();
            }
            else if (_wizardInputManager.NavigationSelect() /*|| _ghostInputManager.NavigationSelect()*/)
            {
                if (selectedIndex == 0)
                {
                    ResumeGame(true);
                    ResumeGame(false);
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
            else if (_wizardInputManager.NavigationPause() /*|| _ghostInputManager.NavigationPause()*/)
            {
                if (showingTutorials)
                {
                    HideTutorial();
                }
                else
                {
                    ResumeGame(true);
                    ResumeGame(false);
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
        SetPauseStateServerRpc(true, isGhost ? PlayerState.Ghost : PlayerState.Wizard);
    }

    public void ResumeGame(bool isGhost)
    {
        if (_ghostInputManager)
        {
            _ghostInputManager.SetInputMap(isGhost ? CurrentInputState.Ghost : CurrentInputState.Wizard);
        }

        if (_wizardInputManager)
        {
            _wizardInputManager.SetInputMap(isGhost ? CurrentInputState.Ghost : CurrentInputState.Wizard);
        }

        HidePauseUI();

        // Determine current role
        PlayerState role = (_ghostInputManager != null && _ghostInputManager.enabled)
            ? PlayerState.Ghost
            : PlayerState.Wizard;

        // Notify server
        SetPauseStateServerRpc(false, role);
    }

    private void ShowPauseUI()
    {
        pauseMenu.alpha = 1;
        isPaused = true;
    }

    private void HidePauseUI()
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

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPauseStateServerRpc(bool paused, PlayerState state)
    {
        NetworkPauseManager.Instance?.SetPauseState(paused, state);

        // Inform other clients to show the UI
        TogglePauseMenuClientRpc(paused);
    }

    [ClientRpc]
    private void TogglePauseMenuClientRpc(bool paused)
    {
        if (NetworkManager.Singleton.IsServer) return;

        if (paused) ShowPauseUI();
        else HidePauseUI();
    }
}