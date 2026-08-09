using UnityEngine;
using UnityEngine.UI;

public class FuelSystem : MonoBehaviour
{
    public float timeToEmpty = 300f;
    public SC_SpaceshipControllerBase shipController;
    public Slider fuelBar;
    public BlackHoleEncounter blackHoleEncounter;

    [Header("Throttle scaling")]
    public float minDrainMultiplier = 0.5f;
    public float maxDrainMultiplier = 2f;

    float fuel = 100f;
    bool firstBlackHoleTriggered = false;
    bool finalBlackHoleTriggered = false;

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

        if (!firstBlackHoleTriggered && fuel <= 87.5f)
        {
            firstBlackHoleTriggered = true;
            blackHoleEncounter.TriggerBlackHole();
        }

        if (!finalBlackHoleTriggered && fuel <= 75f)
        {
            finalBlackHoleTriggered = true;
            blackHoleEncounter.TriggerFinalBlackHole();
        }
    }
}