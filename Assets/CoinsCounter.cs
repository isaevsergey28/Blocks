using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinsCounter : MonoBehaviour
{
   #region Singleton

   public static CoinsCounter Instance;

   private void Awake()
   {
      Instance = this;
   }

   #endregion
  
   private TMP_Text coinsText = null;
   private int _coins = 0;

   private void Start()
   {
      coinsText = GetComponentInChildren<TMP_Text>();
      _coins = PlayerPrefs.GetInt("Coins", 0);
      coinsText.text = "" + _coins;
   }

   public void AddCoins(int value)
   {
      _coins += value;
      coinsText.text = "" + _coins;
      PlayerPrefs.SetInt("Coins", _coins);
   }
   
   public void DecreaseCoins(int value)
   {
      _coins -= value;
      coinsText.text = "" + _coins;
      PlayerPrefs.SetInt("Coins", _coins);
   }
}
