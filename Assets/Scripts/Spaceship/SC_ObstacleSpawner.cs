using UnityEngine;

public class SC_ObstacleSpawner : MonoBehaviour
{
    public Transform ship;
    public Camera cam;
    public GameObject asteroidPrefab;
    public GameObject cometPrefab;

    [Header("Far spawns - visible far ahead, slowly grows")]
    public float farSpawnDistance = 300f;
    public float farSpawnSpread = 15f;

    [Header("Near spawns - jump scare, just off screen")]
    public float nearSpawnDistance = 50f;
    public float nearExtraMargin = 5f;
    public float nearSpeedMultiplier = 0.5f;

    public float aimLeadDistance = 30f;
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
        if (Random.value < 0.5f)
            SpawnFar();
        else
            SpawnNear();
    }

    void SpawnFar()
    {
        float x = Random.Range(-farSpawnSpread, farSpawnSpread);
        float y = Random.Range(-farSpawnSpread, farSpawnSpread);

        Vector3 spawnPos = ship.position + ship.forward * farSpawnDistance
            + ship.right * x
            + ship.up * y;

        SpawnObstacle(spawnPos, 1f);
    }

    void SpawnNear()
    {
        float vRadius = nearSpawnDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float hRadius = vRadius * cam.aspect;

        float minX = hRadius + nearExtraMargin;
        float minY = vRadius + nearExtraMargin;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Cos(angle) * (minX + Random.Range(0f, 10f));
        float y = Mathf.Sin(angle) * (minY + Random.Range(0f, 10f));

        Vector3 spawnPos = ship.position + ship.forward * nearSpawnDistance
            + ship.right * x
            + ship.up * y;

        SpawnObstacle(spawnPos, nearSpeedMultiplier);
    }

    void SpawnObstacle(Vector3 spawnPos, float speedMultiplier)
    {
        GameObject prefab = Random.value < 0.5f ? asteroidPrefab : cometPrefab;
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        Vector3 aimPoint = ship.position + ship.forward * aimLeadDistance;
        Vector3 direction = (aimPoint - spawnPos).normalized;
        obj.transform.rotation = Quaternion.LookRotation(direction);

        SpaceObstacle obstacle = obj.GetComponent<SpaceObstacle>();
        if (obstacle != null)
        {
            obstacle.moveSpeed *= speedMultiplier;
        }
    }
}