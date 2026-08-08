using UnityEngine;

public class SC_ObstacleSpawner : MonoBehaviour
{
    public Transform ship;
    public GameObject asteroidPrefab;
    public GameObject cometPrefab;

    [Header("Spawn settings")]
    public float spawnDistance = 600f;
    public float spawnMaxAngle = 85f; // half angle from forward, 90 = full hemisphere in front

    public float aimLeadDistance = 30f;

    [Header("Spawn rate ramp")]
    public float spawnInterval = 0.1f;
    public float minSpawnInterval = 0.001f;
    public float rateIncreasePercentPerSecond = 5f;

    float timer;

    void Update()
    {
        spawnInterval -= spawnInterval * (rateIncreasePercentPerSecond / 100f) * Time.deltaTime;
        spawnInterval = Mathf.Max(spawnInterval, minSpawnInterval);

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        float angle = Random.Range(0f, spawnMaxAngle);
        float rotationAroundForward = Random.Range(0f, 360f);

        Quaternion coneRotation = Quaternion.AngleAxis(rotationAroundForward, ship.forward) * Quaternion.AngleAxis(angle, ship.up);
        Vector3 direction = coneRotation * ship.forward;

        Vector3 spawnPos = ship.position + direction * spawnDistance;

        SpawnObstacle(spawnPos);
    }

    void SpawnObstacle(Vector3 spawnPos)
    {
        GameObject prefab = Random.value < 0.5f ? asteroidPrefab : cometPrefab;
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        Vector3 aimPoint = ship.position + ship.forward * aimLeadDistance;
        Vector3 direction = (aimPoint - spawnPos).normalized;
        obj.transform.rotation = Quaternion.LookRotation(direction);
    }
}