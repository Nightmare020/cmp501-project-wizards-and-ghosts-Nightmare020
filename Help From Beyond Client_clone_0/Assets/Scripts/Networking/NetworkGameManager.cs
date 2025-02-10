using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnPlayers();
        }
    }

    [ServerRpc]
    public void SpawnPlayersServerRpc(string role, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Spawn both wizard and ghost on all clients
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) 
        {
            GameObject playerObject = Instantiate(playerPrefab);
            NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();

            if (networkObject != null) 
            {
                networkObject.SpawnAsPlayerObject(clientId);
            }
        }
    }
}
