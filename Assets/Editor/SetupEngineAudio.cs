using UnityEditor;
using UnityEngine;

// One-shot scene wiring for the engine sound. Run it from
// Tools > Platygang > Setup Engine Audio with a scene open that contains the
// spaceship; safe to run again, it only fills in whatever is missing.
public static class SetupEngineAudio
{
    const string ClipPath =
        "Assets/Audio/410833__univ_lyon3__buisson_manara_2017-2018_spaceshipenginestart.wav";

    [MenuItem("Tools/Platygang/Setup Engine Audio")]
    static void Run()
    {
        // The loop region is seeked into by the runtime script, and only PCM
        // seeks sample-accurately; Vorbis lands on coarse block boundaries and
        // smears the seam.
        AudioImporter importer = AssetImporter.GetAtPath(ClipPath) as AudioImporter;
        if (importer == null)
        {
            Debug.LogError("Engine clip not found at " + ClipPath);
            return;
        }

        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        if (settings.compressionFormat != AudioCompressionFormat.PCM ||
            settings.loadType != AudioClipLoadType.DecompressOnLoad)
        {
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
            Debug.Log("Engine clip reimported as PCM / Decompress On Load.");
        }

        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);

        SC_SpaceshipController ship =
            Object.FindFirstObjectByType<SC_SpaceshipController>(FindObjectsInactive.Include);
        if (ship == null)
        {
            Debug.LogError("No SC_SpaceshipController in the open scene. Open SampleScene or AlienGalaxy and run this again.");
            return;
        }

        AudioSource source = ship.GetComponent<AudioSource>();
        if (source == null)
        {
            source = Undo.AddComponent<AudioSource>(ship.gameObject);
        }

        Undo.RecordObject(source, "Setup Engine Audio");
        source.clip = clip;
        // The runtime script owns playback from Start
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        if (ship.GetComponent<SC_SpaceshipEngineAudio>() == null)
        {
            Undo.AddComponent<SC_SpaceshipEngineAudio>(ship.gameObject);
        }

        EditorUtility.SetDirty(source);

        // The ambient loop was dragged in while its clip still carried the 3D
        // import default, so the source went fully spatial and falls silent as
        // soon as the ship leaves the origin. Flatten every non-engine source in
        // the scene; background loops are the only other audio here.
        foreach (AudioSource other in
                 Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (other != source && other.spatialBlend > 0f)
            {
                Undo.RecordObject(other, "Setup Engine Audio");
                other.spatialBlend = 0f;
                EditorUtility.SetDirty(other);
                Debug.Log("Flattened '" + other.gameObject.name + "' to 2D so it no longer fades with distance.");
            }
        }

        Debug.Log("Engine audio wired up on '" + ship.gameObject.name + "'. Press Play to test, then save the scene.");
    }
}
