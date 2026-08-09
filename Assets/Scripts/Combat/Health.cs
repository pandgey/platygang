using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    // Suits asteroids and enemies that should be destroyed when their health reaches zero, but not the player ship
    public bool destroyOnDeath = true;

    // Serialized purely so the value is visible in the inspector while playing;
    [SerializeField] float health;

    public float currentHealth { get { return health; } }
    public float Fraction { get { return maxHealth <= 0f ? 0f : Mathf.Clamp01(health / maxHealth); } }
    public bool IsDead { get { return health <= 0f; } }

    // Anything that cares (HUD, VFX, audio) subscribes rather than polling
    public event System.Action<float> OnDamaged;
    public event System.Action OnDied;

    // Awake, not Start: the HUD reads currentHealth in its own Start
    void Awake()
    {
        health = maxHealth;
    }

    public void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0f, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - amount);

        if (OnDamaged != null)
        {
            OnDamaged(amount);
        }

        if (health <= 0f)
        {
            if (OnDied != null)
            {
                OnDied();
            }

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        health = Mathf.Min(maxHealth, health + amount);
    }
}
