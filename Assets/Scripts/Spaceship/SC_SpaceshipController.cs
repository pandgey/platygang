using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]

public class SC_SpaceshipController : MonoBehaviour
{
    public float normalSpeed = 25f;
    public float accelerationSpeed = 45f;
    public float decelerationSpeed = 10f;
    public Transform cameraPosition;
    public Camera mainCamera;
    public Transform spaceshipRoot;
    public float rotationSpeed = 2.0f;
    public float cameraSmooth = 4f;
    public RectTransform crosshairTexture;
    public float mouseSensitivity = 0.1f;
    // Double tap A or D to barrel roll that way
    public float barrelRollDuration = 0.6f;
    public float doubleTapWindow = 0.3f;

    float speed;
    Rigidbody r;
    Quaternion lookRotation;
    float rotationZ = 0;
    float mouseXSmooth = 0;
    float mouseYSmooth = 0;
    Vector3 defaultShipRotation;

    // Input is sampled per frame in Update and consumed in FixedUpdate, which may
    // run zero or several times per frame. Reading it straight from FixedUpdate
    // drops and double-counts mouse movement.
    Vector2 lookInput;
    float rollInput;
    bool accelerating;
    bool decelerating;

    float lastLeftTapTime = -1f;
    float lastRightTapTime = -1f;
    bool barrelRolling;
    // Degrees the coroutine has queued for the next physics step to apply
    float barrelRollStep;

    // Read by the HUD throttle bar: current speed as a share of full boost
    public float ThrottleFraction { get { return Mathf.Clamp01(speed / accelerationSpeed); } }

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("SC_SpaceshipController found no camera. Assign Main Camera, or tag a camera in the scene as MainCamera.", this);
            enabled = false;
            return;
        }

        r = GetComponent<Rigidbody>();
        r.useGravity = false;
        lookRotation = transform.rotation;
        defaultShipRotation = spaceshipRoot.localEulerAngles;
        rotationZ = defaultShipRotation.z;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            // Accumulate, so no movement is lost on frames without a physics step
            lookInput += mouse.delta.ReadValue() * mouseSensitivity;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            rollInput = 0;
            if (keyboard.aKey.isPressed)
            {
                rollInput = 1;
            }
            else if (keyboard.dKey.isPressed)
            {
                rollInput = -1;
            }

            // Same keys as manual roll, so a quick double tap snaps the ship
            if (keyboard.aKey.wasPressedThisFrame)
            {
                TryBarrelRoll(ref lastLeftTapTime, 1f);
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                TryBarrelRoll(ref lastRightTapTime, -1f);
            }
        }

        // Hold Shift to speed up, Ctrl to slow down.
        accelerating = keyboard != null && keyboard.shiftKey.isPressed;
        decelerating = keyboard != null && keyboard.ctrlKey.isPressed;
    }

    void TryBarrelRoll(ref float lastTapTime, float direction)
    {
        if (!barrelRolling && Time.time - lastTapTime <= doubleTapWindow)
        {
            StartCoroutine(BarrelRoll(direction));
            // Cleared so a third tap cannot immediately chain another roll
            lastTapTime = -1f;
            return;
        }

        lastTapTime = Time.time;
    }

    IEnumerator BarrelRoll(float direction)
    {
        barrelRolling = true;

        float remaining = 360f;
        float degreesPerSecond = 360f / barrelRollDuration;

        while (remaining > 0f)
        {
            // Clamped to what is left, so the roll lands on exactly 360 degrees
            float step = Mathf.Min(degreesPerSecond * Time.fixedDeltaTime, remaining);
            remaining -= step;
            barrelRollStep += direction * step;

            // Paced by the physics clock, since FixedUpdate applies the rotation
            yield return new WaitForFixedUpdate();
        }

        barrelRolling = false;
    }

    void FixedUpdate()
    {
        // Accelerating wins if the player somehow holds Shift and Ctrl together
        if (accelerating)
        {
            speed = Mathf.Lerp(speed, accelerationSpeed, Time.deltaTime * 3);
        }
        else if (decelerating)
        {
            speed = Mathf.Lerp(speed, decelerationSpeed, Time.deltaTime * 3);
        }
        else
        {
            speed = Mathf.Lerp(speed, normalSpeed, Time.deltaTime * 10);
        }

        //Set moveDirection to the vertical axis (up and down keys) * speed
        Vector3 moveDirection = new Vector3(0, 0, speed);
        //Transform the vector3 to local space
        moveDirection = transform.TransformDirection(moveDirection);
        //Set the velocity, so you can move
        r.linearVelocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);

        //Camera follow
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraPosition.position, Time.deltaTime * cameraSmooth);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, cameraPosition.rotation, Time.deltaTime * cameraSmooth);

        //Rotation
        mouseXSmooth = Mathf.Lerp(mouseXSmooth, lookInput.x * rotationSpeed, Time.deltaTime * cameraSmooth);
        mouseYSmooth = Mathf.Lerp(mouseYSmooth, lookInput.y * rotationSpeed, Time.deltaTime * cameraSmooth);
        lookInput = Vector2.zero;
        // Consume whatever the barrel roll queued since the last physics step
        float barrelRoll = barrelRollStep;
        barrelRollStep = 0f;
        // Manual roll is ignored mid-roll so the two inputs cannot fight
        float roll = barrelRolling ? 0f : rollInput * rotationSpeed;
        Quaternion localRotation = Quaternion.Euler(-mouseYSmooth, mouseXSmooth, roll + barrelRoll);
        lookRotation = lookRotation * localRotation;
        transform.rotation = lookRotation;
        rotationZ -= mouseXSmooth;
        rotationZ = Mathf.Clamp(rotationZ, -45, 45);
        spaceshipRoot.transform.localEulerAngles = new Vector3(defaultShipRotation.x, defaultShipRotation.y, rotationZ);
        rotationZ = Mathf.Lerp(rotationZ, defaultShipRotation.z, Time.deltaTime * cameraSmooth);

        //Update crosshair texture
        if (crosshairTexture)
        {
            crosshairTexture.position = mainCamera.WorldToScreenPoint(transform.position + transform.forward * 100);
        }
    }
}
