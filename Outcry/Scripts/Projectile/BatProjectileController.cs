using System.Collections;
using UnityEngine;

public class BatProjectileController : ProjectileBase
{
    private Rigidbody2D rb;
    private Collider2D col;
    private Transform homingTarget; // 따라갈 대상
    private float moveSpeed;
    private float homingCutoffDistance; // 유도를 멈출 거리
    private bool isHoming; // 현재 유도 중인지 여부

    protected override void Awake()
    {
        base.Awake(); // Animator 초기화

        rb = GetComponent<Rigidbody2D>();

        col = GetComponent<Collider2D>();
        col.enabled = false;

        // 물리 효과는 끄고 충돌 감지만 활성화
        rb.isKinematic = true;
    }

    /// <summary>
    /// Bat의 유도 및 이동 로직 프레임마다 처리
    /// </summary>
    private void Update()
    {
        if (homingTarget == null)
        {
            return;
        }

        Vector2 direction;
        // 플레이어와의 거리 계산
        // 추적 중지 조건 확인(homingCutoffDistance 값보다 작아지는 순간, 플레이어와 박쥐 사이의 거리)
        float distanceToTarget = Vector2.Distance(transform.position, homingTarget.position);
        if (isHoming && distanceToTarget < homingCutoffDistance)
        {
            // 유도 중지
            isHoming = false;
        }

        if (isHoming)
        {
            direction = (homingTarget.position - transform.position).normalized;
            // 박쥐 스프라이트의 윗부분(머리 방향)이 방금 계산한 direction을 바라보도록 즉시 회전
            transform.up = direction;
        }
        else
        {
            //유도 기능이 꺼지기 직전에 마지막으로 바라봤던 방향
            direction = transform.up;
        }

        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Base.OnTriggerEnter2D()를 호출하여 플레이어 피격 판정을 처리하고
    /// 추가로 벽이나 땅에 부딪혔을 때의 로직 구현
    /// </summary>
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        // 벽이나 바닥 충돌
        if (collision.CompareTag("Ground"))
        {
            RequestRelease();
        }
    }

    /// <summary>
    /// BatProjectile 발사
    /// BatStorm 스킬 노드에서 호출
    /// </summary>
    public void Launch(PlayerController playerTarget, float speed, float cutoffDistance, int projectileDamage)
    {
        // 데미지 등 기본 정보를 설정
        base.Init(projectileDamage);

        // 유도 기능에 필요한 값들을 설정
        this.homingTarget = playerTarget.transform;
        this.moveSpeed = speed;
        this.homingCutoffDistance = cutoffDistance;
        this.isHoming = true;

        // 발사 직후에는 충돌이 일어나지 않도록 콜라이더 비활성화
        // 지연 활성화 코루틴 시작
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(EnableColliderAfterDelay(0.1f));
        }
    }

    /// <summary>
    /// 지정된 시간(delay)이 지난 후 콜라이더를 다시 활성화하는 코루틴
    /// </summary>
    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        // 지정된 시간만큼 대기합니다.
        yield return new WaitForSeconds(delay);

        // 시간이 지난 후 콜라이더를 다시 켭니다.
        if (col != null)
        {
            col.enabled = true;
        }
    }

    /// <summary>
    /// 오브젝트 풀에 반환되기 직전에 호출
    /// 투사체 상태 초기화
    /// </summary>
    protected override void OnPrepareRelease()
    {
        homingTarget = null;
    }
}
