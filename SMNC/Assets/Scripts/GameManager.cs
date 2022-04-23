using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Initialize a singleton for the game manager
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public List<AbilityBase> abilities;
    public List<StatusEffect> statuses;

    public AbilityBase GetAbility(int index)
    {
        return abilities[index];
    }

    public AbilityBase GetAbility(string title_)
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i].title == title_)
                return abilities[i];
        }
        return null;
    }

    public StatusEffect GetStatusEffect(int index)
    {
        return statuses[index];
    }

    public StatusEffect GetStatusEffect(string title_)
    {
        for (int i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].statusName == title_)
                return statuses[i];
        }
        return null;
    }

    public int GetStatusEffectIndex(StatusEffect effect)
    {
        for (int i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].statusName == effect.statusName)
                return i;
        }
        return -1;
    }
}
