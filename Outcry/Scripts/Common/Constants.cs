using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerEffectID
{
    public const int Move = 100000;
    public const int NormalAttack = 100010;
    public const int NormalAttackSound = 100011;
    public const int LastNormalAttackSound = 100012;
    public const int SpecialAttack = 100020;
    public const int JustSpecialAttack = 100021;
    public const int StartParrying = 100030;
    public const int SuccessParrying = 100031;
    public const int SuccessParryingSound = 100032;
    public const int Potion = 100040;

}


public static class AddressablePaths
{
    public const string ROOT = "Assets/09. AddressableAssets/RemoteGroup/";

    public const string AttackRange = "Projectile/AttackRange.prefab";
    public static class Projectile
    {
        public const string Stone = "Projectile/Stone.prefab";
        public const string Fireball = "Projectile/Fireball.prefab";
        public const string Laser = "Projectile/Laser.prefab";
    }
}
public static class Paths
{
    public const string SO = "SO/";
    public static class Prefabs
    {
        public const string UI = "Prefabs/UI/";
        public const string Projectile = "Prefabs/Projectile/";
        public const string Effect = "Prefabs/Effect/";
        public const string Player = "Player/Player.prefab";
    }    
}

public static class EventBusKey
{
    public const string ChangeHealth = "ChangeHealth";
    public const string ChangeStamina = "ChangeStamina";
    public const string ChangeStaminaRecovery = "ChangeStaminaRecovery";
    public const string ChangePlayerDead = "ChangePlayerDead";
    public const string ChangeEnemyDead = "ChangeEnemyDead";
    public const string GetPotion = "GetPotion";
    public const string ChangeHideObstacle = "ChangeHideObstacle";
}

public static class Figures
{
    public static class Monster
    {
        public const float SPECIAL_SKILL_INTERVAL = 2f;
        public const float COMMON_SKILL_INTERVAL = 1f;

        public const int BACKGROUND_SORTING_ORDER_IN_LAYER = 55;
        public const int MONSTER_ORDER_IN_LAYER = 101;
    }
}

public static class AnimatorHash
{
    public static class MonsterParameter
    {
        public static readonly int Running = Animator.StringToHash("Running");
        public static readonly int Walking = Animator.StringToHash("Walking");
        public static readonly int Dead = Animator.StringToHash("Dead");
        public static readonly int Stun = Animator.StringToHash("Stun");
        public static readonly int NormalAttack = Animator.StringToHash("NormalAttack");
        public static readonly int StrongAttack = Animator.StringToHash("StrongAttack");
        public static readonly int Stomp = Animator.StringToHash("Stomp");
        public static readonly int MetalBladeHash = Animator.StringToHash("MetalBlade");
        public static readonly int UpperSlash = Animator.StringToHash("UpperSlash");
        public static readonly int Earthquake = Animator.StringToHash("Earthquake");
        public static readonly int HeavyDestroyer = Animator.StringToHash("HeavyDestroyer");
        public static readonly int IsArrived = Animator.StringToHash("IsArrived");
        public static readonly int ThreePoint = Animator.StringToHash("ThreePoint");
        public static readonly int WhirlWind = Animator.StringToHash("WhirlWind");
        public static readonly int Shark = Animator.StringToHash("Shark");
        public static readonly int RumbleOfRuin = Animator.StringToHash("RumbleOfRuin");
        public static readonly int FinalHorizon = Animator.StringToHash("FinalHorizon");
        public static readonly int IsTired = Animator.StringToHash("IsTired");
        public static readonly int IsReady = Animator.StringToHash("IsReady");
    }

    public static class MonsterAnimation
    {
        public const string Idle = "Idle";
        public const string Run = "Run";
        public static readonly int Stun = Animator.StringToHash("Stun");
        public static readonly int Death = Animator.StringToHash("Death");
        public static readonly int NormalAttack = Animator.StringToHash("NormalAttack");
        public static readonly int StrongAttack = Animator.StringToHash("StrongAttack");
        public static readonly int Stomp = Animator.StringToHash("Stomp");
        public static readonly int UpperSlash = Animator.StringToHash("UpperSlash");
        public static readonly int Earthquake = Animator.StringToHash("Earthquake");
        public static readonly int HeavyDestroyerStart = Animator.StringToHash("HeavyDestroyerStart");
        public static readonly int HeavyDestroyerLoop = Animator.StringToHash("HeavyDestroyerLoop");
        public static readonly int HeavyDestroyerEnd = Animator.StringToHash("HeavyDestroyerEnd");
        public static readonly int Spawn = Animator.StringToHash("Spawn");
        public static readonly int Shark = Animator.StringToHash("Shark");
        public static readonly int ThreePoint = Animator.StringToHash("ThreePoint");
        public static readonly int WhirlWind = Animator.StringToHash("WhirlWind");
        public static readonly int RumbleOfRuinStart = Animator.StringToHash("RumbleOfRuinStart");
        public static readonly int RumbleOfRuinDown = Animator.StringToHash("RumbleOfRuinDown");
        public static readonly int RumbleOfRuinEvent = Animator.StringToHash("RumbleOfRuinEvent");
        public static readonly int RumbleOfRuinComeBack = Animator.StringToHash("RumbleOfRuinComeBack");
        public static readonly int FinalHorizonStart = Animator.StringToHash("FinalHorizonStart");
        public static readonly int FinalHorizonMiddle = Animator.StringToHash("FinalHorizonMiddle");
        public static readonly int FinalHorizonAttack = Animator.StringToHash("FinalHorizonAttack");
        public static readonly int FinalHorizonEnd = Animator.StringToHash("FinalHorizonEnd");
    }

    public static class PlayerAnimation
    {
        // SubState 파라미터
        public static readonly int SubGround = Animator.StringToHash("@Ground");
        public static readonly int SubAir = Animator.StringToHash("@Air");
        public static readonly int SubNormalAttack = Animator.StringToHash("@NormalAttack");
        public static readonly int SubNormalJumpAttack = Animator.StringToHash("@NormalJumpAttack");
        public static readonly int SubDownAttack = Animator.StringToHash("@DownAttack");

        // Bool 파라미터
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Move = Animator.StringToHash("Move");
        public static readonly int Fall = Animator.StringToHash("Fall");

        // Trigger 파라미터
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int DoubleJump = Animator.StringToHash("DoubleJump");
        public static readonly int NormalAttack = Animator.StringToHash("NormalAttack");
        public static readonly int DownAttack = Animator.StringToHash("DownAttack");
        public static readonly int SpecialAttack = Animator.StringToHash("SpecialAttack");
        public static readonly int Dodge = Animator.StringToHash("Dodge");
        public static readonly int StartParry = Animator.StringToHash("StartParry");
        public static readonly int SuccessParry = Animator.StringToHash("SuccessParry");
        public static readonly int Damaged = Animator.StringToHash("Damaged");
        public static readonly int Die = Animator.StringToHash("Die");
        public static readonly int Potion = Animator.StringToHash("Potion");
        public static readonly int AdditionalAttack = Animator.StringToHash("AdditionalAttack");

        // Int 파라미터
        public static readonly int NormalAttackCount = Animator.StringToHash("NormalAttackCount");
        public static readonly int AdditionalAttackID = Animator.StringToHash("AdditionalAttackID");
    }

    public static class ProjectileParameter
    {
        public static readonly int Triggered = Animator.StringToHash("Triggered");
    }
}

