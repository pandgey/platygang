using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Inverted variant of SC_SpaceshipController
[RequireComponent(typeof(Rigidbody))]

public class SC_SpaceshipControllerInverted : SC_SpaceshipControllerBase
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
    // How far sideways the dodge carries the ship, in world units
    public float barrelRollDistance = 8f;

    // Which axes the inversion applies to, so the scheme can be dialled back to
    // the usual pitch-only inversion without touching the code
    public bool invertPitch = true;
    public bool invertYaw = true;
    public bool invertRoll = true;

    float speed;
    Rigidbody r;
    Quaternion lookRotation;
    float rotationZ = 0;
    // Smoothed mouse look, in degrees per second. Kept as a rate rather than as degrees per physics step, so a step that happens to see two frames of mouse movement, or none, does not pulse the turn rate.
    float yawSpeed;
    float pitchSpeed;
    Vector3 defaultShipRotation;

    float rollInput;
    bool accelerating;
    bool decelerating;

    float lastLeftTapTime = -1f;
    float lastRightTapTime = -1f;
    bool barrelRolling;
    // Degrees the coroutine has queued for the next physics step to apply
    float barrelRollStep;
    // Total degrees rolled so far, used to hold the camera steady through it
    float barrelRollAngle;
    // World direction of the dodge, fixed at launch so the ship slides in a straight line instead of corkscrewing with its own spin
    Vector3 barrelRollLateral;

    // Read by the HUD throttle bar: current speed as a share of full boost
    public override float ThrottleFraction { get { return Mathf.Clamp01(speed / accelerationSpeed); } }

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("SC_SpaceshipControllerInverted found no camera. Assign Main Camera, or tag a camera in the scene as MainCamera.", this);
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
        float deltaTime = Time.deltaTime;

        Mouse mouse = Mouse.current;
        Vector2 lookRate = Vector2.zero;
        if (mouse != null && deltaTime > 0f)
        {
            // Per second, so the same flick of the mouse turns the ship equally far whatever the frame rate
            lookRate = mouse.delta.ReadValue() * mouseSensitivity / deltaTime;
        }

        // The inversion, applied to the raw rate before smoothing so the easing is unaffected
        if (invertYaw)
        {
            lookRate.x = -lookRate.x;
        }

        if (invertPitch)
        {
            lookRate.y = -lookRate.y;
        }

        // Smoothed on the frame clock, where the input actually arrives. Exponential form so the quarter second of easing holds at any frame rate.
        float smooth = 1f - Mathf.Exp(-cameraSmooth * deltaTime);
        yawSpeed = Mathf.Lerp(yawSpeed, lookRate.x * rotationSpeed, smooth);
        pitchSpeed = Mathf.Lerp(pitchSpeed, lookRate.y * rotationSpeed, smooth);

        // Flips which way each key rolls, and with it which way a double tap dodges
        float leftKeyDirection = invertRoll ? -1f : 1f;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            rollInput = 0;
            if (keyboard.aKey.isPressed)
            {
                rollInput = leftKeyDirection;
            }
            else if (keyboard.dKey.isPressed)
            {
                rollInput = -leftKeyDirection;
            }

            // Same keys as manual roll, so a quick double tap snaps the ship
            if (keyboard.aKey.wasPressedThisFrame)
            {
                TryBarrelRoll(ref lastLeftTapTime, leftKeyDirection);
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                TryBarrelRoll(ref lastRightTapTime, -leftKeyDirection);
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
        // The dodge always follows the roll, so an inverted roll dodges the other way too
        barrelRollLateral = -transform.right * direction;

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

        // A full turn is the identity rotation anyway, but clearing it keeps the camera from following
        barrelRollAngle = 0f;
        barrelRolling = false;
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        // Accelerating wins if the player somehow holds Shift and Ctrl together
        if (accelerating)
        {
            speed = Mathf.Lerp(speed, accelerationSpeed, deltaTime * 3);
        }
        else if (decelerating)
        {
            speed = Mathf.Lerp(speed, decelerationSpeed, deltaTime * 3);
        }
        else
        {
            speed = Mathf.Lerp(speed, normalSpeed, deltaTime * 10);
        }

        //Set moveDirection to the vertical axis (up and down keys) * speed
        Vector3 moveDirection = new Vector3(0, 0, speed);
        //Transform the vector3 to local space
        moveDirection = transform.TransformDirection(moveDirection);
        // Eased in and out over the roll with a sine, so the dodge swells and settles instead of the ship snapping sideways and stopping dead
        if (barrelRolling)
        {
            float progress = Mathf.Abs(barrelRollAngle) / 360f;
            float peakSpeed = barrelRollDistance * Mathf.PI / (2f * barrelRollDuration);
            moveDirection += barrelRollLateral * peakSpeed * Mathf.Sin(progress * Mathf.PI);
        }

        //Set the velocity, so you can move
        r.linearVelocity = new Vector3(moveDirection.x, moveDirection.y, moveDirection.z);

        //Rotation
        // Consume whatever the barrel roll queued using the last physics step
        float barrelRoll = barrelRollStep;
        barrelRollStep = 0f;
        barrelRollAngle += barrelRoll;
        // Manual roll is ignored mid-roll so the two inputs cannot fight
        float roll = barrelRolling ? 0f : rollInput * rotationSpeed;
        Quaternion localRotation = Quaternion.Euler(-pitchSpeed * deltaTime, yawSpeed * deltaTime, roll + barrelRoll);
        lookRotation = lookRotation * localRotation;
        // A bump leaves the body spinning, and angular damping barely bleeds it off. Left alone it turns the hull between the steps this script authors, and the ship shakes for seconds after every collision.
        r.angularVelocity = Vector3.zero;
        // MoveRotation, not transform.rotation: writing the transform of an interpolated Rigidbody teleports it and throws away the interpolation, so the hull snaps once per physics step while the camera moves every frame
        r.MoveRotation(lookRotation);
    }

    // Camera and cosmetics run on the frame clock, not the physics clock. The Rigidbody interpolates the hull between physics steps, so anything that follows it has to be sampled per frame or it lags the ship by a varying fraction of a step, which is what makes the ship wobble in view.
    void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        float smooth = 1f - Mathf.Exp(-cameraSmooth * deltaTime);

        //Bank the hull into the turn
        rotationZ -= yawSpeed * deltaTime;
        rotationZ = Mathf.Clamp(rotationZ, -45, 45);
        spaceshipRoot.transform.localEulerAngles = new Vector3(defaultShipRotation.x, defaultShipRotation.y, rotationZ);
        rotationZ = Mathf.Lerp(rotationZ, defaultShipRotation.z, smooth);

        //Camera follow
        Vector3 cameraTargetPosition = cameraPosition.position;
        Quaternion cameraTargetRotation = cameraPosition.rotation;

        // keeps the camera from rolling with the ship, so the horizon stays level. The ship can still roll, but the camera will stay upright.
        if (barrelRollAngle != 0f)
        {
            Quaternion cancelRoll = Quaternion.AngleAxis(-barrelRollAngle, transform.forward);
            // The offset orbits the hull as it rolls, so cancel position too
            cameraTargetPosition = transform.position + cancelRoll * (cameraPosition.position - transform.position);
            cameraTargetRotation = cancelRoll * cameraTargetRotation;
        }

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, cameraTargetPosition, smooth);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, cameraTargetRotation, smooth);

        //Update crosshair texture
        if (crosshairTexture)
        {
            crosshairTexture.position = mainCamera.WorldToScreenPoint(transform.position + transform.forward * 100);
        }
    }
}
