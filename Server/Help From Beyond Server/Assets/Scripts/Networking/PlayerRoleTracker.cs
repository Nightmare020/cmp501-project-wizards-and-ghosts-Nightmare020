using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRoleTracker : NetworkBehaviour
{
    public static PlayerRoleTracker Instance;

    private NetworkVariable<ulong> wizardClientID = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private NetworkVariable<ulong> ghostClientID = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public ulong WizardClientID => wizardClientID.Value;
    public ulong GhostClientID => ghostClientID.Value;

    public bool IsLocalWizard => NetworkManager.Singleton.LocalClientId == WizardClientID;
    public bool IsLocalGhost => NetworkManager.Singleton.LocalClientId == GhostClientID;

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RegisterWizard(ulong clientId)
    {
        if (wizardClientID.Value != ulong.MaxValue)
        {
            Debug.LogWarning($"[RoleTracker] Wizard already registered: {wizardClientID.Value}");
            return;
        }

        if (wizardClientID.Value == ulong.MaxValue)
        {
            wizardClientID.Value = clientId;
            Debug.Log($"[RoleTracker] Registered Wizard: {clientId}");
        }
    }

    public void RegisterGhost(ulong clientId)
    {
        if (ghostClientID.Value != ulong.MaxValue)
        {
            Debug.LogWarning($"[RoleTracker] Ghost already registered: {ghostClientID.Value}");
            return;
        }

        if (ghostClientID.Value == ulong.MaxValue)
        {
            ghostClientID.Value = clientId;
            Debug.Log($"[RoleTracker] Registered Ghost: {clientId}");
        }
    }
}
