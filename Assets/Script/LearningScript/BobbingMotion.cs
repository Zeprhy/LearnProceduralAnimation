using UnityEngine;

public class BobbingMotion : MonoBehaviour
{
     [Header("Pengaturan Nafas")]
    public float breathSpeed = 3f;
    public float breathAmount = 0.15f;

    public Vector3 startScale;
    void Start()
    {
       startScale = transform.localScale;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * breathSpeed) * breathAmount;
        transform.localScale = startScale + (Vector3.one * wave);
    }
}
