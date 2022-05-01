using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIMessageBar : MonoBehaviour
{
    [SerializeField] private int maximumLines = 10;
    [SerializeField] private float visibleTime = 10.0f;
    private List<string> messages = new List<string>();
    [SerializeField] private TextMeshProUGUI textBox;
    private Coroutine currentHideTimer;
    

    public void Awake()
    {
        UpdateMessageBlock();
    }
    
    public void AddMessage(string message)
    {
        while(messages.Count >= maximumLines)
        {
            messages.RemoveAt(0);
        }

        messages.Add(message);

        UpdateMessageBlock();
    }

    private void UpdateMessageBlock()
    {
        string tempMsg = "";

        if (currentHideTimer != null)
            StopCoroutine(currentHideTimer); // Stop the hide timer if a new message is received.

        foreach(string line in messages)
        {
            tempMsg += line + "\n";
        }

        textBox.SetText(tempMsg);
        textBox.enabled = true; // Show the message box if it was hidden.

        currentHideTimer = StartCoroutine(hideMessageBox()); // Start the hide timer.
    }

    IEnumerator hideMessageBox()
    {
        yield return new WaitForSeconds(visibleTime);
        textBox.enabled = false;
        messages.Clear();
    }
}
