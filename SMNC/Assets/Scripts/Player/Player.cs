using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using Unity.Collections;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public GameObject overheadUI;
    private GameObject nameTagObj, overheadHealthBarObj;
    private HealthBar overheadHealthBar;
    public HealthBar healthBar;
    public SkinnedMeshRenderer mesh;
    [SyncVar] public string playerNameNetwork;
    [SyncVar] public int maxHealth = 100;
    [SyncVar] public int currentHealth;
    [SyncVar] public double curRtt;
    public float pingUpdateFrequency = 2.0f;
    public float lastRttTime;
    public bool valuesSetFromNetwork = false; // My attempt to fix race conditions and improve performance.
    private bool nameSet = false;

    private AbilityBase ability1, ability2;
    public GameManager gm;

    void Start()
    {
        if (isLocalPlayer)
        {
            string playerName = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().playerName; // Get playername from the network manager GUI.
            gameObject.name = "LocalPlayer"; // Set the clients personal gameobject's name.
            overheadUI.SetActive(false); // Disable the clients overhead info on their end.
            mesh.enabled = false; // The client does not need to see their own body.
            lastRttTime = Time.time; // Begin the timer for the Rtt updates.
            valuesSetFromNetwork = true; // Values should already be set for the client owned object.
            UpdatePlayerName(playerName);
        }
        else
        {
            gameObject.transform.Find("HeadCamera").gameObject.SetActive(false);
            nameTagObj = overheadUI.transform.Find("NameTag").gameObject;
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork);
            overheadHealthBarObj = overheadUI.transform.Find("Health Bar").gameObject;
            overheadHealthBar = overheadHealthBarObj.GetComponent<HealthBar>();
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

            gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

            ability1 = gm.GetAbility("TestAbility1");
            ability2 = gm.GetAbility("TestAbility2");
        }
        else
        {
            GetComponentInChildren<Canvas>().enabled = false;
            overheadHealthBar.SetHealth(currentHealth);
        }
    }

    void Update()
    {
        /* 
         * This block is to prevent race conditions when attemping to set the info from the network upon object creation.
         * Ideally, we do not want code running in Update() unless it is absolutely necessary, as it can affect performance.
         * Once all values are set, a flag is flipped and this code will no longer be ran.
         */ 
        if (!valuesSetFromNetwork && !isLocalPlayer)
        {
            if (!nameSet && playerNameNetwork != "")
            {
                nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork);
                nameSet = true;
            }

            if (overheadHealthBar.GetHealth() == 0f)
            {
                overheadHealthBar.SetHealth(currentHealth);
            }
            
            valuesSetFromNetwork = (nameSet && overheadHealthBar.GetHealth() != 0f); // Stop running this code if the values are set.
        }

        if (isLocalPlayer)
        {
            UpdateClient();
            UpdateRtt();
            UpdateOverheadUIVisibility();
        }

        if (isServer)
            UpdateServer();   
    }

    void UpdateClient()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ability1.Use(transform);
        if (Input.GetKeyDown(KeyCode.K))
            ability2.Use(transform);
    }

    void Die()
    {
        GetComponent<Movement>().ForceMoveClient(new Vector3(0, 0, 0));
        currentHealth = maxHealth;
        RpcSetHealthBar(maxHealth);
    }

    void UpdateServer()
    {
        if (currentHealth <= 0)
            Die();
    }

    [ClientRpc]
    public void RpcChangeHealthBar(int amount)
    {
        if (isLocalPlayer)
            healthBar.changeHealth(amount);
        else
            overheadHealthBar.changeHealth(amount);
    }

    [ClientRpc]
    public void RpcSetHealthBar(int amount)
    {
        if (isLocalPlayer)
            healthBar.SetHealth(amount);
        else
            overheadHealthBar.SetHealth(amount);
        
    }

    [Command]
    void UpdatePlayerName(string nam)
    {
        playerNameNetwork = nam;
    }

    // The following is used to control how often the rtt is updated.
    void UpdateRtt()
    {
        // Minimize the amount of messages sent to the server.
        if (Time.time - lastRttTime >= pingUpdateFrequency)
        {
            lastRttTime = Time.time;
            UpdateNetworkRTT(NetworkTime.rtt);
        }
    }

    [Command]
    void UpdateNetworkRTT(double rtt)
    {
        curRtt = rtt;
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

    void UpdateOverheadUIVisibility()
    {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj != this.gameObject)
            {
                Vector3 dir = ((obj.transform.position + obj.GetComponent<CharacterController>().center) - gameObject.transform.Find("HeadCamera").position).normalized;
                Ray ray = new Ray(gameObject.transform.Find("HeadCamera").position, dir);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == obj)
                    {
                        obj.GetComponent<Player>().overheadUI.SetActive(true);
                    }
                    else
                    {
                        obj.GetComponent<Player>().overheadUI.SetActive(false);
                    }
                }
            }
        }
    }
}
