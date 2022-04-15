using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public List<AbilityBase> abilities;
    void Start()
    {

    }

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
}
