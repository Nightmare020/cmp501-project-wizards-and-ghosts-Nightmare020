
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayersConnectionManager : MonoBehaviour
{
    private List<PlayerInput> players;
    [SerializeField] private List<Transform> startingPoints;
    private PlayerInputManager _playerInputManager;
    private SelectionWizardGhost _selectionWizardGhostPanel;


    private void Awake()
    {
        players = new List<PlayerInput>();
        _playerInputManager = GetComponent<PlayerInputManager>();
        _selectionWizardGhostPanel = FindObjectOfType<SelectionWizardGhost>();
    }

    private void OnEnable()
    {
        _playerInputManager.onPlayerJoined += AddPlayer;
        _playerInputManager.onPlayerLeft += PlayerLeft;
    }

    private void OnDisable()
    {
        _playerInputManager.onPlayerJoined -= AddPlayer;
        _playerInputManager.onPlayerLeft -= PlayerLeft;
    }


    private void AddPlayer(PlayerInput player)
    {
        players.Add(player);
        player.transform.position = (Vector2)startingPoints[players.Count - 1].position;

        //set on the selection panel the number of players conected
        MyInputManager aux = player.transform.gameObject.GetComponent<MyInputManager>();
        _selectionWizardGhostPanel.AddPlayer(aux);
    }

    private void PlayerLeft(PlayerInput player)
    {
        foreach (PlayerInput p in players)
        {
            MyInputManager aux = p.transform.gameObject.GetComponent<MyInputManager>();
            aux.SetInputMap(CurrentInputState.UiNavigation);


            //remove player from selection manager
            if (p == player)
            {
                _selectionWizardGhostPanel.RemovePlayer(aux);
            }
        }
        players.Remove(player);
    }
}