using UnityEngine;

public class SimpleIK : MonoBehaviour
{
    [Header("Titik Sendi")]
    public Transform upperJoint; // Pangkal (Paha)
    public Transform kneeJoint;  // Sendi tengah (Lutut)
    public Transform target;     // Target kaki

    [Header("Panjang Tulang")]
    public float bone1Length = 1f; // Panjang paha
    public float bone2Length = 1f; // Panjang betis

    private void LateUpdate()
    {
        if (upperJoint == null || kneeJoint == null || target == null) return;

        SolveIK();
    }

    void SolveIK()
    {
        Vector3 rootPos = upperJoint.position;
        Vector3 targetPos = target.position;

        // 1. Hitung jarak 3D penuh
        float dist = Vector3.Distance(rootPos, targetPos);
        dist = Mathf.Clamp(dist, 0.001f, bone1Length + bone2Length - 0.001f);

        // 2. Law of Cosines
        float cosUpper = (bone1Length * bone1Length + dist * dist - bone2Length * bone2Length) / (2 * bone1Length * dist);
        cosUpper = Mathf.Clamp(cosUpper, -1f, 1f);

        // Cari tinggi segitiga menggunakan rumus Pythagoras: sin = sqrt(1 - cos^2)
        float sinUpper = Mathf.Sqrt(1f - (cosUpper * cosUpper));

        // 3. Arah garis lurus dari paha menuju telapak kaki
        Vector3 dirToTarget = (targetPos - rootPos).normalized;

        // 4. Arah tonjolan lutut: selalu menghadap ke arah depan paha (upperJoint.forward)
        Vector3 kneeForward = Vector3.ProjectOnPlane(upperJoint.forward, dirToTarget).normalized;
        if (kneeForward == Vector3.zero)
            kneeForward = Vector3.forward;

        // 5. Hitung posisi lutut di ruang 3D:
        // Maju ke arah target sejauh (bone1 * cos), lalu dorong ke arah lutut sejauh (bone1 * sin)
        Vector3 KneePos = rootPos + (dirToTarget * (bone1Length * cosUpper) + (kneeForward * (bone1Length * sinUpper)));
        kneeJoint.position = KneePos;
    }

    void OnDrawGizmos()
    {
        if (upperJoint == null || kneeJoint == null || target == null) return;

        // Gambar garis tulang agar terlihat jelas
        Gizmos.color = Color.green;
        Gizmos.DrawLine(upperJoint.position, kneeJoint.position);
        Gizmos.DrawLine(kneeJoint.position, target.position);
    }
}
