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

        #region 고블린 킹
        // common skills
        dataList.Add(new MetalBladeSkillSequenceNode(103001));
        dataList.Add(new HeavyDestroyerSkillSequenceNode(103002));
        dataList.Add(new ThreePointSkillSequenceNode(103003));
        dataList.Add(new EarthquakeSkillSequenceNode(103004));
        dataList.Add(new StompSkillSequenceNode(103005));
        dataList.Add(new UpperSlashSkillSequenceNode(103006));
        dataList.Add(new NormalAttackSkillSequenceNode(103000));
        dataList.Add(new WhirlWindSkillSequenceNode(103008));
        dataList.Add(new SharkSkillSequenceNode(103007));
        
        // special skills
        dataList.Add(new RumbleOfRuinSkillSequenceNode(103009));
        dataList.Add(new FinalHorizonSkillSequenceNode(103010));
        #endregion

        #region 몽환의 마녀
        // common skills
        dataList.Add(new MeleeAttackSkillSequenceNode(103401,
            AnimatorHash.MonsterParameter.NormalAttack, AnimatorHash.MonsterAnimation.NormalAttack));
        dataList.Add(new MeleeAttackSkillSequenceNode(103403,
            AnimatorHash.MonsterParameter.FireBash, AnimatorHash.MonsterAnimation.FireBash));
        dataList.Add(new MeleeAttackSkillSequenceNode(103404,
            AnimatorHash.MonsterParameter.ThunderBall, AnimatorHash.MonsterAnimation.ThunderBall));
        dataList.Add(new MeleeAttackSkillSequenceNode(103406,
            AnimatorHash.MonsterParameter.IceMountain, AnimatorHash.MonsterAnimation.IceMountain));
        dataList.Add(new MeleeAttackSkillSequenceNode(103407,
            AnimatorHash.MonsterParameter.ThunderBolt, AnimatorHash.MonsterAnimation.ThunderBolt));
        // // FireBall(103405) // 보스가 플레이어 위치까지 투사체를 발사하는 스킬
        //dataList.Add(new ProjectileSpawnSkillSequenceNode(103405,
        //    AnimatorHash.MonsterParameter.FireBall, AnimatorHash.MonsterAnimation.FireBall, RangeMode.Outside, SpawnMode.Front,
        //    new SpawnRequest(AddressablePaths.Projectile.FireBall, 8, offset)))  // 파이어 볼 이름 수정 요청 (일반 고블린이랑 겹침)
        // Thunder Strike(103402) // 체널링 // 보스가 플레이어 위치에 투사체를 생성하는 스킬
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103409,
            AnimatorHash.MonsterParameter.BlackHole, AnimatorHash.MonsterAnimation.BlackHole, RangeMode.Outside, SpawnMode.AtTarget,
            new SpawnRequest(AddressablePaths.Projectile.BlackHole, 9)));
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103409,
            AnimatorHash.MonsterParameter.Infierno, AnimatorHash.MonsterAnimation.Infierno, RangeMode.Outside, SpawnMode.AtTarget,
            new SpawnRequest(AddressablePaths.Projectile.Infierno, 12)));
        // VolcanoDive (103410)	// 이동하면서 나갈 스킬

        // special skills
        // Meteor(103411)
        #endregion

        //VampireLord
        dataList.Add(new MeleeAttackSkillSequenceNode(103501, 
            AnimatorHash.MonsterParameter.NormalAttack, AnimatorHash.MonsterAnimation.NormalAttack)); //일반공격
        dataList.Add(new TurningBloodSkillSequenceNode(103505)); //TurningBlood
        dataList.Add(new BloodStingSkillSequenceNode(103506)); //BloodSting
        dataList.Add(new MeleeAttackSkillSequenceNode(103507, 
            AnimatorHash.MonsterParameter.BloodShard, AnimatorHash.MonsterAnimation.BloodShard)); //BloodShard
        dataList.Add(new MeleeAttackSkillSequenceNode(103508, 
            AnimatorHash.MonsterParameter.BloodVeil, AnimatorHash.MonsterAnimation.BloodVeil)); //BloodVeil
        dataList.Add(new MeleeAttackSkillSequenceNode(103509, 
            AnimatorHash.MonsterParameter.BloodSpear, AnimatorHash.MonsterAnimation.BloodSpear)); //BloodSpear
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
            case MeleeAttackSkillSequenceNode node:
                skillSequenceNode = new MeleeAttackSkillSequenceNode(node);
                break;
            case BloodStingSkillSequenceNode:
                skillSequenceNode = new BloodStingSkillSequenceNode(skillId);
                break;
            case TurningBloodSkillSequenceNode:
                skillSequenceNode = new TurningBloodSkillSequenceNode(skillId);
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
