using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class EndingImageStep
{
    public GameObject imageObject;
    public Image image;
    public bool jitter;
    public Sprite frameA;
    public Sprite frameB;
    public float jitterInterval = 0.1f;
    public float minDisplayTime = 2f;
}

public class PlanetArrivalTrigger : MonoBehaviour
{
    [Header("References")]
    public SC_SpaceshipController shipController;
    public Rigidbody shipRigidbody;
    public CanvasGroup blackFadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Ending images, shown in order")]
    public List<EndingImageStep> endingImages;

    [Header("After the last image")]
    public string mainMenuSceneName = "MainMenu";
    public float delayBeforeInputAccepted = 2f;
    public GameObject theEndText;

    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered)
        {
            return;
        }

        if (other.GetComponentInParent<SC_SpaceshipController>() != null)
        {
            triggered = true;
            StartCoroutine(ArrivalSequence());
        }
    }

    IEnumerator ArrivalSequence()
    {
        shipController.enabled = false;
        shipRigidbody.linearVelocity = Vector3.zero;
        shipRigidbody.isKinematic = true;

        yield return StartCoroutine(Fade());

        foreach (EndingImageStep step in endingImages)
        {
            yield return StartCoroutine(ShowImage(step));
        }

        theEndText.SetActive(true);

        yield return new WaitForSeconds(delayBeforeInputAccepted);

        bool pressed = false;
        while (!pressed)
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                pressed = true;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pressed = true;
            }
            yield return null;
        }

        SceneManager.LoadScene(mainMenuSceneName);
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

    IEnumerator ShowImage(EndingImageStep step)
    {
        step.imageObject.SetActive(true);

        Coroutine jitterRoutine = null;
        if (step.jitter)
        {
            jitterRoutine = StartCoroutine(Jitter(step));
        }

        yield return new WaitForSeconds(step.minDisplayTime);

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

        if (jitterRoutine != null)
        {
            StopCoroutine(jitterRoutine);
        }

        step.imageObject.SetActive(false);
    }

    IEnumerator Jitter(EndingImageStep step)
    {
        bool showingA = true;
        while (true)
        {
            step.image.sprite = showingA ? step.frameA : step.frameB;
            showingA = !showingA;
            yield return new WaitForSeconds(step.jitterInterval);
        }
    }
}