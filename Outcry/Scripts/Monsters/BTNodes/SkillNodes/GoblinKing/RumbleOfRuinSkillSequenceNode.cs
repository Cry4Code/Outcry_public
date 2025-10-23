using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RumbleOfRuinSkillSequenceNode : SkillSequenceNode
{
    private const float DOWN_GRAVITY_MULTIPLIER = 5f;
    private Color darkenedColor = new Color(150f/255f, 150f/255f, 150f/255f, 1f);
    private Vector2 targetPos;
    private float originalGravityScale;
    private Vector2 originalLocalScale;
    private Vector2 originalLocalPosition;
    private Color originalColor;

    
    
    private float ascendSpeed = 50f;
    public RumbleOfRuinSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "RumbleOfRuinSkillSequenceNode";
    }

    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        this.monster = monster;
        this.target = target;
        
        ConditionNode canPerform = new ConditionNode(CanPerform);
        ActionNode jumpAction = new ActionNode(JumpAction);
        ActionNode warningAction = new ActionNode(WarningAction);
        ActionNode downAction = new ActionNode(DownAction);
        ActionNode throwAction = new ActionNode(SkillAction);
        ActionNode combackAction = new ActionNode(CombackAction);
        
        //노드 이름 설정 (디버깅용)
        canPerform.nodeName = "CanPerform";
        jumpAction.nodeName = "JumpAction";
        warningAction.nodeName = "WarningAction";
        downAction.nodeName = "DownAction";
        throwAction.nodeName = "ThrowAction";
        combackAction.nodeName = "CombackAction";
        
        children.Clear();
        AddChild(canPerform);
        AddChild(jumpAction);
        AddChild(warningAction);
        AddChild(downAction);
        AddChild(throwAction);
        AddChild(combackAction);

        nodeName = skillData.skillName + skillData.skillId;
        
        //초기 값 기억
        originalGravityScale = monster.Rb2D.gravityScale;
        originalLocalScale = monster.transform.localScale;
        originalLocalPosition = monster.Rb2D.position;
        originalColor = monster.SpriteRenderer.color;
        lastUsedTime = Time.time;
    }

    protected override bool CanPerform()
    {
        if (skillTriggered) //한번 실행되면 다시 실행될 일 없음. 실행 끝난 이후 리셋 X
        {
            return false;
        }
        
        //체력이 일정 이하일때
        bool isLowHealth = monster.Condition.CurrentHealth.CurValue() < skillData.triggerHealth * monster.Condition.MaxHealth;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) {isLowHealth} : {monster.Condition.CurrentHealth} / {monster.Condition.MaxHealth} < {skillData.triggerHealth}");
        if (isLowHealth) return true;
        
        //쿨타임 체크
        bool isCooldownComplete = Time.time - lastUsedTime >= skillData.cooldown;
        Debug.Log($"Skill {skillData.skillName} (ID: {skillData.skillId}) cooldownComplete={isCooldownComplete} : {Time.time - lastUsedTime} / {skillData.cooldown}");
        return isCooldownComplete;
    }
    // protected override bool CanPerform()
    // {
    //     bool result;
    //     bool isCooldownComplete;
    //     bool isLowHealth;
    //     if (skillTriggered) //한번 실행되면 다시 실행될 일 없음. 실행 끝난 이후 리셋 X
    //     {
    //         return false;
    //     }
    //     
    //     //todo. think. 지금 연산을... 무조건 두개 다 하게 되어있음.
    //     // 메모리를 위해서 둘 중 하나라도 실패하면 return 하도록 하면 나머지 연산을 할 필요가 없어짐
    //     // 다만 가독성이 떨어질 수 있음.
    //     // 우선은 가독성 우선으로 하여 놔둠.
    //     
    //     //체력이 일정 이하일때
    //     isLowHealth = monster.Condition.CurrentHealth < skillData.triggerHealth * monster.Condition.MaxHealth;
    //     
    //     //쿨타임 체크
    //     //todo. 완성 후에는 테스트 매니저 참조가 아닌 SkillData 참조로 변경할 것!
    //     isCooldownComplete = Time.time - lastUsedTime >= skillData.cooldown;
    //
    //     result = (isLowHealth || isCooldownComplete) && !skillTriggered;
    //     Debug.Log($"Skill {skillData.skillName} used? {result} : {Time.time - lastUsedTime} / {skillData.cooldown} || {monster.Condition.CurrentHealth} / {monster.Condition.MaxHealth}");
    //     return result;
    // }
    private bool isJumping = false;
    private bool isJumpAnimationStarted = false;
    private NodeState JumpAction()
    {
        if (!isJumping) //아직 점프 시작 전.
        {
            // 0. 스킬 사용됨. 몬스터 무적 상태로 만들기
            skillTriggered = true;
            monster.Condition.SetInivincible(true);
            
            // 1. 몬스터가 점프해서 화면 밖으로 사라질 곳 설정
            isJumping = true;
            float ascendHeight = 10f;
            targetPos = new Vector2(
                monster.Rb2D.position.x, //+ forwardDistance * direction,
                monster.Rb2D.position.y + ascendHeight
            );
            
            //
            // lastUsedTime = Time.time; //skillTriggered로만 체크해도 됨. (단 한번만 사용되는 스킬이므로)
            // timer = Time.time; //애니메이션 시작했는지 체크하는 로직 변경하면서 이제 필요없음
            
            //2. 점프 애니메이션 실행
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.RumbleOfRuin);

            // 낙석 이벤트 호출
            GoblinKingAI ai = monster.GetComponent<GoblinKingAI>();
            if (ai != null)
            {
                ai.TriggerFallingRocksPattern();
            }

            return NodeState.Running;
        }

        // 3. 점프 애니메이션이 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isJumpAnimationStarted)
        {
            isJumpAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinStart);
            return NodeState.Running;
        }

        // 4. 점프 애니메이션이 재생되는 동안 대기.
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinStart, 0.0f, 0.80f))
        {
            return NodeState.Running;
        }

        // 5. 점프 애니메이션이 끝났다면, 목표 지점까지 이동.
        // if (monster.Rb2D.position.y < targetPos.y)
        // {
        //     Vector2 nextPos = Vector2.MoveTowards(monster.Rb2D.position, targetPos, ascendSpeed * Time.fixedDeltaTime);
        //     monster.Rb2D.MovePosition(nextPos);
        //     return NodeState.Running;
        // }

        // 6. 목표 지점에 도달했다면, 다음 액션을 대비해서 투명/크기 변경후 다음 단계로 진행.
        monster.SpriteRenderer.color = new Color(monster.SpriteRenderer.color.r, monster.SpriteRenderer.color.g, monster.SpriteRenderer.color.b, 0);
        monster.transform.localScale = originalLocalScale * 0.6f; //오리지널 크기의 절반정도.
        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) JumpAction Done!");
        return NodeState.Success;
    }
    
    private bool isWarningUITriggerd = false;
    private NodeState WarningAction() //얘는 나중에 다른 보스들이랑 warning 컷씬 공유하게 되면 얘를 따로 빼면 됨.
    {
        if (!isWarningUITriggerd) //아직 경고 UI 시작 전.
        {
            // 0. 경고창 띄우기
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.ScreenUI);
            isWarningUITriggerd = true;
            return NodeState.Running;
        }
        
        if (EffectManager.Instance.IsEffectPlaying(skillData.skillId, EffectType.ScreenUI))
        {
            // 1. 경고창이 재생되는 동안 대기.
            return NodeState.Running;
        }
        
        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) WarningAction Done!");
        return NodeState.Success;
    }

    private bool isDownTriggerd = false;
    private bool isDownAnimationStarted = false;
    private NodeState DownAction()
    {
        if (!isDownTriggerd) //아직 착지 시작 전
        {
            //0. 착지 시작됨. 카메라 기준 화면 정 중앙으로 이동.
            isDownTriggerd = true;
            targetPos = new Vector2(Camera.main.transform.position.x, monster.Rb2D.position.y);
            monster.Rb2D.position = targetPos;
            
            //1. 추락 직전. 몬스터 컬러 어둡게(그림자), orderinLayer 중간으로 변경.
            monster.SpriteRenderer.color = darkenedColor;
            monster.SpriteRenderer.sortingOrder = Figures.Monster.BACKGROUND_SORTING_ORDER_IN_LAYER;
            
            //2. 착지 애니메이션 실행: 중력 강화.
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.IsReady);
            // return NodeState.Running;
        }
        
        // 3. 착지 애니메이션이 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isDownAnimationStarted)
        {
            isDownAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinDown);
            if (isDownAnimationStarted)
            {
                //3-1 애니메이션 시작이 된게 확보되면 그때 중력 상승.
                monster.Rb2D.gravityScale = originalGravityScale * DOWN_GRAVITY_MULTIPLIER;
            }
            return NodeState.Running;
        }
        
        //4. 착지 애니메이션 재생이 끝나고, 몬스터가 땅에 닿을 때까지 대기.
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinDown)
            && monster.Rb2D.velocity.y <= 0f)
        {
            return NodeState.Running;
        }

        Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) DownAction Done!");
        // monster.Rb2D.gravityScale = originalGravityScale; //중력 초기화 //todo. 초기화 여기서는 안해도 될 것 같아서 주석 처리. 중력이 이상하면 주석 풀기.
        return NodeState.Success;
    }

    private bool isThrowRockTriggerd = false;
    private bool isROREventAnimationStarted = false;
    private bool isRockInstantiated = false;
    private bool isCameraShaked = false;
    protected override NodeState SkillAction()
    {
        if (!isThrowRockTriggerd) //아직 돌 던지기 시작 전
        {
            effectStarted = false;
            // 0. 돌 던지기 시작됨.
            isThrowRockTriggerd = true;
            
            // 1. 돌 던지기 애니메이션 실행: (이전 착지 애니메이션 끝난 상태에서 바로 실행됨)
            return NodeState.Running;
        }
        
        // 2. 던지기 애니메이션 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isROREventAnimationStarted)
        {
            isROREventAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinEvent);
            return NodeState.Running;
        }

        //3. 애니메이션이 절반 쯤 실행되었을 때까지 기다리기. 
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinEvent, 0f,
                0.5f))
        {
            return NodeState.Running;
        }
        
        //4. 돌 생성.
        if(!isRockInstantiated)
        {
            Debug.Log($"[몬스터] {skillData.skillName} (ID: {skillData.skillId}) SkillAction: Throw Rock!");
            isRockInstantiated = true;
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId * 10, EffectType.Sound, monster.gameObject).Forget();
            EffectManager.Instance.PlayEffectsByIdAsync(skillData.skillId, EffectOrder.SpecialEffect, monster.gameObject).Forget();
            return NodeState.Running;
        }
        
        //5. 돌이 날아가는 이펙트가 재생되는 동안 대기.
        if (EffectManager.Instance.IsEffectPlaying(skillData.skillId, EffectType.Sprite))
            // || AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinEvent))
        {
            //5-1. 몬스터는 애니메이션 종료(마지막 프레임)되면 투명하게 숨기기
            if(AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinEvent, 0f, 0.95f))
            {
                monster.SpriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
            }
            return NodeState.Running;
        }
        
        // 5-1. 돌 날라가는 이펙트 끝나면 한 번 터지는 사운드
        if (!effectStarted)
        {
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId * 10 + 1, EffectType.Sound, monster.gameObject).Forget();
            effectStarted = true;
        }
        
        //6. 돌이 애니메이션이 끝나면 카메라 진동 및 데미지.
        if (!isCameraShaked && !EffectManager.Instance.IsEffectPlaying(skillData.skillId, EffectType.Sprite))
        {
            //6-1. 카메라 진동 주기
            Debug.Log($"[몬스터] (ID: {skillData.skillId}) 카메라 진동! 데미지!");
            
            EffectManager.Instance.PlayEffectByIdAndTypeAsync(skillData.skillId, EffectType.Camera).Forget();
            isCameraShaked = true;
            
            //6-2. 플레이어가 데미지 입기
            if (!PlayerManager.Instance.player.Condition.behindObstacle.Value)
            {
                PlayerManager.Instance.player.Condition.TakeDamage(skillData.damage1);
            }
            
            return NodeState.Running;
        }
        
        //7. 카메라 진동이 끝날 때까지 대기.
        if (EffectManager.Instance.IsEffectPlaying(skillData.skillId, EffectType.Camera))
        {
            
            return NodeState.Running;   
        }
        
        return NodeState.Success;
    }

    private bool isCombackTriggerd = false;
    private bool isComebackAnimationStarted = false;
    private NodeState CombackAction()
    {
        if (!isCombackTriggerd) //아직 복귀 시작 전.
        {
            //0. 컴백 시작
            isCombackTriggerd = true;
            
            //1. 처음 스킬 시작했던 위치에서 1f 위로 이동. (살짝 떨어지는 느낌 주기 위함)
            targetPos = new Vector2(originalLocalScale.x, originalLocalScale.y + 1f);
            monster.Rb2D.position = targetPos;
            
            //2. 복귀 애니메이션 실행: 
            monster.Animator.SetTrigger(AnimatorHash.MonsterParameter.IsReady);
            
            return NodeState.Running;
        }
        
        // 3. 착지 애니메이션이 시작될 때까지 대기. (애니메이션 재생 확보: 프레임 하나 이상 재생된 상태)
        if (!isComebackAnimationStarted)
        {
            isComebackAnimationStarted = AnimatorUtility.IsAnimationStarted(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinComeBack);
            if (isComebackAnimationStarted)
            {
                //3-1 애니메이션 시작이 된게 확보되면 그때 중력과 초기화.
                ResetState();
                monster.Rb2D.gravityScale = originalGravityScale * DOWN_GRAVITY_MULTIPLIER;
            }
            return NodeState.Running;
        }
        
        //4. 착지 애니메이션 재생이 끝날때까지 대기.
        if (AnimatorUtility.IsAnimationPlaying(monster.Animator, AnimatorHash.MonsterAnimation.RumbleOfRuinComeBack))
        {
            return NodeState.Running;
        }
        
        //final: 놓쳤을 수도 있는 부분 전부 초기화
        ResetState();
        return NodeState.Success;
    }

    private void ResetState()
    {
        monster.Rb2D.gravityScale = originalGravityScale;
        monster.SpriteRenderer.sortingOrder = Figures.Monster.MONSTER_ORDER_IN_LAYER;
        monster.SpriteRenderer.color = originalColor;
        monster.transform.localScale = originalLocalScale;
        // monster.Rb2D.position = originalLocalPosition;
        monster.Condition.SetInivincible(false);
    }
}
