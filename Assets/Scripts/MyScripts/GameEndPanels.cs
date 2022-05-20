using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameEndPanels : MonoBehaviour
{
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _defeatPanel;
    [SerializeField] private GameObject _transactionLevelPanel;

    private LevelController _levelController;
    private Button _victoryButton;
    private Button _defeatButton;   
    private delegate void InvokeLevel();
    private InvokeLevel _invokeLevel;

    private void Start()
    {
        BuildingRegistrar.onVictory += ShowVictoryPanel;
        Player.onPlayerDeath += ShowDefeatPanel;
        Player.onPlayerRevive += HideDefeatPanel;
    }

    private void OnDisable()
    {
        BuildingRegistrar.onVictory -= ShowVictoryPanel;
        Player.onPlayerDeath -= ShowDefeatPanel;
        Player.onPlayerRevive -= HideDefeatPanel;
    }

    private void ShowVictoryPanel()
    {
        StartCoroutine(WaitForShow(_victoryPanel));
    }

    private void ShowDefeatPanel()
    {
        StartCoroutine(WaitForShow(_defeatPanel));
    }

    private void HideDefeatPanel()
    {
        _defeatPanel.SetActive(false);
    }

    private IEnumerator WaitForShow(GameObject panel)
    {
        yield return new WaitForSeconds(3f);
        panel.SetActive(true);
        AddListenerToButtons();
    }

    private void AddListenerToButtons()
    {
        _levelController = FindObjectOfType<LevelController>();
        _victoryButton = _victoryPanel.GetComponent<Button>();
        _defeatButton = _defeatPanel.GetComponent<Button>();
        _victoryButton.onClick.AddListener(LoadNextLevel);
        _defeatButton.onClick.AddListener(StartLevelAgain);
    }

    private void LoadNextLevel()
    {
        _invokeLevel = _levelController.LoadNextLevel;
        DarkenScene(_invokeLevel);
    }

    private void StartLevelAgain()
    {
        _invokeLevel = _levelController.StartLevelAgain;
        DarkenScene(_invokeLevel);
    }

    private void DarkenScene(InvokeLevel invokeLevel)
    {
        _transactionLevelPanel.SetActive(true);
        Image transactionleLevelImage = _transactionLevelPanel.GetComponent<Image>();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transactionleLevelImage.DOColor(
            new Color(transactionleLevelImage.color.r, transactionleLevelImage.color.g, transactionleLevelImage.color.b, 1), 1f));
        sequence.AppendCallback(() => invokeLevel.Invoke());
    }
}
