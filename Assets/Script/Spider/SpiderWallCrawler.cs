using UnityEngine;

public class SpiderwallCrawler : MonoBehaviour
{
    [Header("Wall or Floor Detection")]
    public LayerMask groundLayer;
    public float hoverHeight = 0.4f;      // Jarak perut dari permukaan
    public float surfaceCheckDist = 2.5f;

    [Header("creeping speed")]
    public float moveSpeed = 4f;
    public float turnSpeed = 120f;

    [Header("Jump Feature (Space(Key))")]
    public float jumpForce = 8f;          
    public float jumpGravity = 15f;     

    // Status Laba-laba
    private bool isGrounded = true;   
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 airborneVelocity;

    void Update()
    {
        if (!isGrounded)
        {
            airborneVelocity.y -= jumpGravity * Time.deltaTime;
            transform.position += airborneVelocity * Time.deltaTime;

            Ray landRay = new Ray(transform.position, transform.forward + Vector3.down * 0.5f);
            if (Physics.Raycast(landRay, out RaycastHit landHit, 1.5f, groundLayer))
            {
                isGrounded = true;
                surfaceNormal = landHit.normal;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isGrounded = false;
            airborneVelocity = (transform.forward * 4f) + (surfaceNormal * jumpForce);  // Loncat serong ke depan-atas permukaan
            return;
        }

        Ray ray = new Ray(transform.position, -transform.up); //Magnet Penempel Permukaan
        if (Physics.Raycast(ray, out RaycastHit hit, surfaceCheckDist, groundLayer))
        {
            surfaceNormal = hit.normal;
            Vector3 targetPos = hit.point + (surfaceNormal * hoverHeight);
            transform.position = Vector3.Lerp(transform.position, targetPos, 15f * Time.deltaTime);
        }
        else
        {
            if (Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up + transform.forward
            , out RaycastHit cornerHit, surfaceCheckDist, groundLayer)) // Deteksi sudut siku-siku tembok di depan
                surfaceNormal = cornerHit.normal;
        }

        //Putar Tubuh Selaras dengan Kemiringan Permukaan
        Quaternion targetRot = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (Mathf.Abs(v) > 0.1f)
            transform.position += transform.forward * (v * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(h) > 0.1f)
            transform.Rotate(Vector3.up, h * turnSpeed * Time.deltaTime, Space.Self);
    }
}