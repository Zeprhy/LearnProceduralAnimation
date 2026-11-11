using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Pengaturan Jarak")]
    public float defaultDistance = 5f;  // Jarak normal kamera
    public float minDistance = 1f;      // Jarak terdekat saat terhalang tembok
    public float height = 1.5f;         // Ketinggian kamera di atas laba-laba
    public float smoothSpeed = 12f;

    [Header("Deteksi Tembok (Anti-Tembus)")]
    public LayerMask obstacleLayers;    // Pilih layer Wall dan Ground

    [Header("Sensitivitas Mouse")]
    public float mouseSensitivity = 3f;
    public float minPitch = -15f;
    public float maxPitch = 65f;

    private float yaw = 0f;
    private float pitch = 15f;
    private float currentDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentDistance = defaultDistance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetFocusPos = target.position + (Vector3.up * height);
        Vector3 desiredCameraDir = -(cameraRotation * Vector3.forward);

        float targetDist = defaultDistance;
        if (Physics.SphereCast(targetFocusPos, 0.2f, desiredCameraDir, out RaycastHit hit, defaultDistance, obstacleLayers))
            targetDist = Mathf.Clamp(hit.distance - 0.1f, minDistance, defaultDistance);

        currentDistance = Mathf.Lerp(currentDistance, targetDist, 15f * Time.deltaTime);

        Vector3 finalPosition = targetFocusPos + (desiredCameraDir * currentDistance);
        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed * Time.deltaTime);

        transform.LookAt(targetFocusPos);
    }
}