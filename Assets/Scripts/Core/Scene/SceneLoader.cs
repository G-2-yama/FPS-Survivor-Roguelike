using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void Load(string scenePath)
    {
        if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
        {
            Debug.LogError($"{scenePath} が Build Settings に登録されていないため遷移できません");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(scenePath);
    }

    public static void ReloadActiveScene()
    {
        Load(SceneManager.GetActiveScene().path);
    }
}
