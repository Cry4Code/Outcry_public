using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]  //임시, 지워야됨.
public class SkillSequenceNodeDataList: DataListBase<SkillSequenceNode>
{
    /// <summary>
    /// Json 데이터로 불러오지 않기 때문에, 스킬 노드를 생성할때마다 이 곳에 추가해줘야함.
    /// todo. 하드코딩 피하고 자동 업데이트 할 수 있는 방법을 찾아볼 것. 팩토리 패턴? 같은거?
    /// </summary>
    public override void Initialize()
    {
        dataList = new List<SkillSequenceNode>();
        
        //스킬 시퀀스 노드 생성
        dataList.Add(new MetalBladeSkillSequenceNode(103001));
        dataList.Add(new HeavyDestroyerSkillSequenceNode(103002));
        dataList.Add(new ThreePointSkillSequenceNode(103003));
        dataList.Add(new EarthquakeSkillSequenceNode(103004));
        dataList.Add(new StompSkillSequenceNode(103005));
        dataList.Add(new UpperSlashSkillSequenceNode(103006));
        dataList.Add(new NormalAttackSkillSequenceNode(103000));
        dataList.Add(new WhirlWindSkillSequenceNode(103008));
        dataList.Add(new SharkSkillSequenceNode(103007));
        
        dataList.Add(new RumbleOfRuinSkillSequenceNode(103009));
        dataList.Add(new FinalHorizonSkillSequenceNode(103010));

        #region 일반 몬스터

        // Goblin Rogue
        dataList.Add(new GoblinCommonAttackSkillSequenceNode(103101));
        dataList.Add(new GoblinRogueStrongAttackSkillSequenceNode(103102));

        // Goblin Fighter
        dataList.Add(new GoblinCommonAttackSkillSequenceNode(103301));
        dataList.Add(new GoblinFighterStrongAttackSkillSequenceNode(103302));

        // Goblin Firekeeper
        dataList.Add(new GoblinCommonAttackSkillSequenceNode(103201));
        dataList.Add(new GoblinFirekeeperStrongAttackSkillSequenceNode(103202));

        #endregion

    }

    /// <summary>
    /// 데이터리스트에 id에 해당하는 스킬시퀀스노드가 있다면 true, 없다면 false 반환
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="skillSequenceNode"></param>
    /// <returns></returns>
    public bool TryGetSkillSequenceNode(int skillId, out SkillSequenceNode skillSequenceNode)
    {
        var tempData = dataList.FirstOrDefault(node => node.SkillId == skillId);

        switch (tempData)
        {
            case MetalBladeSkillSequenceNode:
                skillSequenceNode = new MetalBladeSkillSequenceNode(skillId);
                break;
            case EarthquakeSkillSequenceNode:
                skillSequenceNode = new EarthquakeSkillSequenceNode(skillId);
                break;
            case StompSkillSequenceNode:
                skillSequenceNode = new StompSkillSequenceNode(skillId);
                break;
            case UpperSlashSkillSequenceNode:
                skillSequenceNode = new UpperSlashSkillSequenceNode(skillId);
                break;
            case HeavyDestroyerSkillSequenceNode:
                skillSequenceNode = new HeavyDestroyerSkillSequenceNode(skillId);
                break;
            case ThreePointSkillSequenceNode:
                skillSequenceNode = new ThreePointSkillSequenceNode(skillId);
                break;
            case NormalAttackSkillSequenceNode:
                skillSequenceNode = new NormalAttackSkillSequenceNode(skillId);
                break;
            case WhirlWindSkillSequenceNode:
                skillSequenceNode = new WhirlWindSkillSequenceNode(skillId);
                break;
            case SharkSkillSequenceNode:
                skillSequenceNode = new SharkSkillSequenceNode(skillId);
                break;
            case RumbleOfRuinSkillSequenceNode:
                skillSequenceNode = new RumbleOfRuinSkillSequenceNode(skillId);
                break;
            case FinalHorizonSkillSequenceNode:
                skillSequenceNode = new FinalHorizonSkillSequenceNode(skillId);
                break;
            case GoblinCommonAttackSkillSequenceNode:
                skillSequenceNode = new GoblinCommonAttackSkillSequenceNode(skillId);
                break;
            case GoblinRogueStrongAttackSkillSequenceNode:
                skillSequenceNode = new GoblinRogueStrongAttackSkillSequenceNode(skillId);
                break;
            case GoblinFighterStrongAttackSkillSequenceNode:
                skillSequenceNode = new GoblinFighterStrongAttackSkillSequenceNode(skillId);
                break;
            case GoblinFirekeeperStrongAttackSkillSequenceNode:
                skillSequenceNode = new GoblinFirekeeperStrongAttackSkillSequenceNode(skillId);
                break;
            default:
                skillSequenceNode = null;
                break;
        }
        
        if (tempData == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
