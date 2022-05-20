using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    private void Start()
    {
        SayKit.trackLevelStarted(InitialLevelLoader.currentLevel);
        SayKit.showBanner();
        CheckForRateApp();
    }

    private void CheckForRateApp()
    {
        if (!(PlayerPrefs.HasKey("FirstCycleGame")) && SceneManager.GetActiveScene().buildIndex == 2)
        {
            PlayerPrefs.SetString("FirstCycleGame", "FirstCycle");
            SayKit.showRateAppPopup();
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt(InitialLevelLoader.currentLevelText, InitialLevelLoader.currentLevel);
    }

    public void LoadNextLevel()
    {
        int nextLevel = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextLevel > SceneManager.sceneCountInBuildSettings - 1)
        {
            nextLevel = 1;
        }
        SayKit.trackLevelCompleted(InitialLevelLoader.currentLevel, 0);
        InitialLevelLoader.currentLevel = nextLevel;
        StartCoroutine(LoadSceneAsync(nextLevel));
    }

    public void StartLevelAgain()
    {
        int nextLevel = SceneManager.GetActiveScene().buildIndex;
        SayKit.trackLevelFailed(InitialLevelLoader.currentLevel, 0);
        StartCoroutine(LoadSceneAsync(nextLevel));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }
}