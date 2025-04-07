using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class SelectionWizardGhost : MonoBehaviour
{
    [SerializeField] private TMP_Text selectPlayerText;
    [SerializeField] private Image playerImage;
    [SerializeField] private Transform wizardX, ghostX;
    [SerializeField] private CanvasGroup acceptImageCanvas;

    private MyInputManager player;
    private int selectedRole = 0; // 1 = Wizard, -1 = Ghost, 0 = None

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

        // Inititate connection to server with selected role
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