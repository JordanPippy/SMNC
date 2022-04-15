using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Database
{
    public static List<AbilityBase> abilities = new List<AbilityBase>();

    public static void FillAbilities()
    {
        abilities.Add(ScriptableObject.CreateInstance<TestAbility>());
    }

    public static AbilityBase GetAbility(int index)
    {
        return abilities[index];
    }

    static Database()
    {
        FillAbilities();
    }
}
