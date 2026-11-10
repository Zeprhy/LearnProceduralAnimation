using UnityEngine;

public class ProceduralAbdormenSpring : MonoBehaviour
{
    [Header ("Spring Settings")]
    public float stiffness = 120f; //=> spring power back to the body
    public float damping = 8f; //=> damper body for the swing
    public float swayIntensity = 2f; //=> flexsibility shake 

    private Quaternion InitialLocalRot;
    private Vector3 angularVelocity;
    private Vector3 currentRotationOffset;
    private Vector3 lastParentPos;
    private float lastParentYaw;

    void Start()
    {
        InitialLocalRot = transform.localRotation;

        if (transform.parent != null)
        {
            lastParentPos = transform.parent.position;
            lastParentYaw = transform.parent.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (transform.parent == null) return;

        Vector3 ParentMove = transform.parent.position - lastParentPos;
        float parentYawDelta = Mathf.DeltaAngle(lastParentYaw, transform.parent.eulerAngles.y);

        lastParentPos = transform.parent.position;
        lastParentYaw = transform.parent.eulerAngles.y;

        // Naik-turun saat melangkah, dan mengayun kiri-kanan saat belok
        float pitchPush = -transform.parent.InverseTransformDirection(ParentMove).z * 150f * swayIntensity;
        float yawPush = -parentYawDelta * swayIntensity;

        Vector3 targetOffset = new Vector3(
            Mathf.Clamp(pitchPush, -15f, 15f),          // Batas maksimal nungging/nunduk
            Mathf.Clamp(yawPush, -20f, 20f),            // Batas maksimal goyang kiri-kanan
            0f
        );

            Vector3 displacement = targetOffset - currentRotationOffset;
            Vector3 springForce = displacement * stiffness;
            Vector3 dampingForce = -angularVelocity * damping;

            Vector3 force = springForce + dampingForce;
            angularVelocity += force * Time.deltaTime;
            currentRotationOffset += angularVelocity * Time.deltaTime;

            transform.localRotation = InitialLocalRot * Quaternion.Euler(currentRotationOffset);
    }

}
