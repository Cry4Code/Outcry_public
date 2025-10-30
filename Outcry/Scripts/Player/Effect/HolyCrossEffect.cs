using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HolyCrossEffect : MonoBehaviour
{
   private void OnEnable()
   {
      EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.HolySlash * 10, EffectType.Sound, PlayerManager.Instance.player.gameObject).Forget();
   }
}
