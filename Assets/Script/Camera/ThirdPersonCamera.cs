using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdCameraMotion : MonoBehaviour
{
    [Header ("Camera Target")]
    public Transform Target;

    [Header ("Distance Settings & Position")]
    public float distance = 5f;
    public float height = 2f;
    public float smoothSpeed = 10f;

    [Header ("Mouse Sensitivity")]
    public float mouseSensitivity = 3f;
    public float minPitch = -10f;
    public float maxPitch = 60f;

    private float yaw = 0f;
    private float pitch = 15f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    void LateUpdate()
    {
        if (Target == null) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion CameraRot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetFocusPos = Target.position + Vector3.up * (height * 0.5f);
        Vector3 desiredPostion =  targetFocusPos - (CameraRot * Vector3.forward * distance);

        transform.position = Vector3.Lerp(transform.position, desiredPostion, smoothSpeed * Time.deltaTime);
        transform.LookAt(targetFocusPos);
    }
}
