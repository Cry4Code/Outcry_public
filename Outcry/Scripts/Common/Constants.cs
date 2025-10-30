using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerEffectID
{
    public const int Move = 100000;
    public const int LowHp = 100001;
    public const int Damaged = 100002;
    public const int Jump = 100003;
    public const int NormalAttack = 100010;
    public const int NormalAttackSound = 100011;
    public const int LastNormalAttackSound = 100012;
    public const int MonsterDamaged = 100013;
    public const int SpecialAttack = 100020;
    public const int JustSpecialAttack = 100021;
    public const int StartParrying = 100030;
    public const int SuccessParrying = 100031;
    public const int SuccessParryingSound = 100032;
    public const int Potion = 100040;
    public const int Dodge = 100050;
    public const int JumpAttack = 100060;
    public const int JumpDownAttack = 100070;

    public const int FlameSlash = 102000;
    public const int SuperCrash = 102001;
    public const int ScrewAttack = 102002;
    public const int HolySlash = 102003;
    public const int PowerUp = 102004;
    public const int DeadHard = 102005;
}

public static class UIEffectID
{
    public const int Click = 10000;
    public const int EquipSkill = 10010;
    public const int BuySkill = 10011;
    public const int OpenChapterMap = 10020;
}

public static class Stage1BossEffectID
{
    public const int NormalAttack = 103000;
    public const int MetalBlade = 103001;
    public const int HeavyDestroyer = 103002;
    public const int ThreePoint = 103003;
    public const int Earthquake =  103004;
    public const int Stomp = 103005;
    public const int UpperSlash = 103006;
    public const int Shark = 103007;
    public const int WhirlWind = 103008;
    public const int RumbleOfRuin = 103009;
    public const int FinalHorizon = 103010;
}

public static class Stage2BossEffectID
{
    public const int Takkong = 103401;
    public const int ThunderStrike =  103402;
    public const int FireBash =  103403;
    public const int ThunderBall = 103404;
    public const int FireBolt = 103405;
    public const int IceMountain = 103406;
    public const int ThunderBolt = 103407;
    public const int BlackHole =  103408;
    public const int Infeirno = 103409;
    public const int VolcanoDive =  103410;
    public const int Meteor = 103411;
}

public static class Stage3BossEffectID
{
    public const int NormalATK = 103501;
    public const int DarkSwamp = 103502;
    public const int Darkness = 103503;
    public const int DarkBomb =  103504;
    public const int TurningBlood = 103505;
    public const int BloodSting = 103506;
    public const int BloodShard = 103507;
    public const int BloodVeil = 103508;
    public const int BloodSpear = 103509;
    public const int BatStorm = 103510;
    public const int BloodMoon = 103511;
}

public static class MonsterID
{
    public const int GoblinRogue = 101000;
    public const int GoblinFirekeeper = 101001;
    public const int GoblinFighter = 101002;
    public const int GoblinKing = 101201;
    public const int PhantomWitch = 101202;
    public const int VampireLord = 101205;
    public const int VampireLordFlying = 101206;
}


public static class StageID
{
    public const int Tutorial = 0;
    public const int Village = 106000;
    public const int RuinsOfTheFallenKing = 106001;
    public const int AbandonedMine = 106002;
    public const int HallOfBlood = 106003;
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
        public const string TurningBlood = "Projectile/TurningBlood.prefab";
        public const string BlackHole = "Projectile/BlackHole.prefab";
        public const string Infierno = "Projectile/Infierno.prefab";
        public const string FireBolt = "Projectile/FireBolt.prefab";
        public const string ThunderStrike = "Projectile/ThunderStrike.prefab";
        public const string IceMountain = "Projectile/IceMountain.prefab";
        public const string Meteor = "Projectile/Meteor.prefab";
        public const string Bat = "Projectile/Bat.prefab";
        public const string DarkSwamp = "Projectile/DarkSwamp.prefab";
        public const string BloodSpear = "Projectile/BloodSpear.prefab";
        public const string Darkness = "Projectile/Darkness.prefab";
        public const string DarkBomb = "Projectile/DarkBomb.prefab";
        public const string BloodShard = "Projectile/BloodShard.prefab";
    }

    public static class UI
    {
        public const string BloodMoon = "Stages/HallOfBlood/BloodMoon.prefab";
        public const string QTE_UI = "Stages/HallOfBlood/QTE_UI.prefab";
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
        public const string Cursor = "Player/InGameCursor.prefab";
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
    public const string ChangeBossHealth =  "ChangeBossHealth";
    public const string ChangePotionCount = "ChangePotionCount";
    public const string CantUseCuzStamina = "CantUseCuzStamina";
    public const string CantUseCuzCooldown = "CantUseCuzCooldown";
}

public static class Figures
{
    public static class Monster
    {
        public const float SPECIAL_SKILL_INTERVAL = 2f;
        public const float COMMON_SKILL_INTERVAL = 1f;
        public const float BOSS2_COMMON_SKILL_INTERVAL = 0.5f;

        public const int BACKGROUND_SORTING_ORDER_IN_LAYER = 55;
        public const int MONSTER_ORDER_IN_LAYER = 200;
    }
}

public static class AnimatorHash
{
    public static class MonsterParameter
    {
        // 공용
        public static readonly int Running = Animator.StringToHash("Running");
        public static readonly int Walking = Animator.StringToHash("Walking");
        public static readonly int Dead = Animator.StringToHash("Dead");
        public static readonly int Stun = Animator.StringToHash("Stun");
        public static readonly int NormalAttack = Animator.StringToHash("NormalAttack");
        public static readonly int StrongAttack = Animator.StringToHash("StrongAttack");
        public static readonly int IsArrived = Animator.StringToHash("IsArrived");
        public static readonly int IsTired = Animator.StringToHash("IsTired");
        public static readonly int IsReady = Animator.StringToHash("IsReady");

        // GoblinKing
        public static readonly int Stomp = Animator.StringToHash("Stomp");
        public static readonly int MetalBlade = Animator.StringToHash("MetalBlade");
        public static readonly int UpperSlash = Animator.StringToHash("UpperSlash");
        public static readonly int Earthquake = Animator.StringToHash("Earthquake");
        public static readonly int HeavyDestroyer = Animator.StringToHash("HeavyDestroyer");
        public static readonly int ThreePoint = Animator.StringToHash("ThreePoint");
        public static readonly int WhirlWind = Animator.StringToHash("WhirlWind");
        public static readonly int Shark = Animator.StringToHash("Shark");
        public static readonly int RumbleOfRuin = Animator.StringToHash("RumbleOfRuin");
        public static readonly int FinalHorizon = Animator.StringToHash("FinalHorizon");

        // DreamlikeWitch        
        public static readonly int Takkong = Animator.StringToHash("Takkong");
        public static readonly int FireBash = Animator.StringToHash("FireBash");
        public static readonly int ThunderBall = Animator.StringToHash("ThunderBall");
        public static readonly int IceMountain = Animator.StringToHash("IceMountain");
        public static readonly int ThunderBolt = Animator.StringToHash("ThunderBolt");
        public static readonly int VolcanoDive = Animator.StringToHash("VolcanoDive");
        public static readonly int FireBolt = Animator.StringToHash("FireBolt");
        public static readonly int ThunderStrike = Animator.StringToHash("ThunderStrike");
        public static readonly int BlackHole = Animator.StringToHash("BlackHole");
        public static readonly int Infierno = Animator.StringToHash("Infierno");
        public static readonly int Meteor = Animator.StringToHash("Meteor");

        //VampireLord
        public static readonly int Ascending = Animator.StringToHash("Ascending");
        public static readonly int Descending = Animator.StringToHash("Descending");
        
        public static readonly int BloodSting = Animator.StringToHash("BloodSting");
        public static readonly int BloodShard = Animator.StringToHash("BloodShard");
        public static readonly int BloodVeil = Animator.StringToHash("BloodVeil");
        public static readonly int BloodSpear = Animator.StringToHash("BloodSpear");
        public static readonly int TurningBlood = Animator.StringToHash("TurningBlood");
        public static readonly int ChargeSkill = Animator.StringToHash("ChargeSkill");

        public static readonly int BatStorm = Animator.StringToHash("BatStorm");
        public static readonly int BatStormFlying = Animator.StringToHash("BatStormFlying");
        public static readonly int ChargeStart = Animator.StringToHash("ChargeStart");
        public static readonly int IsFlying = Animator.StringToHash("IsFlying");
        public static readonly int BrokenMoon = Animator.StringToHash("BrokenMoon");
    }

    public static class MonsterAnimation
    {
        // 공용
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Tired = Animator.StringToHash("Tired");
        public const string Run = "Run";
        public static readonly int Stun = Animator.StringToHash("Stun");
        public static readonly int Death = Animator.StringToHash("Death");
        public static readonly int NormalAttack = Animator.StringToHash("NormalAttack");
        public static readonly int StrongAttack = Animator.StringToHash("StrongAttack");
        public static readonly int Spawn = Animator.StringToHash("Spawn");

        // GoblinKing
        public static readonly int Stomp = Animator.StringToHash("Stomp");
        public static readonly int MetalBlade = Animator.StringToHash("MetalBlade");
        public static readonly int UpperSlash = Animator.StringToHash("UpperSlash");
        public static readonly int Earthquake = Animator.StringToHash("Earthquake");
        public static readonly int HeavyDestroyerStart = Animator.StringToHash("HeavyDestroyerStart");
        public static readonly int HeavyDestroyerLoop = Animator.StringToHash("HeavyDestroyerLoop");
        public static readonly int HeavyDestroyerEnd = Animator.StringToHash("HeavyDestroyerEnd");
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

        // DreamlikeWitch
        public static readonly int Takkong = Animator.StringToHash("Takkong");
        public static readonly int FireBash = Animator.StringToHash("FireBash");
        public static readonly int ThunderBall = Animator.StringToHash("ThunderBall");
        public static readonly int IceMountain = Animator.StringToHash("IceMountain");
        public static readonly int ThunderBolt = Animator.StringToHash("ThunderBolt");
        public static readonly int VolcanoDive = Animator.StringToHash("VolcanoDive");
        public static readonly int FireBolt = Animator.StringToHash("FireBolt");
        public static readonly int ThunderStrike = Animator.StringToHash("ThunderStrike");
        public static readonly int BlackHole = Animator.StringToHash("BlackHole");
        public static readonly int Infierno = Animator.StringToHash("Infierno");
        public static readonly int Meteor = Animator.StringToHash("Meteor");

        //VampireLord
        public static readonly int BloodStingStart = Animator.StringToHash("BloodStingStart");
        public static readonly int BloodStingLoop = Animator.StringToHash("BloodStingLoop");
        public static readonly int BloodStingEnd = Animator.StringToHash("BloodStingEnd");
        public static readonly int BloodShard = Animator.StringToHash("BloodShard");
        public static readonly int BloodVeil = Animator.StringToHash("BloodVeil");
        public static readonly int BloodSpear = Animator.StringToHash("BloodSpear");
        public static readonly int TurningBlood = Animator.StringToHash("TurningBlood");
        public static readonly int ChargeSkill = Animator.StringToHash("ChargeSkill");

        public static readonly int BatStormAscend = Animator.StringToHash("BatStormAscend");
        public static readonly int ChargeStart = Animator.StringToHash("ChargeStart");
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
        public static readonly int StartQTE = Animator.StringToHash("StartQTE");
        public static readonly int SuccessQTE = Animator.StringToHash("SuccessQTE");
        public static readonly int EndQTE = Animator.StringToHash("EndQTE");

        // Int 파라미터
        public static readonly int NormalAttackCount = Animator.StringToHash("NormalAttackCount");
        public static readonly int AdditionalAttackID = Animator.StringToHash("AdditionalAttackID");
    }

    public static class ProjectileParameter
    {
        public static readonly int Triggered = Animator.StringToHash("Triggered");
    }

}

public static class LocalizationStrings
{
    public const string TableName = "LocalizationTable";
    
    public static class Loading
    {
        public const string COMPLETE = "LOADING_COMPLETE";
        public const string LOADING = "LOADING_LOADING";
        public const string PREPARINGRESOURCES = "LOADING_PREPARINGRESOURCES";
        public const string SCENEDATA = "LOADING_SCENEDATA";
        public const string CLEANINGUP = "LOADING_CLEANINGUP";
        public const string SAVING = "LOADING_SAVING";
        public const string SOUNDS = "LOADING_SOUNDS";
    }

    public static class Store
    {
        public const string PURCHASECONFIRM = "STORE_PURCHASECONFIRM";
    }

    public static class Skill
    {
        public const string NAME = "SKILL_NAME";
        public const string DAMAGE = "SKILL_DAMAGE";
        public const string COST = "SKILL_COST";
        public const string COOLDOWN = "SKILL_COOLDOWN";
    }

    public static class LinkAccount
    {
        public const string TITLE_SUCCESS = "LINKACCOUNT_TITLE_SUCCESS";
        public const string MESSAGE_SUCCESS = "LINKACCOUNT_MESSAGE_SUCCESS";
        
        public const string TITLE_FAIL = "LINKACCOUNT_TITLE_FAIL";
        public const string MESSAGE_FAIL_INVALID = "LINKACCOUNT_MESSAGE_FAIL_INVALID";
        public const string MESSAGE_FAIL_EXIST = "LINKACCOUNT_MESSAGE_FAIL_EXIST";
        public const string MESSAGE_FAIL_SERVER = "LINKACCOUNT_MESSAGE_FAIL_SERVER";
        public const string MESSAGE_FAIL_NETWORK = "LINKACCOUNT_MESSAGE_FAIL_NETWORK";
        public const string MESSAGE_FAIL_UNKNOWN = "LINKACCOUNT_MESSAGE_FAIL_UNKNOWN";
        
  }

    public static class SaveLoad
    {
        public const string SAVELOAD = "SAVELOAD_SAVELOAD";
        public const string MESSAGE_DELETE = "SAVELOAD_MESSAGE_DELETE";
        public const string MESSAGE_FAIL_DELETE = "SAVELOAD_MESSAGE_FAIL_DELETE";
        public const string MESSAGE_GUESTWARNING = "SAVELOAD_MESSAGE_GUESTWARNING";
    }

    public static class UI
    {
        public const string CANCEL = "UI_CANCEL";
        public const string OK = "UI_OK";
        public const string QUIT = "UI_QUIT";
        public const string BACKTOLOBBY = "UI_BACKTOLOBBY";
        public const string DELETE = "UI_DELETE";
        public const string WARNING = "UI_WARNING";
        public const string ERROR = "UI_ERROR";
        public const string EXIT = "UI_EXIT";
    }
}
