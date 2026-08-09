using UnityEngine;

public class PlanetRotator : MonoBehaviour
{
    public Transform planetBody;
    public Transform ringOne;
    public Transform ringTwo;

    public float planetRotationSpeed = 5f;
    public float ringOneRotationSpeed = 2f;
    public float ringTwoRotationSpeed = 3f;

    void Update()
    {
        if (planetBody != null)
        {
            planetBody.Rotate(Vector3.up, planetRotationSpeed * Time.deltaTime);
        }

        if (ringOne != null)
        {
            ringOne.Rotate(Vector3.up, ringOneRotationSpeed * Time.deltaTime);
        }

        if (ringTwo != null)
        {
            ringTwo.Rotate(Vector3.up, ringTwoRotationSpeed * Time.deltaTime);
        }
    }
}