using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public bool runAsServer = true;

    public string serverIP = "127.0.0.1"; // Local server for testing
    public ushort serverPort = 7777; // Default Netcode port

    // Start is called before the first frame update
    void Start()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        Debug.Log("Starting as a dedicated server...");
        transport.SetConnectionData("0.0.0.0", serverPort); // Bind to all interfaces
        NetworkManager.Singleton.StartServer();
        Debug.Log($"Dedicated Server Started on {serverIP}:{serverPort}");

        // Add logging for connections
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log($"Client connected with id: {clientId}");
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            Debug.Log($"Client disconnected with id: {clientId}");
        };
    }
}
