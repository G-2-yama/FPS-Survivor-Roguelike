using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class TutorialPlayModeFallback
{
    private const string TutorialScenePath = "Assets/Scenes/TutorialScene.unity";
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";

    static TutorialPlayModeFallback()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        if (!IsTutorialSceneOpen())
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        SceneAsset titleScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitleScenePath);
        if (titleScene == null)
        {
            Debug.LogError($"{TitleScenePath} が見つからないため、TutorialScene からそのまま再生します");
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        EditorSceneManager.playModeStartScene = titleScene;
        Debug.Log("TutorialScene は単独で再生できないため、タイトル画面から再生します");
    }

    private static bool IsTutorialSceneOpen()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).path == TutorialScenePath)
                return true;
        }
        return false;
    }
}
