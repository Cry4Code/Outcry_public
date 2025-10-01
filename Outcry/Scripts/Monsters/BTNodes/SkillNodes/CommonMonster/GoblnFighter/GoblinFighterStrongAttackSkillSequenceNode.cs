using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GoblinFighterStrongAttackSkillSequenceNode : SkillSequenceWithChaseNode
{
    // 경과 시간, 쿨타임 등 계산용
    private float stateEnterTime;   // 스킬(상태)에 진입한 시간

    // 애니메이션 클립 초당 프레임 수
    private const float ANIMATION_FRAME_RATE = 20f;
    // 투사체 생성 프레임
    private const float INSTANTIATE_STONE_TIME = (1.0f / ANIMATION_FRAME_RATE) * 11;    // 12프레임이 지난 시점

    // 투사체 오브젝트
    private GameObject stone;

    // 투사체 좌표
    private Vector2 position = new Vector2(4.63f, -1f);

    // 투사체 생성 플레그
    private bool isSpawned = false;

    private string projectilePath;

    public GoblinFighterStrongAttackSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "GoblinFighterStrongAtkNode";        
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);

        // 투사체 로드
        projectilePath = AddressablePaths.Projectile.Stone;
        await ObjectPoolManager.Instance.RegisterPoolAsync(projectilePath);
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isCooldownComplete;

        // 쿨다운 체크
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
            isSpawned = false;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isCooldownComplete;
        Debug.Log($"[{monster.name}] Skill {skillData.skillName} usable?" +
            $" {result} : Cooldown {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;

        /*
        기본 피해 : 2
        공격 판정 앞쪽에 돌기둥(Stone 투사체) 생성

         - **플레이어 대응**
             - 패링 사용 불가
        */

        // 패리 불가 불값 수정
        monster.AttackController.SetIsCountable(false);

        // 스킬 트리거 켜기
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.StrongAttack);
            monster.AttackController.SetDamages(skillData.damage1); // 플레이어 데미지 주가

            skillTriggered = true;

            // 상태 시작 시간 저장
            stateEnterTime = Time.time;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.StrongAttack);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화
            monster.AttackController.SetIsCountable(true); // 카운터블 변수 초기화
            skillTriggered = false;
            state = NodeState.Success;
        }

        // 애니메이션 경과 시간 계산
        float animationElapsedTime = Time.time - stateEnterTime;
        // 몬스터가 바라보는 방향
        bool faceRight = monster.transform.localScale.x >= 0f;

        // 애니메이션의 동작 시간에 투사체(Stone) 생성 로직 실행
        if (animationElapsedTime >= INSTANTIATE_STONE_TIME && !isSpawned)
        {
            Debug.Log($"{skillData.skillName} : spawn {projectilePath} at {position}");
            monster.AttackController.InstantiateProjectile(projectilePath, position, faceRight, skillData.damage1, false);
            isSpawned = true;
        }

        return state;
    }
}
