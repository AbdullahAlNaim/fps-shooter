using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    float xRotation = 0f;
    float yRotation = 0f;

    public float topClamp = -90f;
    public float bottomClamp = 90f;

    void Start()
    {
        // remove cursor visibility when playing
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // rotation around the x axis (look up and down)
        xRotation -= mouseY; 
        yRotation += mouseX; 

        // Camp the rotation
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        // Apply rotations to our transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

    }
}
