using UnityEngine;

// Drives the engine from one clip that is really two parts: an ignition swell at
// the head, then a steady tail. Launch plays the swell once from 0, the tail
// repeats while the player is on the throttle, and braking silences it. Pitch and
// volume ride the throttle on top of all that.
[RequireComponent(typeof(AudioSource))]
public class SC_SpaceshipEngineAudio : MonoBehaviour
{
    public SC_SpaceshipController ship;

    [Header("Loop region, in seconds")]
    // The steady part of the clip. Everything before loopStart is the ignition,
    // which only ever plays on launch.
    public float loopStart = 3f;
    public float loopEnd = 4f;

    [Header("Sustain")]
    // Off: the tail only repeats while Shift is held, so letting go lets the clip
    // run out and stop. On: it keeps repeating unless the player is braking.
    public bool sustainWhileCoasting = true;

    [Header("Pitch")]
    public float idlePitch = 0.75f;
    public float fullPitch = 1.35f;

    [Header("Volume")]
    public float idleVolume = 0.25f;
    public float fullVolume = 0.7f;

    // How fast the sound chases the throttle. The hull takes roughly a second to
    // settle at full boost, so this is only smoothing the tail of that.
    public float response = 6f;

    AudioSource source;
    // Smoothed 0..1 power rather than the raw throttle, so the pitch glides
    float power;
    // Throttle value the ship sits at under full braking, cached in Start
    float throttleFloor;
    // Whether the engine was silenced last frame, since a paused AudioSource and
    // one that has played itself out both report isPlaying false
    bool silenced;

    void Start()
    {
        source = GetComponent<AudioSource>();

        if (ship == null)
        {
            ship = GetComponentInParent<SC_SpaceshipController>();
        }

        if (ship == null || source.clip == null)
        {
            Debug.LogError("SC_SpaceshipEngineAudio needs an AudioClip, and a Ship assigned or a controller on a parent.", this);
            enabled = false;
            return;
        }

        // Kept inside the clip and in order, so a mistyped region cannot strand the
        // playhead past the end where it would stop instead of repeating
        loopEnd = Mathf.Clamp(loopEnd, 0f, source.clip.length);
        loopStart = Mathf.Clamp(loopStart, 0f, loopEnd);

        // ThrottleFraction never reaches 0: the ship coasts at normalSpeed and only
        // drops as far as decelerationSpeed. Rescaled against that floor so the whole
        // pitch range is spent on throttle the player can actually reach.
        throttleFloor = ship.accelerationSpeed > 0f
            ? Mathf.Clamp01(ship.decelerationSpeed / ship.accelerationSpeed)
            : 0f;

        // The region is held by hand below, so the source must not wrap on its own
        source.loop = false;
        // Flat, and deaf to the ship's own velocity: a 3D source on a hull this fast
        // doppler-shifts against the camera trailing it, and that warble fights the
        // pitch this script is authoring.
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        power = Power();
        Apply();

        // From 0, so launch gets the ignition
        source.time = 0f;
        source.Play();
    }

    void Update()
    {
        // Mirrors the controller, where holding both keys counts as accelerating
        bool braking = ship.Decelerating && !ship.Accelerating;

        if (braking && !silenced)
        {
            source.Pause();
        }
        else if (!braking && silenced)
        {
            source.UnPause();
        }

        silenced = braking;

        if (!braking)
        {
            HoldLoop();
        }

        float smooth = 1f - Mathf.Exp(-response * Time.deltaTime);
        power = Mathf.Lerp(power, Power(), smooth);
        Apply();
    }

    void HoldLoop()
    {
        if (!source.isPlaying)
        {
            // The tail ran out while coasting. Boost picks it straight back up at the
            // loop rather than replaying the ignition, which only belongs at launch.
            if (ship.Accelerating)
            {
                source.time = loopStart;
                source.Play();
            }

            return;
        }

        if (!ship.Accelerating && !sustainWhileCoasting)
        {
            return;
        }

        if (source.time >= loopEnd)
        {
            // Whatever the frame overshot by is carried across the seam, so the loop
            // does not slowly drift later with framerate or with a raised pitch
            float region = loopEnd - loopStart;
            source.time = region > 0f
                ? loopStart + Mathf.Repeat(source.time - loopEnd, region)
                : loopStart;
        }
    }

    float Power()
    {
        return Mathf.InverseLerp(throttleFloor, 1f, ship.ThrottleFraction);
    }

    void Apply()
    {
        source.pitch = Mathf.Lerp(idlePitch, fullPitch, power);
        source.volume = Mathf.Lerp(idleVolume, fullVolume, power);
    }
}
