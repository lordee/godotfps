using Godot;
using System;

public class Grenade : Projectile
{
    public override void _Ready()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        _particleResource = "res://Scenes/Weapons/RocketExplosion.tscn";
        _areaOfEffect = true;
        _areaOfEffectRadius = 5;
        base.Init(shooter, vel, weapon, game);
        
        _moveType = MOVETYPE.BOUNCE;
        _maxLifeTime = 2.5f;
    }
}
