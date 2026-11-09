using Unity.VisualScripting;
using UnityEngine;

public class VerletRope : MonoBehaviour
{
    // Struktur data untuk tiap titik tali
    public struct RopeSegment
    {
        public Vector3 currentPos;
        public Vector3 oldPos;

        public RopeSegment(Vector3 pos)
        {
            currentPos = pos;
            oldPos = pos;
        }
    }

    [Header("Pengaturan Tali")]
    public int segmentCount = 10;       // Jumlah titik simpul tali
    public float ropeSegmentLen = 0.3f; // Jarak kaku antar titik
    public Vector3 gravity = new Vector3(0, -9.81f, 0);

    [SerializeField] private RopeSegment[] ropeSegments;

    void Start()
    {
        ropeSegments = new RopeSegment[segmentCount];
        Vector3 pos = transform.position;

        // Siapkan titik tali berjejer ke bawah
        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments[i] = new RopeSegment(pos);
            pos.y -= ropeSegmentLen;
        }
    }

    void Update()
    {
        SimulatePhysics();
        ApplyConstraints();
    }

    void SimulatePhysics()
    {
        // Titik ke-0 dipegang oleh objek ini
        ropeSegments[0].currentPos = transform.position;

        for (int i = 1; i < segmentCount; i++)
        {
            RopeSegment seg = ropeSegments[i];
            
            // 1. Hitung inersia kecepatan dari selisih posisi
            Vector3 velocity = seg.currentPos - seg.oldPos;
            seg.oldPos = seg.currentPos;

            // 2. Terapkan rumus Verlet (Posisi + Kecepatan + Gravitasi)
            seg.currentPos += velocity;
            seg.currentPos += gravity * (Time.deltaTime * Time.deltaTime);

            ropeSegments[i] = seg;
        }
    }

    void ApplyConstraints()
    {
        // Titik ke-0 tetap dikunci di tangan/objek
        ropeSegments[0].currentPos = transform.position;

        // Kunci jarak kaku agar tali tidak melar
        for (int i = 0; i < segmentCount - 1; i++)
        {
            RopeSegment first = ropeSegments[i];
            RopeSegment second = ropeSegments[i + 1];

            float dist = (first.currentPos - second.currentPos).magnitude;
            float error = dist - ropeSegmentLen; // Seberapa melarnya tali?
            Vector3 changeDir = (first.currentPos - second.currentPos).normalized;

            // Tarik titik kedua agar pas kembali ke jarak aslinya
            second.currentPos += changeDir * error;

            ropeSegments[i + 1] = second;
        }
    }

    // Menggambar tali di tab Scene agar bisa dilihat tanpa perlu bikin objek 3D banyak
    void OnDrawGizmos()
    {
        if (ropeSegments == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < segmentCount - 1; i++)
        {
            Gizmos.DrawLine(ropeSegments[i].currentPos, ropeSegments[i + 1].currentPos);
            Gizmos.DrawSphere(ropeSegments[i].currentPos, 0.05f);
        }
    }
}