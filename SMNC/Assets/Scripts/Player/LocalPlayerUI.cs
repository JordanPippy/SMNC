using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class LocalPlayerUI : NetworkBehaviour
{
    private Player player;
    public GameObject overheadUI;
    private GameObject nameTagObj, overheadHealthBarObj;
    public PlayerUIMessageBar messageBar;
    public TextMeshProUGUI statusEffectUI;
    private HealthBar overheadHealthBar;
    public HealthBar healthBar;
    public float pingUpdateFrequency = 2.0f;
    public bool valuesSetFromNetwork = false; // My attempt to fix race conditions and improve performance.
    private bool nameSet = false;
    public float lastRttTime;
    [SyncVar] public double curRtt;

    void Start()
    {
        player = GetComponent<Player>();
        overheadUI.SetActive(!isLocalPlayer); // Disable the clients overhead info on their end.
        GetComponentInChildren<Canvas>().enabled = isLocalPlayer; // Enable GUI canvas only for local player.

        if (isLocalPlayer)
        {
            lastRttTime = Time.time; // Begin the timer for the Rtt updates.
            valuesSetFromNetwork = true; // Values should already be set for the client owned object.
            healthBar.SetMaxHealth(player.maxHealth);
        }
        else
        {
            nameTagObj = overheadUI.transform.Find("NameTag").gameObject;
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(player.playerNameNetwork);

            overheadHealthBarObj = overheadUI.transform.Find("Health Bar").gameObject;
            overheadHealthBar = overheadHealthBarObj.GetComponent<HealthBar>();
            overheadHealthBar.SetHealth(player.currentHealth);
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
            if (!nameSet && player.playerNameNetwork != "")
            {
                nameTagObj.GetComponent<TextMeshProUGUI>().SetText(player.playerNameNetwork);
                nameSet = true;
            }

            if (overheadHealthBar.GetHealth() == 0f)
            {
                overheadHealthBar.SetHealth(player.currentHealth);
            }
            
            valuesSetFromNetwork = (nameSet && overheadHealthBar.GetHealth() != 0f); // Stop running this code if the values are set.
        }

        if (isLocalPlayer)
        {
            UpdateOverheadUIVisibility();
            UpdateRtt();
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
                        obj.GetComponent<LocalPlayerUI>().overheadUI.SetActive(true);
                    }
                    else
                    {
                        obj.GetComponent<LocalPlayerUI>().overheadUI.SetActive(false);
                    }
                }
            }
        }
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

    [TargetRpc]
    public void SendMessageToPlayer(NetworkConnection target, string message)
    {
        GameObject.Find("LocalPlayer").GetComponent<LocalPlayerUI>().messageBar.AddMessage(message);
    }

    [ClientRpc]
    public void SendMessageToAll(string message)
    {
        GameObject.Find("LocalPlayer").GetComponent<LocalPlayerUI>().messageBar.AddMessage(message);
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
    void UpdateNetworkRTT(double rtt)
    {
        curRtt = rtt;
    }
}
