using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Health health;
    public Slider slider;

    void Start()
    {
        if (health == null || slider == null)
        {
            Debug.LogError("HealthBar needs both Health and Slider assigned.", this);
            enabled = false;
            return;
        }

        // A Slider is interactive by default
        slider.interactable = false;
        slider.maxValue = health.maxHealth;
        slider.value = health.currentHealth;
    }

    void Update()
    {
        // Health destroys its GameObject on death unless told otherwise
        if (health == null)
        {
            return;
        }

        slider.value = health.currentHealth;
    }
}
