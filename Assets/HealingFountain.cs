using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingFountain : MonoBehaviour
{
   [SerializeField] private int treatmentFrequencySec = 0;
   [SerializeField] private int healingValue = 0;
   public bool isStayingInCollider = false;
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent(out Player player))
      {
         isStayingInCollider = true;
         StartCoroutine(HealingFountainCor(player));
      }
   }

   private void OnTriggerExit(Collider other)
   {
      if(other.TryGetComponent(out Player player))
      {
         StopAllCoroutines();
         isStayingInCollider = false;
      }
   }

   private IEnumerator HealingFountainCor(Player player)
   {
      while (isStayingInCollider)
      {
         yield return new WaitForSeconds(treatmentFrequencySec);
         player.OnHealingEnemyDeath(healingValue);
      }
     
   }
}
