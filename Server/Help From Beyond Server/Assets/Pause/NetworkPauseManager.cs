using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPauseManager : NetworkBehaviour
{
    public static NetworkPauseManager Instance;

    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(
        value: false,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Callbacks when multiple sources affect pause
        isGamePaused.OnValueChanged += OnPauseStateChanged;

        // Ensure the correct time scale when joining
        if (!IsServer)
        {
            Time.timeScale = isGamePaused.Value ? 0 : 1;
        }
    }

    private void OnPauseStateChanged(bool oldValue, bool newValue)
    {
        // Run on both server and clients
        Time.timeScale = newValue ? 0 : 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPauseStateServerRpc(bool paused)
    {
        isGamePaused.Value = paused;
    }
}