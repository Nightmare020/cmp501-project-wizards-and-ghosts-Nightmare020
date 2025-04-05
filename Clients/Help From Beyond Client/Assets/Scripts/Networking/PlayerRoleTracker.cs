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

    [ServerRpc(RequireOwnership = false)]
    public void RegisterRoleServerRpc(PlayerState role, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (role == PlayerState.Wizard && wizardClientID.Value == ulong.MaxValue)
        {
            wizardClientID.Value = senderId;
            Debug.Log($"[RoleTracker] Registered Wizard: {senderId}");
        }
        else if (role == PlayerState.Ghost && ghostClientID.Value == ulong.MaxValue)
        {
            ghostClientID.Value = senderId;
            Debug.Log($"[RoleTracker] Registered Ghost: {senderId}");
        }
    }
}
