using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 5.0f;
    public float rotationSpeed = 100.0f;

    void Update()
    {
        // WASD tu�lar� ile hareket
        float horizontalInput = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        float verticalInput = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;

        transform.Translate(horizontalInput, 0, verticalInput);

        // Mouse ile kamera d�n���
        if (Input.GetMouseButton(1)) // Sa� t�k bas�l�yken
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.eulerAngles += new Vector3(mouseY, mouseX, 0);
        }
    }
}

