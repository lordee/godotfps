using Godot;
using System;
using System.Collections.Generic;

public class ConcussionGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/ConcussionGrenade.tscn";
    private float _blastPower = 100;

    public ConcussionGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPON weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 0;
        _inflictLength = 10;
        _grenadeType = WEAPON.CONCUSSION;
        _areaOfEffectRadius = 10;
    }

    override protected void PrimeTimeFinished()
    {
        // when lifetime ends, call explode
        this.Explode(null, _blastPower);
    }

    override public void Explode(Player ignore, float pow)
    {
        Godot.Collections.Array result = _game.World.FindRadius(this, _areaOfEffectRadius);
        foreach (Godot.Collections.Dictionary r in result) {
            if (r["collider"] is Player pl)
            {
                // find how far from explosion as a percentage
                float dist = this.Transform.origin.DistanceTo(pl.Transform.origin);
                dist = dist > this._areaOfEffectRadius ? (this._areaOfEffectRadius*.99f) : dist;
                float pc = ((this._areaOfEffectRadius - dist) / this._areaOfEffectRadius);
                pl.Inflict(WEAPON.CONCUSSION, _inflictLength, _playerOwner);
                pl.AddVelocity(this.GlobalTransform.origin, pow * (1 - pc));
            }
        }
        
        this.FinishExplode();
    }
}