using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    private GameObject localPlayer;
    void Start()
    {
        
    }
    void Update()
    {
        if (localPlayer != null)
        {
            transform.LookAt(localPlayer.transform.position);
        }  
        else
        {
            localPlayer = GameObject.Find("LocalPlayer");
        }
    }
}
