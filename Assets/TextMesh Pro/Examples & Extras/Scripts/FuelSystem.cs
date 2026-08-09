using UnityEngine;
using UnityEngine.UI;

public class FuelSystem : MonoBehaviour
{
    public float timeToEmpty = 300f; // seconds for fuel to go from 100% to 0% at normal throttle
    public SC_SpaceshipController shipController;
    public Slider fuelBar;
    public BlackHoleEncounter blackHoleEncounter;

    [Header("Throttle scaling")]
    public float minDrainMultiplier = 0.5f; // fuel use when decelerating/idle
    public float maxDrainMultiplier = 2f;   // fuel use at full boost

    float fuel = 100f;
    bool blackHoleTriggered = false;

    void Start()
    {
        fuelBar.maxValue = 100f;
        fuelBar.value = fuel;
    }

    void Update()
    {
        if (fuel <= 0f)
        {
            return;
        }

        float baseDrainPerSecond = 100f / timeToEmpty;
        float drainMultiplier = Mathf.Lerp(minDrainMultiplier, maxDrainMultiplier, shipController.ThrottleFraction);

        fuel -= baseDrainPerSecond * drainMultiplier * Time.deltaTime;
        fuel = Mathf.Max(fuel, 0f);
        fuelBar.value = fuel;

        if (!blackHoleTriggered && fuel <= 87.5f)
        {
            blackHoleTriggered = true;
            blackHoleEncounter.TriggerBlackHole();
        }
    }
}