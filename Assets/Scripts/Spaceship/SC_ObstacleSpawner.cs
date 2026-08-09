using UnityEngine;

public class SC_ObstacleSpawner : MonoBehaviour
{
    public Transform ship;
    public GameObject[] obstaclePrefabs;

    [Header("Spawn settings")]
    public float spawnDistance = 600f;
    public float spawnMaxAngle = 85f;

    public float aimLeadDistance = 30f;

    [Header("Spawn rate ramp")]
    public float spawnInterval = 2f;
    public float minSpawnInterval = 0.3f;
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
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        Vector3 aimPoint = ship.position + ship.forward * aimLeadDistance;
        Vector3 direction = (aimPoint - spawnPos).normalized;
        obj.transform.rotation = Quaternion.LookRotation(direction);
    }
}