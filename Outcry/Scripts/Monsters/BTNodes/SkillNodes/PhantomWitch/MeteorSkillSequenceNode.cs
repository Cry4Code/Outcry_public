using System;
using UnityEngine;

public class MeteorSkillSequenceNode : SkillSequenceNode
{
    // 스킬 내 구간 생성용 Segment, FSM의 간략화 버전
    private struct Segment
    {
        public float start, end;
        public Action OnEnter, OnExit;
        public Action<float> OnUpdate;
    }

    private float stateEnterTime;
    private string projectilePaths;
    private Vector3 startPosition;

    private Segment[] segments;
    private int currentSeg;
    private LayerMask groundMask = LayerMask.GetMask("Ground");

    private const float ANIMATION_FRAME_RATE = 20f;
    private const float JUMP_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 0;
    private const float JUMP_INTERVAL_TIME = (1.0f / ANIMATION_FRAME_RATE) * 6;
    private const float JUMP_SPEED_UP_TIME = (1.0f / ANIMATION_FRAME_RATE) * 11;
    private const float FLOATING_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 17;
    private const float ATTACK_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 26;
    private const float FALLING_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 47;
    private const float BACK_TO_LAND_TIME = (1.0f / ANIMATION_FRAME_RATE) * 51;

    private const float SPEED_UP = 15f;
    private const float SPEED_UP_FASTTER = 22f;
    private const float SPEED_DOWN = 44f;

    public MeteorSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "MeteorSkillSequenceNode" + skillId;
        projectilePaths = AddressablePaths.Projectile.Meteor;
    }
    public MeteorSkillSequenceNode(MeteorSkillSequenceNode node) : base(node.skillId)
    {
        this.nodeName = node.nodeName;
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);
        await ObjectPoolManager.Instance.RegisterPoolAsync(projectilePaths);

        segments = new[]
        {
            new Segment // 상승 구간 1
            {
                start = JUMP_START_TIME, end = JUMP_INTERVAL_TIME,
                OnEnter = () =>
                {
                    startPosition = monster.Rb2D.position;
                    monster.Rb2D.velocity = Vector2.up * SPEED_UP;
                    monster.Rb2D.gravityScale = 0f;
                },
                OnUpdate = dt => { },
                OnExit = () => { monster.Rb2D.velocity = Vector2.zero; }
            },
            new Segment // 상승 중간 체류
            {
                start = JUMP_INTERVAL_TIME, end = JUMP_SPEED_UP_TIME,
                OnEnter = () =>
                {
                    monster.Rb2D.velocity = Vector2.zero;
                    monster.Rb2D.gravityScale = 0f;
                },
                OnUpdate = dt => { },
                OnExit = () => { }
            },
            new Segment // 상승 구간 2
            {
                start = JUMP_SPEED_UP_TIME, end = FLOATING_START_TIME,
                OnEnter = () =>
                {
                    monster.Rb2D.velocity = Vector2.up * SPEED_UP_FASTTER;
                    monster.Rb2D.gravityScale = 0f;
                },
                OnUpdate = dt => { },
                OnExit = () => { monster.Rb2D.velocity = Vector2.zero; }
            },
            new Segment // 최상단 체류 구간
            {
                start = FLOATING_START_TIME, end = ATTACK_START_TIME,
                OnEnter = () =>
                {
                    monster.Rb2D.velocity = Vector2.zero;
                    monster.Rb2D.gravityScale = 0f;
                },
                OnUpdate = dt => { },
                OnExit = () => { }
            },
            new Segment // 공격 소환 구간
            {
                start = ATTACK_START_TIME, end = FALLING_START_TIME,
                OnEnter = () =>
                { 
                    // 투사체 소환
                    SpawnMeteors();
                },
                OnUpdate = dt => { },
                OnExit = () => { }
            },
            new Segment // 하강 구간
            {
                start = FALLING_START_TIME, end = BACK_TO_LAND_TIME,
                OnEnter = () =>
                {
                    monster.Rb2D.velocity = Vector2.down * SPEED_DOWN;
                    monster.Rb2D.gravityScale = 1f;
                },
                OnUpdate = dt =>
                {
                    if (TryGroundHit(out var hit))  // 바닥 접촉 시 조기 종료
                    {
                        monster.Rb2D.position = new Vector2(monster.Rb2D.position.x,
                            hit.point.y - (-1f) + 0.02f);  // 살짝 위로 옮겨서 바닥과 겹치지 않도록 스냅
                            // -1f는 발 위치, 0.02f는 살짝 옮기는 위치
                        JumpToEndOfCurrentSegment();
                        return;
                    }
                },
                OnExit = () =>
                {
                    monster.Rb2D.velocity = Vector2.zero;
                }
            },
            new Segment // 바닥 이동 확정 구간
            {
                start = BACK_TO_LAND_TIME, end = float.PositiveInfinity,
                OnEnter = () =>
                {
                    if (Vector2.Distance(monster.Rb2D.position, startPosition) > 0.1f)
                    {
                        monster.Rb2D.position = startPosition + (Vector3.up * 0.02f);
                        monster.Rb2D.velocity = Vector2.zero;
                        monster.Rb2D.gravityScale = 1f;
                    }
                },
                OnUpdate = dt => { },
                OnExit = () => { }
            }
        };
    }

    protected override bool CanPerform()
    {
        bool result;

        // 체력 확인
        bool isLowHealth;
        if (monster.Condition.CurrentHealth.CurValue() < skillData.triggerHealth * monster.Condition.MaxHealth) isLowHealth = true;
        else isLowHealth = false;

        //쿨다운 확인
        bool isCooldownComplete;
        if (Time.time - lastUsedTime >= skillData.cooldown) isCooldownComplete = true;
        else isCooldownComplete = false;
         
        result = isCooldownComplete && isLowHealth;
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
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.Meteor);
            monster.AttackController.SetDamages(skillData.damage1);

            skillTriggered = true;
            stateEnterTime = Time.time; // 상태 시작 시간 저장
            currentSeg = 0;
            segments[currentSeg].OnEnter?.Invoke();
        }

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.Meteor);
            return NodeState.Running;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.Meteor);
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

    private bool TryGroundHit(out RaycastHit2D hit)
    {
        float fallDist = Mathf.Abs(monster.Rb2D.velocity.y) * Time.deltaTime + 0.05f;
        Vector2 footOffset = new Vector2(0, -1f);   // 발 위치

        Vector2 origin = (Vector2)monster.Rb2D.position + footOffset;
        hit = Physics2D.Raycast(origin, Vector2.down, fallDist, groundMask);
        return hit.collider != null;
    }

    /// <summary>
    /// 현재 세그먼트 즉시 종료하고 다음 세그먼트로 전환
    /// </summary>
    private void JumpToEndOfCurrentSegment()
    {
        segments[currentSeg].OnExit?.Invoke();
        currentSeg++;
        if (currentSeg < segments.Length)
            segments[currentSeg].OnEnter?.Invoke();
    }

    private void SpawnMeteors()
    {
        // 무작위 갯수 설정 12~15
        int count = UnityEngine.Random.Range(12, 16);

        // 소환 위치 설정
        Vector3[] positions = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 pos;
            float x, y;

            int j = i % 3;
            switch (j)
            {
                case 0:
                    x = UnityEngine.Random.Range(-30f, -10f);
                    y = UnityEngine.Random.Range(-3f, 3f);
                    pos = new Vector3(x, y, 0);
                    positions[i] = pos;
                    break;
                case 1:
                    x = UnityEngine.Random.Range(-10f, 10f);
                    y = UnityEngine.Random.Range(-3f, 3f);
                    pos = new Vector3(x, y, 0);
                    positions[i] = pos;
                    break;
                case 2:
                    x = UnityEngine.Random.Range(10f, 30f);
                    y = UnityEngine.Random.Range(-3f, 3f);
                    pos = new Vector3(x, y, 0);
                    positions[i] = pos;
                    break;
            }
        }

        // 소환
        for (int i = 0; i < count; i++)
            monster.AttackController.InstantiateProjectile(projectilePaths, positions[i], true, skillData.damage1);
    }
}
