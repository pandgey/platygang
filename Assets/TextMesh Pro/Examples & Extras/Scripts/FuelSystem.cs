using UnityEngine;
using UnityEngine.UI;

public class FuelSystem : MonoBehaviour
{
    public float timeToEmpty = 300f;
    public SC_SpaceshipController shipController;
    public Slider fuelBar;
    public BlackHoleEncounter blackHoleEncounter;

    [Header("Throttle scaling")]
    public float minDrainMultiplier = 0.5f;
    public float maxDrainMultiplier = 2f;

    [Header("One-off thresholds (SampleScene style)")]
    public bool useOneOffThresholds = false;
    public float firstBlackHoleAt = 87.5f;
    public float finalBlackHoleAt = 75f;

    [Header("Repeating thresholds (AlienGalaxy style)")]
    public bool useRepeatingThresholds = false;
    public float repeatIntervalPercent = 25f;

    float fuel = 100f;
    bool firstBlackHoleTriggered = false;
    bool finalBlackHoleTriggered = false;
    float nextRepeatThreshold;

    public float CurrentFuel { get { return fuel; } }

    void Start()
    {
        fuel = GameState.carriedFuel;
        fuelBar.maxValue = 100f;
        fuelBar.value = fuel;

        // start counting down from the next multiple of the interval below current fuel
        nextRepeatThreshold = Mathf.Floor(fuel / repeatIntervalPercent) * repeatIntervalPercent;
        if (nextRepeatThreshold >= fuel)
        {
            nextRepeatThreshold -= repeatIntervalPercent;
        }
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

        if (useOneOffThresholds)
        {
            if (!firstBlackHoleTriggered && fuel <= firstBlackHoleAt)
            {
                firstBlackHoleTriggered = true;
                blackHoleEncounter.TriggerBlackHole();
            }

            if (!finalBlackHoleTriggered && fuel <= finalBlackHoleAt)
            {
                finalBlackHoleTriggered = true;
                blackHoleEncounter.TriggerFinalBlackHole();
            }
        }

        if (useRepeatingThresholds)
        {
            if (fuel <= nextRepeatThreshold && nextRepeatThreshold > 0f)
            {
                blackHoleEncounter.TriggerBlackHole();
                nextRepeatThreshold -= repeatIntervalPercent;
            }
        }
    }
}