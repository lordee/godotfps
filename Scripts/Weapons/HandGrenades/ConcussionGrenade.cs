using Godot;
using System;
using System.Collections.Generic;

public class ConcussionGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/ConcussionGrenade.tscn";
    public static float BlastPower = 100;

    public ConcussionGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 0;
        _inflictLength = 10;
        _grenadeType = WEAPONTYPE.CONCUSSION;
    }
}