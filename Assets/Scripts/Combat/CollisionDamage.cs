using UnityEngine;

// Turns running into things into hull damage.
[RequireComponent(typeof(Health))]
public class CollisionDamage : MonoBehaviour
{
    // Gentle scrapes are free, so docking and grazing scenery is not punished
    public float safeSpeed = 8f;
    public float damagePerUnit = 3f;

    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact <= safeSpeed)
        {
            return;
        }

        health.TakeDamage((impact - safeSpeed) * damagePerUnit);
    }
}
