using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    public float rotationSpeed = 1f;

    void Update()
    {
        float rotation = RenderSettings.skybox.GetFloat("_Rotation");
        rotation += rotationSpeed * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", rotation);
    }
}