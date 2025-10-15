using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Pool;

public class FinalHorizonSkillSequenceNode : SkillSequenceNode
{
    //레이저 생성 범위
    private float minX;
    private float maxX;

    private const float LASER_LENGTH = 4.8f; //레이저 오브젝트의 x축 길이

    private GameObject attackRange;  //임시
    
    
    public FinalHorizonSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "FinalHorizonSkillSequenceNode";
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        this.monster = monster;
        this.target = target;
        
        ConditionNode canPerform = new ConditionNode(CanPerform);
        ActionNode chargingAction = new ActionNode(ChargingAction);
        ActionNode warningAction = new ActionNode(WarningAction);
        ActionNode skillAction = new ActionNode(SkillAction);
        ActionNode returnAction = new ActionNode(ReturnAction);

        canPerform.nodeName = "CanPerform";
        chargingAction.nodeName = "ChargingAction";
        warningAction.nodeName = "WarningAction";
        skillAction.nodeName = "SkillAction";
        returnAction.nodeName = "ReturnAction";
        
        children.Clear();
        AddChild(canPerform);
        AddChild(chargingAction);
        AddChild(warningAction);
        AddChild(skillAction);
        AddChild(returnAction);
        
        nodeName = skillData.skillName + skillData.skillId;
        
        //CinemachineVirtualCamera에 붙은 polygon collider 영역을 기준으로 레이저가 생성될 x축의 좌우 범위를 파악하기
        CinemachineVirtualCamera vcam = StageManager.Instance.StageCamera;
        if(vcam == null) Debug.LogError("vcam is null");
        CinemachineConfiner2D confiner = vcam.GetComponent<CinemachineConfiner2D>();
        if (confiner != null && confiner.m_BoundingShape2D != null)
        {
            Collider2D boundingCollider = confiner.m_BoundingShape2D;
            
            Bounds bounds = boundingCollider.bounds;
            minX = bounds.min.x; 
            maxX = bounds.max.x;

            Debug.Log($"Confiner의 가장 좌측 X 좌표: {minX}");
            Debug.Log($"Confiner의 가장 우측 X 좌표: {maxX}");
        }
        else
        {
            Debug.LogError("CinemachineConfiner 2D 또는 Bounding Shape 2D가 할당되지 않았음");
        }
        
        //laser Object 풀에 등록 미리 로드 (minX, maxX 사이에 최대로 필요한 갯수 계산해야함)
        int maxLaserCount = Mathf.CeilToInt((maxX - minX) / LASER_LENGTH) + 1; //레이저 오브젝트의 x축 길이 간격으로 생성하므로, 필요한 최대 갯수 계산.
        Debug.Log($"ID {skillData.skillId}: 레이저 오브젝트 풀에 등록. maxLaserCount=" + maxLaserCount);
        await ObjectPoolManager.Instance.RegisterPoolAsync(AddressablePaths.Projectile.Laser,maxLaserCount, maxLaserCount);
        
        //laser 범위 표시용 오브젝트
        await ObjectPoolManager.Instance.RegisterPoolAsync(AddressablePaths.AttackRange, 1, 1);
    }

    protected override bool CanPerform()
    {
        //For Test
        if (TestManager.Instance.triggerForFH)
        {
            Debug.LogWarning("[TestManager] FinalHorizon 스킬 강제 발동!");
            TestManager.Instance.triggerForFH = false;
            
            //스킬 실행용 플래그들 초기화
            skillTriggered = false;
            isCharging = false;
            isChargingAnimationStarted = false;
            isWarningUITriggerd = false;
            isReturnAnimationStarted = false;
            isStompAnimationReady = false;
            isStompAnimationStarted = false;
            isLaserEffectTriggered = false;
            isReturnTriggered = false;
            isReturnAnimationStarted = false;
            return true;
        }
        
        if (skillTriggered) //한번 실행되면 다시 실행될 일 없음. 실행 끝난 이후 리셋 X
        {
            return false;
        }
        
        //체력이 일정 이하일때
        bool isLowHealth = monster.Condition.CurrentHealth < skillData.triggerHealth * monster.Condition.MaxHealth;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) {isLowHealth} : {monster.Condition.CurrentHealth} / {monster.Condition.MaxHealth} < {skillData.triggerHealth}");
        if (!isLowHealth) return false;
        
        //쿨타임 체크
        bool isCooldownComplete = Time.time - lastUsedTime >= skillData.cooldown;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) cooldownComplete={isCooldownComplete} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return isCooldownComplete;
    }

    //1. 바람 이펙트로 기 모으기
    //2. 워닝 UI
    //3. 발구르기
    //4. 레이저 발사
    //5. 돌아오기 => 이건 몬스터 애니메이션으로 진행할지 고민.

    private bool isCharging = false;
    private bool isChargingAnimationStarted = false;
    private NodeState ChargingAction()
    {
        if (!isCharging)
        {
            //0. 스킬 사용 됨. 몬스터 무적 상태
            skillTriggered = true;
            monster.Condition.SetInivincible(true);

            //1. 점프 애니메이션 실행.
            isCharging = true;
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.FinalHorizon);
        }

        //2. 차징 애니메이션이 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isChargingAnimationStarted)
        {
            isChargingAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.FinalHorizonStart);
            return NodeState.Running;
        }
        
        //3. 차징 애니메이션이 재생되는 동안 대기.
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.FinalHorizonStart))
        {
            return NodeState.Running;
        }
        
        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) ChargingAction Done!");
        return NodeState.Success; 
    }

    private bool isWarningUITriggerd = false;
    private NodeState WarningAction()
    {
        if (!isWarningUITriggerd) //아직 경고 UI 시작 전.
        {
            // 0. 경고창 띄우기
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.ScreenUI);
            isWarningUITriggerd = true;
            attackRange = ObjectPoolManager.Instance.GetObject(AddressablePaths.AttackRange, monster.transform);
            //attackRange 크기를 레이저 크기에 맞게 조정
            attackRange.transform.localScale = new Vector3(maxX - minX, 1f, 1f); //레이저 오브젝트의 x축 길이 간격으로 생성하므로, 그에 맞게 크기 조정.
            attackRange.transform.localPosition = new Vector3(0f, -1f, 0f);
            
            return NodeState.Running;
        }
        
        if (EffectManager.Instance.IsEffectPlaying(skillData.skillId, EffectType.ScreenUI))
        {
            // 1. 경고창이 재생되는 동안 대기.
            return NodeState.Running;
        }
        ObjectPoolManager.Instance.ReleaseObject(AddressablePaths.AttackRange, attackRange);
        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) WarningAction Done!");
        return NodeState.Success;
    }
    
    private bool isStompAnimationReady = false;
    private bool isStompAnimationStarted = false;
    private bool isLaserEffectTriggered = false;
    private bool isSkillAnimationStarted = false;
    
    protected override NodeState SkillAction()
    {
        //1. 전조 애니메이션 시작
        //1-1. FinalHorizonMiddle (내려찍는모양) 시작
        if (!isStompAnimationReady)
        {
            isStompAnimationReady = true;
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.IsReady);
        }
        //1-2. FinalHorizonMiddle 내려찍는 애니메이션이 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isStompAnimationStarted)
        {
            isStompAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.FinalHorizonMiddle);
            return NodeState.Running;
        }
        //1-3. FinalHorizonMiddle 내려찍는 애니메이션이 재생되는 동안 대기.
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.FinalHorizonMiddle))
        {
            return NodeState.Running;
        }
        
        //2. 레이저와 함께 다음 애니메이션 재생
        //레이저 생성 minX부터 maxX까지 laser Object의 x축 길이 간격으로 생성
        if (!isLaserEffectTriggered)
        {
            isLaserEffectTriggered = true;
            
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.IsReady);
            
            for (float x = minX; x <= maxX; x += LASER_LENGTH)
            {
                Vector3 spawnPosition = new Vector3(x + LASER_LENGTH / 2, -1f, 0f); //레이저 오브젝트의 pivot이 중앙이므로, 길이의 절반만큼 더해줌.
                GameObject laser = ObjectPoolManager.Instance.GetObject(AddressablePaths.Projectile.Laser, monster.transform, spawnPosition);
                laser.GetComponent<LaserController>().SetDamage(skillData.damage1); //비용이 높은 메서드...
                //레이저는 애니메이션 종료 후 스스로 오브젝트 풀로 돌아감.
            }
        }
        return NodeState.Success;
    }

    private bool isReturnTriggered = false;
    private bool isReturnAnimationStarted = false;
    private NodeState ReturnAction()
    {
        //리턴은 HasExit으로 처리.
        //1. 리턴 애니메이션이 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isReturnAnimationStarted)
        {
            isReturnAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.FinalHorizonEnd);
            return NodeState.Running;
        }

        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.FinalHorizonEnd))
        {
            return NodeState.Running;
        }

        ResetState();
        return NodeState.Success;
    }

    private void ResetState()
    {
        monster.Condition.SetInivincible(false);
    }
}
