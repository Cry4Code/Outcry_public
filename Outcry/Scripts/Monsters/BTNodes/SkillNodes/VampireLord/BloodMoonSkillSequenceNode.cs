using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BloodMoonSkillSequenceNode : SkillSequenceNode
{
    private const int TOTAL_QTE_REPS = 3;
    private const float QTE_UI_Y_OFFSET = 2.0f; // QTE 생성 위치(플레이어 머리 위 오프셋 값)
    private const float QTE_SUCCESS_DELAY = 2.0f; // QTE 성공 후 다음 QTE까지 대기 시간
    private const float INTRO_ANIM_DELAY = 1.5f;  // 인트로 애니메이션 길이(가정)
    private const float BLOODMOON_SHOW_DELAY = 2.0f; // 블러드문 등장 후 QTE 시작까지 대기 시간
    private const float END_QTE_ANIM_DELAY = 1.0f;

    //QTE 키 시퀀스 저장 배열
    private readonly string[] qteSequences = { "ADFS", "QAS", "DSAWEQ", "RFED", "SD", "RQ", "FREQ", "WASDF", "EWQ" };
    // 이번 스킬에서 사용할 3개의 랜덤 시퀀스를 저장할 리스트
    private List<string> selectedQteSequences = new List<string>();

    private HallOfBloodStageController stageController;
    private QTEController qteController;
    private GameObject bloodMoonInstance;
    private GameObject qte_UI;

    // 상태 관리 플래그 및 내부 변수
    private bool isIntroDelaying;
    private float introTimer;
    private bool isBloodMoonDelaying;
    private float bloodMoonTimer;
    private bool qteFailed;
    private int qteSuccessCount;
    private bool isWaitingForNextQTE; // QTE 성공 후 다음 QTE를 기다리는 중인지
    private float qteWaitTimer;       // QTE 대기 시간 측정용 타이머
    private int originalSortingOrder;
    private bool isWarningUITriggerd = false;
    private Vector3 originalPlayerScale;

    // 비동기 작업 진행 상태 추적
    private bool isCleanupRunning = false;
    private bool isCleanupComplete = false;

    public BloodMoonSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "BloodMoonSkillSequenceNode";
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        this.monster = monster;
        this.target = target;
        stageController = StageManager.Instance.currentStageController as HallOfBloodStageController;

        // 노드 정의
        ConditionNode canPerform = new ConditionNode(CanPerform);
        ActionNode warningAction = new ActionNode(WarningAction);
        ActionNode introAction = new ActionNode(IntroAction);
        ActionNode showBloodMoonAction = new ActionNode(ShowBloodMoonAction);
        ActionNode executeQTEAction = new ActionNode(SkillAction);
        ActionNode cleanupAction = new ActionNode(CleanupAction_Wrapper);

        children.Clear();
        AddChild(canPerform);
        AddChild(warningAction);
        AddChild(introAction);
        AddChild(showBloodMoonAction);
        AddChild(executeQTEAction);
        AddChild(cleanupAction);

        // BloodMoon UI 프리팹 미리 로드
        await ObjectPoolManager.Instance.RegisterPoolAsync(AddressablePaths.UI.BloodMoon);
        await ObjectPoolManager.Instance.RegisterPoolAsync(AddressablePaths.UI.QTE_UI);
    }

    protected override bool CanPerform()
    {
        if (skillTriggered)
        {
            return false;
        }

        bool isLowHealth = monster.Condition.CurrentHealth.CurValue() <= skillData.triggerHealth * monster.Condition.MaxHealth;
        if (isLowHealth)
        {
            ResetAllFlags(); // 모든 상태 플래그 초기화

            // 스킬 시작이 확정된 시점에서 랜덤 QTE 시퀀스 3개 선택
            SelectRandomQteSequences();

            return true;
        }

        return false;
    }

    private void ResetAllFlags()
    {
        skillTriggered = false;
        isIntroDelaying = false;
        isBloodMoonDelaying = false;
        qteFailed = false;
        qteSuccessCount = 0;
        isWaitingForNextQTE = false;
        isWarningUITriggerd = false;
        isCleanupRunning = false;
        isCleanupComplete = false;
    }

    /// <summary>
    /// 원본 qteSequences 배열에서 중복되지 않게 3개의 시퀀스를 랜덤으로 선택
    /// </summary>
    private void SelectRandomQteSequences()
    {
        selectedQteSequences.Clear();
        var availableSequences = new List<string>(qteSequences);

        for (int i = 0; i < TOTAL_QTE_REPS; i++)
        {
            // 더 이상 뽑을 시퀀스가 없으면 중단
            if (availableSequences.Count == 0)
            {
                break;
            }

            int randomIndex = UnityEngine.Random.Range(0, availableSequences.Count);
            selectedQteSequences.Add(availableSequences[randomIndex]);
            availableSequences.RemoveAt(randomIndex); // 중복 선택을 방지하기 위해 뽑힌 항목은 제거
        }
    }

    private NodeState WarningAction()
    {
        if (!isWarningUITriggerd) //아직 경고 UI 시작 전.
        {
            // 경고창 띄우기
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(103010, EffectType.ScreenUI).Forget();
            isWarningUITriggerd = true;
            return NodeState.Running;
        }

        if (EffectManager.Instance.IsEffectPlaying(103010, EffectType.ScreenUI))
        {
            // 경고창이 재생되는 동안 대기
            return NodeState.Running;
        }

        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) WarningAction Done!");
        return NodeState.Success;
    }

    private NodeState IntroAction()
    {
        if (!isIntroDelaying)
        {
            isIntroDelaying = true;
            skillTriggered = true;
            monster.Condition.SetInivincible(true);
            introTimer = Time.time; // 타이머 시작

            originalPlayerScale = target.transform.localScale;

            // TODO: 보스 인트로 애니메이션 실행?
            // monster.Animator.SetTrigger();

            // 전용 스킬 효과음 및 카메라 효과
            //EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.Sound).Forget();
            //EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.Camera).Forget();
        }

        // 인트로 애니메이션 길이만큼 대기
        if (Time.time - introTimer < INTRO_ANIM_DELAY)
        {
            return NodeState.Running;
        }

        // 인트로 애니메이션 종료 후 보스 가리기
        if (monster.SpriteRenderer != null)
        {
            originalSortingOrder = monster.SpriteRenderer.sortingOrder;
            monster.SpriteRenderer.sortingOrder = 0;
        }

        // 암전 효과
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.Background).Forget();

        PlayerManager.Instance.player.runFSM = false;
        PlayerManager.Instance.player.ForceChangeAnimation(AnimatorHash.PlayerAnimation.StartQTE);

        return NodeState.Success;
    }

    private NodeState ShowBloodMoonAction()
    {
        if (!isBloodMoonDelaying)
        {
            isBloodMoonDelaying = true;
            bloodMoonTimer = Time.time; // 타이머 시작

            // BloodMoon 프리팹을 화면에 표시
            bloodMoonInstance = ObjectPoolManager.Instance.GetObject(AddressablePaths.UI.BloodMoon, stageController.BloodMoon.transform);
            bloodMoonInstance.transform.position = new Vector3(stageController.BloodMoon.transform.position.x, stageController.BloodMoon.transform.position.y);
            if (bloodMoonInstance != null)
            {
                // 플레이어가 BloodMoon을 바라보도록 방향 전환
                var playerTransform = target.transform;
                var currentScale = playerTransform.localScale;

                // BloodMoon이 플레이어의 오른쪽에 있는지 확인
                if (bloodMoonInstance.transform.position.x > playerTransform.position.x)
                {
                    // 오른쪽을 바라보도록 localScale.x 양수로 설정
                    playerTransform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
                }
                else
                {
                    // 왼쪽을 바라보도록 localScale.x 음수로 설정
                    playerTransform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
                }

                qte_UI = ObjectPoolManager.Instance.GetObject(AddressablePaths.UI.QTE_UI);
                if (qte_UI != null)
                {
                    qteController = qte_UI.GetComponent<QTEController>();

                    // UI 위치 계산 및 보정(카메라 경계 내로)
                    // 이상적인 위치 계산 (플레이어 머리 위)
                    Vector3 desiredPosition = target.transform.position + Vector3.up * QTE_UI_Y_OFFSET;

                    // 카메라 경계 정보 가져오기
                    var confiner = StageManager.Instance.StageCamera.GetComponent<CinemachineConfiner2D>();
                    if (confiner != null && confiner.m_BoundingShape2D != null)
                    {
                        Bounds cameraBounds = confiner.m_BoundingShape2D.bounds;

                        // UI의 월드 기준 절반 크기 계산
                        RectTransform qteRectTransform = qte_UI.GetComponent<RectTransform>();
                        float uiHalfWidth = (qteRectTransform.rect.width / 2f) * qteRectTransform.lossyScale.x;
                        float uiHalfHeight = (qteRectTransform.rect.height / 2f) * qteRectTransform.lossyScale.y;

                        // Mathf.Clamp를 사용하여 UI의 중심 위치가 경계를 벗어나지 않도록 보정
                        desiredPosition.x = Mathf.Clamp(
                            desiredPosition.x,                  // 목표 X 위치
                            cameraBounds.min.x + uiHalfWidth,   // 허용 가능한 최소 X (왼쪽 경계 + UI 절반 너비)
                            cameraBounds.max.x - uiHalfWidth    // 허용 가능한 최대 X (오른쪽 경계 - UI 절반 너비)
                        );
                        desiredPosition.y = Mathf.Clamp(
                            desiredPosition.y,                  // 목표 Y 위치
                            cameraBounds.min.y + uiHalfHeight,  // 허용 가능한 최소 Y (아래쪽 경계 + UI 절반 높이)
                            cameraBounds.max.y - uiHalfHeight   // 허용 가능한 최대 Y (위쪽 경계 - UI 절반 높이)
                        );
                    }

                    // 최종 보정된 위치로 UI 좌표 설정
                    qteController.SetPosition(desiredPosition);
                }
            }
        }

        // 블러드문 등장 후 2초 대기
        if (Time.time - bloodMoonTimer < BLOODMOON_SHOW_DELAY)
        {
            return NodeState.Running;
        }

        return NodeState.Success;
    }

    protected override NodeState SkillAction()
    {
        // 3회 성공했거나 한 번이라도 실패했다면 QTE 시퀀스 종료
        if (qteSuccessCount >= TOTAL_QTE_REPS || qteFailed)
        {
            return NodeState.Success;
        }

        // QTE 컨트롤러가 없거나, 현재 QTE가 진행 중이면 계속 대기
        if (qteController == null || qteController.CurrentState == QTEController.EQTEState.InProgress)
        {
            return NodeState.Running;
        }

        // 타이머 사용하여 다음 QTE까지 대기
        if (isWaitingForNextQTE)
        {
            if (Time.time - qteWaitTimer >= QTE_SUCCESS_DELAY)
            {
                isWaitingForNextQTE = false;
                // qteSuccessCount를 인덱스로 사용하여 다음 시퀀스 가져옴
                qteController.StartNewQTE(selectedQteSequences[qteSuccessCount]);
            }
            return NodeState.Running;
        }

        // 이전 QTE 결과 처리
        switch (qteController.CurrentState)
        {
            case QTEController.EQTEState.Success:
                qteSuccessCount++;
                Debug.Log($"QTE {qteSuccessCount}/{TOTAL_QTE_REPS}회 성공!");
                // TODO: 성공 피드백?(효과음, 화면 효과 등)
                PlayerManager.Instance.player.ForceChangeAnimation(AnimatorHash.PlayerAnimation.SuccessQTE);

                // 마지막 성공이 아니라면 다음 QTE 대기 상태로 전환
                if (qteSuccessCount < TOTAL_QTE_REPS)
                {
                    isWaitingForNextQTE = true;
                    qteWaitTimer = Time.time; // 대기 타이머 시작
                }
                break;

            case QTEController.EQTEState.Failure:
                qteFailed = true; // 실패 플래그 설정
                break;

            case QTEController.EQTEState.Idle:
                // 첫 QTE 시작
                qteController.StartNewQTE(selectedQteSequences[qteSuccessCount]);
                break;
        }

        return NodeState.Running;
    }

    /// <summary>
    /// ActionNode에 연결될 동기 래퍼 메서드
    /// 비동기 작업을 시작시키고 진행 상태만 체크하여 반환
    /// </summary>
    private NodeState CleanupAction_Wrapper()
    {
        // 비동기 작업이 아직 시작되지 않았다면
        if (!isCleanupRunning)
        {
            isCleanupRunning = true;
            // 비동기 메서드를 호출하되 기다리지 않고 다음으로 넘어감
            CleanupAction_Async().Forget();
            // 비동기 작업이 진행 중임을 알리기 위해 Running 상태 반환
            return NodeState.Running;
        }

        // 비동기 작업이 완료되었다면
        if (isCleanupComplete)
        {
            // 성공 상태를 반환하여 시퀀스 종료
            return NodeState.Success;
        }

        // 비동기 작업이 아직 진행 중이라면
        return NodeState.Running;
    }

    private async UniTask CleanupAction_Async()
    {
        if (qteFailed)
        {
            Debug.Log("[QTE] 블러드문 실패! 플레이어 즉사.");
            target.Condition.TakeDamage(skillData.damage1);
            monster.MonsterAI.DeactivateBt();
        }
        else
        {
            Debug.Log("[QTE] 블러드문 3회 성공!");

            PlayerManager.Instance.player.ForceChangeAnimation(AnimatorHash.PlayerAnimation.EndQTE);

            // EndQTE 애니메이션이 끝날 때까지 기다림
            await UniTask.Delay(TimeSpan.FromSeconds(END_QTE_ANIM_DELAY));

            await FadeManager.Instance.FadeOut();

            // 암전 해제
            EffectManager.Instance.StopEffectByType(EffectType.Background);

            if (bloodMoonInstance != null)
            {
                ObjectPoolManager.Instance.ReleaseObject(AddressablePaths.UI.BloodMoon, bloodMoonInstance);
                //bloodMoonInstance.SetActive(false);
            }
            if (qte_UI != null)
            {
                ObjectPoolManager.Instance.ReleaseObject(AddressablePaths.UI.QTE_UI, qte_UI);
            }

            await FadeManager.Instance.FadeIn();

            // 플레이어의 방향(스케일) 원래대로 복원
            target.transform.localScale = originalPlayerScale;

            PlayerManager.Instance.player.runFSM = true;

            if (monster.SpriteRenderer != null)
            {
                monster.SpriteRenderer.sortingOrder = originalSortingOrder;
            }

            monster.Condition.SetInivincible(false);
        }

        // 모든 비동기 작업이 끝났음을 래퍼 메서드에 알림
        isCleanupComplete = true;
    }
}