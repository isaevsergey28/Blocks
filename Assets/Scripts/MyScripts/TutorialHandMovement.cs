using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TutorialHandMovement : MonoBehaviour
{
    [SerializeField] private GameObject _hand;
    [SerializeField] private GameObject[] _routeDots;

    private List<Vector3> _route = new List<Vector3>();

    private void Start()
    {
        PlayerMovement.onStartMove += Hide;
        CreateRoute();
        Move();
    }

    private void OnDisable()
    {
        PlayerMovement.onStartMove -= Hide;
    }

    private void CreateRoute()
    {
        foreach (GameObject dot in _routeDots)
        {
            _route.Add(dot.transform.position);
        }
    }

    private void Move()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_hand.transform.DOPath(_route.ToArray(), 3f).SetEase(Ease.Linear));
        sequence.AppendCallback(() => Move());
    }

    private void Hide()
    {
        PlayerMovement.onStartMove -= Hide;
        gameObject.SetActive(false);
    }
}
