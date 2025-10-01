using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustSlashEffect : MonoBehaviour
{
    private void OnEnable()
    {
        Vector2 playerToMonster = (Vector2) gameObject.transform.position - PlayerManager.Instance.player.Attack.justAttackStartPosition;
        float seeAngle =  Mathf.Atan2(playerToMonster.y, playerToMonster.x) *  Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, seeAngle);
        Debug.Log($"[이펙트] 섬단 이펙트 회전 {transform.localRotation.eulerAngles}");
    }
}
