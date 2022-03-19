using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
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
    }
}
