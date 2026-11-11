using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpiderHybridController : MonoBehaviour
{
    public enum MovementState { GroundWalk, WallClimb, Airborne }

    [Header("Kamera TPP )")]
    public Transform cameraTransform;

    [Header("Layer Permukaan")]
    public LayerMask groundLayer; 
    public LayerMask wallLayer; 

    [Header("Kecepatan")]
    public float moveSpeed = 4f;
    public float turnSpeed = 10f;
    public float wallTurnSpeed = 120f;
    public float tiltSmoothSpeed = 8f;

    [Header("Lompatan & Gravitasi")]
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Pengaturan Merayap Dinding")]
    public float hoverHeight = 0.4f;

    private CharacterController controller;
    public MovementState currentState = GroundedState();
    private float verticalVelocity = 0f;
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 airborneVelocity;

    static MovementState GroundedState() => MovementState.GroundWalk;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update() 
    {
        switch (currentState)
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


    // Mode Lantai Biasa -
    void HandleGroundMovement()
    {
        controller.enabled = true;
        Vector3 moveDir = GetCameraRelativeMoveDirection(Vector3.up);

        if (controller.isGrounded)
        {
            verticalVelocity = -2;
            if (Input.GetKeyDown(KeyCode.Space))
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = moveDir * moveSpeed;
        motion.y = verticalVelocity;

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

        Ray wallRay = new Ray(transform.position + Vector3.up * 0.2f, transform.forward);
        if (Physics.Raycast(wallRay, out RaycastHit wallhit, 1.2f, wallLayer))
        {
            controller.enabled = false;
            surfaceNormal = wallhit.normal;
            currentState = MovementState.WallClimb;// TRANSISI KE MODE TEMBOK
        }
    }

    //Mode Merayap ke Dinding
     void HandleWallClimbing()
    {
        // Lompat dari dinding
        if (Input.GetKeyDown(KeyCode.Space))
        {
            airborneVelocity = (transform.forward * 4f) + (surfaceNormal * jumpForce);
            currentState = MovementState.Airborne;
            return;
        }

        // SENSOR DEPAN: Deteksi jika mendekati lantai dasar agar tidak tembus!
        Ray forwardCheckRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(forwardCheckRay, out RaycastHit frontHit, 1.2f, groundLayer))
        {
            if (Vector3.Dot(frontHit.normal, Vector3.up) > 0.7f)
            {
                surfaceNormal = frontHit.normal;
                transform.position = frontHit.point + Vector3.up * hoverHeight;
                currentState = MovementState.GroundWalk;
                return;
            }
        }

        // Magnet Ke Dinding
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, 2.5f, wallLayer | groundLayer))
        {
            surfaceNormal = hit.normal;
            Vector3 targetPos = hit.point + (surfaceNormal * hoverHeight);
            transform.position = Vector3.Lerp(transform.position, targetPos, 15f * Time.deltaTime);

            if (Vector3.Dot(surfaceNormal, Vector3.up) > 0.9f)
            {
                currentState = MovementState.GroundWalk;
                return;
            }
        }

        // Putar tubuh sejajar dengan permukaan dinding
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

    // Mode Airborne / Melayang (Laser Lurus Menangkap Lantai)
    void HandleAirborne()
    {
        airborneVelocity.y += gravity * Time.deltaTime;
        transform.position += airborneVelocity * Time.deltaTime;

        // Tembak laser lurus ke bawah bumi sejauh 6 meter agar tidak pernah tembus Plane
        Ray landRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(landRay, out RaycastHit landHit, 1.8f, groundLayer | wallLayer))
        {
            surfaceNormal = landHit.normal;

            airborneVelocity = Vector3.zero;
            verticalVelocity = -2f;
            
            if (Vector3.Dot(surfaceNormal, Vector3.up) > 0.7f)
                currentState = MovementState.GroundWalk;
            else    
                currentState = MovementState.WallClimb;
        }
    }
}