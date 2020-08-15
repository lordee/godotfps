using Godot;
using System;
using System.Collections.Generic;



public class FragGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/HandGrenades/FragGrenade.tscn";

    public FragGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 100;
        _grenadeType = WEAPONTYPE.FRAG;
    }
}