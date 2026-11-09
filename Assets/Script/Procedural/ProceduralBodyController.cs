using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ProceduralBodyController : MonoBehaviour
{
    [Header ("Ground Layer")]
    public LayerMask groundLayer;

    [Header ("Camera")]
    public Transform cameraTransform;

    [Header("Kecepatan & Gravitasi")]
    public float moveSpeed = 4f;
    public float turnSpeed = 10f;
    public float gravity = -20f;

    [Header("Dinamika Tubuh (Bobbing)")]
    public float bobFrequency = 10f;
    public float bobAmount = 0.05f;
    public float tiltSmoothSpeed = 5f;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private float bobTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = Vector3.zero;

        if (moveDir.magnitude > 0.1f)
        {
            motion = moveDir * moveSpeed;
            if (controller.isGrounded)
                bobTimer += Time.deltaTime * bobFrequency;
        }
        else
        {
            bobTimer = 0f;
        }

        Vector3 SurfaceNormal = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f, groundLayer))
            SurfaceNormal = hit.normal;

        Vector3 forwardHeading = (moveDir.magnitude > 0.1f) ? moveDir : transform.forward;
        Vector3 projectedForward = Vector3.ProjectOnPlane(forwardHeading, SurfaceNormal).normalized;

        if (projectedForward != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(projectedForward, SurfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, tiltSmoothSpeed * Time.deltaTime);
        }

        motion.y = verticalVelocity;
        if (controller.isGrounded && moveDir.magnitude > 0.1f)
        {
            motion.y += Mathf.Sin(bobTimer) * bobAmount * 5f;
        }
        controller.Move(motion * Time.deltaTime);
    }
}