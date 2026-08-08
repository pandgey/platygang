using UnityEngine;

public class SpaceObstacle : MonoBehaviour
{
    public float minScale = 0.5f;
    public float maxScale = 8f;

    public float baseSpeed = 55f;
    public float baseSpin = 80f;  

    public float moveSpeed;
    public float spinSpeed;

    private Vector3 direction;

    void Start()
    {
        direction = transform.forward;

        float scale = Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * scale;

        float scaleFactor = Mathf.Sqrt(scale);
        moveSpeed = baseSpeed / scaleFactor;
        spinSpeed = baseSpin / scaleFactor;
    }

    void Update()
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}