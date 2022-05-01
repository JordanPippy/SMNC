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
    public PlayerUIMessageBar messageBar;
    public TextMeshProUGUI statusEffectUI;
    private HealthBar overheadHealthBar;
    public HealthBar healthBar;
    public SkinnedMeshRenderer mesh;
    public MouseLook mouseLook;
    [SerializeField] private GameObject tempAbilityObject;
    [SyncVar] public string playerNameNetwork;
    [SyncVar] public int maxHealth = 100;
    [SyncVar] public int currentHealth;
    [SyncVar] public double curRtt;
    public float pingUpdateFrequency = 2.0f;
    public float lastRttTime;
    public bool valuesSetFromNetwork = false; // My attempt to fix race conditions and improve performance.
    private bool nameSet = false;

    private Camera headCamera;

    public List<AbilityBase> abilities = new List<AbilityBase>();
    public List<StatusEffectInfo> statusEffects = new List<StatusEffectInfo>();

    void Start()
    {
        SetupPlayer(); // Initialize a player.

        //abilities.Add(GameManager.Instance.GetAbility("TestAbility2"));
        abilities.Add(GameManager.Instance.GetAbility("Throw"));
        abilities.Add(GameManager.Instance.GetAbility("StunShot"));
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

    void FixedUpdate()
    {
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
        GetComponent<Movement>().ForceMoveClient(SpawnManager.Instance.GetAvailableSpawnPoint());
        currentHealth = maxHealth;
        RpcSetHealthBar(maxHealth);
        SendMessageToAll(playerNameNetwork + " died.");
    }

    void UpdateServer()
    {
        if (currentHealth <= 0)
            Die();
    }

    void SetupPlayer()
    {
        overheadUI.SetActive(!isLocalPlayer); // Disable the clients overhead info on their end.
        gameObject.transform.Find("HeadCamera").gameObject.SetActive(isLocalPlayer); // Disable camera if not owned by the player.
        GetComponentInChildren<Canvas>().enabled = isLocalPlayer; // Enable GUI canvas only for local player.

        if (isServer)
        {
            maxHealth = 100;
            currentHealth = maxHealth;
            GetComponent<Movement>().ForceMoveClient(SpawnManager.Instance.GetAvailableSpawnPoint());
        }

        if (isLocalPlayer)
        {
            string playerName = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().playerName; // Get playername from the network manager GUI.
            UpdatePlayerName(playerName);

            gameObject.name = "LocalPlayer"; // Set the clients personal gameobject's name.
            mesh.enabled = false; // The client does not need to see their own body.
            lastRttTime = Time.time; // Begin the timer for the Rtt updates.
            valuesSetFromNetwork = true; // Values should already be set for the client owned object.

            headCamera = gameObject.transform.Find("HeadCamera").GetComponent<Camera>();

            healthBar.SetMaxHealth(maxHealth);   
        }
        else
        {
            nameTagObj = overheadUI.transform.Find("NameTag").gameObject;
            nameTagObj.GetComponent<TextMeshProUGUI>().SetText(playerNameNetwork);

            overheadHealthBarObj = overheadUI.transform.Find("Health Bar").gameObject;
            overheadHealthBar = overheadHealthBarObj.GetComponent<HealthBar>();
            overheadHealthBar.SetHealth(currentHealth);
        }
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
                if (effect.status.tickRate >= 0)
                {
                    if (Time.time - effect.lastDuringCall >= effect.status.tickRate)
                    {
                        effect.lastDuringCall = Time.time;
                        effect.status.DuringEffect(this.gameObject);
                    }
                }
            }

            effect.elapsed += Time.deltaTime;

            if (isLocalPlayer)
                UIText += (effect.status.statusName + ": " + (effect.GetTimeLeft()) + "\n");
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
        // Resetting values for the temp gameobject.
        tempAbilityObject.transform.position = Vector3.zero;
        tempAbilityObject.transform.localEulerAngles = Vector3.zero;
        tempAbilityObject.transform.rotation = Quaternion.identity;

        // Set gameobject position and rotation to a combination of the clients viewing angle and position.
        tempAbilityObject.transform.position = new Vector3(transform.position.x, transform.position.y + 1.727f, transform.position.z);
        tempAbilityObject.transform.Rotate(mouseLook.cameraLookRot.x, transform.localEulerAngles.y, 0);
        abilities[abilityIndex].Use(tempAbilityObject.transform);
    }

    [ClientRpc]
    void RpcAddStatusEffect(int statusIndex, float duration)
    {
        StatusEffect effect = GameManager.Instance.GetStatusEffect(statusIndex);
        
        if (effect.duplicateType == StatusEffect.DuplicateHandling.Stack) // Apply both effects simultaneously.
        {
            statusEffects.Add(new StatusEffectInfo(effect, duration));
        }
        else if (effect.duplicateType == StatusEffect.DuplicateHandling.Extend || effect.duplicateType == StatusEffect.DuplicateHandling.Ignore)
        {
            int existingEffectIndex = statusEffects.FindIndex(x => x.status == effect);

            if (existingEffectIndex != -1) // If found existing effect.
            {
                if (effect.duplicateType == StatusEffect.DuplicateHandling.Extend)
                {
                    if (duration > statusEffects[existingEffectIndex].GetTimeLeft()) // Extend the duration if longer.
                    {
                        statusEffects[existingEffectIndex].duration = duration; // Just extend duration of existing effect.
                        statusEffects[existingEffectIndex].elapsed = 0;
                    }
                }
            }
            else
            {
                statusEffects.Add(new StatusEffectInfo(effect, duration));
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

    [TargetRpc]
    void SendMessageToPlayer(NetworkConnection target, string message)
    {
        GameObject.Find("LocalPlayer").GetComponent<Player>().messageBar.AddMessage(message);
    }

    [ClientRpc]
    void SendMessageToAll(string message)
    {
        GameObject.Find("LocalPlayer").GetComponent<Player>().messageBar.AddMessage(message);
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

    public float GetTimeLeft()
    {
        return duration - elapsed;
    }
}
