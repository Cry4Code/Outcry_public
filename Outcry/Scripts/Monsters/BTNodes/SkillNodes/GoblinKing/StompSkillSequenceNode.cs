using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
[Serializable]
public class StompSkillSequenceNode : SkillSequenceNode
{

    private float stateEnterTime; // 시작 시간 정의용
    private float animationElapsedTime; // 지난 시간
    
    // 애니메이션 클립 초당 프레임 수
    private const float ANIMATION_FRAME_RATE = 20f;
    // 바닥 찍는 프레임
    private const float STOMP_TIME = (1.0f / ANIMATION_FRAME_RATE) * 15;   // 16프레임이 지난 시점
    
    public StompSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "StompSkillSequenceNode";
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;
        
        //플레이어와 거리 이내에 있을때
        if (Vector2.Distance(monster.transform.position, target.transform.position) <= skillData.range)
        {
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

        //쿨다운 확인
        if(Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isInRange && isCooldownComplete;
        Debug.Log($"Skill {skillData.skillName} used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;
        //기본 피해 : HP 2칸 감소
        //추가 효과 : 피격시 플레이어 다운

        // - **플레이어 대응**
        //     - 회피 사용 가능
        //     - 패링 사용 가능

        if (!skillTriggered)
        {
            effectStarted = false;
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.Stomp);
            //todo. player damage 처리
            monster.AttackController.SetDamages(skillData.damage1);
            skillTriggered = true;

            animationElapsedTime = 0;
            // 상태 시작 시간 저장
            stateEnterTime = Time.time;
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.Stomp);
            return NodeState.Running;
        }

        // 시작 직후는 무조건 Running
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 경과 시간 계산
        animationElapsedTime = Time.time - stateEnterTime;
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.Stomp);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"Skill End: {skillData.skillName} (ID: {skillData.skillId})");
            
            monster.AttackController.SetDamages(0); //데미지 초기화.
            skillTriggered = false;
            state = NodeState.Success;
        }

        if (animationElapsedTime > STOMP_TIME && !effectStarted)
        {
            effectStarted = true;
            EffectManager.Instance.PlayEffectsByIdAsync(skillId, EffectOrder.Monster, monster.gameObject).Forget();
        }

        return state;
    }
}
