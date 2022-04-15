using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkClientList : NetworkBehaviour
{
    public List<PlayerInfo> playerList = new List<PlayerInfo>();
    void Update()
    {
        // The list of RTTs are based from client to server for each client. Not RTT from one client to another.
        if (isServer)
        {
            List<PlayerInfo> tempList = new List<PlayerInfo>();

            // For each connection, grab the NetworkIdentity (which is attached to the player gameobject) and grab its set name and current RTT.
            foreach(NetworkConnectionToClient cli in NetworkServer.connections.Values)
            {
                // Just because the client connected, doesn't mean the game object (and therefore the NetworkIdentity) has spawned in yet.
                if (cli.identity != null)
                {
                    tempList.Add(new PlayerInfo(cli.identity.gameObject.GetComponent<Player>().playerNameNetwork, 
                    Math.Round(cli.identity.gameObject.GetComponent<Player>().curRtt * 1000)));
                }
            }

            // Push an update out to all clients (including this server) to update their lists.
            UpdateList(tempList);
        }
    }

    void OnGUI()
    {
        // Generate the UI for the list, top righthand corner.
        GUILayout.BeginArea(new Rect(Screen.width - 200, 0, 200, 9999));
        foreach(PlayerInfo info in playerList)
        {
            GUILayout.Label($"{info.playerName}: {info.rttTimeMs} ms");
        }
        GUILayout.EndArea();
    }

    [ClientRpc]
    void UpdateList(List<PlayerInfo> playerList)
    {
        this.playerList = playerList;
    }
}

[Serializable]
public struct PlayerInfo
{
    public string playerName;
    public double rttTimeMs;

    public PlayerInfo(string playerName, double rttTimeMs)
    {
        this.playerName = playerName;
        this.rttTimeMs = rttTimeMs;
    }
}
