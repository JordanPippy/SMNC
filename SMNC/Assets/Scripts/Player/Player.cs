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
    public TextMeshProUGUI statusEffectUI;
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

    public List<AbilityBase> abilities = new List<AbilityBase>();
    public List<StatusEffectInfo> statusEffects = new List<StatusEffectInfo>();

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
        }
        else
        {
            GetComponentInChildren<Canvas>().enabled = false;
            overheadHealthBar.SetHealth(currentHealth);
        }

        abilities.Add(GameManager.Instance.GetAbility("StunShot"));
        abilities.Add(GameManager.Instance.GetAbility("TestAbility2"));
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
        
        UpdateStatusAffects();
    }

    void UpdateClient()
    {
        if (Input.GetKeyDown(KeyCode.P))
            UseAbility(0);
        if (Input.GetKeyDown(KeyCode.K))
            UseAbility(1);
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

    // Activate each status effect active on the player.
    void UpdateStatusAffects()
    {
        List<StatusEffectInfo> toBeRemoved = new List<StatusEffectInfo>();
        string UIText = "";
        foreach(StatusEffectInfo effect in statusEffects)
        {
            if (effect.elapsed <= 0)
            {
                effect.status.StartEffect(this.gameObject);
            }
            else if (effect.elapsed >= effect.duration)
            {
                effect.status.EndEffect(this.gameObject);
                toBeRemoved.Add(effect);
            }
            else
            {
                if (Time.time - effect.lastDuringCall >= effect.status.tickRate)
                {
                    effect.lastDuringCall = Time.time;
                    effect.status.DuringEffect(this.gameObject);
                }
            }

            effect.elapsed += Time.deltaTime;

            if (isLocalPlayer)
                UIText += (effect.status.statusName + ": " + (effect.duration - effect.elapsed) + "\n");
        }

        if (isLocalPlayer)
            statusEffectUI.SetText(UIText);

        foreach(StatusEffectInfo effect in toBeRemoved)
        {
            statusEffects.Remove(effect);
        }
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

    void UseAbility(int abilityIndex)
    {
        RPCUseAbility(abilityIndex);
    }

    [Command]
    void RPCUseAbility(int abilityIndex)
    {
        RPCUseAbilityClient(abilityIndex);
    }

    [ClientRpc]
    void RPCUseAbilityClient(int abilityIndex)
    {
        abilities[abilityIndex].Use(transform);
    }

    [ClientRpc]
    void RpcAddStatusEffect(int statusIndex, float duration)
    {
        statusEffects.Add(new StatusEffectInfo(GameManager.Instance.GetStatusEffect(statusIndex), duration));
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
                Projectile proj = other.gameObject.GetComponent<Projectile>();
                int damage = proj.damage;
                currentHealth -= damage;
                RpcChangeHealthBar(-damage);

                // If the projectile has a status effect applied to it, apply it.
                if (other.gameObject.TryGetComponent(out ApplyStatusEffect statusEff))
                {
                    if (statusEff.OnHitEffect != null)
                    {
                        RpcAddStatusEffect(GameManager.Instance.GetStatusEffectIndex(statusEff.OnHitEffect.effect), statusEff.OnHitEffect.duration);
                    }
                }

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

[Serializable]
public class StatusEffectInfo
{
    public StatusEffect status;
    public float lastDuringCall = 0; // When was During last called?
    public float elapsed = 0; // How long the effect has ran for.
    public float duration = 0; // How long the effect lasts for.

    public StatusEffectInfo(StatusEffect status, float duration)
    {
        this.status = status;
        this.duration = duration;
    }
}
