using UnityEngine;

public class LerpFolower : MonoBehaviour
{
    // Rumus kalau di MTK = Hasil = A + (B - A) t
    /// <summary>
    /// t = follower speed
    /// A = titik awal
    /// B = titik target
    /// </summary>
    public float FolowerSpeed = 5f;
    public Transform target;

    void Update()
    {
    
        transform.position = Vector3.Lerp(transform.position, target.position, FolowerSpeed * Time.deltaTime);
    }
}
