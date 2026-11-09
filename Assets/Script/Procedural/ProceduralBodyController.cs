using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ProceduralBodyController : MonoBehaviour
{
    [Header("Kecepatan & Gravitasi")]
    public float moveSpeed = 4f;
    public float turnSpeed = 10f;
    public float gravity = -20f; // Gravitasi jatuh saat di tebing

    [Header("Dinamika Tubuh (Bobbing)")]
    public float bobFrequency = 10f;
    public float bobAmount = 0.05f;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private float bobTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. Baca Input WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. Gravitasi Otomatis dari CharacterController
        if (controller.isGrounded)
        {
            // Beri sedikit dorongan ke bawah (-2f) agar kapsul selalu merekat di lereng/tangga
            verticalVelocity = -2f;
        }
        else
        {
            // Tarik jatuh bebas jika keluar dari tepi tebing/tangga
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 3. Gerakan Mendatar & Rotasi Halus
        Vector3 motion = Vector3.zero;

        if (moveDir.magnitude > 0.1f)
        {
            motion = moveDir * moveSpeed;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            if (controller.isGrounded)
                bobTimer += Time.deltaTime * bobFrequency;
        }
        else
        {
            bobTimer = 0f;
        }

        // 4. Masukkan Kecepatan Gravitasi ke Gerakan
        motion.y = verticalVelocity;

        // Tambahkan efek membal (bobbing) saat berjalan di lantai
        if (controller.isGrounded && moveDir.magnitude > 0.1f)
        {
            motion.y += Mathf.Sin(bobTimer) * bobAmount * 5f;
        }

        // 5. Jalankan Pergerakan Fisika Kapsul
        controller.Move(motion * Time.deltaTime);
    }
}