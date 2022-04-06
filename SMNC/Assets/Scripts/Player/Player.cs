using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public GameObject nameTagObj;
    public bool nameSet = false; // Keep attempting to set the text of the nametag until set.
    public HealthBar healthBar;
    [SyncVar] public string playerNameNetwork;

    [SyncVar] public int maxHealth = 100;
    [SyncVar] public int currentHealth;

    void Start()
    {
        if (isLocalPlayer)
        {
            string playerName = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().playerName; // Get playername from the network manager GUI.
            gameObject.name = "LocalPlayer"; // Set the clients personal gameobject's name.
            nameTagObj.SetActive(false); // Disable the clients nametag on their end.
            UpdatePlayerName(playerName);
        }
        else
        {
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork);
        }

        if (isServer)
        {
            maxHealth = 100;
            currentHealth = maxHealth;
        }
        if (isLocalPlayer)
        {
            GetComponentInChildren<Canvas>().enabled = true;
            healthBar.SetMaxHealth(maxHealth);
        }
        else
            GetComponentInChildren<Canvas>().enabled = false;
    }

    void Update()
    {
        if (!isLocalPlayer && !nameSet)
        {
            if (playerNameNetwork != "")
            {
                nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork);
                nameSet = true;
            }
        }

        if (isLocalPlayer)
            UpdateClient();
        if (isServer)
            UpdateServer();
    }

    void UpdateClient()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Before jump current Health: " + currentHealth);
            RequestTakeDamage(20);
            //healthBar.SetHealth(currentHealth);
            healthBar.changeHealth(-20);
            Debug.Log("After jump current Health: " + currentHealth);
        }
    }

    void UpdateServer()
    {

    }

    [Command]
    void UpdatePlayerName(string nam)
    {
        playerNameNetwork = nam;
    }

    [Command]
    void RequestTakeDamage(int amount)
    {
        currentHealth -= amount;
    }
}
