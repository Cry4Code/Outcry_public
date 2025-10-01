using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(BossMonster))] //todo. think. 쓸까 말까?
public class GoblinKingAI : MonsterAIBase
{
    // 고블린 킹만의 고유 이벤트 선언
    public event Action OnFallingRocksPattern;

    /// <summary>
    /// 스킬 노드와 같은 외부에서 이벤트를 안전하게 호출하기 위한 메서드
    /// </summary>
    public void TriggerFallingRocksPattern() 
    {
        Debug.Log("[GoblinKingAI] 낙석 이벤트를 발행합니다!");
        OnFallingRocksPattern?.Invoke();
    }

    protected override void InitializeBehaviorTree()
    {
        SelectorNode rootNode = new SelectorNode(); 

        // //isDead
        // ConditionNode isDeadNode = new ConditionNode(() => false); //임시
        // rootNode.AddChild(isDeadNode);
                
        //AttackSequence
        SequenceNode attackSequenceNode = new SequenceNode();
        CanAttackConditionNode canAttackConditionNode = new CanAttackConditionNode(this);
        SelectorNode attackSelectorNode = new SelectorNode();
        attackSequenceNode.AddChild(canAttackConditionNode);
        attackSequenceNode.AddChild(attackSelectorNode);
        rootNode.AddChild(attackSequenceNode);
        
        /* 스킬 구버전
        //각 스킬 노드 테스트용
        //TestSkillSequenceNode 대신에 본인이 제작한 SkillSequenceNode 상속 노드로 교체해서 테스트하세요.
        // MetalBladeSkillSequenceNode metalBladeSkillSequenceNode = new MetalBladeSkillSequenceNode(103001);
        // metalBladeSkillSequenceNode.InitializeSkillSequenceNode(monster, target);
        //
        // StompSkillSequenceNode stompSkillSequenceNode = new StompSkillSequenceNode(103005);
        // stompSkillSequenceNode.InitializeSkillSequenceNode(monster, target);
        //
        // UpperSlashSequenceNode upperSlashSequenceNode = new UpperSlashSequenceNode(103006);
        // upperSlashSequenceNode.InitializeSkillSequenceNode(monster, target);
        //
        // EarthquakeSkillSequenceNode earthquakeSkillSequenceNode = new EarthquakeSkillSequenceNode(103004);
        // earthquakeSkillSequenceNode.InitializeSkillSequenceNode(monster, target);

        //HeavyDestroyerSkillSequenceNode heavyDestroyerSkillSequenceNode = new HeavyDestroyerSkillSequenceNode(103002);
        //heavyDestroyerSkillSequenceNode.InitializeSkillSequenceNode(monster, target);


        // attackSelectorNode.AddChild(metalBladeSkillSequenceNode);
        // attackSelectorNode.AddChild(stompSkillSequenceNode);
        // attackSelectorNode.AddChild(upperSlashSequenceNode);
        // attackSelectorNode.AddChild(earthquakeSkillSequenceNode);
        //attackSelectorNode.AddChild(heavyDestroyerSkillSequenceNode);
        */

        // 스킬은 보스몬스터로 형변환 후에 접근.
        BossMonsterModel monsterModel = (BossMonsterModel)monster.MonsterData;
        if (monsterModel == null)
        {
            Debug.Log("monsterModel 이게 null이라서 짜증나겠지만 어쨋든 null인걸 어쩌라고.. 짜증나......");
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
