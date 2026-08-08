using UnityEngine;

public class SpaceObstacle : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float spinSpeed = 60f;

    private Vector3 direction;

    void Start()
    {
        direction = transform.forward;
    }

    void Update()
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}