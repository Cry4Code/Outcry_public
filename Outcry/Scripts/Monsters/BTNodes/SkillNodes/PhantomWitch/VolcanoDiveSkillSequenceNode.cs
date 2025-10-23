using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoDiveSkillSequenceNode : SkillSequenceNode
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
    private Vector2 stateStartPosition;

    private const float ANIMATION_FRAME_RATE = 20f;
    private const float JUMP_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 0; // 시작 직후 상승
    private const float JUMP_END_TIME = (1.0f / ANIMATION_FRAME_RATE) * 9;
    private const float DROP_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 22;
    private const float DROP_END_TIME = (1.0f / ANIMATION_FRAME_RATE) * 26;
    private float SPEED_UP = 15f;
    private float SPEED_DOWN = 30f;

    public VolcanoDiveSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "VolcanoDiveSkillSequenceNode";
    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);

        segments = new[]
        {
            new Segment // 상승 구간
            {
                start = JUMP_START_TIME, end = JUMP_END_TIME,
                OnEnter = () => 
                {
                    monster.Rb2D.velocity = Vector2.up * SPEED_UP;
                    monster.Rb2D.gravityScale = 0f;
                    stateStartPosition = monster.Rb2D.position;
                },
                OnUpdate = dt => { },
                OnExit = () => { monster.Rb2D.velocity = Vector2.zero; }
            },
            new Segment // 공중 체류 구간
            {
                start = JUMP_END_TIME, end = DROP_START_TIME,
                OnEnter = () => 
                { 
                    monster.Rb2D.velocity = Vector2.zero;
                    monster.Rb2D.gravityScale = 0f;
                },
                OnUpdate = dt => { },
                OnExit = () => { }
            },
            new Segment // 하강 구간
            {
                start = DROP_START_TIME, end = DROP_END_TIME,
                OnEnter = () => 
                {
                    monster.Rb2D.velocity = Vector2.down * SPEED_DOWN;
                    monster.Rb2D.gravityScale = 0f;
                },
                OnUpdate = dt => 
                {
                    var curPos = monster.Rb2D.position;
                    // 너무 가깝다면 바로 종료
                    if ((stateStartPosition - curPos).sqrMagnitude <= 0.01f)
                    {
                        monster.Rb2D.position = stateStartPosition + Vector2.up * 0.02f;
                        JumpToEndOfCurrentSegment();
                        return;
                    }                    

                    Vector2 curVel = monster.Rb2D.velocity;
                    float nx = Mathf.SmoothDamp(curPos.x, stateStartPosition.x, ref curVel.x, 0.1f, Mathf.Infinity, dt);
                    float ny = Mathf.SmoothDamp(curPos.y, stateStartPosition.y, ref curVel.y, 0.1f, Mathf.Infinity, dt);
                    var next = new Vector2(nx, ny);
                    monster.Rb2D.MovePosition(next);
                },
                OnExit = () => 
                { 
                    monster.Rb2D.velocity = Vector2.zero;
                    monster.Rb2D.gravityScale = 1f;
                }
            },
            new Segment // 지상 체류 구간
            {
                start = DROP_END_TIME, end = float.PositiveInfinity,
                OnEnter = () => 
                {                    
                    monster.Rb2D.position = stateStartPosition + Vector2.up * 0.02f;
                    monster.Rb2D.velocity = Vector2.zero;
                    monster.Rb2D.gravityScale = 1f;
                },
                OnUpdate = dt => { },
                OnExit = () => { }
            }
        };
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;

        Vector2 distance = monster.transform.position - target.transform.position;
        float rangeSqr = skillData.range * skillData.range;
        float distanceSqr = Vector2.SqrMagnitude(distance);

        if (distanceSqr <= rangeSqr)
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
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.VolcanoDive);
            monster.AttackController.SetDamages(skillData.damage1);

            skillTriggered = true;
            stateEnterTime = Time.time; // 상태 시작 시간 저장
            currentSeg = 0;
            segments[0].OnEnter();  // 초기 세그먼트 OnEnter 바로 호출
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.VolcanoDive);
            return NodeState.Running;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.VolcanoDive);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화
            monster.Rb2D.velocity = Vector2.zero;   // 속도, 중력 초기화 안전 장치
            monster.Rb2D.gravityScale = 1f;
            skillTriggered = false;
            currentSeg = 0;
            return NodeState.Success;
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

    /// <summary>
    /// 현재 세그먼트 즉시 종료하고 다음 세그먼트로 전환
    /// </summary>
    void JumpToEndOfCurrentSegment()
    {
        segments[currentSeg].OnExit?.Invoke();
        currentSeg++;
        if (currentSeg < segments.Length)
            segments[currentSeg].OnEnter?.Invoke();
    }
}
