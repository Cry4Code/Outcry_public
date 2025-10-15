using System;
using UnityEngine;

public class VampireLordAI : MonsterAIBase
{
    protected override void InitializeBehaviorTree()
    {
        SelectorNode rootNode = new SelectorNode(); 
                
        //AttackSequence
        SequenceNode attackSequenceNode = new SequenceNode();
        CanAttackConditionNode canAttackConditionNode = new CanAttackConditionNode(this);
        SelectorNode attackSelectorNode = new SelectorNode();
        attackSequenceNode.AddChild(canAttackConditionNode);
        attackSequenceNode.AddChild(attackSelectorNode);
        rootNode.AddChild(attackSequenceNode);

        // 스킬은 보스몬스터로 형변환 후에 접근.
        BossMonsterModel monsterModel = (BossMonsterModel)monster.MonsterData;
        if (monsterModel == null)
        {
            Debug.Log("monsterModel 이게 null");
        }
        
        // 스페셜 스킬 시퀀스 노드
        SequenceNode specialSkillSequence = new SequenceNode();
        SkillSelectorNode specialSkillSelectorNode = new SkillSelectorNode();
        WaitActionNode specialWaitActionNode = new WaitActionNode(Figures.Monster.SPECIAL_SKILL_INTERVAL);
        
        specialSkillSequence.AddChild(specialSkillSelectorNode);
        specialSkillSequence.AddChild(specialWaitActionNode);
        
        attackSelectorNode.AddChild(specialSkillSequence);
        
        // 스페셜 스킬 셀럭터 노드 자식들 생성.
        foreach (int id in monsterModel.specialSkillIds )
        {
            DataManager.Instance.SkillSequenceNodeDataList.TryGetSkillSequenceNode(id, out SkillSequenceNode skillNode);
            DataManager.Instance.MonsterSkillDataList.TryGetMonsterSkillModelData(id, out MonsterSkillModel skillData);
            if (skillNode != null)
            {
                skillNode.InitializeSkillSequenceNode(monster, target);
                skillNode.nodeName = "S_SkillNode_" + skillData.skillName; //디버깅용 노드 이름 설정.
                specialSkillSelectorNode.AddChild(skillNode);                
            }
        }        
        
        // 일반 스킬 시퀀스 노드
        SequenceNode commonSkillSequence = new SequenceNode();
        SkillSelectorNode commonSkillSelectorNode = new SkillSelectorNode();
        WaitActionNode commonWaitActionNode = new WaitActionNode(Figures.Monster.COMMON_SKILL_INTERVAL);
        
        commonSkillSequence.AddChild(commonSkillSelectorNode);
        commonSkillSequence.AddChild(commonWaitActionNode);
        
        attackSelectorNode.AddChild(commonSkillSequence);
        
        //일반 스킬 셀럭터 노드 자식들 생성.
        foreach (int id in monsterModel.commonSkillIds)
        {
            DataManager.Instance.SkillSequenceNodeDataList.TryGetSkillSequenceNode(id, out SkillSequenceNode skillNode);
            DataManager.Instance.MonsterSkillDataList.TryGetMonsterSkillModelData(id, out MonsterSkillModel skillData);
            if (skillNode != null)
            {
                skillNode.InitializeSkillSequenceNode(monster, target);
                skillNode.nodeName = "C_SkillNode_" + skillData.skillName; //디버깅용 노드 이름 설정.
                commonSkillSelectorNode.AddChild(skillNode);
            }
        }
        
        //ChaseSelector
        ChaseActionNode chaseActionNode = new ChaseActionNode(
            monster.Rb2D, monster.transform, target.transform, monster.MonsterData.chaseSpeed, monster.MonsterData.approachRange,
            monster.Animator);
        rootNode.AddChild(chaseActionNode);

        #region NamingForDebug

        rootNode.nodeName = "RootNode";
        attackSequenceNode.nodeName = "AttackSequenceNode";
        canAttackConditionNode.nodeName = "CanAttackConditionNode";
        attackSelectorNode.nodeName = "AttackSelectorNode";
        chaseActionNode.nodeName = "ChaseActionNode";
        specialSkillSelectorNode.nodeName = "SpecialSkillSelectorNode";
        commonSkillSequence.nodeName = "CommonSkillSequenceNode";
        specialSkillSelectorNode.nodeName = "SpecialSkillSelectorNode";
        commonWaitActionNode.nodeName = "CommonWaitActionNode";
        specialWaitActionNode.nodeName = "SpecialWaitActionNode";
        specialSkillSequence.nodeName = "SpecialSkillSequenceNode";
        
        #endregion
        
        this.rootNode = rootNode;
        Debug.Log("rootNode initialized");
    }

}
