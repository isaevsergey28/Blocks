using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    private TextMeshProUGUI _scoreText;
    private int _score = 0;

    private void Start()
    {
        _scoreText = transform.GetComponentInChildren<TextMeshProUGUI>();
        DestructibleObjectVoxel.onSuckingCube += IncreaseScore;
        Backpack.onUsingResources += DecreaseScore;
    }

    private void OnDisable()
    {
        DestructibleObjectVoxel.onSuckingCube -= IncreaseScore;
        Backpack.onUsingResources -= DecreaseScore;
    }

    private void IncreaseScore()
    {
        _score++;
        _scoreText.text = _score.ToString();
    }

    private void DecreaseScore()
    {
        _score--;
        _scoreText.text = _score.ToString();
    }
}
