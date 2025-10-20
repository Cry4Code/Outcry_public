using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public enum RangeMode { Inside, Outside }
public enum SpawnMode { Front, AtTarget }

public struct SpawnRequest
{
    public string prefabPath;
    public int spawnFrame;
    public Vector3 offset;

    public SpawnRequest(string prefabPath, int spawnFrame, Vector3 offset = default)
    {
        this.prefabPath = prefabPath;
        this.spawnFrame = spawnFrame;
        this.offset = offset;
    }
}

/// <summary>
/// 보스용 범용 투사체 스킬 노드 (일반 몬스터는 부모 노드가 다름)
/// </summary>
public class ProjectileSpawnSkillSequenceNode : SkillSequenceNode
{
    private int animatorTriggerHash;
    private int animatorNameHash;    
    private float stateEnterTime;
    private int count;
    private int nextSpawnIndex;
    private readonly RangeMode rangeMode;
    private readonly SpawnMode spawnMode;

    private float[] spawnFrames;
    private Vector3[] offsets;
    private string[] projectilePaths;

    private const float ANIMATION_FRAME_RATE = 20f;

    public ProjectileSpawnSkillSequenceNode(int skillId, int animatorTriggerHash, int animatorNameHash,
        RangeMode rangeMode, SpawnMode spawnMode, params SpawnRequest[] projectileSpawnRequest) : base(skillId)
    {
        this.nodeName = "ProjectileSpawnSkillSequenceNode" + skillId;
        this.animatorTriggerHash = animatorTriggerHash;
        this.animatorNameHash = animatorNameHash;
        
        // 모드 설정
        this.rangeMode = rangeMode;
        this.spawnMode = spawnMode;
        
        // 투사체 소환 수만큼 배열 크기 설정       
        count = projectileSpawnRequest.Length;
        spawnFrames = new float[count];
        offsets = new Vector3[count];
        projectilePaths = new string[count];

        for(int i = 0; i < count; i++)
        {
            spawnFrames[i] = (1.0f / ANIMATION_FRAME_RATE) * projectileSpawnRequest[i].spawnFrame;
            offsets[i] = projectileSpawnRequest[i].offset;
            projectilePaths[i] = projectileSpawnRequest[i].prefabPath;
        }

        // 생성 프레임에 따라 정렬
        var idx = new List<int>(count);
        for (int i = 0; i < count; i++) idx.Add(i);
        idx.Sort((x, y) => spawnFrames[x].CompareTo(spawnFrames[y]));

        // 정렬 적용
        var frames = new float[count];
        var offs = new Vector3[count];
        var paths = new string[count];
        for (int i = 0; i < count; i++)
        {
            int j = idx[i];
            frames[i] = spawnFrames[j];
            offs[i] = offsets[j];
            paths[i] = projectilePaths[j];
        }
        spawnFrames = frames;
        offsets = offs;
        projectilePaths = paths;
    }

    public override async void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);

        // 투사체 로드
        var tasks = new List<UniTask>(count);   // 리스트로 저장
        for (int i = 0; i < count; i++)
        {
            var path = projectilePaths[i];
            if (!string.IsNullOrEmpty(path))
                tasks.Add(ObjectPoolManager.Instance.RegisterPoolAsync(path));
        }

        if (tasks.Count > 0)    // 한번에 로드
            await UniTask.WhenAll(tasks);
    }

    protected override bool CanPerform()
    {
        bool result;
        bool isInRange;
        bool isCooldownComplete;

        Vector2 distance = monster.transform.position - target.transform.position;
        float rangeSqr = skillData.range * skillData.range;
        float distanceSqr = Vector2.SqrMagnitude(distance);

        switch(rangeMode)
        {
            case RangeMode.Outside: // 플레이어가 특정 거리 외에 있을 때
                if (distanceSqr >= rangeSqr)
                {
                    isInRange = true;
                }
                else
                {
                    isInRange = false;
                }
                break;
            default:    // RangeMode.Inside, 플레이어가 특정 거리 이내에 있을 때
                if (distanceSqr <= rangeSqr)
                {
                    isInRange = true;
                }
                else
                {
                    isInRange = false;
                }
                break;
        }

        //쿨다운 확인
        if (Time.time - lastUsedTime >= skillData.cooldown)
        {
            isCooldownComplete = true;
        }
        else
        {
            isCooldownComplete = false;
        }

        result = isInRange && isCooldownComplete;
        Debug.Log($"Skill {skillData.skillName} used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return result;
    }

    protected override NodeState SkillAction()
    {
        NodeState state;
                
        // 스킬 트리거 켜기
        if (!skillTriggered)
        {
            lastUsedTime = Time.time;
            FlipCharacter();
            monster.Animator.SetTrigger(animatorTriggerHash);
            monster.AttackController.SetDamages(skillData.damage1);

            skillTriggered = true;            
            stateEnterTime = Time.time; // 상태 시작 시간 저장
            nextSpawnIndex = 0;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, animatorNameHash);
        if (isSkillAnimationPlaying)
        {
            Debug.Log($"[{monster.name}] Running skill: {skillData.skillName} (ID: {skillData.skillId})");
            state = NodeState.Running;
        }
        else
        {
            Debug.Log($"[{monster.name}] Skill End: {skillData.skillName} (ID: {skillData.skillId})");

            monster.AttackController.SetDamages(0); //데미지 초기화
            skillTriggered = false;
            return NodeState.Success;
        }

        // 애니메이션 경과 시간 계산
        float elapsedTime = Time.time - stateEnterTime;
        // 몬스터가 바라보는 방향
        bool faceRight = monster.transform.localScale.x >= 0f;

        // count 만큼 투사체 생성
        while (nextSpawnIndex < count && elapsedTime >= spawnFrames[nextSpawnIndex])
        {
            int i = nextSpawnIndex;
            switch (spawnMode) 
            {
                case SpawnMode.AtTarget:    // 타겟 위치에 소환 (월드 좌표 바로 넘김)
                    Vector3 worldPos = target.transform.position + offsets[i];
                    Debug.Log($"{skillData.skillName} : spawn {projectilePaths[i]} at world position {worldPos}");
                    monster.AttackController.InstantiateProjectileAtWorld(projectilePaths[i], worldPos, faceRight, skillData.damage1, false);
                    break;
                default: // SpawnMode.Front, 몬스터 기준의 로컬 좌표로 소환
                    Debug.Log($"{skillData.skillName} : spawn {projectilePaths[i]} at {offsets[i]}");
                    monster.AttackController.InstantiateProjectile(projectilePaths[i], offsets[i], faceRight, skillData.damage1, false);
                    break;
            }
            nextSpawnIndex++;
        }

        return state;
    }
}
