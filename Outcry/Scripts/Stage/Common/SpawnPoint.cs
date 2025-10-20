using StageEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("이 스폰 지점의 종류를 선택하세요.(플레이어, 보스 등)")]
    public ESpawnType Type;

    [Tooltip("스폰될 몬스터의 순서를 입력하세요.(0부터 시작) StageData의 Monster_ids 목록 순서와 일치해야 합니다.")]
    public int SpawnIndex;

    // 기획자가 위치를 쉽게 보도록 기즈모(Gizmo)를 그려주는 유용한 기능
    private void OnDrawGizmos()
    {
        switch (Type)
        {
            case ESpawnType.Player:
                Gizmos.color = Color.cyan;
                break;
            case ESpawnType.Enemy:
                Gizmos.color = Color.red;
                break;
        }

        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
