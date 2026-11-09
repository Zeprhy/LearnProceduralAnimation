using UnityEngine;
using System.Collections;

public class SpiderLeg : MonoBehaviour
{
    [Header("Referensi")]
    public Transform footHome;
    public LayerMask groundLayer;

    [Header("Pengaturan Langkah")]
    public float stepHeight = 0.35f;
    public float stepSpeed = 8f;

    public bool IsStepping { get; private set; } = false;

    public float GetHorizontalDistance()
    {
        if (footHome == null) return 0f;

        Vector3 toHome = footHome.position - transform.position;
        toHome.y = 0f;
        return toHome.magnitude;
    }

    public void ExecuteStep()
    {
        if (!IsStepping)
            StartCoroutine(PerformStep());
    }

    IEnumerator PerformStep()
    {
        IsStepping = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = footHome.position;
        Quaternion targetRot = footHome.rotation;

        Vector3 rayOrigin = footHome.position + Vector3.up * 1.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit , 4f, groundLayer))
        {
            targetPos = hit.point;
            targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * footHome.rotation;
        }

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * stepSpeed;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * stepHeight;
            transform.position = currentPos;

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