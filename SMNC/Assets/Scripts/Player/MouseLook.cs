using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class MouseLook : NetworkBehaviour
{
    [SerializeField] public float sensitivity;
    [SerializeField] public Camera mainCamera;
    public float mouseX = 0;
    public float mouseY = 0;
    [SyncVar]
    private Vector3 networkRotation;

    void Start()
    {
        // We do what no Unity games can figure out. Lock that cursor.
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            transform.localEulerAngles = networkRotation;
        else
        {
            // Unlock the cursor by pressing Escape.
            if (Input.GetKey(KeyCode.Escape))
                Cursor.lockState = CursorLockMode.None;

            // Don't bother updating if nothing has changed.
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y")!= 0)
            {
                // Grab the mouse delta from the last frame.
                mouseX += Input.GetAxis("Mouse X") * sensitivity;
                mouseY -= Input.GetAxis("Mouse Y") * sensitivity;

                // Lock the viewing angles to be +-90 degrees.
                mouseY = Mathf.Clamp(mouseY, -90f, 90f);
                mouseX %= GameManager.FULL_CIRCLE_DEGREES;

                // Only rotate the camera along the X-axis (Up and down)
                mainCamera.transform.localEulerAngles = new Vector3(mouseY, 0, 0);

                // Only rotate the player along the Y-axis (Left and right)
                transform.localEulerAngles = new Vector3(0, mouseX, 0);

                UpdateNetworkRotate(transform.localEulerAngles);
            }
        }
    }

    // No server validation, because you can't cheat with this alone.
    [Command]
    void UpdateNetworkRotate(Vector3 rot)
    {
        networkRotation = rot;
    }

}
