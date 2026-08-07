using UnityEngine;
using UnityEngine.UI;

public class MechHealthBar : MonoBehaviour
{
    public Health health;

    public Sprite background;
    public Sprite frame;
    public Sprite heart;
    public Sprite filledPip;
    public Sprite emptyPip;

    // Five is what the frame has room for: slot 5 ends at x43, the frame at x47
    public int pipCount = 5;

    // Sprite-pixel coordinates measured from the source art. Change if the sprite is changed
    public Vector2 nativeSize = new Vector2(60f, 30f);
    public float firstPipX = 17f;
    public float pipSpacing = 5f;
    public float filledPipOriginX = 17f;
    public float emptyPipOriginX = 32f;

    Image[] pips;
    float scale;
    int shownPips = -1;

    void Start()
    {
        if (health == null || filledPip == null || emptyPip == null)
        {
            Debug.LogError("MechHealthBar needs Health, Filled Pip and Empty Pip assigned.", this);
            enabled = false;
            return;
        }

        RectTransform rect = (RectTransform)transform;
        // Everything is authored in 60x30 space, so one factor converts the baked pixel offsets to whatever size the bar is displayed at
        scale = rect.rect.width / nativeSize.x;

        AddLayer("Background", background);
        AddLayer("Frame", frame);

        pips = new Image[pipCount];
        for (int i = 0; i < pipCount; i++)
        {
            pips[i] = AddLayer("Pip " + i, filledPip);
        }

        AddLayer("Heart", heart);

        Refresh();
    }

    Image AddLayer(string name, Sprite sprite)
    {
        GameObject created = new GameObject(name, typeof(RectTransform));
        RectTransform rect = (RectTransform)created.transform;
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = nativeSize * scale;
        rect.anchoredPosition = Vector2.zero;

        Image image = created.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        // A layer with no art assigned would otherwise draw a white box
        image.enabled = sprite != null;
        return image;
    }

    void Update()
    {
        // Health destroys its GameObject on death unless told otherwise
        if (health == null)
        {
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        // Rounded up, so any health at all still shows a pip and the bar only empties completely on death
        int filled = Mathf.CeilToInt(health.Fraction * pipCount);

        if (filled == shownPips)
        {
            return;
        }

        shownPips = filled;

        for (int i = 0; i < pips.Length; i++)
        {
            bool isFilled = i < filled;
            Image pip = pips[i];
            pip.sprite = isFilled ? filledPip : emptyPip;

            // The two sprites bake their pip at different x positions, so the shift depends on which one is currently showing
            float originX = isFilled ? filledPipOriginX : emptyPipOriginX;
            float offset = (firstPipX + i * pipSpacing) - originX;
            pip.rectTransform.anchoredPosition = new Vector2(offset * scale, 0f);
        }
    }
}
