using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float horizontalSpeed = 2f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float minVerticalAngle = -45f;
    [SerializeField] private float maxVerticalAngle = 45f;
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;

    private float rotationX;
    private float rotationY;

    public Quaternion PlanarRotation => Quaternion.Euler(0f, rotationY, 0f);

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {   
        rotationX += InputManager.Instance.LookInput.y * verticalSpeed * (invertX ? -1f : 1f);
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += InputManager.Instance.LookInput.x * horizontalSpeed * (invertY ? -1f : 1f);
    }
    private void LateUpdate()
    {
        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0f);

        transform.position = followTarget.position - targetRotation * offset;
        transform.rotation = targetRotation;
    }
}
