using System.Collections;
using TMPro;
using UnityEngine;

public class InvertedControlsWarning : MonoBehaviour
{
    public TMP_Text warningText;
    public float displayDuration = 3f;
    public float fadeOutDuration = 1f;

    void Start()
    {
        warningText.gameObject.SetActive(true);
        Color c = warningText.color;
        warningText.color = new Color(c.r, c.g, c.b, 1f);

        StartCoroutine(ShowThenFade());
    }

    IEnumerator ShowThenFade()
    {
        yield return new WaitForSeconds(displayDuration);

        Color startColor = warningText.color;
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            warningText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        warningText.gameObject.SetActive(false);
    }
}