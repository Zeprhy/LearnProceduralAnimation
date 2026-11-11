using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class SpiderLeg : MonoBehaviour
{
    [Header("Referensi")]
    public Transform footHome;
    public LayerMask WalkableLayer;

    [Header("Pengaturan Langkah")]
    public float stepDistance = 0.8f; 
    public float stepHeight = 0.35f;
    public float stepSpeed = 9f;
    public float stepOvershoot = 0.4f;

    public bool IsStepping { get; private set; } = false;

    public float GetHorizontalDistance()
    {
        if (footHome == null) return 0f;

        Vector3 toHome = footHome.position - transform.position;
        Vector3 surfaceDist = Vector3.ProjectOnPlane(toHome, transform.up);
        return surfaceDist.magnitude;
    }

    // Dipanggil serentak oleh SpiderGaitManager8
    public void ExecuteStep(Vector3 moveDirection)
    {
        if (!IsStepping)
        {
            StartCoroutine(PerformStep(moveDirection));
        }
    }

    IEnumerator PerformStep(Vector3 moveDirection)
    {
        IsStepping = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 projectedMove = Vector3.ProjectOnPlane(moveDirection, transform.up).normalized;
        Vector3 predictedHome = footHome.position + (projectedMove * stepOvershoot);
        Vector3 targetPos = predictedHome;
        Quaternion targetRot = footHome.rotation;

        Vector3 rayOrigin = predictedHome + footHome.up * 1f;
        if (Physics.Raycast(rayOrigin, -footHome.up, out RaycastHit hit, 3f, WalkableLayer))
        {
            targetPos = hit.point;
            targetRot = Quaternion.FromToRotation(footHome.up, hit.normal) * footHome.rotation;
        }

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * stepSpeed;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos += footHome.up * (Mathf.Sin(progress * Mathf.PI) * stepHeight);
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