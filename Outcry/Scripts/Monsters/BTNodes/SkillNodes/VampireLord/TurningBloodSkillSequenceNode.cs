using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurningBloodSkillSequenceNode : SkillSequenceNode
{
    private bool isAnimationStarted = false;
    private int projectileLaunched = 0;

    private string projectilePath = AddressablePaths.Projectile.TurningBlood;
    private Vector2 projectilePosition = new Vector2(1, 0);
    
    
    public TurningBloodSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "TurningBloodSkillSequenceNode";

    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);
        await ObjectPoolManager.Instance.RegisterPoolAsync(projectilePath);
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

        // - **플레이어 대응**
        //     - 패링 사용 불가
        monster.AttackController.SetIsCountable(false);
        
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.TurningBlood);
            skillTriggered = true;
        }

        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.TurningBlood);
            return NodeState.Running;
        }

        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.TurningBlood, 0f, 0.6f))
        {
            Debug.Log($"Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            return NodeState.Running;
        }

        if (projectileLaunched < 1)
        {
            projectileLaunched++;
            bool faceRight = monster.transform.localScale.x >= 0f;
            Debug.Log($"{skillData.skillName} : spawn {projectilePath}");

            //projectilePosition을 monster.transform 기준에서 world좌표로 변환
            projectilePosition = monster.transform.position + new Vector3(faceRight ? this.projectilePosition.x + 1f : -this.projectilePosition.x - 1f, this.projectilePosition.y);
            var projectileInstance = ObjectPoolManager.Instance.GetObject(projectilePath, position: projectilePosition);
            var projectileController = projectileInstance.GetComponent<TracerProjectileController>();
            projectileController.SetPoolKey(projectilePath);
            projectileController.Init(skillData.damage1, false);
            return NodeState.Running;
        }

        ResetTriggers();
        return NodeState.Success;
    }

    private void ResetTriggers()
    {
        skillTriggered = false;
        isAnimationStarted = false;
        projectileLaunched = 0;
    }
}
