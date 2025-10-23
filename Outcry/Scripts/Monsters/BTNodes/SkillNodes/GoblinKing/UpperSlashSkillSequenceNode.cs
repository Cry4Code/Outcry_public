using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class UpperSlashSkillSequenceNode : SkillSequenceNode
{
    private int animationHash = AnimatorHash.MonsterParameter.UpperSlash;  
    
    // 애니메이션 클립 초당 프레임 수
    private const float ANIMATION_FRAME_RATE = 20f;

    private float[] attackSoundTime = new[]
    {
        (1f / ANIMATION_FRAME_RATE) * 19f
    };
    private int attackSoundIndex = 0;
    private float elapsedTime = 0;
    
    public UpperSlashSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "UpperSlashSequenceNode";
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;

        // 플레이어와 거리 2m 이내에 있을때
        // MonsterSkillModel 수정 필요 (Stomp 스킬 참조)
        if (Vector2.Distance(monster.transform.position, target.transform.position) <= skillData.range)
        {
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

        // 쿨다운 체크
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isInRange && isCooldownComplete;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        // 기본 피해 : HP 2칸 감소
        // 넉백 추가는 미정

        // ** 플레이어 대응 **
        //      - 회피 사용 가능
        //      - 패링 사용 가능

        if (!skillTriggered)
        {
            elapsedTime = 0;
            attackSoundIndex = 0;
            effectStarted = false;
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(animationHash);

            monster.AttackController.SetDamages(skillData.damage1);

            skillTriggered = true;
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.UpperSlash);
            return NodeState.Running;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }        

        if (!effectStarted)
        {
            effectStarted = true;
            EffectManager.Instance.PlayEffectsByIdAsync(skillId, EffectOrder.Monster, monster.gameObject).Forget();
        }

        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.UpperSlash);
        if (isSkillAnimationPlaying)
        {
            elapsedTime += Time.deltaTime;
            if (attackSoundIndex < attackSoundTime.Length)
            {
                if (elapsedTime >= attackSoundTime[attackSoundIndex])
                {
                    attackSoundIndex++;
                    EffectManager.Instance.PlayEffectByIdAndTypeAsync(Stage1BossEffectID.NormalAttack * 10 + (Random.Range(0, 2)), EffectType.Sound,
                        monster.gameObject).Forget();
                }    
            }
            Debug.Log($"Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.ResetDamages();  // 데미지 초기화
            skillTriggered = false;
            state = NodeState.Success;
        }

        return state;
    }
}
