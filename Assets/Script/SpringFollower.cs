using UnityEngine;

public class SpringFollower : MonoBehaviour
{
    public Transform target;

    [Header ("Pengaturan Pegas")]
    public float Stiffness = 100f; // kekuatan tarikan pegas (makin gede makin kenceng)
    public float Damping = 10f; // kekuatan rem/perdam (makin besar makin cepat berhenti)

    public Vector3 Velocity;

    private void Update()
    {
        // 1. Hitung jarak/selisih dari kubus ke target
        Vector3 displacement = target.position - transform.position;

         // 2. Gaya pegas menarik ke arah target
        Vector3 springForce = displacement * Stiffness;

        // 3. Gaya redaman mengerem kecepatan yang sedang ada
        Vector3 dampingForce = -Velocity * Damping;

        // 4. Total Kecepatannya
        Vector3 Force = springForce + dampingForce;

        // 5. Update kecepatan dan posisi
        Velocity += Force * Time.deltaTime;
        transform.position += Velocity * Time.deltaTime;
    }
}
