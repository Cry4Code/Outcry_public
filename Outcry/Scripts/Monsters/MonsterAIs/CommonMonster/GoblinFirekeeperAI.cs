using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class GoblinFirekeeperAI : MonsterAIBase
{
    private const float PATROL_INTERVAL = 1f; 
    private const float SKILL_INTERVAL = 1f;

    protected override void ConfigurePotionOverrideModes()
    {
        reactToPotion = false; // 포션 반응 안함.
    }

    // 트리 초기화
    protected override void InitializeBehaviorTree()
    {
        // 몬스터 데이터를 일반 몬스터로 형변환
        if (monster.MonsterData is not CommonMonsterModel monsterModel)
        {
            Debug.LogError($"[{nameof(GoblinRogueAI)}] CommonMonsterModel 필요, 실제 타입: {monster?.MonsterData?.GetType().Name}");
            return; // monsterModel 미할당 시 바로 return
        }

        // 필요 노드들 생성
        // root 노드
        SelectorNode root = new SelectorNode();
        root.nodeName = "Root";
        this.rootNode = root;

        #region 바닥 확인
        var startWhenGrounded = new SequenceNode();
        startWhenGrounded.nodeName = "CheckGroundThenStart";
        root.AddChild(startWhenGrounded);

        var isGrounded = new IsGroundedConditionNode(monster.transform);
        startWhenGrounded.AddChild(isGrounded);

        var behaviorSelector = new SelectorNode();
        behaviorSelector.nodeName = "BehaivorSelector";
        startWhenGrounded.AddChild(behaviorSelector);
        #endregion

        #region 공격 시퀀스 노드 (추격 포함)
        SequenceNode attackSequence = new SequenceNode();   // 공격 시퀀스
        attackSequence.nodeName = "AttackSequenceNode";
        behaviorSelector.AddChild(attackSequence);

        SelectorNode skillSelector = new SelectorNode();    // 공격 셀랙터
        skillSelector.nodeName = "SkillSelectorNode";
        attackSequence.AddChild(skillSelector);

        // 스킬 쿨타임 순으로 재정렬
        var entries = monsterModel.commonSkillsIds
            .Select(id =>
            {
                DataManager.Instance.MonsterSkillDataList.TryGetMonsterSkillModelData(id, out MonsterSkillModel data);
                return new { id, data };
            })
            .Where(x => x.data != null)                 // null 무시
            .OrderByDescending(x => x.data.cooldown)    // 쿨타임 내림차순으로 정렬
            .ThenBy(x => x.id)                          // 동률 시, id 순 정렬
            .ToList();

        // 정렬된 순서로 노드 생성, 추가
        foreach (var x in entries)
        {
            DataManager.Instance.SkillSequenceNodeDataList.TryGetSkillSequenceNode(x.id, out SkillSequenceNode skillNode);

            skillNode.InitializeSkillSequenceNode(monster, target);
            skillNode.nodeName = "S_SkillNode_" + x.data.skillName;  // 디버깅용 노드 이름 설정
            skillSelector.AddChild(skillNode);
        }

        // 공격 후 대기
        WaitActionNode attackWait = new WaitActionNode(SKILL_INTERVAL);
        attackSequence.AddChild(attackWait);
        #endregion

        #region 추격 노드
        SequenceNode chaseSequence = new SequenceNode();
        behaviorSelector.AddChild(chaseSequence);
        chaseSequence.nodeName = "ChaseSequenceNode";

        var chaseStartCondition = new IsInRangeConditionNode(monster.transform, target.transform,
            monsterModel.detectRange);
        chaseSequence.AddChild(chaseStartCondition);

        var chaseKeepCondition = new IsInRangeConditionNode(monster.transform, target.transform,
            monsterModel.disdetectRange);
        var chaseAction = new ChaseActionNode(monster.Rb2D, monster.transform, target.transform,
            monsterModel.chaseSpeed, monsterModel.approachRange,
            monster.Animator);
        var chaseGuarded = new WhileTrueDecorator(chaseKeepCondition, chaseAction);
        chaseSequence.AddChild(chaseGuarded);
        #endregion

        #region 순찰 노드
        SequenceNode patrolSeqence = new SequenceNode();
        patrolSeqence.nodeName = "patrolSequenceNode";
        behaviorSelector.AddChild(patrolSeqence);

        var notDetected = new InverterNode();
        var isDetected = new IsInRangeConditionNode(monster.transform, target.transform,
            monsterModel.detectRange);
        notDetected.SetChild(isDetected);
        var patrolAction = new PatrolActionNode(monster.Rb2D, monster.transform,
            monsterModel.patrolSpeed, monster.Animator);
        var patrolGuarded = new WhileTrueDecorator(notDetected, patrolAction);
        patrolSeqence.AddChild(patrolGuarded);

        WaitActionNode patrolWait = new WaitActionNode(PATROL_INTERVAL);
        patrolSeqence.AddChild(patrolWait);
        #endregion

        Debug.Log($"{gameObject.name} rootNode initialized");
    }
}
