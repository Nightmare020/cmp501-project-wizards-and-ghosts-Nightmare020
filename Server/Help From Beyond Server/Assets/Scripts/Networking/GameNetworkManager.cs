using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Local server IP for testing
    public ushort serverPort = 7777; // Default Netcode port

    // Start is called before the first frame update
    void Start()
    {
        // Get the UnityTransport component from the NetworkManager
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Log the start of the server
        Debug.Log("Starting as a dedicated server...");
        
        // Set the connection data to bind to all interfaces
        transport.SetConnectionData("0.0.0.0", serverPort);
        
        // Start the server
        NetworkManager.Singleton.StartServer();
        Debug.Log($"Dedicated Server Started on {serverIP}:{serverPort}");

        // Add logging for client connections
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log($"Client connected with id: {clientId}");
        };

        // Add logging for client disconnections
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            Debug.Log($"Client disconnected with id: {clientId}");
        };
    }
}