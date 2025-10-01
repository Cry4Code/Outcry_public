using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract 클래스라고 생각하고 new로 생성하지 말 것.
/// </summary>
[Serializable]
public class MonsterModelBase
{
    public int monsterId;
    public string monsterName;
    public int health;
    public float chaseSpeed;  
    public float approachRange;
    public float detectRange;

    public MonsterModelBase(int monsterId, string monsterName, int health, float chaseSpeed, float approachRange, float detectRange)
    {
        this.monsterId = monsterId;
        this.monsterName = monsterName;
        this.health = health;
        this.chaseSpeed = chaseSpeed;
        this.approachRange = approachRange;
        this.detectRange = detectRange;
    }

    public MonsterModelBase(MonsterModelBase other)
    {
        this.monsterId = other.monsterId;
        this.monsterName = other.monsterName;
        this.health = other.health;
        this.chaseSpeed = other.chaseSpeed;
        this.approachRange = other.approachRange;
        this.detectRange = other.detectRange;
    }
}