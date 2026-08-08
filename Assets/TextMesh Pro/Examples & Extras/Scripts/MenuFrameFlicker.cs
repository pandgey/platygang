using UnityEngine;
using UnityEngine.UI;

public class MenuFrameFlicker : MonoBehaviour
{
    public Sprite frameA;
    public Sprite frameB;
    public float switchInterval = 0.5f;

    Image image;
    float timer;
    bool showingA = true;

    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = frameA;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            showingA = !showingA;
            image.sprite = showingA ? frameA : frameB;
        }
    }
}