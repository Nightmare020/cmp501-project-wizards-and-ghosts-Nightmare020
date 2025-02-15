using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
    public bool runAsServer = true;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Application.isBatchMode || runAsServer)
        {
            Debug.Log("Starting as a dedicated server...");
            NetworkManager.Singleton.StartServer();
        }
        else
        {
            Debug.Log("Starting as client...");
            NetworkManager.Singleton.StartClient();
        }
    }
}
