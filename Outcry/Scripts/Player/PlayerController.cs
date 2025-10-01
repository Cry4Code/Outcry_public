using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Dictionary<System.Type, IPlayerState> states; // 상태 저장용
    public PlayerInputs Inputs { get; private set; }
    public PlayerMove Move { get; private set; }
    public PlayerAttack Attack { get; private set; }
    
    public PlayerCondition Condition { get; private set; }
    
    public PlayerAnimator Animator { get; private set; }
    
    public PlayerDataModel Data {get; private set;}
    
    public AttackHitbox Hitbox { get; private set; }
    
    public PlayerSkill Skill { get; private set; }

    public float halfPlayerHeight;

    public bool runFSM = true;
    
    private IPlayerState currentState;
    [HideInInspector] public bool isLookLocked = false;
    
    
    
    private void Awake()
    {
        Inputs = new PlayerInputs();
        Inputs.Enable();
        Skill = GetComponent<PlayerSkill>();
        Skill.Init(this);
        Move = GetComponent<PlayerMove>();
        Attack = GetComponent<PlayerAttack>();
        Attack.Init(this);
        Condition = GetComponent<PlayerCondition>();
        Hitbox = GetComponentInChildren<AttackHitbox>();
        Hitbox.Init(this);
        
        halfPlayerHeight = GetComponent<BoxCollider2D>().bounds.size.y / 2;
        
        states = new Dictionary<System.Type, IPlayerState>
        {
            { typeof(IdleState), new IdleState() },
            { typeof(MoveState), new MoveState() },
            { typeof(JumpState), new JumpState() },
            { typeof(DoubleJumpState), new DoubleJumpState() },
            { typeof(WallJumpState), new WallJumpState() },
            { typeof(WallHoldState), new WallHoldState() },
            { typeof(FallState), new FallState() },
            { typeof(NormalAttackState), new NormalAttackState() },
            { typeof(NormalJumpAttackState), new NormalJumpAttackState()},
            { typeof(DownAttackState), new DownAttackState()},
            { typeof(SpecialAttackState), new SpecialAttackState()},
            { typeof(DodgeState), new DodgeState()},
            { typeof(StartParryState), new StartParryState()},
            { typeof(SuccessParryState), new SuccessParryState()},
            { typeof(DamagedState), new DamagedState()},
            { typeof(DieState), new DieState()},
            { typeof(PotionState), new PotionState()},
            { typeof(AdditionalAttackState), new AdditionalAttackState()},
        };

        // TODO : 보스 처음에 나올 때 FSM 멈춰두기
        // runFSM = false;
        runFSM = true;
    }

    private void Start()
    {
        Animator = GetComponentInChildren<PlayerAnimator>();
        ChangeState<IdleState>();
    }

    private void OnEnable()
    {
        Inputs.Change.Pause.started += Move.OnPause;
        Data = DataManager.Instance.PlayerDataModel;
        Debug.Log($"[플레이어] 플레이어 데이터 로드 완료 -> 테스트 : 최대 체력 = {Data.maxHealth}");
    }

    private void OnDisable()
    {
        Inputs.Change.Pause.started -= Move.OnPause;
    }

    private void Update()
    {
        Debug.Log($"[플레이어] 상태 : {currentState.GetType().Name}");
        Debug.Log($"[플레이어] 벽 터치 : {Move.isWallTouched}");
        // Debug.Log($"[플레이어] 땅 : {PlayerMove.isGrounded} || 일반 점프 : {PlayerMove.isGroundJump} || 이단 점프 : {PlayerMove.isDoubleJump}");
        currentState.HandleInput(this);
        currentState.LogicUpdate(this);
    }

    private void LateUpdate()
    {
        if(!isLookLocked) Move.Look();
    }

    public void ChangeState<T>() where T : IPlayerState
    {
        currentState?.Exit(this);

        currentState = states[typeof(T)];
        currentState.Enter(this);
    }

    public bool IsCurrentState<T>() where T : IPlayerState
    {
        return currentState is T;
    }

    public void SetAnimation(int animHash, bool isTrigger = false)
    {
        if (isTrigger) Animator.SetTriggerAnimation(animHash);
        else  Animator.SetBoolAnimation(animHash);
    }

    public void PlayerInputDisable()
    {
        Inputs.Player.Move.Disable();
        Inputs.Player.Jump.Disable();
        Inputs.Player.SpecialAttack.Disable();
        Inputs.Player.AdditionalAttack.Disable();
        Inputs.Player.Potion.Disable();
        Inputs.Player.Parry.Disable();
        Inputs.Player.Dodge.Disable();
    }

    public void PlayerInputEnable()
    {
        Inputs.Player.Move.Enable();
        Inputs.Player.Jump.Enable();
        Inputs.Player.SpecialAttack.Enable();
        Inputs.Player.AdditionalAttack.Enable();
        Inputs.Player.Potion.Enable();
        Inputs.Player.Parry.Enable();
        Inputs.Player.Dodge.Enable();
    }
}