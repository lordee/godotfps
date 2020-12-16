using Godot;
using System;
using System.Collections.Generic;

abstract public class HandGrenade : Projectile
{
    protected float _activeTime = 0;
    protected float _maxPrimedTime = 3.0f;
    protected float _debuffLength = 0;
    private bool _thrown = false;
    public bool Thrown { get { return _thrown; }}
    // used by nail grenade to indicate if it should be getting affected by gravity, shooting nails etc
    protected int _stage = 1;
    public int Stage { get { return _stage; }}

    protected WEAPONTYPE _grenadeType;
    public WEAPONTYPE GrenadeType { get { return _grenadeType; }}

    // FIXME - this should be a weapon based class

    public HandGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        _particleResource = "res://Scenes/Weapons/RocketExplosion.tscn";
        base.Init(shooter, vel, weapon, game);
        
        _areaOfEffectRadius = 10f;
        _moveType = MOVETYPE.BOUNCE;
        _areaOfEffect = true;
        _speed = 40;
        _verticalSpeed = 20;
    }

    public override void _PhysicsProcess(float delta)
    {
        _activeTime += delta;
       
        // after 3 seconds, explode
        if (_activeTime > _maxPrimedTime)
        {
            if (_thrown)
            {
                if (_grenadeType == WEAPONTYPE.SHOCK)
                {
                    this.StageTwoPhysicsProcess(delta);
                    return;
                }
                this.PrimeTimeFinished();
            }
            else
            {
                this.Transform = this._playerOwner.GlobalTransform;
                _thrown = true;
            }
        }
    }

    virtual protected void StageTwoPhysicsProcess(float delta)
    {
    }

    public void Throw(Vector3 dir)
    {
        this.GlobalTransform = PlayerOwner.Head.GlobalTransform;
        Velocity = dir.Normalized();
        Velocity.x *= Speed;
        Velocity.z *= Speed;
        Velocity.y *= VerticalSpeed;
        _thrown = true;
        this.Visible = true;
    }

    virtual protected void PrimeTimeFinished()
    {
        this.Explode(null, _damage);
        this.FinishExplode();
    }

    protected void FinishExplode()
    {
        Particles p = (Particles)_particleScene.Instance();
        p.GlobalTransform = this.GlobalTransform;
        this.GetParent().AddChild(p);
        p.Emitting = true;

        if (_playerOwner.PrimingGren == this)
        {
            _playerOwner.PrimingGren = null;
        }

        this.Debuff();

        _game.World.ProjectileManager.Projectiles.Remove(this);
        GetTree().QueueDelete(this);
    }

    virtual public void Debuff()
    {
        
    }
}