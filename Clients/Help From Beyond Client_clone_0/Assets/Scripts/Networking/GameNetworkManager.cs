using Unity.Netcode.Transports.UTP;
using Unity.Netcode;

using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    // Singleton instance of the GameNetworkManager
    public static GameNetworkManager Instance;

    // IP address of the server to connect to
    public string serverIP = "127.0.0.1"; // Local server for testing

    // Port number of the server to connect to
    public ushort serverPort = 7777; // Default Netcode port

    private void Awake()
    {
        // Set the singleton instance to this instance
        Instance = this;
    }

    public void ConnectAsRole(int selectedRole)
    {
        // Get the UnityTransport component from the NetworkManager singleton
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Log the start of the client connection process
        Debug.Log("Starting as client...");

        // Set the connection data for the transport (IP and port)
        transport.SetConnectionData(serverIP, serverPort); // Connect to server

        // Start the client connection
        NetworkManager.Singleton.StartClient();

        // Log the connection attempt details
        Debug.Log($"Client trying to connect to {serverIP}:{serverPort}");
    }
}