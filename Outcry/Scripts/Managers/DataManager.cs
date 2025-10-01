using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 데이터베이스들 보관 및 반환 담당
/// </summary>\
[Serializable]
public class DataManager : Singleton<DataManager>
{
    [SerializeField] private MonsterDataList monsterDataList;
    [SerializeField] private SkillSequenceNodeDataList skillSequenceNodeDataList;
    [SerializeField] private MonsterSkillDataList monsterSkillDataList;
    [SerializeField] private PlayerDataModel playerDataModel;
    [SerializeField] private PlayerSkillDataList playerSkillDataList;
    public MonsterDataList MonsterDataList => monsterDataList;
    public SkillSequenceNodeDataList SkillSequenceNodeDataList => skillSequenceNodeDataList;
    public MonsterSkillDataList MonsterSkillDataList => monsterSkillDataList;
    public PlayerDataModel PlayerDataModel => playerDataModel;
    
    public PlayerSkillDataList  PlayerSkillDataList => playerSkillDataList;
    
    
    public Dictionary<int, SkillBase> AllSkills = new Dictionary<int, SkillBase>();

    
    protected override void Awake() {
        base.Awake();

        Initialize();
        LoadSkills();
    }

    public void Initialize()
    {
        //Monster 리스트 초기화
        monsterDataList = new MonsterDataList();
        monsterDataList.InitializeWithDataList(TableDataHandler.LoadMonsterData());
        
        //MonsterSkill 리스트 초기화
        monsterSkillDataList = new MonsterSkillDataList();
        monsterSkillDataList.InitializeWithDataList(TableDataHandler.LoadMonsterSkillData());
        // SetMonsterSkillDataList();
        
        //SkillNode 리스트 초기화
        skillSequenceNodeDataList = new SkillSequenceNodeDataList();    
        skillSequenceNodeDataList.Initialize();
        
        // Player 데이터 초기화
        playerDataModel = TableDataHandler.LoadPlayerData();

        playerSkillDataList = new PlayerSkillDataList();
        playerSkillDataList.InitializeWithDataList(TableDataHandler.LoadPlayerSkillData());
    }
    
    private void LoadSkills()
    {
        List<PlayerSkillModel> skillModels = PlayerSkillDataList.DataList;
        foreach (var skillModel in skillModels)
        {
            AllSkills[skillModel.skillId] = CreateSkill(skillModel.skillId, skillModel.skillName);
        }
        Debug.Log($"[플레이어] 스킬 세트 준비 완료");
    }

    private SkillBase CreateSkill(int skillId, string skillName)
    {
        if (!AllSkills.TryGetValue(skillId, out SkillBase skill))
        {
            Type skillType = Assembly.GetExecutingAssembly().GetType(skillName);
            if (skillType == null)
            {
                Debug.LogError($"[플레이어] 스킬 클래스 {skillName} 을 찾을 수 없삼");
                return null;
            }

            if (!PlayerSkillDataList.TryGetPlayerSkillModelData(skillId, out var skillModel))
            {
                Debug.LogError($"[플레이어] 스킬 아이디 {skillId} 는 없는 아이디임");
                return null;
            }
            
            SkillBase newSkill = Activator.CreateInstance(skillType) as SkillBase;
            if (newSkill == null) return null;
            
            newSkill.Init(
                skillModel.skillId, skillModel.damages,  skillModel.buffValue,
                skillModel.stamina, skillModel.cooldown, skillModel.duration
            );

            Debug.Log($"[플레이어] 스킬 클래스 {skillName} 을 생성함");
            return newSkill;
        }
        else return skill;
    }
}
