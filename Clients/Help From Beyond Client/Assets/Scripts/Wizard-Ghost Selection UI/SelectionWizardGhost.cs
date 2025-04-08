using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectionWizardGhost : MonoBehaviour
{
    // UI text element to display the selected player
    [SerializeField] private TMP_Text selectPlayerText;

    // UI image element to represent the player
    [SerializeField] private Image playerImage;

    // Transform positions for the wizard and ghost selections
    [SerializeField] private Transform wizardX, ghostX;

    // Canvas group for the accept image
    [SerializeField] private CanvasGroup acceptImageCanvas;

    // Reference to the input manager
    private MyInputManager player;

    // Variable to store the selected role (1 = Wizard, -1 = Ghost, 0 = None)
    private int selectedRole = 0;

    public void Initialize(MyInputManager inputManager)
    {
        player = inputManager;
        selectedRole = 0;
        playerImage.color = Color.white;
        acceptImageCanvas.alpha = 0;
    }

    public void SelectLeft()
    {
        selectedRole = 1; // Wizard
        MovePlayerImageTo(wizardX);
    }

    public void SelectRight()
    {
        selectedRole = -1; // Ghost
        MovePlayerImageTo(ghostX);
    }

    public void PlayerAccept()
    {
        Debug.Log("Player Accept Selection");

        // Hide UI
        gameObject.SetActive(false);

        // Initiate connection to server with selected role
        GameNetworkManager.Instance.ConnectAsRole(selectedRole);
    }

    private void MovePlayerImageTo(Transform target)
    {
        Vector2 newPos = new Vector2(target.position.x, playerImage.transform.position.y);
        playerImage.transform.position = newPos;
        playerImage.color = Color.green;
        UpdateAcceptImage();
    }

    private void UpdateAcceptImage()
    {
        acceptImageCanvas.alpha = selectedRole != 0 ? 1 : 0;
    }

    public int GetSelectedRole()
    {
        return selectedRole;
    }

    public bool HasMadeSelection()
    {
        return selectedRole != 0;
    }
}