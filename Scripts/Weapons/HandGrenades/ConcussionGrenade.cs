using Godot;
using System;
using System.Collections.Generic;

public class ConcussionGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/ConcussionGrenade.tscn";
    private float _blastPower = 30;

    public ConcussionGrenade()
    {
        _damage = 0;
        _inflictLength = 10;
        _grenadeType = WEAPON.CONCUSSION;
    }

    override protected void PrimeTimeFinished()
    {
        // when lifetime ends, call explode
        this.Explode(null, _blastPower);
    }

    override public void Explode(Player ignore, float damage)
    {
        Godot.Collections.Array result = _game.World.FindRadius(this, damage);

        foreach (Godot.Collections.Dictionary  r in result) {
            if (r["collider"] is Player pl)
            {
                // find how far from explosion as a percentage
                float dist = this.Transform.origin.DistanceTo(pl.Transform.origin);
                dist = dist > this._areaOfEffectRadius ? (this._areaOfEffectRadius*.99f) : dist;
                float pc = ((this._areaOfEffectRadius - dist) / this._areaOfEffectRadius);

                //pl.Inflict("concussiongrenade", _inflictLength, _playerOwner);
                //pl.AddVelocity(this.Transform.origin, val * (1 - pc));
            }
        }
        
        this.FinishExplode();
    }
}