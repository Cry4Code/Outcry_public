using System;
using UnityEngine;

public class VampireLordFlyingAI : MonsterAIBase
{
    private bool isPhase3 = false;
    private TimedWayPoint[] wayPoints;
    private ZoneMarker flyZoneMarker;
    protected override void InitializeBehaviorTree()
    {
        SelectorNode rootNode = new SelectorNode();
        
        //웨이포인트 로직 먼저
        var stageController = StageManager.Instance.currentStageController as HallOfBloodStageController;
        if (stageController is HallOfBloodStageController)
        {
            SetWayPoints(stageController.WayPoints);
            flyZoneMarker = stageController.Phase3BossZone;
            Debug.Log("[몬스터BT] WayPoints set from StageController");
        }
        else
        {
            wayPoints = new TimedWayPoint[3];
            wayPoints[0] = new TimedWayPoint { position = new Vector2(-14f, 7.5f), time = 13f };
            wayPoints[1] = new TimedWayPoint { position = new Vector2(3.3f, 16.3f), time = 25f };
            wayPoints[2] = new TimedWayPoint { position = new Vector2(-4.6f, 22.0f), time = 30f };
            Debug.LogError("[몬스터BT] flyZoneMarker cannot be found from StageController");
        }
        

        WayPointFlySequenceNode wayPointFlySequenceNode =
            new WayPointFlySequenceNode(monster.transform, monster.Rb2D, wayPoints, monster.MonsterData.chaseSpeed, monster.MonsterData.approachRange);
        wayPointFlySequenceNode.nodeName = "WayPointFlySequenceNode";
        rootNode.AddChild(wayPointFlySequenceNode);

        //phase 3일때만
        SequenceNode phase3FlySequenceNode = new SequenceNode();
        ConditionNode isPhase3ConditionNode = new ConditionNode(() => isPhase3);
        FlyRandomInZoneSequenceNode flyRandomInZoneSequenceNode =
            new FlyRandomInZoneSequenceNode(
                monster.transform, monster.Rb2D, flyZoneMarker, monster.MonsterData.chaseSpeed,
                monster.MonsterData.approachRange, 5f, 10f);
        rootNode.AddChild(phase3FlySequenceNode);
        phase3FlySequenceNode.AddChild(isPhase3ConditionNode);
        phase3FlySequenceNode.AddChild(flyRandomInZoneSequenceNode);
        

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
        foreach (int id in monsterModel.specialSkillIds)
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

        // //ChaseSelector
        // FlyToTargetActionNode flyToTargetActionNode = new FlyToTargetActionNode(
        //     monster.Rb2D, monster.transform, target.transform, monster.MonsterData.chaseSpeed,
        //     monster.MonsterData.approachRange);




        #region NamingForDebug

        rootNode.nodeName = "RootNode";
        attackSequenceNode.nodeName = "AttackSequenceNode";
        canAttackConditionNode.nodeName = "CanAttackConditionNode";
        attackSelectorNode.nodeName = "AttackSelectorNode";
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
    public void TransitionToPhase3()
    {
        isPhase3 = true;
        Debug.Log("[몬스터BT] VampireLordFlyingAI transitioned to Phase 3");
    }
    public void SetWayPoints(WayPoint[] wayPoints)
    {
        this.wayPoints = new TimedWayPoint[wayPoints.Length];
        for (int i = 0; i < wayPoints.Length; i++)
        {
            this.wayPoints[i] = new TimedWayPoint
            {
                position = wayPoints[i].transform.position,
                time = wayPoints[i].duration
            };
        }
    }
}
