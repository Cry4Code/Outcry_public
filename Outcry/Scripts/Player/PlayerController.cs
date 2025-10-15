using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[Flags]
public enum eTransitionType
{
    None = 0,
    IdleState = 1 << 0,
    MoveState = 1 << 1,
    JumpState = 1 << 2,
    DoubleJumpState = 1 << 3,
    FallState = 1 << 4,
    NormalAttackState = 1 << 5,
    NormalJumpAttackState = 1 << 6,
    DownAttackState = 1 << 7,
    SpecialAttackState = 1 << 8,
    DodgeState = 1 << 9,
    StartParryState = 1 << 10,
    SuccessParryState = 1 << 11,
    DamagedState = 1 << 12,
    DieState = 1 << 13,
    PotionState = 1 << 14,
    AdditionalAttackState = 1 << 15,
}



public class PlayerController : MonoBehaviour
{

    private Dictionary<System.Type, BasePlayerState> states; // 상태 저장용
    public Dictionary<string, System.Type> stringStateTypes;
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
    
    private BasePlayerState currentState;
    /*[HideInInspector] */public bool isLookLocked = false;
    
    
    
    private void Awake()
    {
        Inputs = new PlayerInputs();
        
        Skill = GetComponent<PlayerSkill>();
        Skill.Init(this);
        Move = GetComponent<PlayerMove>();
        Attack = GetComponent<PlayerAttack>();
        Attack.Init(this);
        Condition = GetComponent<PlayerCondition>();
        Hitbox = GetComponentInChildren<AttackHitbox>();
        Hitbox.Init(this);
        Animator = GetComponentInChildren<PlayerAnimator>();
        
        halfPlayerHeight = GetComponent<BoxCollider2D>().bounds.size.y / 2;
        
        
        stringStateTypes = new  Dictionary<string, System.Type>()
        {
            { "IdleState", typeof(IdleState)},
            { "MoveState", typeof(MoveState)},
            { "JumpState" , typeof(JumpState)},
            { "DoubleJumpState" , typeof(DoubleJumpState)},
            { "FallState", typeof(FallState)},
            { "NormalAttackState", typeof(NormalAttackState)},
            { "NormalJumpAttackState", typeof(NormalJumpAttackState)},
            { "DownAttackState", typeof(DownAttackState)},
            { "SpecialAttackState", typeof(SpecialAttackState)},
            { "DodgeState", typeof(DodgeState)},
            { "StartParryState", typeof(StartParryState)},
            { "SuccessParryState", typeof(SuccessParryState)},
            { "DamagedState", typeof(DamagedState)},
            { "DieState", typeof(DieState)},
            { "PotionState", typeof(PotionState)},
            { "AdditionalAttackState", typeof(AdditionalAttackState)},
        };
        
        
        states = new Dictionary<System.Type, BasePlayerState>
        {
            { typeof(IdleState), new IdleState() },
            { typeof(MoveState), new MoveState() },
            { typeof(JumpState), new JumpState() },
            { typeof(DoubleJumpState), new DoubleJumpState() },
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
        foreach (var skill in DataManager.Instance.AllSkills)
        {
            skill.Value.SettingController(this);
        }
        
        ChangeState<IdleState>();
    }

    private void OnEnable()
    {
        
        Inputs.Enable();
        Inputs.Change.Pause.started += Move.OnPause;
        Data = DataManager.Instance.PlayerDataModel;
        Debug.Log($"[플레이어] 플레이어 데이터 로드 완료 -> 테스트 : 최대 체력 = {Data.maxHealth}");
    }

    private void OnDisable()
    {
        Inputs.Disable();
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

    public void ChangeState<T>() where T : BasePlayerState
    {
        currentState?.Exit(this);

        currentState = states[typeof(T)];
        currentState.Enter(this);
    }

    public void ChangeState(Type type)
    {
        currentState?.Exit(this);

        currentState = states[type];
        currentState.Enter(this);
    }

    public bool IsCurrentState<T>() where T : BasePlayerState
    {
        return currentState is T;
    }

    public void SetAnimation(int animHash, bool isTrigger = false)
    {
        if (isTrigger) Animator.SetTriggerAnimation(animHash);
        else  Animator.SetBoolAnimation(animHash);
    }

    public IEnumerator IgnoreInputInTime(float time)
    {
        PlayerInputDisable();
        yield return new WaitForSecondsRealtime(time);
        PlayerInputEnable();
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