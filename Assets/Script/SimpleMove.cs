using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}
