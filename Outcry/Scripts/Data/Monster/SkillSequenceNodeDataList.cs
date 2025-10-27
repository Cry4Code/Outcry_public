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
        /*  투사체 공용 노드 테스트용, 이상없이 작동함.
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103004,
            AnimatorHash.MonsterParameter.Earthquake, AnimatorHash.MonsterAnimation.Earthquake, RangeMode.Inside, SpawnMode.Front,
            new SpawnRequest(AddressablePaths.Projectile.Stone, 19, new Vector3(5.7f, -1.47f, 0f)),
            new SpawnRequest(AddressablePaths.Projectile.Stone, 25, new Vector3(10.2f, -1.47f, 0f)),
            new SpawnRequest(AddressablePaths.Projectile.Stone, 31, new Vector3(14.7f, -1.47f, 0f))));
        */
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
            AnimatorHash.MonsterParameter.Takkong, AnimatorHash.MonsterAnimation.Takkong));
        dataList.Add(new MeleeAttackSkillSequenceNode(103403,
            AnimatorHash.MonsterParameter.FireBash, AnimatorHash.MonsterAnimation.FireBash));
        dataList.Add(new MeleeAttackSkillSequenceNode(103404,
            AnimatorHash.MonsterParameter.ThunderBall, AnimatorHash.MonsterAnimation.ThunderBall));
        dataList.Add(new MeleeAttackSkillSequenceNode(103407,
            AnimatorHash.MonsterParameter.ThunderBolt, AnimatorHash.MonsterAnimation.ThunderBolt));
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103406,
            AnimatorHash.MonsterParameter.IceMountain, AnimatorHash.MonsterAnimation.IceMountain, RangeMode.Inside, SpawnMode.Front, 
            new SpawnRequest(AddressablePaths.Projectile.IceMountain, 10, Vector3.zero)));
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103405, 
            AnimatorHash.MonsterParameter.FireBolt, AnimatorHash.MonsterAnimation.FireBolt, RangeMode.Inside, SpawnMode.Front,
            new SpawnRequest(AddressablePaths.Projectile.FireBolt, 9, new Vector3(2.0f, 0.2f, 0f))));
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103408,
            AnimatorHash.MonsterParameter.BlackHole, AnimatorHash.MonsterAnimation.BlackHole, RangeMode.Inside, SpawnMode.AtTarget,
            new SpawnRequest(AddressablePaths.Projectile.BlackHole, 5)));
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103409,
            AnimatorHash.MonsterParameter.Infierno, AnimatorHash.MonsterAnimation.Infierno, RangeMode.Inside, SpawnMode.AtTarget,
            new SpawnRequest(AddressablePaths.Projectile.Infierno, 6)));
        dataList.Add(new VolcanoDiveSkillSequenceNode(103410));
        dataList.Add(new ThunderStrikeSkillSequenceNode(103402));

        // special skills
        dataList.Add(new MeteorSkillSequenceNode(103411));        
        #endregion

        //VampireLord 1페
        dataList.Add(new MeleeAttackSkillSequenceNode(103501, 
            AnimatorHash.MonsterParameter.NormalAttack, AnimatorHash.MonsterAnimation.NormalAttack)); //일반공격
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103503,
            AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Inside, SpawnMode.BothEndOfGround,
            new SpawnRequest(AddressablePaths.Projectile.Darkness, 30, new Vector3(0f, 0f, 0f)))); //darkness
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103505,
            AnimatorHash.MonsterParameter.TurningBlood, AnimatorHash.MonsterAnimation.TurningBlood, RangeMode.Inside, SpawnMode.Front,
            new SpawnRequest(AddressablePaths.Projectile.TurningBlood, 14, new Vector3(0.5f, 0f, 0f)))); //turningBlood
        // dataList.Add(new TurningBloodSkillSequenceNode(103505)); //TurningBlood
        dataList.Add(new BloodStingSkillSequenceNode(103506)); //BloodSting
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103507,
            AnimatorHash.MonsterParameter.BloodShard, AnimatorHash.MonsterAnimation.BloodShard, RangeMode.Inside, SpawnMode.Front,
            new SpawnRequest(AddressablePaths.Projectile.BloodShard, 25, new Vector3(0.3f, 0f, 0f)))); //BloodShard
        // dataList.Add(new MeleeAttackSkillSequenceNode(103507, 
        //     AnimatorHash.MonsterParameter.BloodShard, AnimatorHash.MonsterAnimation.BloodShard)); //BloodShard
        dataList.Add(new MeleeAttackSkillSequenceNode(103508, 
            AnimatorHash.MonsterParameter.BloodVeil, AnimatorHash.MonsterAnimation.BloodVeil)); //BloodVeil
        dataList.Add(new MeleeAttackSkillSequenceNode(103509, 
            AnimatorHash.MonsterParameter.BloodSpear, AnimatorHash.MonsterAnimation.BloodSpear)); //BloodSpear
        dataList.Add(new BatStormSkillSequenceNode(103510)); //BatStorm
        
        //vampireLord 2페
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103502,
            AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Inside, SpawnMode.TargetGround,
            new SpawnRequest(AddressablePaths.Projectile.DarkSwamp, 30, new Vector3(0f, 0.6f, 0f)))); //darkSwamp
        // dataList.Add(new ProjectileSpawnSkillSequenceNode(103503,
        //     AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Inside, SpawnMode.Front,
        //     new SpawnRequest(AddressablePaths.Projectile.Darkness, 30, new Vector3(0f, 0f, 0f)))); //darkness
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103514,
            AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Inside, SpawnMode.BothEndOfGround,
            new SpawnRequest(AddressablePaths.Projectile.Darkness, 30, new Vector3(0f, 0f, 0f)))); //darkness
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103504,
            AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Outside, SpawnMode.RandomGround,
            new SpawnRequest(AddressablePaths.Projectile.DarkBomb, 30, new Vector3(0f, 0.6f, 0f)),
            new SpawnRequest(AddressablePaths.Projectile.DarkBomb, 30, new Vector3(0f, 0.6f, 0f)))); //darkBomb
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103513,
            AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Inside, SpawnMode.TargetGround,
            new SpawnRequest(AddressablePaths.Projectile.BloodSpear, 30, new Vector3(0f, 1.6f, 0f)))); //bloodSpearGround
        // dataList.Add(new TurningBloodSkillSequenceNode(103512)); //TurningBloods
        dataList.Add(new ProjectileSpawnSkillSequenceNode(103512,
            AnimatorHash.MonsterParameter.ChargeSkill, AnimatorHash.MonsterAnimation.ChargeSkill, RangeMode.Outside, SpawnMode.Front,
            new SpawnRequest(AddressablePaths.Projectile.TurningBlood, 30, new Vector3(0.5f, 0f, 0f)))); //turningBlood
        dataList.Add(new BloodMoonSkillSequenceNode(103511)); //BloodMoon
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
            case MeleeAttackSkillSequenceNode node:
                skillSequenceNode = new MeleeAttackSkillSequenceNode(node);
                break;
            case ProjectileSpawnSkillSequenceNode node:
                skillSequenceNode = new ProjectileSpawnSkillSequenceNode(node);
                break;
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
            case VolcanoDiveSkillSequenceNode:
                skillSequenceNode = new VolcanoDiveSkillSequenceNode(skillId);
                break;
            case ThunderStrikeSkillSequenceNode:
                skillSequenceNode = new ThunderStrikeSkillSequenceNode(skillId);
                break;
            case MeteorSkillSequenceNode:
                skillSequenceNode = new MeteorSkillSequenceNode(skillId);
                break;
            case BloodStingSkillSequenceNode:
                skillSequenceNode = new BloodStingSkillSequenceNode(skillId);
                break;
            case TurningBloodSkillSequenceNode:
                skillSequenceNode = new TurningBloodSkillSequenceNode(skillId);
                break;
            case BatStormSkillSequenceNode:
                skillSequenceNode = new BatStormSkillSequenceNode(skillId);
                break;
            case BloodMoonSkillSequenceNode:
                skillSequenceNode = new BloodMoonSkillSequenceNode(skillId);
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
