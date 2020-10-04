using Godot;
using System;

public class Pipebomb : Grenade
{
    public override void _Ready()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        
        _maxLifeTime = 120f;
    }
}