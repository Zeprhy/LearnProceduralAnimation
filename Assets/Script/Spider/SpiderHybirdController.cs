using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpiderHybridController : MonoBehaviour
{
    public enum MovementState { GroundWalk, WallClimb, Airborne }

    [Header("Kamera TPP")]
    public Transform cameraTransform;

    [Header("Layer Permukaan")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Kecepatan")]
    public float moveSpeed = 4f;
    public float groundTurnSpeed = 10f;
    public float wallTurnSpeed = 120f;
    public float tiltSmoothSpeed = 8f;

    [Header("Lompatan & Gravitasi")]
    public float jumpForce = 9f;
    public float gravity = -20f;

    [Header("Pengaturan Merayap Dinding")]
    public float hoverHeight = 0.28f;

    public MovementState CurrentState { get; private set; } = MovementState.GroundWalk;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 airborneVelocity;
    private float climbCooldown = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (climbCooldown > 0f)
            climbCooldown -= Time.deltaTime;

        switch (CurrentState)
        {
            case MovementState.GroundWalk:
                HandleGroundMovement();
                break;

            case MovementState.WallClimb:
                HandleWallClimbing();
                break;

            case MovementState.Airborne:
                HandleAirborne();
                break;
        }
    }

    Vector3 GetCameraRelativeMoveDirection(Vector3 normal)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, normal).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, normal).normalized;

        return (camForward * v + camRight * h).normalized;
    }

    // 1. MODE LANTAI
    void HandleGroundMovement()
    {
        controller.enabled = true;
        Vector3 moveDir = GetCameraRelativeMoveDirection(Vector3.up);

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = moveDir * moveSpeed;
        motion.y = verticalVelocity;

        // Miring di bukit bola
        Vector3 currentGroundNormal = Vector3.up;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, 3f, groundLayer))
        {
            currentGroundNormal = groundHit.normal;
        }

        Vector3 heading = (moveDir.magnitude > 0.1f) ? moveDir : transform.forward;
        Vector3 projectedHeading = Vector3.ProjectOnPlane(heading, currentGroundNormal).normalized;

        if (projectedHeading != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(projectedHeading, currentGroundNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, tiltSmoothSpeed * Time.deltaTime);
        }

        controller.Move(motion * Time.deltaTime);

        // TRANSISI DARI LANTAI KE TEMBOK 90 DERAJAT:
        Ray wallRay = new Ray(transform.position + Vector3.up * 0.2f, transform.forward);
        if (Physics.Raycast(wallRay, out RaycastHit wallHit, 1.4f, wallLayer))
        {
            controller.enabled = false;
            surfaceNormal = wallHit.normal;

            transform.position = wallHit.point + (surfaceNormal * hoverHeight);
            transform.rotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;

            climbCooldown = 0.5f;
            CurrentState = MovementState.WallClimb;
        }
    }

    // 2. MODE MERAYAP TEMBOK
    void HandleWallClimbing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            airborneVelocity = (transform.forward * 4f) + (surfaceNormal * jumpForce);
            CurrentState = MovementState.Airborne;
            return;
        }

        // Magnet Dinding
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, wallLayer | groundLayer))
        {
            surfaceNormal = hit.normal;
            Vector3 targetPos = hit.point + (surfaceNormal * hoverHeight);
            transform.position = Vector3.Lerp(transform.position, targetPos, 15f * Time.deltaTime);

            // Hanya izinkan kembali ke lantai jika masa jeda 0.5 detik sudah habis!
            if (climbCooldown <= 0f && Vector3.Dot(surfaceNormal, Vector3.up) > 0.9f)
            {
                CurrentState = MovementState.GroundWalk;
                return;
            }
        }
        else
        {
            // Jika keluar dari tepi dinding ke ruang kosong: Jatuh bebas!
            CurrentState = MovementState.Airborne;
            airborneVelocity = transform.forward * 2f;
            return;
        }

        Quaternion targetRot = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (Mathf.Abs(v) > 0.1f)
        {
            transform.position += transform.forward * (v * moveSpeed * Time.deltaTime);
        }

        if (Mathf.Abs(h) > 0.1f)
        {
            transform.Rotate(Vector3.up, h * wallTurnSpeed * Time.deltaTime, Space.Self);
        }
    }

    // 3. MODE AIRBORNE
    void HandleAirborne()
    {
        airborneVelocity.y += gravity * Time.deltaTime;
        transform.position += airborneVelocity * Time.deltaTime;

        Ray landRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(landRay, out RaycastHit landHit, 1.8f, groundLayer | wallLayer))
        {
            surfaceNormal = landHit.normal;

            airborneVelocity = Vector3.zero;
            verticalVelocity = -2f;
            transform.position = landHit.point + (surfaceNormal * hoverHeight);

            if (Vector3.Dot(surfaceNormal, Vector3.up) > 0.7f)
                CurrentState = MovementState.GroundWalk;
            else
                CurrentState = MovementState.WallClimb;
        }
    }
}