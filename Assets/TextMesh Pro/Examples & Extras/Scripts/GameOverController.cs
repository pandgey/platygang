using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class GameOverController : MonoBehaviour
{
    public GameObject explosionPrefab;
    public GameObject gameOverText;
    public float delayBeforeText = 1f;
    public float delayBeforeInputAccepted = 2f;
    public string mainMenuSceneName = "MainMenu";

    Health health;
    Rigidbody rb;
    SC_SpaceshipController shipController;
    Renderer[] shipRenderers;
    bool waitingForInput = false;
    bool isGameOver = false;

    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();
        shipController = GetComponent<SC_SpaceshipController>();
        shipRenderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        health.OnDied += HandleDeath;
    }

    void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }

    void HandleDeath()
    {
        TriggerGameOver(true);
    }

    // Call this for non-damage game overs, e.g. running out of fuel - same sequence, no explosion
    public void TriggerGameOver(bool spawnExplosion)
    {
        if (isGameOver)
        {
            return;
        }
        isGameOver = true;

        if (spawnExplosion)
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            foreach (Renderer r in shipRenderers)
            {
                r.enabled = false;
            }
        }

        shipController.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        StartCoroutine(ShowGameOverAfterDelay());
    }

    IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeText);

        gameOverText.SetActive(true);

        yield return new WaitForSeconds(delayBeforeInputAccepted);

        waitingForInput = true;
    }

    void Update()
    {
        if (!waitingForInput)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}