using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BlackHoleEncounter : MonoBehaviour
{
    [Header("References")]
    public Transform ship;
    public SC_SpaceshipControllerBase shipController;
    public Health shipHealth;
    public Rigidbody shipRigidbody;
    public Camera mainCamera;
    public GameObject blackHolePrefab;

    [Header("UI")]
    public GameObject mashPromptUI;
    public TMP_Text mashPromptText;
    public Slider escapeBar;
    public CanvasGroup blackFadeCanvasGroup;
    public GameObject drawingImageObject;

    [Header("Spawn")]
    public float sideDistance = 150f;
    public float forwardDistance = 200f;
    public float fadeInDuration = 1f;

    [Header("Mash Challenge")]
    public float barDecayPerSecond = 12f;
    public float barGainPerPress = 6f;
    public float timeLimit = 6f;

    [Header("Final black hole (inescapable)")]
    public float finalBarDecayPerSecond = 15f; // similar feel to the normal one
    public float finalBarGainPerPress = 1.5f;  // deliberately too weak to keep up with decay
    public float finalTimeLimit = 8f;
    public float cameraZoomDuration = 2f;
    public float blackFadeDuration = 2f;
    public float drawingDisplayDuration = 3f;
    public string nextSceneName = "AlienGalaxy";

    [Header("Slow motion")]
    public float slowMoTimeScale = 0.15f;

    [Header("Black hole pull")]
    public float pullSpeed = 2f;
    public float finalPullSpeed = 6f;

    [Header("Escape bounce")]
    public float overshootDistance = 5f;
    public float bounceDuration = 1.5f;

    bool spawnedOnLeft;
    bool escapeKeyIsA;
    bool isFinal = false;
    GameObject blackHoleInstance;
    bool challengeActive = false;
    float barValue;
    float timer;

    float currentDecayRate;
    float currentGainPerPress;
    float currentPullSpeed;

    Vector3 originalStartPos;

    public void TriggerBlackHole()
    {
        isFinal = false;
        BeginEncounter(barDecayPerSecond, timeLimit, barGainPerPress);
    }

    public void TriggerFinalBlackHole()
    {
        isFinal = true;
        BeginEncounter(finalBarDecayPerSecond, finalTimeLimit, finalBarGainPerPress);
    }

    void BeginEncounter(float decayRate, float limit, float gainPerPress)
    {
        spawnedOnLeft = Random.value < 0.5f;
        float sideSign = spawnedOnLeft ? -1f : 1f;

        Vector3 spawnPos = ship.position + ship.forward * forwardDistance + ship.right * sideDistance * sideSign;
        blackHoleInstance = Instantiate(blackHolePrefab, spawnPos, Quaternion.identity);

        Vector3 targetScale = blackHoleInstance.transform.localScale;
        blackHoleInstance.transform.localScale = Vector3.zero;
        StartCoroutine(FadeInBlackHole(targetScale));

        escapeKeyIsA = !spawnedOnLeft;

        StartChallenge(decayRate, limit, gainPerPress);
    }

    IEnumerator FadeInBlackHole(Vector3 targetScale)
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            if (blackHoleInstance != null)
            {
                blackHoleInstance.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t / fadeInDuration);
            }
            yield return null;
        }

        if (blackHoleInstance != null)
        {
            blackHoleInstance.transform.localScale = targetScale;
        }
    }

    void StartChallenge(float decayRate, float limit, float gainPerPress)
    {
        challengeActive = true;
        barValue = 50f;
        timer = limit;
        currentDecayRate = decayRate;
        currentGainPerPress = gainPerPress;
        currentPullSpeed = isFinal ? finalPullSpeed : pullSpeed;

        originalStartPos = ship.position;

        shipController.enabled = false;
        shipRigidbody.linearVelocity = Vector3.zero;
        shipRigidbody.isKinematic = true;

        Time.timeScale = slowMoTimeScale;

        mashPromptUI.SetActive(true);
        escapeBar.maxValue = 100f;
        escapeBar.value = barValue;
        escapeBar.direction = escapeKeyIsA ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;
        mashPromptText.text = "MASH " + (escapeKeyIsA ? "A" : "D") + " TO ESCAPE!";
    }

    void Update()
    {
        if (!challengeActive)
        {
            return;
        }

        if (blackHoleInstance != null)
        {
            Vector3 pullDirection = (blackHoleInstance.transform.position - ship.position).normalized;
            ship.position += pullDirection * currentPullSpeed * Time.unscaledDeltaTime;
        }

        timer -= Time.unscaledDeltaTime;
        barValue -= currentDecayRate * Time.unscaledDeltaTime;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            bool pressed = escapeKeyIsA ? kb.aKey.wasPressedThisFrame : kb.dKey.wasPressedThisFrame;
            if (pressed)
            {
                barValue += currentGainPerPress;
            }
        }

        barValue = Mathf.Clamp(barValue, 0f, 100f);
        escapeBar.value = barValue;

        if (barValue >= 100f)
        {
            EndChallenge(true);
        }
        else if (barValue <= 0f || timer <= 0f)
        {
            EndChallenge(false);
        }
    }

    void EndChallenge(bool success)
    {
        challengeActive = false;
        mashPromptUI.SetActive(false);

        if (success)
        {
            Time.timeScale = 1f;

            if (blackHoleInstance != null)
            {
                StartCoroutine(ShrinkAndDestroyBlackHole());
            }
            StartCoroutine(EscapeBounce());
        }
        else
        {
            if (isFinal)
            {
                // stays in slow-mo through the cinematic, no explosion/game over here
                StartCoroutine(FinalBlackHoleCinematic());
            }
            else
            {
                Time.timeScale = 1f;
                if (blackHoleInstance != null)
                {
                    Destroy(blackHoleInstance);
                }
                shipHealth.TakeDamage(shipHealth.maxHealth);
            }
        }
    }

    IEnumerator ShrinkAndDestroyBlackHole()
    {
        Vector3 startScale = blackHoleInstance.transform.localScale;
        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            if (blackHoleInstance != null)
            {
                blackHoleInstance.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / fadeInDuration);
            }
            yield return null;
        }

        if (blackHoleInstance != null)
        {
            Destroy(blackHoleInstance);
        }
    }

    IEnumerator FinalBlackHoleCinematic()
    {
        shipRigidbody.linearVelocity = Vector3.zero;
        shipRigidbody.isKinematic = true;

        Vector3 camStartPos = mainCamera.transform.position;
        Quaternion camStartRot = mainCamera.transform.rotation;
        Vector3 camTargetPos = blackHoleInstance != null ? blackHoleInstance.transform.position : ship.position;
        Quaternion camTargetRot = camStartRot;

        if (blackHoleInstance != null)
        {
            camTargetRot = Quaternion.LookRotation(blackHoleInstance.transform.position - camStartPos);
        }

        float t = 0f;
        while (t < cameraZoomDuration)
        {
            t += Time.unscaledDeltaTime;
            float frac = t / cameraZoomDuration;
            mainCamera.transform.position = Vector3.Lerp(camStartPos, camTargetPos, frac);
            mainCamera.transform.rotation = Quaternion.Slerp(camStartRot, camTargetRot, frac);
            yield return null;
        }

        Time.timeScale = 1f;

        float fadeT = 0f;
        blackFadeCanvasGroup.gameObject.SetActive(true);
        blackFadeCanvasGroup.alpha = 0f;
        while (fadeT < blackFadeDuration)
        {
            fadeT += Time.deltaTime;
            blackFadeCanvasGroup.alpha = Mathf.Clamp01(fadeT / blackFadeDuration);

            // actively pin the camera here too, so nothing else can nudge it during the fade
            mainCamera.transform.position = camTargetPos;
            mainCamera.transform.rotation = camTargetRot;

            yield return null;
        }
        blackFadeCanvasGroup.alpha = 1f;

        if (blackHoleInstance != null)
        {
            Destroy(blackHoleInstance);
        }

        drawingImageObject.SetActive(true);

        yield return new WaitForSeconds(drawingDisplayDuration);

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator EscapeBounce()
    {
        Vector3 pullDirectionAtEnd = (originalStartPos - ship.position).normalized;
        Vector3 currentPos = ship.position;
        Vector3 overshotPos = originalStartPos + pullDirectionAtEnd * overshootDistance;

        float t = 0f;
        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            float half = bounceDuration / 2f;
            if (t < half)
            {
                ship.position = Vector3.Lerp(currentPos, overshotPos, t / half);
            }
            else
            {
                ship.position = Vector3.Lerp(overshotPos, originalStartPos, (t - half) / half);
            }
            yield return null;
        }

        ship.position = originalStartPos;
        shipRigidbody.isKinematic = false;
        shipController.enabled = true;
    }
}