using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRoleTracker : NetworkBehaviour
{
    public static PlayerRoleTracker Instance;

    public ulong WizardClientID { get; private set; } = ulong.MaxValue;
    public ulong GhostClientID { get; private set; } = ulong.MaxValue;

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
    public void registerRoleServerRpc(PlayerState role, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (role == PlayerState.Wizard)
        {
            WizardClientID = clientId;
        }
        else if (role == PlayerState.Ghost)
        {
            GhostClientID = clientId;
        }
    }
}
