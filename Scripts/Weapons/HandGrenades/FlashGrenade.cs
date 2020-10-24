using Godot;
using System;
using System.Collections.Generic;

public class FlashGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/FlashGrenade.tscn";

    public FlashGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 10;
        _debuffLength = 5;
        _grenadeType = WEAPONTYPE.FLASH;
    }
}