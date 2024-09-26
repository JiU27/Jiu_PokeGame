using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 80f; // 上下最大旋转角度
    public float maxHorizontalAngle = 180f; // 左右最大旋转角度

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private Transform playerCamera;

    private void Start()
    {
        playerCamera = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 水平旋转（左右）
        horizontalRotation += mouseX;
        horizontalRotation = Mathf.Clamp(horizontalRotation, -maxHorizontalAngle, maxHorizontalAngle);

        // 垂直旋转（上下）
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // 处理移动输入
        float moveForward = Input.GetAxis("Vertical");

        Vector3 movement = transform.forward * moveForward;

        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}