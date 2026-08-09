using UnityEngine;

public class SpaceObstacle : MonoBehaviour
{
    public float minScale = 0.5f;
    public float maxScale = 15f;

    public float baseSpeed = 35f;
    public float baseSpin = 80f;

    public float moveSpeed;
    public float spinSpeed;

    Vector3 direction;
    bool initialized = false;

    void Start()
    {
        if (!initialized)
        {
            // fallback if this obstacle wasn't spawned through the spawner
            Initialize(transform.forward, 1f);
        }
    }

    public void Initialize(Vector3 travelDirection, float speedMultiplier)
    {
        direction = travelDirection.normalized;

        float scale = Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * scale;

        float scaleFactor = Mathf.Sqrt(scale);
        moveSpeed = (baseSpeed / scaleFactor) * speedMultiplier;
        spinSpeed = (baseSpin / scaleFactor) * speedMultiplier;

        initialized = true;
    }

    void Update()
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}