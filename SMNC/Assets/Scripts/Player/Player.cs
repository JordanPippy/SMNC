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
    public SkinnedMeshRenderer mesh;
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
            mesh.enabled = false; // The client does not need to see their own body.
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
            Debug.Log("After jump current Health: " + currentHealth);
        }
    }

    void Die()
    {
        GetComponent<Movement>().ForceMoveClient(new Vector3(0, 0, 0));
        currentHealth = maxHealth;
        RpcSetHealthBar(maxHealth);
    }

    void UpdateServer()
    {
        Debug.Log("Current Health: " + currentHealth);
        if (currentHealth <= 0)
            Die();
    }

    [ClientRpc]
    public void RpcChangeHealthBar(int amount)
    {
        healthBar.changeHealth(amount);
    }

    [ClientRpc]
    public void RpcSetHealthBar(int amount)
    {
        healthBar.SetHealth(amount);
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
        RpcChangeHealthBar(-amount);
    }


    void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            if (other.gameObject.CompareTag("Projectile"))
            {
                int damage = other.gameObject.GetComponent<Projectile>().damage;
                currentHealth -= damage;
                RpcChangeHealthBar(-damage);
                Destroy(other.gameObject);
            }
        }
    }
}
