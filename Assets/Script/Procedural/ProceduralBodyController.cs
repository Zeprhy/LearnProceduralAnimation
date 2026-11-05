using UnityEngine;

public class ProceduralBodyController : MonoBehaviour
{
    [Header("Kecepatan")]
    public float moveSpeed = 3f;      // Kecepatan jalan
    public float turnSpeed = 10f;     // Kecepatan putar badan

    [Header("Dinamika Tubuh (Bobbing)")]
    public float bobFrequency = 10f;  // Kecepatan naik-turun tubuh
    public float bobAmount = 0.08f;   // Seberapa dalam tubuh membal saat melangkah

    private float baseHeight;         // Menyimpan ketinggian normal tubuh
    private float bobTimer = 0f;

    void Start()
    {
        baseHeight = transform.position.y;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveInput = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveInput.magnitude > 0.1f)
        {
            //Pindah Posisi Tubuh
            transform.position += moveInput * moveSpeed * Time.deltaTime;

            // Putar badan menghadap ke arah jalan secara halus
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Hitung Efek Bobing dengan sinus saat berjalan
            bobTimer += Time.deltaTime * bobFrequency;
        }
        else
        {
            bobTimer = 0f;
        }

        //Deteksi Ketinggian lantai di bawah tubuh
        // Tembak laser dari atas tubuh ke lantai
        float currentGroundY = 0f;
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
            if (hit.transform != transform)
                currentGroundY = hit.point.y;// Ketinggian lantai tanjakan
            

        // Ketinggian Tubuh = Ketinggian Lantai + Jarak 1.3m + Efek Bobbing
        float bobOffset = (moveInput.magnitude > 0.1f) ? Mathf.Sin(bobTimer) * bobAmount : 0f;
        float targetBodyY = currentGroundY + baseHeight + bobOffset;

        float SmoothY = Mathf.Lerp(transform.position.y, targetBodyY, 10f * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, SmoothY, transform.position.z);
    }
}