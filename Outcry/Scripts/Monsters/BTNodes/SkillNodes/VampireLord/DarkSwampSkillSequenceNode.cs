using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkSwampSkillSequenceNode : SkillSequenceNode
{
    private bool isAnimationStarted = false;
    private int projectileLaunched = 0;

    private string projectilePath = AddressablePaths.Projectile.TurningBlood;
    private Vector2 projectilePosition = new Vector2(1, 0);


    public DarkSwampSkillSequenceNode(int skillId) : base(skillId)
    {
        this.nodeName = "BloodStingSkillSequenceNode";
    }
    
    public override void InitializeSkillSequenceNode(MonsterBase monster, PlayerController target)
    {
        base.InitializeSkillSequenceNode(monster, target);
    }

    protected override bool CanPerform()
    {
        throw new System.NotImplementedException();
    }

    protected override NodeState SkillAction()
    {
        throw new System.NotImplementedException();
    }
}
