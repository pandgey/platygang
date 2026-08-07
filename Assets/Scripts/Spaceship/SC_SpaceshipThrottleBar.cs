using TMPro;
using UnityEngine;

// Drives the HUD throttle: a vertical bar that fills toward full boost, plus
// the percentage readout beside it.
public class SC_SpaceshipThrottleBar : MonoBehaviour
{
    public SC_SpaceshipController ship;
    // Anchored to the bottom of its track, so growing its height fills upward
    public RectTransform fill;
    public TMP_Text label;

    float trackHeight;

    void Start()
    {
        if (ship == null)
        {
            ship = GetComponentInParent<SC_SpaceshipController>();
        }

        if (ship == null || fill == null || label == null)
        {
            Debug.LogError("SC_SpaceshipThrottleBar needs Ship, Fill and Label assigned.", this);
            enabled = false;
            return;
        }

        // Sized against the track rather than a hard-coded number, so resizing
        // the bar in the inspector just works
        trackHeight = ((RectTransform)fill.parent).rect.height;
    }

    void LateUpdate()
    {
        float throttle = ship.ThrottleFraction;

        fill.sizeDelta = new Vector2(fill.sizeDelta.x, trackHeight * throttle);
        label.text = Mathf.RoundToInt(throttle * 100f) + "%";
    }
}
