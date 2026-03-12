using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ExplorerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation = 0f;

    [SerializeField] private Transform cameraTransform;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveSpeed * move + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }
}