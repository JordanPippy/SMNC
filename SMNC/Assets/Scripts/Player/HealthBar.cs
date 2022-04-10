using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    public void SetHealth(int health)
    {
        slider.value = health;
        Debug.Log("Setting health to: " + health);
    }

    public void changeHealth(int change)
    {
        slider.value += change;
        Debug.Log("Changing health to: " + slider.value);
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        SetHealth(health);
    }

    public float GetHealth()
    {
        return slider.value;
    }

}
