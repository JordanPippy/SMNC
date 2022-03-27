using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour
{
    private NetworkVariable<Vector3> networkRotation = new NetworkVariable<Vector3>();
    [SerializeField] public float sensitivity;
    [SerializeField] public Camera mainCamera;
    public float mouseX = 0;
    public float mouseY = 0;

    void Start()
    {
        // We do what no Unity games can figure out. Lock that cursor.
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer)
            transform.localEulerAngles = networkRotation.Value;
        else
        {
            // Unlock the cursor by pressing Escape.
            if (Input.GetKey(KeyCode.Escape))
                Cursor.lockState = CursorLockMode.None;

            // Grab the mouse delta from the last frame.
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY -= Input.GetAxis("Mouse Y") * sensitivity;

            // Lock the viewing angles to be +-90 degrees.
            mouseY = Mathf.Clamp(mouseY, -90f, 90f);

            // Only rotate the camera along the X-axis (Up and down)
            mainCamera.transform.localEulerAngles = new Vector3(mouseY, 0, 0);

            // Only rotate the player along the Y-axis (Left and right)
            transform.localEulerAngles = new Vector3(0, mouseX, 0);
            
            RequestRotationServerRpc(transform.localEulerAngles);
        }
    }

    [ServerRpc]
    public void RequestRotationServerRpc(Vector3 pos)
    {
        networkRotation.Value = pos;
    }
}
