using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlaneController : MonoBehaviour
{
    private bool hasBeenTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 한 번 발동되었거나 충돌한 대상이 플레이어가 아니면 무시
        if (hasBeenTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("플레이어가 DeathPlane에 닿았습니다. 스테이지 실패 처리 시작.");

        // 중복 실행을 막기 위해 플래그 설정
        hasBeenTriggered = true;

        // StageManager가 듣고 있는 플레이어 사망 이벤트 발행
        PlayerManager.Instance.player.runFSM = false; // 플레이어 FSM 멈추기
        EventBus.Publish(EventBusKey.ChangePlayerDead, true);
    }
}
