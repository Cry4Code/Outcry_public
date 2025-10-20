using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreamlikeWitchAI : MonsterAIBase
{
    protected override void InitializeBehaviorTree()
    {
        SelectorNode root = new SelectorNode();
        root.nodeName = "Root";
        this.rootNode = root;

        BossMonsterModel monsterModel = (BossMonsterModel)monster.MonsterData;
        if (monsterModel == null)
        {
            Debug.LogError($"[DreamlikeWitch] (AI) monster model is null!");
        }

        #region 특수 공격 노드
        SequenceNode specialSkillSequence = new SequenceNode();
        specialSkillSequence.nodeName = "SpecialSkillSequenceNode";
        root.AddChild(specialSkillSequence);

        SelectorNode specialSkillSelector = new SelectorNode();
        specialSkillSelector.nodeName = "SpecialSkillSelectorNode";
        specialSkillSequence.AddChild(specialSkillSelector);

        WaitActionNode specialWaitActionNode = new WaitActionNode(Figures.Monster.SPECIAL_SKILL_INTERVAL);
        specialWaitActionNode.nodeName = "SpecialSkillWaitNode";
        specialSkillSequence.AddChild(specialWaitActionNode);

        foreach (int id in monsterModel.specialSkillIds)
        {
            DataManager.Instance.SkillSequenceNodeDataList.TryGetSkillSequenceNode(id, out SkillSequenceNode skillNode);
            DataManager.Instance.MonsterSkillDataList.TryGetMonsterSkillModelData(id, out MonsterSkillModel skillData);
            if (skillData != null)
            {
                skillNode.InitializeSkillSequenceNode(monster, target);
                skillNode.nodeName =  "S_SkillNode_" + skillData.skillName;
                specialSkillSelector.AddChild(skillNode);
            }
        }
        #endregion

        #region 일반 공격 노드
        SequenceNode commonSkillSequence = new SequenceNode();
        commonSkillSequence.nodeName = "CommonSkillSequenceNode";
        root.AddChild(commonSkillSequence);

        SkillSelectorNode commonSkillSelector = new SkillSelectorNode();
        commonSkillSelector.nodeName = "CommonSkillSelectorNode";
        commonSkillSequence.AddChild(commonSkillSelector);

        WaitActionNode commonWait = new WaitActionNode(Figures.Monster.COMMON_SKILL_INTERVAL);
        commonWait.nodeName = "CommonSkillWaitNode";
        commonSkillSequence.AddChild(commonWait);

        foreach (int id in monsterModel.commonSkillIds)
        {
            DataManager.Instance.SkillSequenceNodeDataList.TryGetSkillSequenceNode(id, out SkillSequenceNode skillNode);
            DataManager.Instance.MonsterSkillDataList.TryGetMonsterSkillModelData(id, out MonsterSkillModel skillData);
            if (skillData != null)
            {
                skillNode.InitializeSkillSequenceNode(monster, target);
                skillNode.nodeName = "C_SkillNode_" + skillData.skillName;
                commonSkillSelector.AddChild(skillNode);
            }
        }
        #endregion

        #region 추격 노드
        ChaseActionNode chase = new ChaseActionNode(
            monster.Rb2D, monster.transform, target.transform,
            monster.MonsterData.chaseSpeed, monster.MonsterData.approachRange, monster.Animator);
        chase.nodeName = "ChaseNode";
        root.AddChild(chase);
        #endregion

        Debug.Log("[DreamlikeWitch] (AI) RootNode initialized");
    }
}
