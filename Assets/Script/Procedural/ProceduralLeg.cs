using UnityEngine;
using System.Collections;

public class ProceduralLeg : MonoBehaviour
{
    [Header("Referensi")]
    public Transform footHome;
    public ProceduralLeg otherLeg;
    public LayerMask groundLayer;

    [Header("Pengaturan Langkah")]
    public float stepDistance = 0.75f;
    public float stepHeight = 0.35f;   
    public float stepSpeed = 7f;  

    public bool IsStepping { get; private set; } = false;

    void Update()
    {
        if (footHome == null) return;
        if (IsStepping) return;

        // Jangan melangkah jika kaki sebelah belum selesai menapak
        if (otherLeg != null && otherLeg.IsStepping) return;

        // Hitung jarak horizontal murni (X & Z)
        Vector3 toHome = footHome.position - transform.position;
        toHome.y = 0f;
        float horizontalDist = toHome.magnitude;

        if (horizontalDist > stepDistance)
        {
            StartCoroutine(PerformStep());
        }
    }

    IEnumerator PerformStep()
    {
        IsStepping = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = footHome.position;
        // Target rotasi diselaraskan dengan rotasi footHome (mengikuti arah hadap tubuh)
        Quaternion targetRot = footHome.rotation;

        // Tembak laser dari ketinggian aman 1.0m di atas footHome
        if (Physics.Raycast(footHome.position + Vector3.up * 1.0f, Vector3.down, out RaycastHit hit, 3f, groundLayer))
        {
            targetPos = hit.point;
            // Selaraskan kemiringan telapak kaki dengan kontur permukaan lantai
            targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * footHome.rotation;
        }

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * stepSpeed;

            // Perpindahan posisi (Parabola Lerp + Sinus)
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * stepHeight;
            transform.position = currentPos;

            // Perpindahan rotasi halus
            transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        IsStepping = false;
    }

    void OnDrawGizmos()
    {
        if (footHome != null)
        {
            Gizmos.color = IsStepping ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, footHome.position);
        }
    }
}