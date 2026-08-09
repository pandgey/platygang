using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public RectTransform playerIcon; // triangle, rotates with ship heading
    public RectTransform targetBlip; // dot marking the destination
    public Transform ship;
    public Transform target;
    public float mapRadius = 80f; // UI radius in pixels, matches your map circle's size
    public float worldRangeForEdge = 500f; // world distance at which the blip sits right at the map's edge

    void Update()
    {
        if (ship == null || target == null)
        {
            return;
        }

        // player icon rotates to match ship's heading, map itself stays fixed (north-up)
        Vector3 shipForwardFlat = ship.forward;
        shipForwardFlat.y = 0f;
        if (shipForwardFlat.sqrMagnitude > 0.0001f)
        {
            float shipAngle = Mathf.Atan2(shipForwardFlat.x, shipForwardFlat.z) * Mathf.Rad2Deg;
            playerIcon.localEulerAngles = new Vector3(0f, 0f, -shipAngle);
        }

        // direction + distance from ship to target, flattened onto the map's plane
        Vector3 toTarget = target.position - ship.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        Vector2 mapDir = new Vector2(toTarget.x, toTarget.z);
        if (mapDir.sqrMagnitude > 0.0001f)
        {
            mapDir.Normalize();
        }

        // clamp so the blip stays inside the circle even at huge real distances
        float clampedDistance = Mathf.Min(distance / worldRangeForEdge, 1f) * mapRadius;
        targetBlip.anchoredPosition = mapDir * clampedDistance;
    }
}