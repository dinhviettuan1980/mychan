using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public static class CreateDayXepBaiScene
{
    [MenuItem("MyChan/Create DayXepBai Scene")]
    static void Create()
    {
        // 1. New empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // 3. Camera (required even with ScreenSpaceOverlay canvas)
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        cam.orthographic = true;

        // 4. DayXepBaiGame object + controller
        var gameGO = new GameObject("DayXepBaiGame");
        var controller = gameGO.AddComponent<DayXepBaiController>();

        // 4. Find and assign CardCollection
        var guids = AssetDatabase.FindAssets("t:CardCollection");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var collection = AssetDatabase.LoadAssetAtPath<CardCollection>(path);
            if (collection != null)
            {
                var so = new SerializedObject(controller);
                so.FindProperty("fullDeck").objectReferenceValue = collection;
                so.ApplyModifiedProperties();
                Debug.Log($"[DayXepBai] Assigned CardCollection from: {path}");
            }
            else
            {
                Debug.LogWarning("[DayXepBai] Found GUID but could not load CardCollection asset.");
            }
        }
        else
        {
            Debug.LogWarning("[DayXepBai] No CardCollection asset found in project. Assign fullDeck manually.");
        }

        // 5. Save scene
        const string scenePath = "Assets/Scenes/day_xep_bai.unity";
        bool saved = EditorSceneManager.SaveScene(scene, scenePath);
        if (!saved)
        {
            Debug.LogError($"[DayXepBai] Failed to save scene to {scenePath}");
            return;
        }

        // 6. Add to EditorBuildSettings if not already present
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);

        bool alreadyAdded = false;
        foreach (var bs in buildScenes)
        {
            if (bs.path == scenePath) { alreadyAdded = true; break; }
        }

        if (!alreadyAdded)
        {
            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"[DayXepBai] Added '{scenePath}' to Build Settings.");
        }

        // 7. Log success
        Debug.Log($"[DayXepBai] Scene created and saved to: {scenePath}");

        AssetDatabase.Refresh();
    }
}
