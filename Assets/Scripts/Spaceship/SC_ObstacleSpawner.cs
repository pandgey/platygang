using UnityEngine;

public class SC_ObstacleSpawner : MonoBehaviour
{
    public Transform ship;
    public GameObject asteroidPrefab;
    public GameObject cometPrefab;

    public float spawnDistance = 100f;
    public float spawnRadius = 20f;
    public float spawnInterval = 2f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        // pick a random point on a circle in front of the ship
        Vector2 circlePoint = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = ship.position + ship.forward * spawnDistance
            + ship.right * circlePoint.x
            + ship.up * circlePoint.y;

        GameObject prefab = Random.value < 0.5f ? asteroidPrefab : cometPrefab;
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        // aim it back toward roughly where the ship will be
        Vector3 direction = (ship.position - spawnPos).normalized;
        obj.transform.rotation = Quaternion.LookRotation(direction);
    }
}