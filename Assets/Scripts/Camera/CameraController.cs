using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomSize = 1f;
    [SerializeField] private float maxZoomSize = 10f;
    [SerializeField] private float smoothZoomTime = 0.2f;

    private Vector3 origin;
    private Vector3 difference;
    private Vector3 resetCamera;

    private bool drag = false;

    private Camera mainCamera;
    private float targetZoomSize;
    private float currentZoomVelocity;

    private void Start()
    {
        mainCamera = Camera.main;
        targetZoomSize = mainCamera.orthographicSize;
        resetCamera = transform.position;
    }

    private void Update()
    {
        MouseDraggingMovement();

        // Camera Movement with WASD
        transform.position += KeyboardMovement() * movementSpeed * Time.deltaTime;

        // Handle camera zoom
        CameraZoom();
    }

    private void MouseDraggingMovement()
    {
        if (Input.GetMouseButton(0))
        {
            difference = (mainCamera.ScreenToWorldPoint(Input.mousePosition)) - transform.position;
            if (!drag)
            {
                drag = true;
                origin = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                origin.y = transform.position.y; // Keep the Y position constant
            }
        }
        else
        {
            drag = false;
        }

        if (drag)
        {
            Vector3 newPosition = origin - difference;
            newPosition.y = transform.position.y; // Keep the Y position constant
            transform.position = new Vector3(
                Mathf.Clamp(newPosition.x, -60f, 60f),
                newPosition.y,
                Mathf.Clamp(newPosition.z, -60f, 60f)
            );
        }
    }

    private Vector3 KeyboardMovement()
    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }

        movement.Normalize();

        return movement;
    }

    private void CameraZoom()
    {
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        targetZoomSize -= scrollWheelInput * zoomSpeed;
        targetZoomSize = Mathf.Clamp(targetZoomSize, minZoomSize, maxZoomSize);
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, targetZoomSize, ref currentZoomVelocity, smoothZoomTime);
    }
}
