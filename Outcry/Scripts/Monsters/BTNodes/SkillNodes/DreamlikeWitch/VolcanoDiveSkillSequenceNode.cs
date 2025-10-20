using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoDiveSkillSequenceNode : SkillSequenceNode
{
    private float stateEnterTime;
    private int nextActionIndex;
    private float[] actionFrames;

    private const float ANIMATION_FRAME_RATE = 20f;
    // 행동 프레임, 상승은 시작 직후 바로 시작
    private const float FLY_END_TIME = (1.0f / ANIMATION_FRAME_RATE) * 9;
    private const float DROP_START_TIME = (1.0f / ANIMATION_FRAME_RATE) * 13;
    private const float DROP_END_TIME = (1.0f / ANIMATION_FRAME_RATE) * 26;

    public VolcanoDiveSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "VolcanoDiveSkillSequenceNode";
    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);
        actionFrames = new float[3];
        actionFrames[0] = FLY_END_TIME;
        actionFrames[1] = DROP_START_TIME;
        actionFrames[2] = DROP_END_TIME;
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
            nextActionIndex = 0;
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
            skillTriggered = false;
            return NodeState.Success;
        }

        // 애니메이션 경과 시간 계산
        float elapsedTime = Time.time - stateEnterTime;
        // 몬스터가 바라보는 방향
        bool faceRight = monster.transform.localScale.x >= 0f;

        // todo. 실제 행동 로직
        while (nextActionIndex < 3 /* 행동의 갯수 */ && elapsedTime >= actionFrames[nextActionIndex])
        {
            switch (nextActionIndex)
            {
                case 0:     // 상승
                    break;
                case 1:     // 공중에 체류
                    break;
                case 2:     // 하강
                default:
                    break;
            }
            nextActionIndex++;
        }

        return state;
    }
}
