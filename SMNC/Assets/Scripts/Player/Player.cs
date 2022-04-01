using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    public GameObject nameTagObj;
    private bool nameSet = false; // Keep attempting to set the text of the nametag until set.
    public NetworkVariable<FixedString32Bytes> playerNameNetwork = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            string playerName = GameObject.Find("NetworkManager").GetComponent<NetworkGUI>().playerName; // Get playername from the network manager GUI.
            gameObject.name = "LocalPlayer"; // Set the clients personal gameobject's name.
            nameTagObj.SetActive(false); // Disable the clients nametag on their end.
            RequestPlayerNameServerRpc(playerName);
        }
        else
        {
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork.Value.ToString());
        }
    }

    [ServerRpc]
    public void RequestPlayerNameServerRpc(FixedString32Bytes nam)
    {
        playerNameNetwork.Value = nam;
    }

    void Update()
    {
        if (!IsLocalPlayer && !nameSet)
        {
            if (playerNameNetwork.Value.ToString() != "")
            {
                nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork.Value.ToString());
                nameSet = true;
            }
        }
    }
}
