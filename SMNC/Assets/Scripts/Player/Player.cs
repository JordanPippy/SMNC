using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class Player : NetworkBehaviour
{
    public GameObject nameTagObj;
    public bool nameSet = false;
    public NetworkVariable<FixedString32Bytes> playerNameNetwork = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        /*
        if (IsOwner)
        {
            Move();
        }
        */
        if (IsLocalPlayer)
        {
            gameObject.name = "LocalPlayer";
            nameTagObj.SetActive(false);
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(GameObject.Find("NetworkManager").GetComponent<NetworkGUI>().playerName);
            RequestPlayerNameServerRpc(GameObject.Find("NetworkManager").GetComponent<NetworkGUI>().playerName);
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
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork.Value.ToString());
        }
    }

    /*
    public void Move(Vector3 newPosition)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = handleMovement(newPosition, position.Value);
            transform.position = randomPosition;
            position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc(newPosition);
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
    {
        position.Value = handleMovement(newPosition, position.Value);
        transform.position = position.Value;
    }

    static Vector3 handleMovement(Vector3 newPosition, Vector3 oldPosition)
    {
        newPosition *= 2.0f;
        return oldPosition + newPosition;

    }

    void Update()
    {
        transform.position = position.Value;
    }
    */
}
