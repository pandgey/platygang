using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BlackHoleEncounter : MonoBehaviour
{
    [Header("References")]
    public Transform ship;
    public SC_SpaceshipController shipController;
    public Health shipHealth;
    public Rigidbody shipRigidbody;
    public GameObject blackHolePrefab;

    [Header("UI")]
    public GameObject mashPromptUI;
    public TMP_Text mashPromptText;
    public Slider escapeBar;

    [Header("Spawn")]
    public float sideDistance = 150f;
    public float forwardDistance = 200f;
    public float fadeInDuration = 1f;

    [Header("Mash Challenge")]
    public float barDecayPerSecond = 12f;
    public float barGainPerPress = 6f;
    public float timeLimit = 6f;

    [Header("Slow motion")]
    public float slowMoTimeScale = 0.15f;

    [Header("Black hole pull")]
    public float pullSpeed = 2f;

    [Header("Escape bounce")]
    public float overshootDistance = 5f;
    public float bounceDuration = 1.5f;

    bool spawnedOnLeft;
    bool escapeKeyIsA;
    GameObject blackHoleInstance;
    bool challengeActive = false;
    float barValue;
    float timer;

    Vector3 originalStartPos;

    public void TriggerBlackHole()
    {
        spawnedOnLeft = Random.value < 0.5f;
        float sideSign = spawnedOnLeft ? -1f : 1f;

        Vector3 spawnPos = ship.position + ship.forward * forwardDistance + ship.right * sideDistance * sideSign;
        blackHoleInstance = Instantiate(blackHolePrefab, spawnPos, Quaternion.identity);

        Vector3 targetScale = blackHoleInstance.transform.localScale;
        blackHoleInstance.transform.localScale = Vector3.zero;
        StartCoroutine(FadeInBlackHole(targetScale));

        escapeKeyIsA = !spawnedOnLeft;

        StartChallenge();
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

    void StartChallenge()
    {
        challengeActive = true;
        barValue = 50f;
        timer = timeLimit;

        originalStartPos = ship.position;

        shipController.enabled = false;
        shipRigidbody.linearVelocity *= 0.1f;

        Time.timeScale = slowMoTimeScale;

        mashPromptUI.SetActive(true);
        escapeBar.maxValue = 100f;
        escapeBar.value = barValue;
        // bar fills toward whichever side you're actually mashing to escape
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
            ship.position += pullDirection * pullSpeed * Time.unscaledDeltaTime;
        }

        timer -= Time.unscaledDeltaTime;
        barValue -= barDecayPerSecond * Time.unscaledDeltaTime;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            bool pressed = escapeKeyIsA ? kb.aKey.wasPressedThisFrame : kb.dKey.wasPressedThisFrame;
            if (pressed)
            {
                barValue += barGainPerPress;
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
        Time.timeScale = 1f;

        if (success)
        {
            if (blackHoleInstance != null)
            {
                StartCoroutine(ShrinkAndDestroyBlackHole());
            }
            StartCoroutine(EscapeBounce());
        }
        else
        {
            if (blackHoleInstance != null)
            {
                Destroy(blackHoleInstance);
            }
            shipHealth.TakeDamage(shipHealth.maxHealth);
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
        shipController.enabled = true;
    }
}