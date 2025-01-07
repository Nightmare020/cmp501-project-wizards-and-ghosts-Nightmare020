using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SelectionWizardGhost : MonoBehaviour
{
    // Start is called before the first frame update
    private List<MyInputManager> players;
    private List<int> playerRolPosition;
    private List<PlayerManager> playerManagers;
    private CanvasGroup _canvasGroup;
    private bool otherPlayerFound;
    private Coroutine searchPlayerTextCoroutine;
    private Coroutine foundPlayerTextCoroutine;
    [SerializeField] private TMP_Text selectPlayerText;
    [SerializeField] private TMP_Text searchForPlayersText;
    [SerializeField] private TMP_Text playerFoundText;
    [SerializeField] private List<Image> playerImages;
    [SerializeField] private Transform wizardX, ghostX;

    //inputs
    public bool inputEnabled;

    // Create event-based system for when other player is found
    public delegate void PlayerFoundEventHandler(bool found);
    public static event PlayerFoundEventHandler OnPlayerFoundChanged;

    //accept image
    [SerializeField] private CanvasGroup acceptImageCanvas;
    private float originalImageX;

    private void Awake()
    {
        players = new List<MyInputManager>();
        playerRolPosition = new List<int>();
        playerManagers = new List<PlayerManager>();
        _canvasGroup = GetComponent<CanvasGroup>();
        originalImageX = playerImages[0].transform.position.x;
        UpdateAcceptImage();
    }

    public void ShowUI()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        inputEnabled = true;
        UpdateAcceptImage();
    }

    // Call thos method when OtherPlayerFound changes
    public void UpdateOtherPlayerFound (bool playerFound)
    {
        otherPlayerFound = playerFound;
        OnPlayerFoundChanged?.Invoke(playerFound);
    }

    // Subscribe to event
    private void OnEnable()
    {
        OnPlayerFoundChanged += SearchOtherPlayer;
    }

    private void OnDisable()
    {
        OnPlayerFoundChanged -= SearchOtherPlayer;
    }

    public void SearchOtherPlayer(bool playerFound)
    {
        // Set the other player found global variable to the delegate variable
        otherPlayerFound = playerFound;

        if (!otherPlayerFound)
        {
            selectPlayerText.gameObject.SetActive(false);
            playerFoundText.gameObject.SetActive(false);
            searchForPlayersText.gameObject.SetActive(true);

            // For trial, wait a few seconds and then set otherPlayerFound to true
            StartCoroutine(SetOtherPlayerFound());

            // Start a loading animation coroutine
            if (searchPlayerTextCoroutine == null)
            {
                // Start corroutine for dor loop
                searchPlayerTextCoroutine = StartCoroutine(LoadingSearchForPlayerText());
            }
        }
        else
        {
            // Stop the corroutine and reset the text
            if (searchPlayerTextCoroutine != null)
            {
                StopCoroutine(searchPlayerTextCoroutine);
                searchPlayerTextCoroutine = null;
            }

            // Show player found text, and start the game
            selectPlayerText.gameObject.SetActive(false);
            searchForPlayersText.gameObject.SetActive(false);
            playerFoundText.gameObject.SetActive(true);

            if (foundPlayerTextCoroutine == null)
            {
                foundPlayerTextCoroutine = StartCoroutine(LoadingLevel());
            }

            // Start a courutine to wait a few seconds to load the game and then start it
            StartCoroutine(LoadAndHideUI());
        }
    }

    private IEnumerator LoadingSearchForPlayerText()
    {
        int dotCount = 0;

        while (!otherPlayerFound)
        {
            // Update text with dots
            searchForPlayersText.text = searchForPlayersText.text.TrimEnd('.') + new string('.', dotCount);

            // Increment dot count, looping back to 0 after 3
            dotCount = (dotCount + 1) % 4;

            // Wait 0.5 secs before updating again
            yield return new WaitForSeconds(0.5f); 
        }
    }

    private IEnumerator LoadingLevel()
    {
        int dotCount = 0;

        while (true)
        {
            // Update text with dots
            playerFoundText.text = playerFoundText.text.TrimEnd('.') + new string('.', dotCount);

            // Increment dot count, looping back to 0 after 3
            dotCount = (dotCount + 1) % 4;

            // Wait 0.5 secs before updating again
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator SetOtherPlayerFound()
    {
        // Wait for seconds
        yield return new WaitForSeconds(12f);

        // Make other player found
        SearchOtherPlayer(true);
    }

    private IEnumerator LoadAndHideUI()
    {
        // Wait for seconds
        yield return new WaitForSeconds(12f);

        // Hide UI
        HideUI();
    }

    private void HideUI()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        inputEnabled = false;
    }

    public void AddPlayer(MyInputManager playerInputManager)
    {
        players.Add(playerInputManager);
        int index = players.Count - 1;
        if (players.Count == 2)
            playerRolPosition.Add(playerRolPosition[0]);
        else
            playerRolPosition.Add(0);

        playerManagers.Add(playerInputManager.transform.GetComponent<PlayerManager>());
        playerImages[index].color = Color.green;

        //update the player count
        UpdateAcceptImage();
    }

    public void RemovePlayer(MyInputManager myInputManager)
    {
        int deletedPlayerIndex = players.FindIndex(x => x == myInputManager);
        players.RemoveAt(deletedPlayerIndex);
        playerManagers.RemoveAt(deletedPlayerIndex);
        playerRolPosition.RemoveAt(deletedPlayerIndex);
        playerImages[players.Count].color = Color.white;
        foreach (var image in playerImages)
        {
            Vector2 newPos = new Vector2(originalImageX, image.transform.position.y);
            image.transform.position = newPos;
        }

        for (int i = 0; i < playerRolPosition.Count; i++)
        {
            playerRolPosition[i] = 0;
        }

        //update the player count
        ShowUI();
        UpdateAcceptImage();
    }

    #region player inputs

    public void PlayerAccept()
    {
        if (players.Count == 1 && inputEnabled && (playerRolPosition[0] == -1 || playerRolPosition[0] == 1))
        {
            //set players as ghost and wizard
            SetWizardOrGhost(0);
            //SetWizardOrGhost(1);

            //hide menu and disable the inputs
            SearchOtherPlayer(false);
            //HideUI();
            print("Accepted");
        }
    }

    public void SelectLeft(MyInputManager myInputManager)
    {
        int playerIndex = players.FindIndex(x => x == myInputManager);
        playerRolPosition[playerIndex] = 1;
        Vector2 newPos = new Vector2(wizardX.position.x, playerImages[playerIndex].transform.position.y);
        playerImages[playerIndex].transform.position = newPos;
        UpdateAcceptImage();
    }

    public void SelectRight(MyInputManager myInputManager)
    {
        int playerIndex = players.FindIndex(x => x == myInputManager);
        playerRolPosition[playerIndex] = -1;
        Vector2 newPos = new Vector2(ghostX.position.x, playerImages[playerIndex].transform.position.y);
        playerImages[playerIndex].transform.position = newPos;
        UpdateAcceptImage();
    }

    #endregion


    public void UpdateAcceptImage()
    {
        if (players.Count == 1 && inputEnabled && (playerRolPosition[0] == -1 || playerRolPosition[0] == 1))
        {
            ShowAcceptText();
        }
        else
        {
            HideAcceptText();
        }
    }

    public void ShowAcceptText()
    {
        acceptImageCanvas.alpha = 1;
    }

    public void HideAcceptText()
    {
        acceptImageCanvas.alpha = 0;
    }

    private void SetWizardOrGhost(int index)
    {
        if (playerRolPosition[index] == 1)
        {
            players[index].SetInputMap(CurrentInputState.Wizard);
            playerManagers[index].SetCurrentState(PlayerState.Wizard);
        }
        else if (playerRolPosition[index] == -1)
        {
            players[index].SetInputMap(CurrentInputState.Ghost);
            playerManagers[index].SetCurrentState(PlayerState.Ghost);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}