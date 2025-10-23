using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 시퀀스를 만들때마다 상속 받으면 됩니다.
/// </summary>
[Serializable]
public abstract class SkillSequenceNode : SequenceNode
{
    [SerializeField]  protected int skillId;
    protected MonsterBase monster;
    protected PlayerController target;
    protected MonsterSkillModel skillData; //인스펙터에 직렬화 시키면 에러뜸.
    
    protected float lastUsedTime;
    protected bool skillTriggered = false;
    protected bool isAnimationStarted = false;
    protected bool isRunning = false;
    protected bool effectStarted = false;

    public bool IsRunning => isRunning;
    public int SkillId => skillId;

    public override NodeState Tick()
    {
        Debug.Log("[몬스터BT] SkillSequenceNode Tick 실행: " + nodeName);
        NodeState state = base.Tick();
        isRunning = (state == NodeState.Running);   // 실행 중일 때, 외부에 상황 노출
        return state;
    }

    public SkillSequenceNode(int skillId)
    {
        this.skillId = skillId;
        if (!DataManager.Instance.MonsterSkillDataList.TryGetMonsterSkillModelData(skillId, out skillData))
        {
            Debug.LogError($"Skill ID {skillId} could not be found.");
        }
    }
    
    public virtual void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        this.monster = monster;
        this.target = target;
        
        ConditionNode canPerform = new ConditionNode(CanPerform);
        ActionNode skillAction = new ActionNode(SkillAction);
        
        
        //노드 이름 설정 (디버깅용)
        canPerform.nodeName = "CanPerform";
        skillAction.nodeName = "SkillAction";
        
        children.Clear();
        AddChild(canPerform);
        AddChild(skillAction);

        nodeName = skillData.skillName + skillData.skillId;
        lastUsedTime = Time.time - skillData.cooldown;
    }
    protected abstract bool CanPerform();

    protected abstract NodeState SkillAction();
    
    protected void FlipCharacter()
    {
        float originalScaleX = monster.transform.localScale.x;
        if (monster.transform.position.x < target.transform.position.x)
            monster.transform.localScale = new Vector3(Mathf.Abs(originalScaleX), monster.transform.localScale.y, monster.transform.localScale.z);
        else
            monster.transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), monster.transform.localScale.y, monster.transform.localScale.z);
    }
    
    protected bool IsSkillAnimationPlaying(string animationName)
    {
        //스킬 애니메이션이 끝났는지 확인.
        bool isSkillAnimationPlaying = monster.Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"Running skill animation: {animationName} from {skillData.skillName} (ID: {skillData.skillId})");
            return true;
        }
        else
        {
            Debug.Log($"Using skill: {animationName} from {skillData.skillName} (ID: {skillData.skillId})");
            return false;
        }
    }
    
    /// <summary>
    /// Stun 등의 애니메이션으로 인해 Idle 애니메이션으로 복귀했는지 확인.
    /// </summary>
    /// <returns></returns>
    protected bool IsIdleAnimationPlaying()
    {
        if(AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.Idle))
        {
            //Idle 애니메이션이 재생 중
            return true;
        }

        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.Tired))
        {
            return true;
        }

        return false;
    }
}
