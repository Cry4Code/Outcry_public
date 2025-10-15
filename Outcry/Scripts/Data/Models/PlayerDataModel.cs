using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDataModel
{
    public int maxHealth;
    public int maxStamina;
    public float rateStamina;
    public float fullStamina;
    public int specialAttackStamina;
    public int specialAttackDamage;
    public int justSpecialAttackDamage;
    public int dodgeStamina;
    public float dodgeInvincibleTime;
    public float dodgeDistance;
    public int parryStamina;
    public float parryInvincibleTime;
    public int parryDamage;
    public float invincibleTime;
    public int[] normalAttackDamage;
    public int jumpAttackDamage;
    public int downAttackDamage;
    public float jumpforce;
    public float doubleJumpForce;
    public int skill_Ids;
    public float moveSpeed;

    public PlayerDataModel(
        int  maxHealth, int  maxStamina, float rateStamina, float fullStamina, int specialAttackStamina, int specialAttackDamage, int justSpecialAttackDamage, int dodgeStamina, float dodgeInvincibleTime, float dodgeDistance, int parryStamina,  float parryInvincibleTime,
        int parryDamage, float invincibleTime, int[] normalAttackDamage, int jumpAttackDamage, int downAttackDamage, float jumpforce, float doubleJumpForce, int skill_Ids, float moveSpeed
        )
    {
        this.maxHealth = maxHealth;
        this.maxStamina = maxStamina;
        this.rateStamina = rateStamina;
        this.fullStamina = fullStamina;
        this.specialAttackStamina = specialAttackStamina;
        this.specialAttackDamage = specialAttackDamage;
        this.justSpecialAttackDamage = justSpecialAttackDamage;
        this.dodgeStamina = dodgeStamina;
        this.dodgeInvincibleTime = dodgeInvincibleTime;
        this.dodgeDistance = dodgeDistance;
        this.parryStamina = parryStamina;
        this.parryInvincibleTime = parryInvincibleTime;
        this.parryDamage = parryDamage;
        this.invincibleTime = invincibleTime;
        this.normalAttackDamage = normalAttackDamage;
        this.jumpAttackDamage = jumpAttackDamage;
        this.downAttackDamage = downAttackDamage;
        this.jumpforce = jumpforce;
        this.doubleJumpForce = doubleJumpForce;
        this.skill_Ids = skill_Ids;
        this.moveSpeed = moveSpeed;
    }
}
