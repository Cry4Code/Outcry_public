using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public enum RangeMode { Inside, Outside }
public enum SpawnMode { Front, AtTarget, TargetGround, RandomGround, BothEndOfGround }

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
    private int count;
    private readonly RangeMode rangeMode;
    private readonly SpawnMode spawnMode;

    private float stateEnterTime;
    private int nextSpawnIndex;

    private float[] spawnFrames;
    private Vector3[] offsets;
    private string[] projectilePaths;
    
    private Vector3 lastGeneratedPos = Vector3.zero;
    
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
    public ProjectileSpawnSkillSequenceNode(ProjectileSpawnSkillSequenceNode node) : base(node.skillId)
    {
        this.nodeName = node.nodeName;
        this.animatorTriggerHash = node.animatorTriggerHash;
        this.animatorNameHash = node.animatorNameHash;
        this.count = node.count;

        this.rangeMode = node.rangeMode;
        this.spawnMode = node.spawnMode;

        this.spawnFrames = (float[])node.spawnFrames.Clone();
        this.offsets = (Vector3[])node.offsets.Clone();
        this.projectilePaths = (string[])node.projectilePaths.Clone();
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
        if (skillData.skillId == 103504)
        {
            //
        }
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

        // 애니메이션 출력 보장
        if (!isAnimationStarted)
        {
            isAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, animatorNameHash);
            return NodeState.Running;
        }

        // 시작 직후 Running 강제
        if (Time.time - lastUsedTime < 0.1f)
        {
            return NodeState.Running;
        }        

        // 애니메이션 중 Running 리턴 고정
        bool isSkillAnimationPlaying = AnimatorUtility.IsAnimationPlaying(monster.Animator, animatorNameHash);
        #region 디버그용 

        var cur = monster.Animator.GetCurrentAnimatorStateInfo(0);
        var next = monster.Animator.GetNextAnimatorStateInfo(0);
        Debug.Log($"[{skillData.skillName}] play? {isSkillAnimationPlaying} cur:{cur.shortNameHash} next:{next.shortNameHash} expected:{animatorNameHash}");
        
        #endregion
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
                case SpawnMode.TargetGround:    //타겟과 같은 X 좌표, 땅 위에 소환
                    //타겟의 아래 방향으로 레이를 쏴서 Platform 혹은 Ground 태그에 닿는 지점 찾기
                    float rayStartOffset = 1f;
                    float maxGroundCheckDistance = 10f;
                    int groundLayerMask = LayerMask.GetMask("Ground");
                    
                    Vector2 origin = (Vector2)target.transform.position + Vector2.up * rayStartOffset;
                    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxGroundCheckDistance, groundLayerMask);
                    
                    if (hit.collider != null)
                    {
                        Vector3 spawnPos = new Vector3(target.transform.position.x + offsets[i].x, hit.point.y + offsets[i].y, 0f);
                        Debug.Log($"{skillData.skillName} : spawn {projectilePaths[i]} at ground position {spawnPos}");
                        monster.AttackController.InstantiateProjectileAtWorld(projectilePaths[i], spawnPos, faceRight, skillData.damage1, false);
                    }
                    else
                    {
                        Debug.LogWarning($"{skillData.skillName} : Failed to spawn {projectilePaths[i]} - No ground found below target.");
                    }
                    
                    break;
                case SpawnMode.RandomGround: // 타겟 주변 랜덤 지점, 땅 위에 소환
                    float radius = 10f; // 랜덤 반경 > 좌우 범위
                    float minDistanceBetweenSpawns = 3f; // 이전 생성 위치와의 최소 거리
                    int maxAttempts = 20; // 무한루프 방지
                    bool isGenerated = false;
                    bool hasLastPos = lastGeneratedPos != Vector3.zero;
                    
                    //ray 쏴서 ground 걸리는 거 없는지 먼저 체크.
                    //찾으면 그게 minx, maxx.
                    float minX = target.transform.position.x - radius;
                    float maxX = target.transform.position.x + radius;
                    float rayStartHeight = 1f; // 타겟 위에서 레이 시작
                    int groundMask = LayerMask.GetMask("Ground");
                    
                    Vector2 rayOriginForXRandomLength = new Vector2(target.transform.position.x, target.transform.position.y + rayStartHeight);
                    
                    //왼쪽 체크
                    RaycastHit2D hitForXRandomLengthLeft =
                        Physics2D.Raycast(rayOriginForXRandomLength, Vector2.left, radius, groundMask);
                    if (hitForXRandomLengthLeft.collider != null)
                    {
                        minX = hitForXRandomLengthLeft.point.x + 3f; // 약간 안쪽으로
                    }
                    
                    //오른쪽 체크
                    RaycastHit2D hitForXRandomLengthRight =
                        Physics2D.Raycast(rayOriginForXRandomLength, Vector2.right, radius, groundMask);
                    if (hitForXRandomLengthRight.collider != null)
                    {
                        maxX = hitForXRandomLengthRight.point.x - 3f; // 약간 안쪽으로
                    }
                    
                    for(int attempt = 0; attempt < maxAttempts && !isGenerated; attempt++)
                    {
                        float randomX = Random.Range(minX, maxX);
                        Vector2 rayOrigin = new Vector2(randomX, target.transform.position.y + rayStartHeight);
                        float rayDistance = 10f;
                        RaycastHit2D randomHit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundMask);

                        if (randomHit.collider != null)
                        {
                            Vector3 randomSpawnPos = new Vector3(randomX, randomHit.point.y + offsets[i].y, 0f);
                            
                            if (hasLastPos && Mathf.Abs(lastGeneratedPos.x - randomSpawnPos.x) < minDistanceBetweenSpawns)
                            {
                                // 이전 생성 위치와 너무 가깝다면 재시도
                                continue;
                            }
                            
                            lastGeneratedPos = randomSpawnPos;
                            hasLastPos = true;
                            
                            Debug.Log($"{skillData.skillName} : spawn {projectilePaths[i]} at random ground position {randomSpawnPos}");
                            monster.AttackController.InstantiateProjectileAtWorld(projectilePaths[i], randomSpawnPos, faceRight, skillData.damage1, false);
                            isGenerated = true;
                        }
                    }
                    
                    if(!isGenerated)
                    {
                        Debug.LogWarning($"{skillData.skillName} : Failed to spawn {projectilePaths[i]} - No ground found at random position.");
                    }
                    break;
                case SpawnMode.BothEndOfGround:
                    // 화면 좌우 끝 지점의 땅 위에 소환
                    // 타겟 하단으로 레이 쏴서 땅 높이 찾기
                    // 땅 높이(타겟x, groundY)에서 좌우 수직으로 레이 쏴서 땅 끝 찾기
                    // 양쪽 끝에 투사체 소환
                    // 못 찾으면 최대로 레이를 쐈던 지점에 소환
                    // 타겟(즉 중앙)으로 발사체 소환
                    float sideRayStartOffset = 1f;
                    float sideMaxCheckDistance = 15f;
                    int sideGroundLayerMask = LayerMask.GetMask("Ground");
                    Vector2 sideOrigin = (Vector2)target.transform.position + Vector2.up * sideRayStartOffset;
                    RaycastHit2D sideHit = Physics2D.Raycast(sideOrigin, Vector2.down, sideMaxCheckDistance, sideGroundLayerMask);
                    float groundY;
                    if (sideHit.collider == null){ Debug.Log($"{skillData.skillName} : fail to find ground to spawn projectiles."); }
                    groundY = sideHit.point.y;
                    
                    // 왼쪽 끝
                    float leftX;
                    RaycastHit2D leftHit = Physics2D.Raycast(new Vector2(target.transform.position.x, groundY + 1f), Vector2.left, sideMaxCheckDistance, sideGroundLayerMask);
                    leftX = leftHit.collider == null ? target.transform.position.x - sideMaxCheckDistance : leftHit.point.x;
                    Vector3 leftSpawnPos = new Vector3(leftX + offsets[i].x + 2f, groundY + 1f + offsets[i].y, 0f);
                    // 오른쪽 끝
                    float rightX;
                    RaycastHit2D rightHit = Physics2D.Raycast(new Vector2(target.transform.position.x, groundY + 1f), Vector2.right, sideMaxCheckDistance, sideGroundLayerMask);
                    rightX = rightHit.collider == null ? target.transform.position.x + sideMaxCheckDistance : rightHit.point.x;
                    Vector3 rightSpawnPos = new Vector3(rightX + offsets[i].x -2f, groundY + 1f + offsets[i].y, 0f);
                    
                    Debug.Log($"{skillData.skillName} : spawn {projectilePaths[i]} at left ground end {leftSpawnPos} and right ground end {rightSpawnPos}");
                    monster.AttackController.InstantiateProjectileAtWorld(projectilePaths[i], leftSpawnPos,true, skillData.damage1, false);
                    //todo. 오른쪽 발사체 방향 진행 에러라서 일단 하나만.
                    monster.AttackController.InstantiateProjectileAtWorld(projectilePaths[i], rightSpawnPos, false, skillData.damage1, false);
                    
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
