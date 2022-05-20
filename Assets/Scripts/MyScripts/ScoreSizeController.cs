using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreSizeController : MonoBehaviour
{
    private TextMeshProUGUI _scoreText;
    private RectTransform _rectTransformScoreObject;
    private RectTransform _rectTransformScoreTextObject;
    private int _scoreTextLength = 1;

    private void Start()
    {
        _scoreText = transform.GetComponentInChildren<TextMeshProUGUI>();
        _rectTransformScoreObject = GetComponent<RectTransform>();
        _rectTransformScoreTextObject = _scoreText.gameObject.GetComponent<RectTransform>();
        DestructibleObjectVoxel.onSuckingCube += IncreaseScoreRectTransform;
    }

    private void OnDisable()
    {
        DestructibleObjectVoxel.onSuckingCube -= IncreaseScoreRectTransform;
    }

    private void IncreaseScoreRectTransform()
    {
        if(_scoreTextLength != _scoreText.text.ToString().Length)
        {
            _rectTransformScoreObject.anchoredPosition =
                new Vector2(_rectTransformScoreObject.anchoredPosition.x - 10, _rectTransformScoreObject.anchoredPosition.y);
            _rectTransformScoreObject.sizeDelta =
                new Vector2(_rectTransformScoreObject.sizeDelta.x + 20, _rectTransformScoreObject.sizeDelta.y);

            _rectTransformScoreTextObject.anchoredPosition = 
                new Vector2(_rectTransformScoreTextObject.anchoredPosition.x + 10, _rectTransformScoreTextObject.anchoredPosition.y);
            _rectTransformScoreTextObject.sizeDelta = 
                new Vector2(_rectTransformScoreTextObject.sizeDelta.x + 20, _rectTransformScoreTextObject.sizeDelta.y);

            _scoreTextLength = _scoreText.text.ToString().Length;
        }
        
    }

}
