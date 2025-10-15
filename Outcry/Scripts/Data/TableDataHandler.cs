using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
/// 순수 변환만을 담당함 (기획테이블 데이터 -> 모델)
/// DataManager에서 Initialize할때 호출됨
/// 데이터를 보관하지 않으며, 가공하는 메서드만 존재.
/// 추가로 필요한 변환 메서드는 본인의 원하는 형태로 구현.
/// </summary>
public static class TableDataHandler
{
    public static List<MonsterModelBase> LoadMonsterData()
    {
        List<MonsterModelBase> monsterList = new List<MonsterModelBase>();

        // tableData: json에서 불러온 데이터
        //DataTableManager.Instance.LoadCollectionData<EnemyDataTable>(); // GameManager에서 이미 로드함
        Dictionary<int, IData> tableData = DataTableManager.Instance.CollectionData[typeof(EnemyData)] as Dictionary<int, IData>;
        
        // tableData의 각 아이템을 MonsterModelBase로 변환하여 리스트에 추가
        foreach (var item in tableData.Values)
        {
            if (item is EnemyData enemyData) //어택쿨다운 임시 1
            {
                if(enemyData.Exskill_Set.Length > 0)
                {
                    BossMonsterModel bossMonsterData = new BossMonsterModel(
                        enemyData.Enemy_Id, enemyData.Enemy_Name,
                        enemyData.Max_Hp, enemyData.Chase_Speed,
                        enemyData.Approch_Range, enemyData.Detect_Range,
                        enemyData.Exskill_Set, enemyData.Skill_Set);
                    monsterList.Add(bossMonsterData);
                }
                else
                {
                    CommonMonsterModel commonMonsterModel = new CommonMonsterModel(
                        enemyData.Enemy_Id, enemyData.Enemy_Name,
                        enemyData.Max_Hp, enemyData.Chase_Speed, enemyData.Approch_Range, enemyData.Detect_Range,
                        enemyData.Disdetect_Range, enemyData.Patrol_Speed,
                        enemyData.Skill_Set);
                    monsterList.Add(commonMonsterModel);
                }
                Debug.Log($"{enemyData.Enemy_Id} : {enemyData.Enemy_Name}");
            }
        }

        return monsterList;
    }

    public static List<MonsterSkillModel> LoadMonsterSkillData()
    {
        List<MonsterSkillModel> monsterSkillDataList = new List<MonsterSkillModel>();
        
        // tableData: json에서 불러온 데이터
        DataTableManager.Instance.LoadCollectionData<EnemySkillDataTable>();
        Dictionary<int, IData> tableData = DataTableManager.Instance.CollectionData[typeof(EnemySkillData)] as Dictionary<int, IData>;
        
        // tableData의 각 아이템을 MonsterSkillModel로 변환하여 리스트에 추가
        foreach (var item in tableData.Values)
        {
            MonsterSkillModel monsterSkillData = MapMonsterSkillDataFromTableData(item as EnemySkillData);
            monsterSkillDataList.Add(monsterSkillData);
            Debug.Log($"{monsterSkillData.skillId} : {monsterSkillData.skillName}");
        }

        return monsterSkillDataList;
    }
    private static MonsterSkillModel MapMonsterSkillDataFromTableData(EnemySkillData tableData)
    {
        MonsterSkillModel newMonsterSkillModel = new MonsterSkillModel(
            tableData.Skill_id, tableData.Skill_name, 
            tableData.Damage, tableData.Damage2, tableData.Damage3, tableData.HealAmount, 
            tableData.Cooldown, tableData.Range, tableData.TriggerHP, tableData.Desc);
        
        return newMonsterSkillModel;
    }

    public static PlayerDataModel LoadPlayerData()
    {
        DataTableManager.Instance.LoadSingleData<PlayerData>();
        PlayerData tableData =  DataTableManager.Instance.SingleData[typeof(PlayerData)] as PlayerData;
        PlayerDataModel result = new PlayerDataModel(
            tableData.MaxHealth, 
            tableData.MaxStamina, 
            tableData.RateStamina, 
            tableData.FullStamina,
            tableData.SpecialAttackStamina, 
            tableData.SpecialAttackDamage, 
            tableData.JustSpecialAttackDamage,
            tableData.DodgeStamina, 
            tableData.DodgeInvincibleTime,
            tableData.DodgeDistance,
            tableData.ParryStamina, 
            tableData.ParryInvincibleTime, 
            tableData.ParryDamage,
            tableData.InvincibleTime, 
            tableData.NormalAttackDamage,
            tableData.JumpAttackDamage, 
            tableData.DownAttackDamage, 
            tableData.Jumpforce, 
            tableData.DoubleJumpForce,
            tableData.Skill_Ids,
            tableData.MoveSpeed);
        return result;
    }

    public static List<PlayerSkillModel> LoadPlayerSkillData()
    {
        List<PlayerSkillModel> playerSkillDataList = new List<PlayerSkillModel>();
        
        DataTableManager.Instance.LoadCollectionData<SkillDataTable>();
        Dictionary<int, IData> tableData =  DataTableManager.Instance.CollectionData[typeof(SkillData)] as Dictionary<int, IData>;

        foreach (var item in tableData.Values)
        {
            PlayerSkillModel playerSkillData = PlayerSkillDataFromTableData(item as SkillData);
            playerSkillDataList.Add(playerSkillData);
            Debug.Log($"[플레이어] 스킬 데이터 | {playerSkillData.skillId} : {playerSkillData.skillName}");
        }
        
        return playerSkillDataList;
    }

    private static PlayerSkillModel PlayerSkillDataFromTableData(SkillData tableData)
    {
        return new PlayerSkillModel(tableData.Skill_id, tableData.P_Skill_Name,
            tableData.Damages, tableData.BuffValue, tableData.Stamina,
            tableData.Cooldown, tableData.Duration, tableData.Desc);
    }
    
    
    
    //todo. 몬스터스킬데이터를 기획테이블에서 가져와서 맵핑하는 메서드 추가하기.
}
