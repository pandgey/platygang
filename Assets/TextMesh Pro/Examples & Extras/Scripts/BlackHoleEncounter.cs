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
    public float approachTime = 3f;

    [Header("Mash Challenge")]
    public float barDecayPerSecond = 12f;
    public float barGainPerPress = 6f;
    public float timeLimit = 6f;

    [Header("Slow motion")]
    public float slowMoTimeScale = 0.15f;

    [Header("Black hole pull")]
    public float pullSpeed = 2f;

    [Header("Escape bounce")]
    public float overshootDistance = 5f; // how far past the original point it swings before settling
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

        escapeKeyIsA = !spawnedOnLeft;

        StartCoroutine(ApproachThenChallenge());
    }

    IEnumerator ApproachThenChallenge()
    {
        yield return new WaitForSeconds(approachTime);
        StartChallenge();
    }

    void StartChallenge()
    {
        challengeActive = true;
        barValue = 50f;
        timer = timeLimit;

        // remember where the ship actually started, before any pull happens
        originalStartPos = ship.position;

        shipController.enabled = false;
        shipRigidbody.linearVelocity *= 0.1f; // kill most of the forward drift instantly

        Time.timeScale = slowMoTimeScale;

        mashPromptUI.SetActive(true);
        escapeBar.maxValue = 100f;
        escapeBar.value = barValue;
        mashPromptText.text = "MASH " + (escapeKeyIsA ? "A" : "D") + " TO ESCAPE!";
    }

    void Update()
    {
        if (!challengeActive)
        {
            return;
        }

        // slowly drag the ship toward the black hole while the challenge is active
        if (blackHoleInstance != null)
        {
            Vector3 pullDirection = (blackHoleInstance.transform.position - ship.position).normalized;
            ship.position += pullDirection * pullSpeed * Time.unscaledDeltaTime;
        }

        // unscaled so the mash window stays real-time even while the world is in slow-mo
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

        if (blackHoleInstance != null)
        {
            Destroy(blackHoleInstance);
        }

        if (success)
        {
            StartCoroutine(EscapeBounce());
        }
        else
        {
            shipHealth.TakeDamage(shipHealth.maxHealth);
        }
    }

    IEnumerator EscapeBounce()
    {
        // overshoot past the original point, away from where the black hole was
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