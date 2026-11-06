using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class ProceduralBodyController : MonoBehaviour
{
    [Header ("Deteksi Lantai")]
    public LayerMask GroundLayer;

    [Header ("Referensi Kaki")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Kecepatan")]
    public float moveSpeed = 3f;     
    public float turnSpeed = 10f;   

    [Header("Dinamika Tubuh (Bobbing)")]
    public float bobFrequency = 10f;  
    public float bobAmount = 0.08f;  

    private float baseHeightOffset;      
    private float bobTimer = 0f;

    private ProceduralLeg leftLegScript;
    private ProceduralLeg rightLegScript;

    void Start()
    {
        if (leftFoot != null) leftLegScript = leftFoot.GetComponent<ProceduralLeg>();
        if (rightFoot != null) rightLegScript = rightFoot.GetComponent<ProceduralLeg>();

        float InitialFootY = (leftFoot.position.y + rightFoot.position.y) / 2f; 
        baseHeightOffset = transform.position.y - InitialFootY;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveInput = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveInput.magnitude > 0.1f)
        {
            
            transform.position += moveInput * moveSpeed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Hitung Efek Bobing dengan sinus saat berjalan
            bobTimer += Time.deltaTime * bobFrequency;
        }
        else
        {
            bobTimer = 0f;
        }

        float groundedFootY = GetGroundedFootY();

        // Ketinggian Tubuh = Ketinggian Lantai + Jarak 1.3m + Efek Bobbing
        float bobOffset = (moveInput.magnitude > 0.1f) ? Mathf.Sin(bobTimer) * bobAmount : 0f;
        float targetBodyY = groundedFootY + baseHeightOffset + bobOffset;

        float SmoothY = Mathf.Lerp(transform.position.y, targetBodyY, 8f * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, SmoothY, transform.position.z);
    }

     float GetGroundedFootY()
    {
        bool leftStepping = leftLegScript != null && leftLegScript.IsStepping;
        bool rightStepping = rightLegScript != null && rightLegScript.IsStepping;

        // Jika kaki kiri melayang, tumpuan murni pada kaki kanan
        if (leftStepping && !rightStepping)
        {
            return rightFoot.position.y;
        }
        // Jika kaki kanan melayang, tumpuan murni pada kaki kiri
        if (rightStepping && !leftStepping)
        {
            return leftFoot.position.y;
        }

        // Jika kedua kaki menapak (idle / double stance), gunakan rata-rata keduanya
        return (leftFoot.position.y + rightFoot.position.y) / 2f;
    }
}