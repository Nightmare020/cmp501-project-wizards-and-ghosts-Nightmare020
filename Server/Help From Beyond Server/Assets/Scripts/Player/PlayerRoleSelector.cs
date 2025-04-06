using System;
using Unity.Netcode;
using UnityEngine;

public enum PlayerRole
{
    None,
    Wizard,
    Ghost
}

public class PlayerRoleSelector : NetworkBehaviour
{
    public static PlayerRoleSelector Instance;

    private NetworkVariable<PlayerRole> selectedRole = new NetworkVariable<PlayerRole>(
        PlayerRole.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // Only select if client already picked something before connecting
        if (PlayerPrefs.HasKey("SelectedRole"))
        {
            PlayerRole selected = (PlayerRole)PlayerPrefs.GetInt("SelectedRole");
            SelectRole(selected);
            Debug.Log($"[Client {OwnerClientId} sent selected role to server: {selected}");
        }
    }

    public void SelectRole(PlayerRole role)
    {
        if (!IsOwner) return;

        SubmitRoleSelectionServerRpc(role);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitRoleSelectionServerRpc(PlayerRole role, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"Player {clientId} selected role: {role}");

        var tracker = FindObjectOfType<PlayerRoleTracker>();

        if (tracker == null)
        {
            Debug.LogError("PlayerRoleTracker not found!");
            return;
        }

        if (role == PlayerRole.Wizard && tracker.HasWizard())
        {
            Debug.LogWarning("Wizard role already taken");
            return;
        }

        if (role == PlayerRole.Ghost && tracker.HasGhost())
        {
            Debug.LogWarning("Ghost role already taken");
            return;
        }

        // Register player in tracker
        if (role == PlayerRole.Wizard) tracker.RegisterWizard(clientId);
        if (role == PlayerRole.Ghost) tracker.RegisterGhost(clientId);

        // Store selected role in tracker
        tracker.StorePlayerRole(clientId, role);
    }
}
