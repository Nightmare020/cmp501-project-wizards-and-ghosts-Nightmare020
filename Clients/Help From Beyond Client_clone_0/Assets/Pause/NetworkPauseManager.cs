using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPauseManager : NetworkBehaviour
{
    public static NetworkPauseManager Instance;

    private NetworkVariable<bool> wizardPaused = new NetworkVariable<bool>(
        value: false,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> ghostPaused = new NetworkVariable<bool>(
            value: false,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

    public bool IsGamePaused => wizardPaused.Value || ghostPaused.Value;

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        wizardPaused.OnValueChanged += OnPauseChanged;
        ghostPaused.OnValueChanged += OnPauseChanged;
    }

    public void SetPauseState(bool paused, PlayerState state)
    {
        if (!IsServer) return;

        if (state == PlayerState.Wizard)
        {
            wizardPaused.Value = paused;
        }
        else if (state == PlayerState.Ghost)
        {
            ghostPaused.Value = paused;
        }
    }

    private void OnPauseChanged(bool previousValue, bool newValue)
    {
        Time.timeScale = IsGamePaused ? 0 : 1;
    }
}