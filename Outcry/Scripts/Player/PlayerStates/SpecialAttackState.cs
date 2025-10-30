using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpecialAttackState : BasePlayerState
{
    private float startStateTime;
    private float startAttackTime = 0.01f;
    private float animRunningTime = 0f;
    private float attackAnimationLength;
    private float specialAttackSpeed = 10f;
    private Vector2 specialAttackDirection;
    private float specialAttackDistance = 7f;
    private float justTiming = 0.05f;
    private Vector2 startPos;
    private Vector2 targetPos;
    private Vector2 newPos;
    private Vector2 curPos;
    private float cursorAngle = 0f;

    private Vector2 lastSpeed;
    private bool hasLastSpeed;
    
    private bool isSpecialAttacking = false;

    private readonly HashSet<Collider2D> ignoredPlatforms = new HashSet<Collider2D>();

    private const float PLATFROM_TOP_EPS = 0.02f;
    private const float SKIN = 0.03f;

    private float t;
    public override eTransitionType ChangableStates { get; }

    public async override void Enter(PlayerController controller)
    {
        isSpecialAttacking = false;
        ignoredPlatforms.Clear();
        controller.Attack.isStartSpecialAttack = false;
        if (!controller.Condition.TryUseStamina(controller.Data.specialAttackStamina))
        {
            if (controller.Move.isGrounded)
            {
                controller.ChangeState<IdleState>();
                return;
            }
            else
            {
                controller.ChangeState<FallState>();
                return;
            }
        }
        isSpecialAttacking = true;
        controller.Attack.isStartSpecialAttack = true;
        controller.isLookLocked = false;
        controller.Move.ForceLook(CursorManager.Instance.mousePosition.x - controller.transform.position.x < 0);
        controller.Move.rb.velocity = Vector2.zero;
        controller.Animator.ClearTrigger();
        controller.Animator.ClearInt();
        controller.Animator.ClearBool();
        controller.Inputs.Player.Move.Disable();
        controller.Hitbox.AttackState = AttackState.SpecialAttack;
        animRunningTime = 0f;
        attackAnimationLength = 
            controller.Animator.animator.runtimeAnimatorController
                .animationClips.First(c => c.name == "SpecialAttack").length;
        // 커서가 바라보는 방향 구하기
        specialAttackDirection = (CursorManager.Instance.mousePosition - controller.transform.position).normalized;
        #region 방향 보정
        // 1. 지면에서 '아래 각도'인 경우 보정: 
        // - onGround && angle in [270°, 360°)  -> x = +1 (오른쪽, 앞으로)
        // - onGround && angle in [180°, 270°)  -> x = -1 (왼쪽, 뒤로)
        if (controller.Move.isGrounded && specialAttackDirection.y < 0f)
        {
            float deg = Mathf.Atan2(specialAttackDirection.y, specialAttackDirection.x) * Mathf.Rad2Deg;
            float deg360 = (deg < 0f) ? deg + 360f : deg;

            if (deg360 >= 270f && deg360 < 360f)
            {
                specialAttackDirection = Vector2.right;
            }
            else if (deg360 >= 180f && deg360 < 270f)
            {
                specialAttackDirection = Vector2.left;
            }
        }        
        #endregion
        controller.Attack.SetDamage(controller.Data.specialAttackDamage);
        controller.Attack.isStartJustAttack = true;
        controller.Hitbox.specialAttackDamaged = false;
        controller.Condition.isSuperArmor = true;
        EffectManager.Instance.PlayEffectByIdAndTypeAsync(PlayerEffectID.SpecialAttack, EffectType.Sound,
            controller.gameObject).Forget();
        controller.Animator.SetTriggerAnimation(AnimatorHash.PlayerAnimation.SpecialAttack);
        
        
        controller.isLookLocked = true;
        hasLastSpeed = false;

        // 목표 방향으로 캐릭터 돌리기
        // 1. 회전 각도는 보정 후 방향으로 계산
        cursorAngle = Mathf.Atan2(specialAttackDirection.y, specialAttackDirection.x) * Mathf.Rad2Deg;

        // 2. 스프라이트만 따로 빼서
        var gfx = controller.Move.SpriteRenderer != null ? controller.Move.SpriteRenderer.transform : controller.transform;

        // 3. 그 각도대로 돌리기
        if (specialAttackDirection.x > 0)
        {
            gfx.rotation = Quaternion.Euler(0, 0, cursorAngle);
        }
        else
        {
            gfx.rotation = Quaternion.Euler(0, 0, -180f + cursorAngle);
        }
        
        startPos = controller.transform.position;
        targetPos = startPos + (specialAttackDirection * specialAttackDistance);
        controller.Attack.justAttackStartPosition = startPos;
    }

    public override void HandleInput(PlayerController controller)
    {
        
    }

    public override void LogicUpdate(PlayerController controller)
    {
        // 멈춤 상태. 별로면 나중에 이부분 빼면 됨
        if (controller.Animator.animator.speed < 1)
        {
            if (!hasLastSpeed)
            {
                hasLastSpeed = true;
                lastSpeed = controller.Move.rb.velocity;
            }
            controller.Move.rb.velocity = Vector2.zero;
            return;
        }

        animRunningTime += Time.deltaTime;

        if (animRunningTime >= justTiming)
        {
            controller.Attack.isStartJustAttack = false;
        }

        if (hasLastSpeed)
        {
            controller.Move.rb.velocity = lastSpeed;
            hasLastSpeed = false;
        }

        t = animRunningTime / attackAnimationLength;

        newPos = Vector2.MoveTowards(startPos, targetPos, t * specialAttackSpeed);

        curPos = controller.transform.position;

        // 목표 지점까지 거리 계산
        Vector2 direction = (newPos - curPos).normalized;
        float distance = Vector2.Distance(curPos, newPos);

        // 플레이어 콜라이더 크기 그대로 BoxCast
        var box = controller.Move.boxCollider;
        var size = (Vector2)box.bounds.size;
        var origin = (Vector2)controller.Move.boxCollider.bounds.center;

        // 모든 ground 레이어 대상으로 캐스트, 태그/법선으로 필터
        var hits = Physics2D.BoxCastAll(origin, size, 0f, direction, distance, controller.Move.groundMask);
#if UNITY_EDITOR
        {
            // 1. 박스 캐스트 스윕 시각화
            var angle = 0f; // 지금은 BoxCollider2D가 회전 안하니 0도.
            DebugPhysics2D.DrawBoxCast(origin, size, angle, direction, distance, Color.yellow, new Color(1f, 0.6f, 0f), 0f);

            // 2. 히트 지점/노멀 시각화
            DebugPhysics2D.DrawHits(hits, 0.25f, 0f);

            // 3. 관통 허용 중인 플랫폼 콜라이더 테두리 표시
            foreach (var plat in ignoredPlatforms)
                if (plat) DebugPhysics2D.DrawBox(plat.bounds, 0f, Color.magenta, 0f);
        }
#endif

        RaycastHit2D? nearestBlock = null;  // 가장 가까운 장애물
        float minDist = float.MaxValue; // 가장 가까운 장애물까지의 거리
        float feetY = controller.Move.boxCollider.bounds.min.y; // 캐릭터의 발바닥 높이
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null || h.collider.isTrigger) continue; // 콜라이더가 없거나 트리거 콜라이더면 무시

            string tag = h.collider.tag;
            bool isGroundTag = tag == "Ground"; // 일반 바닥
            bool isPlatformTag = tag == "Platform"; // 플랫폼

            // 차단 규칙
            // - Ground: 항상 차단
            // - Platform: 위에서 지나가려 할 때만 차단 (옆면/아랫면은 통과)
            // - - dir.y <= 0: 아래/수평 접근
            // - - h.normal.y > 0.2: 상단에서 부딪힘
            bool blocks = false;
            if (isGroundTag) blocks = true; // 땅과 충돌의 경우
            else if (isPlatformTag) // 플랫폼과 충돌의 경우
            {
                float topY = h.point.y; // 윗면 접점 근사
                bool upward = direction.y > 0f;
                bool fromBelow = (feetY < topY - PLATFROM_TOP_EPS);

                if (fromBelow) // 아래에서 위로 진입일 경우
                {
                    var playerCol = controller.Move.boxCollider;
                    var platCol = h.collider;
                    if (!ignoredPlatforms.Contains(platCol))
                    {
                        // 플랫폼과 충돌 끄기
                        Physics2D.IgnoreCollision(playerCol, platCol, true);
                        ignoredPlatforms.Add(platCol);
                    }
                    blocks = false;
                }
                else
                {
                    // 옆/위에서 윗면 경계로 진입일 경우
                    if (h.normal.y > 0.2f)
                        blocks = true;
                }                  
            }
            else blocks = false; // 그 외의 충돌은 통과

            if (!blocks) continue;  // 막히지 않았으면 무시

            // blocks가 있으면 가장 가까운 걸 장애물에 저장
            if (h.distance < minDist)
            {
                minDist = h.distance;
                nearestBlock = h;
            }
        }

        #region 장애물이 있는 경우 

        /* 각도 보정, Enter로 이동됨.
        // 지면에서 아래로 섬단 시도 시, 애매한 각도는 앞으로 보정(짧은 섬단 방지)
        bool OnGround = controller.Move.isGrounded;
        bool slightDownFromGround = OnGround && direction.y <= 0f;

        if (nearestBlock.HasValue && minDist <= SHORT_STOP && slightDownFromGround)
        {
            // 섬단 방향을 앞으로 보정 (y=0)
            float adjustedX = Mathf.Sign(direction.x) != 0 ? Mathf.Sign(direction.x) : 1f;
            direction = new Vector2(adjustedX, 0f).normalized;
            hits = Physics2D.BoxCastAll(origin, size, 0f, direction, distance, ~0);

            // 보정 방향으로 다시 최단 차단 재계산
            nearestBlock = null;
            minDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h.collider == null || h.collider.isTrigger) continue;

                string tag = h.collider.tag;
                bool isGround = (tag == "Ground");
                bool isPlatform = (tag == "Platform");

                bool blocks = isGround || (isPlatform && direction.y <= 0f && h.normal.y > 0.2f);
                if (!blocks) continue;

                if (h.distance < minDist)
                {
                    minDist = h.distance;
                    nearestBlock = h;
                }
            }
        }
        */

        // nearestBlock(장애물)이 있으면, 접점 직전까지만 이동, 이후 다음 상태로 바로 이동
        if (nearestBlock.HasValue)
        {
            var hit = nearestBlock.Value;
            var stop = (Vector2) controller.Move.rb.position + direction * Mathf.Max(0f, hit.distance - SKIN);

            controller.Move.rb.MovePosition(stop);

            // 장애물에 막힌 경우 낙하 속도 초기화
            if (hit.normal.y > 0.2f && controller.Move.rb.velocity.y < 0f)
            {
                controller.Move.rb.velocity = new Vector2(controller.Move.rb.velocity.x, 0f);
            }

            // 다음 상태로 바로 전환
            if (controller.Move.isGrounded)
                controller.ChangeState<IdleState>();
            else
                controller.ChangeState<FallState>();

            return;
        }

        #endregion

        /* 기존 로직 - 레이 기반 탐색
        // 현재 위치에서 이동할 위치만큼 선 하나 그어서, 그게 벽에 닿으면 벽 끝에까지만 가고 상태 바뀌게함
        RaycastHit2D hit =
            Physics2D.Raycast(controller.transform.position, direction, distance, controller.Move.groundMask);
        
        if (hit.collider != null)
        {
            if (!hit.collider.CompareTag("Platform"))
            {
                controller.Move.rb.MovePosition(hit.point - direction * 0.01f);
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;    
            }
        }
        */

        controller.Move.rb.MovePosition(newPos);
        
        if (Vector2.Distance(newPos, targetPos) < 0.01f)
        {
            controller.Move.rb.velocity = Vector2.zero;
            if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
            else controller.ChangeState<FallState>();
            return;
        }
        
        if (Time.time - startStateTime > startAttackTime)
        {
            AnimatorStateInfo curAnimInfo = controller.Animator.animator.GetCurrentAnimatorStateInfo(0);

            if (curAnimInfo.IsName("SpecialAttack"))
            { 
                float animTime = curAnimInfo.normalizedTime;

                if (animTime >= 1.0f)
                {
                    if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                    else controller.ChangeState<FallState>();
                    return;
                }
            }

            /*
            if (animRunningTime >= attackAnimationLength)
            {
                if (controller.Move.isGrounded) controller.ChangeState<IdleState>();
                else controller.ChangeState<FallState>();
                return;
            }*/
                
        }
    }

    public override void Exit(PlayerController controller)
    {
        controller.isLookLocked = false;
        controller.Inputs.Player.Move.Enable();
        controller.Condition.isSuperArmor = false;
        controller.Attack.isStartSpecialAttack = false;
        controller.Attack.isStartJustAttack = false;
        controller.Attack.successJustAttack = false;
        controller.transform.rotation = Quaternion.Euler(0, 0, 0);
        controller.Move.SpriteRenderer.transform.rotation = Quaternion.identity;
        foreach (var col in ignoredPlatforms)
        {
            if (col)
                Physics2D.IgnoreCollision(controller.Move.boxCollider, col, false);
        }
        ignoredPlatforms.Clear();
        int stageId = StageManager.Instance.CurrentStageData.Stage_id;
        if (isSpecialAttacking &&  stageId != StageID.Village)
        {
            UGSManager.Instance.LogDoAction(stageId, PlayerEffectID.SpecialAttack);
        }
    }
}
