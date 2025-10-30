using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinRogueAttackSkillSequenceNode : SkillSequenceWithChaseNode
{
    // 스킬 내 구간 생성용 Segment, FSM의 간략화 버전
    private struct Segment
    {
        public float start, end;
        public Action OnEnter, OnExit;
        public Action<float> OnUpdate;
    }

    private Segment[] segments;
    private int currentSeg;
    private float stateEnterTime;

    private int animatorTriggerHash;
    private int animationNameHash;

    private const float ANIMATION_FRAME_RATE = 20f;
    private const float MOVE_START_TIME = (1f / ANIMATION_FRAME_RATE) * 10f;
    private const float MOVE_END_TIME = (1f / ANIMATION_FRAME_RATE) * 12f;
    private const float SPEED = 30f;

    public GoblinRogueAttackSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "GoblinRogueAtkNode";
        animatorTriggerHash = AnimatorHash.MonsterParameter.NormalAttack;
        animationNameHash = AnimatorHash.MonsterAnimation.NormalAttack;
    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);

        segments = new[]
        {
            new Segment // 준비 구간
            {
                start = 0f, end = MOVE_START_TIME,
                OnEnter = () => { },
                OnUpdate = dt => { },
                OnExit = () => { }
            },
            new Segment // 이동 구간
            {
                start = MOVE_START_TIME, end = MOVE_END_TIME,
                OnEnter = () => 
                {
                    var direction = (monster.transform.localScale.x >= 0) ? Vector2.right : Vector2.left;
                    monster.Rb2D.velocity = direction * SPEED;                    
                },
                OnUpdate = dt => { },
                OnExit = () => { monster.Rb2D.velocity = Vector2.zero; }
            },
            new Segment // 이동 후 대기 구간
            {
                start = MOVE_END_TIME, end = float.PositiveInfinity,
                OnEnter = () => { monster.Rb2D.velocity = Vector2.zero; },
                OnUpdate = dt => { },
                OnExit = () => { }
            }
        };
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isCooldownComplete;

        // 쿨다운 체크
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
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

        // 스킬 트리거 켜기
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(animatorTriggerHash);
            monster.AttackController.SetDamages(skillData.damage1); // 플레이어 데미지 주가

            skillTriggered = true;
            stateEnterTime = Time.time;
            currentSeg = 0;
            segments[0].OnEnter();
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, animationNameHash);
            return NodeState.Running;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }        

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, animationNameHash);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화
            monster.Rb2D.velocity = Vector2.zero; // 속도 초기화 안전장치
            skillTriggered = false;
            currentSeg = 0;
            state = NodeState.Success;
        }

        // 애니메이션 경과 시간 계산
        float elapsedTime = Time.time - stateEnterTime;

        // segment 전환 실행
        while (currentSeg < segments.Length && elapsedTime >= segments[currentSeg].end)
        {
            segments[currentSeg].OnExit?.Invoke();
            currentSeg++;
            if (currentSeg < segments.Length)
                segments[currentSeg].OnEnter?.Invoke();
        }
        // 현재 segment의 update 실행
        if (currentSeg < segments.Length)
            segments[currentSeg].OnUpdate?.Invoke(Time.fixedDeltaTime);

        return state;
    }
}
