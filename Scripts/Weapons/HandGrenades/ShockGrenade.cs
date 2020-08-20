using Godot;
using System;
using System.Collections.Generic;



public class ShockGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/HandGrenades/ShockGrenade.tscn";
    private float _shockDamage = 40;
    Area _shockArea;
    List<Player> _touchingPlayers = new List<Player>();

    public ShockGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 50;
        _grenadeType = WEAPONTYPE.SHOCK;
        _shockArea = GetNode("CSGMesh/Area") as Area;
        _shockArea.Connect("body_entered", this, "on_Shock_Entered");
        _shockArea.Connect("body_exited", this, "on_Shock_Exited");
    }

    protected override void StageTwoPhysicsProcess(float delta)
    {
        if (_activeTime < _maxPrimedTime * 2) // mer, for now we leave as another 3 seconds
        {
            if (_activeTime <= _maxPrimedTime + 1)
            {
                if (_stage != 2)
                {
                    _stage = 2;
                }
                Velocity.y = 2;
            }
            else if (_activeTime > _maxPrimedTime + 1)
            {
                if (_stage != 3)
                {
                    _stage = 3;
                }
                Velocity = new Vector3(0, 0, 0);
                foreach(Player p in _touchingPlayers)
                {
                    p.TakeDamage(_playerOwner, p.GlobalTransform.origin, _shockDamage);
                }
                this.RotateY(.2f);
            }
            return;
        }
        this.PrimeTimeFinished();
    }

    private void on_Shock_Entered(Player p)
    {
        if (_stage == 3)
        {
            _touchingPlayers.Add(p);
        }
    }

    private void on_Shock_Exited(Player p)
    {
        if (_stage == 3)
        {
            _touchingPlayers.Remove(p);
        }
    }
}