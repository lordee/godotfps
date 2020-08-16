using Godot;
using System;
using System.Collections.Generic;



public class ShockGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/HandGrenades/ShockGrenade.tscn";
    private float _shockDamage = 10;

    public ShockGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 50;
        _grenadeType = WEAPONTYPE.SHOCK;
    }

    protected override void StageTwoPhysicsProcess(float delta)
    {
        if (_activeTime < _maxPrimedTime * 2) // mer, for now we leave as another 3 seconds
        {
            if (_activeTime <= _maxPrimedTime + 1)
            {
                if (_stage != 2)
                {
                    _stage = 2;
                }
                Velocity.y = 2;
            }
            else if (_activeTime > _maxPrimedTime + 1)
            {
                if (_stage != 3)
                {
                    _stage = 3;
                }
                Velocity = new Vector3(0, 0, 0);
                
            }
            return;
        }
    }
}