using UnityEngine;

public class ObstacleImpact : MonoBehaviour
{
    public GameObject explosionPrefab;

    void OnCollisionEnter(Collision collision)
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}