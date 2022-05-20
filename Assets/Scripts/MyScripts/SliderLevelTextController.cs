using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SliderLevelTextController : MonoBehaviour
{
    [SerializeField] private GameObject _currentLevelObject;
    [SerializeField] private GameObject _nextLevelObject;

    private TextMeshProUGUI _currentLevelText;
    private TextMeshProUGUI _nextLevelText;

    private void Start()
    {
        _currentLevelText = _currentLevelObject.GetComponentInChildren<TextMeshProUGUI>();
        _nextLevelText = _nextLevelObject.GetComponentInChildren<TextMeshProUGUI>();

        SetRequiredLevels();
    }

    private void SetRequiredLevels()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;

        _currentLevelText.text = currentLevel.ToString();
        _nextLevelText.text = (currentLevel + 1).ToString();
    }
}
