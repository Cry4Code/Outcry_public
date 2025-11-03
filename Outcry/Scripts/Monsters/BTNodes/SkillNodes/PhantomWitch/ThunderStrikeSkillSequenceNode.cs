using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ThunderStrikeSkillSequenceNode : SkillSequenceNode
{
    private string projectilePath;
    private float stateEnterTime;   // 시작 직후 바로 투사체 생성, 프레임 계산 불필요
    private bool isSpawned;

    private bool isAnimationStarted = false;
    private int animatorNameHash;
    public ThunderStrikeSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "ThunderStrikeSkillSequenceNode";
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);
        projectilePath = AddressablePaths.Projectile.ThunderStrike;
        isAnimationStarted = false;
        animatorNameHash = AnimatorHash.MonsterParameter.ThunderStrike;
        await ObjectPoolManager.Instance.RegisterPoolAsync(projectilePath);
    }

    protected override bool CanPerform()
    {
        // 포션-오버라이드 발동 시, 다른 조건 상관없이 즉시 발동
        if (monster.MonsterAI.blackBoard.PotionOverrideEdge)
            return true;

        bool result;
        bool isInRange;
        bool isCooldownComplete;

        Vector2 distance = monster.transform.position - target.transform.position;
        float rangeSqr = skillData.range * skillData.range;
        float distanceSqr = Vector2.SqrMagnitude(distance);

        // 사거리 체크 (range 이상일 때)
        if (distanceSqr >= rangeSqr)
        {
            isInRange = true;
        }
        else
        {
            isInRange = false;
        }

        //쿨다운 확인
        if (Time.time - lastUsedTime >= skillData.cooldown)
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

        // 스킬 트리거 켜기
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.MonsterAI.TryConsumePotionEdge(); // 포션 레치 소모
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.ThunderStrike);
            monster.AttackController.SetDamages(skillData.damage1);

            skillTriggered = true;
            stateEnterTime = Time.time; // 상태 시작 시간 저장
            isSpawned = false;
            EffectManager.Instance.PlayEffectsByIdAsync(skillId * 10, EffectOrder.Monster, monster.gameObject).Forget();
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, animatorNameHash);
            return NodeState.Running;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.ThunderStrike);
        
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화
            skillTriggered = false;
            isAnimationStarted = false;
            return NodeState.Success;
        }

        // 몬스터가 바라보는 방향
        bool faceRight = monster.transform.localScale.x >= 0f;

        // 투사체 생성
        if (!isSpawned) // 시작 직후 바로 생성, 프레임 계산 불필요
        {
            isSpawned = true;
            Vector3 spawnPos = target.transform.position;
            Debug.Log($"{skillData.skillName} : ThunderStrike spawned - position {spawnPos}");
            monster.AttackController.InstantiateProjectileAtWorld(projectilePath, spawnPos, faceRight, skillData.damage1);
            
        }

        return state;
    }
}
