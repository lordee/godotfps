using Godot;
using System;
using System.Collections.Generic;

public class Napalm : HandGrenade
{
    private float _burnLength = 5;
    public static float BurnDamage = 15;
    
    public override void _Ready()
    {
        _grenadeType = WEAPONTYPE.NAPALM;
    }

    public override void Debuff()
    {
        foreach (KeyValuePair<Player, float> kvp in _explodedPlayers)
        {
            kvp.Key.TakeDamage(_playerOwner, this.GlobalTransform.origin, BurnDamage);

            kvp.Key.AddDebuff(_playerOwner, _grenadeType, _burnLength);
        }
    }
}
