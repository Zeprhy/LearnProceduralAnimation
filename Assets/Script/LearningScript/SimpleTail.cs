using UnityEngine;

public class SimpleTail : MonoBehaviour
{
    [Header("Daftar Ruas (Urut dari Kepala ke Ujung Ekor)")]
    public Transform[] Segment;

    [Header ("Pengaturan Ekor")]
    public float targetDistance = 0.6f;
    public float smoothSpeed = 15f;

    private void Update()
    {
        for (int i = 1; i < Segment.Length ; i++)
        {
            Transform leader = Segment[i - 1]; //ruas didepannya
            Transform current = Segment[i]; //ruas saat ini

            Vector3 toLeader = leader.position - current.position;
            Vector3 targetPosition = leader.position - (toLeader.normalized * targetDistance);
            current.position = Vector3.Lerp(current.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
