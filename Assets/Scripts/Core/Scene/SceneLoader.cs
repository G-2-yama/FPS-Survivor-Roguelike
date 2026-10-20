using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void Load(string scenePath)
    {
        if (!IsInBuildSettings(scenePath))
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(scenePath);
    }

    public static void LoadWithAdditive(string scenePath, string additiveScenePath)
    {
        if (!IsInBuildSettings(scenePath) || !IsInBuildSettings(additiveScenePath))
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(scenePath);
        SceneManager.LoadScene(additiveScenePath, LoadSceneMode.Additive);
    }

    private static bool IsInBuildSettings(string scenePath)
    {
        if (SceneUtility.GetBuildIndexByScenePath(scenePath) >= 0)
            return true;

        Debug.LogError($"{scenePath} が Build Settings に登録されていないため遷移できません");
        return false;
    }

    public static void ReloadActiveScene()
    {
        Load(SceneManager.GetActiveScene().path);
    }
}
