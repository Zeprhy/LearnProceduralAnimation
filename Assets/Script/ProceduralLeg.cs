using UnityEngine;
using System.Collections;

public class ProceduralLeg : MonoBehaviour
{
    [Header("Referensi")]
    public Transform footHome; // Posisi ideal kaki (anak dari Body)
    
    [Header("Pengaturan Langkah")]
    public float stepDistance = 1.2f; // Jarak maksimal sebelum melangkah
    public float stepHeight = 0.4f;   // Tinggi kaki terangkat saat melangkah
    public float stepSpeed = 5f;      // Kecepatan ayunan kaki

    private bool isStepping = false;

    void Update()
    {
        if (footHome == null) return;
        if (isStepping) return;

        float dist = Vector3.Distance(transform.position, footHome.position);

        if (dist > stepDistance)
        {
            Debug.Log("JARAK KEJAUHAN! KAKI MULAI MELANGKAH.");
            StartCoroutine(PerformStep());
        }
    }

    IEnumerator PerformStep()
    {
        isStepping = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = footHome.position;
        if (Physics.Raycast(footHome.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
        {
            targetPos = hit.point;
        }

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * stepSpeed;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);// Gerakan horizontal: Lerp dari posisi lama ke posisi baru
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * stepHeight;// Gerakan vertikal: Kaki melengkung ke atas pakai Sinus

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPos;
        isStepping = false;
    }
}
