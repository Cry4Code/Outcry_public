using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    #region 컴포넌트 관련
    [field : Header ("Components")]
    public BoxCollider2D boxCollider;
    
    #endregion
    
    
    #region 이동 관련

    [field : Header("Movement Settings")] 
    private Vector2 curMoveInput;
    [HideInInspector] public bool keyboardLeft = false;
    [HideInInspector] public bool lookLeft = false;
    [SerializeField] public float maxWallSlideSpeed = 2.5f;
    #endregion
    
    #region 점프 관련
    [field : Header("Jump Settings")]
    [field : SerializeField] public float GroundThresholdForce { get; set; } // 땅으로 인식하는 법선 벡터 크기 조건
    [field : SerializeField] public float AirMoveThresholdTime { get; set; } // 이 초 이상 체공한 후에 움직이면 RunJump 모션 출력
    
    public LayerMask groundMask;
    public LayerMask interactableMask;
    [HideInInspector] public bool isGroundJump = false; // 지상에서 첫 점프 했는지
    [HideInInspector] public bool isDoubleJump = false; // 더블점프 했는지
    
    [HideInInspector] public bool isGrounded = false;
    [HideInInspector] public bool isWallTouched = false;
    [HideInInspector] public float inAirTime = 0f;
    
    private Vector2 rightWallCheckPos;
    private Vector2 leftWallCheckPos;
    private Vector2 wallCheckBoxSize;

    #endregion

    
    private Camera mainCam;

    [HideInInspector] public Rigidbody2D rb;
    
    public SpriteRenderer SpriteRenderer;
    public bool isDodged = false;
    
    public PlayerController Controller { get; set; }
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if(boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();
        Controller = GetComponent<PlayerController>();
        isGrounded = false;
    }

    private void Start()
    {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        

        wallCheckBoxSize = new Vector2(boxCollider.size.x * 0.1f, boxCollider.size.y * 0.85f);
        inAirTime = 0f;
    }

    
    /// <summary>
    /// JumpState 진입할 때 벽에 붙어있었으면 이걸로 실행함.
    /// 좌우 속도를 멈춘 뒤에 점프하는거
    /// </summary>
    public void PlaceJump()
    {
        rb.velocity = Vector2.zero;
        Jump();
    }


    /// <summary>
    /// JumpState 진입 시 한 번 불림.
    /// </summary>
    public void Jump()
    {
        if (isGroundJump) return;
        isGrounded = false;
        // Debug.Log("Jump!");
        rb.AddForce(Vector2.up * Controller.Data.jumpforce, ForceMode2D.Impulse);
        /*rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);*/
        isGroundJump = true;
    }
    
    /// <summary>
    /// DoubleJumpState 진입 시에 한 번 불림
    /// </summary>
    public void DoubleJump()
    {
        if (isDoubleJump) return;
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * Controller.Data.doubleJumpForce, ForceMode2D.Impulse);
        /*rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);*/
        isDoubleJump = true;
    }

    /// <summary>
    /// (임시용) 커서 보이게 하기 위함
    /// </summary>
    /// <param name="context"></param>
    public void OnPause(InputAction.CallbackContext context)
    {
        //StageManager.Instance.TogglePause();
        //CursorManager.Instance.SetInGame(!CursorManager.Instance.IsInGame);

        // 키가 눌리는 순간(Started)에만 한 번 호출되도록 하여 중복 실행을 방지합니다.
        if (context.started)
        {
            StageManager.Instance.TogglePause();
        }
    }


    public void Stop()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }


    /// <summary>
    /// Input에 맞는 움직임
    /// </summary>
    public void Move()
    {
        Vector2 moveInput = Controller.Inputs.Player.Move.ReadValue<Vector2>();
        rb.velocity = new Vector2(moveInput.x * Controller.Data.moveSpeed, rb.velocity.y);
        if (moveInput.x < 0) keyboardLeft = true;
        else if (moveInput.x > 0) keyboardLeft = false;

        if (moveInput.x != 0)
        {
            if (Controller.IsCurrentState<FallState>()) return;
            if (!isGrounded && inAirTime < AirMoveThresholdTime) return;
            Controller.Animator.OnBoolParam(AnimatorHash.PlayerAnimation.Move);
            ForceLook(moveInput.x < 0);
        }
    }

    private void FixedUpdate()
    {
        if (!isGrounded)
        {
            inAirTime += Time.fixedDeltaTime;
        }
    }

    public void Look()
    {
        // 플레이어는 오른쪽을 봐야함.
        if (CursorManager.Instance.mousePosition.x > transform.position.x)
        {
            lookLeft = false;
        }
        // 플레이어는 왼쪽을 봐야함.
        else
        {
            lookLeft = true;
        }

        transform.localScale = new Vector3(lookLeft? -1 : 1, transform.localScale.y, transform.localScale.z);
    }

    public void ForceLook(bool isLeft)
    {
        lookLeft = isLeft;
        transform.localScale = new Vector3(lookLeft? -1 : 1, transform.localScale.y, transform.localScale.z);
    }

    public void TryInteract()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position,  Vector2.up, 0.1f, interactableMask);
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out InteractableObject interactable))
            {
                interactable.Interact();
                
                Controller.PlayerInputDisable();
            }
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            UpdateGrounded(collision);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            UpdateGrounded(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
            isWallTouched = false;
        }
    }

    private void UpdateGrounded(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 캐릭터가 아래로 향하는 충돌에서만 grounded
            if (contact.normal.y > GroundThresholdForce)    
            {

                isGrounded = true;
                isDoubleJump = false;
                isGroundJump = false;
                isWallTouched = false;
                inAirTime = 0f;
                return;
            }
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isWallTouched ? Color.green : Color.red;

        
        Vector2 wallBoxcenter = (Vector2)transform.position
                            + ((keyboardLeft ? Vector2.left : Vector2.right)
                               * (boxCollider.size.x / 2f));
        
        Gizmos.DrawWireCube(wallBoxcenter, wallCheckBoxSize);
    }
#endif
}
