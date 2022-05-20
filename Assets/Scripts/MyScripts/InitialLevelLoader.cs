using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialLevelLoader : MonoBehaviour
{
    public static int currentLevel;
    public static string currentLevelText = "CurrentLevel";

    private void Start()
    {
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        if (PlayerPrefs.HasKey(currentLevelText))
        {
            currentLevel = PlayerPrefs.GetInt(currentLevelText);
        }
        else
        {
            currentLevel = 1;
            PlayerPrefs.SetInt(currentLevelText, currentLevel);
        }
        SceneManager.LoadScene(currentLevel);
    }
}
