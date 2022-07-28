using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIDebugInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;

    public void Update()
    {
        UpdateMessageBlock();
    }

    private void UpdateMessageBlock()
    {
        textBox.SetText(CreateText());
    }

    private string CreateText()
    {
        Player playerInfo = transform.parent.parent.GetComponent<Player>();
        string debugText = "";

        debugText += playerInfo.currentHealth + "/" + playerInfo.maxHealth + " HP\n";
        return debugText;
    }
}

