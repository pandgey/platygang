using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Fade")]
    public CanvasGroup blackFadeCanvasGroup;
    public float fadeDuration = 0.3f;
    public float fadeOutDuration = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rocketSound;

    [Header("Image - jittery")]
    public GameObject imageObject;
    public Image image;
    public Sprite frameA;
    public Sprite frameB;
    public float jitterInterval = 0.1f;

    [Header("Timing")]
    public float minDisplayTime = 2f;
    public string gameplaySceneName = "SampleScene";

    public void BeginIntro()
    {
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        yield return StartCoroutine(Fade());

        if (audioSource != null && rocketSound != null)
        {
            audioSource.PlayOneShot(rocketSound);
        }

        yield return StartCoroutine(ShowImage());
        yield return StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator Fade()
    {
        blackFadeCanvasGroup.gameObject.SetActive(true);
        blackFadeCanvasGroup.alpha = 0f;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            blackFadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        blackFadeCanvasGroup.alpha = 1f;
    }

    IEnumerator ShowImage()
    {
        imageObject.SetActive(true);
        Coroutine jitterRoutine = StartCoroutine(Jitter());

        yield return new WaitForSeconds(minDisplayTime);

        bool skipped = false;
        while (!skipped)
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                skipped = true;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                skipped = true;
            }
            yield return null;
        }

        StopCoroutine(jitterRoutine);
    }

    IEnumerator Jitter()
    {
        bool showingA = true;
        while (true)
        {
            image.sprite = showingA ? frameA : frameB;
            showingA = !showingA;
            yield return new WaitForSeconds(jitterInterval);
        }
    }

    IEnumerator FadeOutAndLoad()
    {
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}