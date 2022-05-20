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
    [SyncVar] public string playerNameNetwork;
    public string playerName;
    public SkinnedMeshRenderer mesh;
    public MouseLook mouseLook;
    [SerializeField] private GameObject tempAbilityObject;
    [SyncVar] public int maxHealth = 100;
    [SyncVar] public int currentHealth;
    private Camera headCamera;
    public List<AbilityBase> abilities = new List<AbilityBase>();
    public List<StatusEffectInfo> statusEffects = new List<StatusEffectInfo>();
    public LocalPlayerUI playerUI;

    void Start()
    {
        playerUI = GetComponent<LocalPlayerUI>();
        SetupPlayer(); // Initialize a player.

        abilities.Add(GameManager.Instance.GetAbility("Shoot"));
        abilities.Add(GameManager.Instance.GetAbility("Throw"));
        abilities.Add(GameManager.Instance.GetAbility("StunShot"));
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            UpdateClient();
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
        if (Input.GetButtonDown("Fire1"))
            UseAbility(0);
        if (Input.GetKeyDown(KeyCode.P))
            UseAbility(1);
        if (Input.GetKeyDown(KeyCode.K))
            UseAbility(2);
    }

    void Die()
    {
        GetComponent<Movement>().ForceMoveClient(SpawnManager.Instance.GetAvailableSpawnPoint());
        currentHealth = maxHealth;
        playerUI.RpcSetHealthBar(maxHealth);
        playerUI.SendMessageToAll(playerNameNetwork + " died.");
    }

    void UpdateServer()
    {
        if (currentHealth <= 0)
            Die();
    }

    void SetupPlayer()
    {
        gameObject.transform.Find("HeadCamera").gameObject.SetActive(isLocalPlayer); // Disable camera if not owned by the player.

        if (isServer)
        {
            maxHealth = 100;
            currentHealth = maxHealth;
            GetComponent<Movement>().ForceMoveClient(SpawnManager.Instance.GetAvailableSpawnPoint());
        }

        if (isLocalPlayer)
        {
            playerName = GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().playerName; // Get playername from the network manager GUI.
            UpdatePlayerName(playerName);

            gameObject.name = "LocalPlayer"; // Set the clients personal gameobject's name.
            mesh.enabled = false; // The client does not need to see their own body.

            headCamera = gameObject.transform.Find("HeadCamera").GetComponent<Camera>();
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
            playerUI.statusEffectUI.SetText(UIText);

        foreach(StatusEffectInfo effect in toBeRemoved)
        {
            statusEffects.Remove(effect);
        }
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

    [Command]
    void RequestTakeDamage(int amount)
    {
        currentHealth -= amount;
        playerUI.RpcChangeHealthBar(-amount);
    }

    [Command]
    void UpdatePlayerName(string nam)
    {
        playerNameNetwork = nam;
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
                playerUI.RpcChangeHealthBar(-damage);

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
