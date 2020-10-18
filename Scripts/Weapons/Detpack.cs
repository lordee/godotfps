using Godot;
using System;

public class Detpack : Projectile
{
    public AudioStreamPlayer3D SetSound;
    public AudioStreamPlayer3D WarnSound;
    public bool WarnSoundPlayed = false;

    public override void _Ready()
    {
        SetSound = GetNode("SetSound") as AudioStreamPlayer3D;
        WarnSound = GetNode("WarnSound") as AudioStreamPlayer3D;
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        _particleResource = "res://Scenes/Weapons/RocketExplosion.tscn";
        _areaOfEffect = true;
        
        base.Init(shooter, vel, weapon, game);
        _areaOfEffectRadius = 500;
        _damage = 500;
        
        _moveType = MOVETYPE.NONE;
        _maxLifeTime = 256f;

        game.World.MoveToFloor(this);
    }
}
