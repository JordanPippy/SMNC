using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkGUI : MonoBehaviour
{
    public string playerName;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
            GUILayout.Label("Name: ");
            playerName = GUILayout.TextField(playerName, 25);
            GUILayout.Label("IP Address: ");
            GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>().ConnectAddress = GUILayout.TextField(GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>().ConnectAddress, 25);
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

        static void StartButtons()
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }
}
