using UnityEngine;
using UnityEngine.UIElements;

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
        // 1. KUNCI SUMBU Z DI SINI (tambahkan baris ini paling atas)
        target.position = new Vector3(target.position.x, target.position.y, upperJoint.position.z);

        Vector3 rootPos = upperJoint.position;
        Vector3 targetPos = target.position;

        float dist = Vector3.Distance(rootPos, targetPos);
        dist = Mathf.Clamp(dist, 0.001f, bone1Length + bone2Length - 0.001f);

        // 2. Law of Cosines
        float cosKnee = (bone1Length * bone1Length + bone2Length * bone2Length - dist * dist) / (2 * bone1Length * bone2Length);
        float KneeAngle = Mathf.Acos(Mathf.Clamp(cosKnee, -1f, 1f)) * Mathf.Rad2Deg;

        // 3. Arah target
        Vector3 dirToTarget = (targetPos - rootPos).normalized;
        float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

        // Sudut paha
        float cosUpper = (bone1Length * bone1Length + dist * dist - bone2Length * bone2Length) / (2 * bone1Length * dist);
        float upperAngle = targetAngle - (Mathf.Acos(Mathf.Clamp(cosUpper, -1f, 1f)) * Mathf.Rad2Deg);

        // 4. Hitung posisi lutut (PERHATIKAN: dua-duanya harus Deg2Rad!)
        Vector3 kneePos = rootPos + new Vector3(
            Mathf.Cos(upperAngle * Mathf.Deg2Rad),
            Mathf.Sin(upperAngle * Mathf.Deg2Rad),
            0
        ) * bone1Length;
        
        kneeJoint.position = kneePos;
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
