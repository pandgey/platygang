using UnityEngine;

// a hud to share the controller state with other systems
public abstract class SC_SpaceshipControllerBase : MonoBehaviour
{
    // Current speed as a share of full boost
    public abstract float ThrottleFraction { get; }
}