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
        _debuffLength = 10;
        _grenadeType = WEAPONTYPE.CONCUSSION;
    }

    public override void Debuff()
    {
        foreach (KeyValuePair<Player, float> kvp in _explodedPlayers)
        {
            float debuffTime = _debuffLength * kvp.Value;
            if (_grenadeType == WEAPONTYPE.CONCUSSION)
            {
                float dist = this.Transform.origin.DistanceTo(kvp.Key.Transform.origin);
                dist = dist > this._areaOfEffectRadius ? (this._areaOfEffectRadius*.99f) : dist;
                float pc = ((this._areaOfEffectRadius - dist) / this._areaOfEffectRadius);
                kvp.Key.AddVelocity(this.GlobalTransform.origin, ConcussionGrenade.BlastPower * (1 - pc));
                debuffTime = _debuffLength;
            }
            kvp.Key.AddDebuff(_playerOwner, _grenadeType, debuffTime);
        }
    }
}