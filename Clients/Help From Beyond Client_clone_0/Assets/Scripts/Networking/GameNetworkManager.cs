using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Local server for testing
    public ushort serverPort = 7777; // Default Netcode port

    // Start is called before the first frame update
    void Start()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        Debug.Log("Starting as client...");
        transport.SetConnectionData(serverIP, serverPort); // Connect to server
        NetworkManager.Singleton.StartClient();
        Debug.Log($"Client trying to connect to {serverIP}:{serverPort}");
    }
}
