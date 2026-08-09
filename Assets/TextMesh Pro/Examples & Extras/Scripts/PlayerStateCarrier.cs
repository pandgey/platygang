using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerStateCarrier : MonoBehaviour
{
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Start()
    {
        health.SetHealth(GameState.carriedHealth);
    }
}