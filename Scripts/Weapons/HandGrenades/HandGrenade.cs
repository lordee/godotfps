using Godot;
using System;
using System.Collections.Generic;

abstract public class HandGrenade : Projectile
{
    protected float _activeTime = 0;
    private float _maxPrimedTime = 3.0f;
    protected float _inflictLength = 0;
    private bool _thrown = false;
    public bool Thrown { get { return _thrown; }}
    // used by nail grenade to indicate if it should be getting affected by gravity, shooting nails etc
    protected int _stage = 1;
    public int Stage { get { return _stage; }}

    protected WEAPON _grenadeType;
    public WEAPON GrenadeType { get { return _grenadeType; }}

    public HandGrenade()
    {
        
    }

    public override void Init(Player shooter, Vector3 vel, WEAPON weapon, Game game)
    {
        _particleResource = "res://Scenes/Weapons/RocketExplosion.tscn";
        base.Init(shooter, vel, weapon, game);
        
        _areaOfEffectRadius = 5f;
        _moveType = MOVETYPE.BOUNCE;
        _areaOfEffect = true;
        _speed = 60;
        _verticalSpeed = 20;
    }

    public override void _PhysicsProcess(float delta)
    {
        _activeTime += delta;
        StageTwoPhysicsProcess(delta);
       
        // after 3 seconds, explode
        if (_activeTime > _maxPrimedTime)
        {
            if (_thrown)
            {
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
        this.Transform = PlayerOwner.Head.GlobalTransform;
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
        p.Transform = this.GlobalTransform;
        this.GetParent().AddChild(p);
        p.Emitting = true;

        if (_playerOwner.PrimingGren == this)
        {
            _playerOwner.PrimingGren = null;
        }
        _game.World.ProjectileManager.Projectiles.Remove(this);
        GetTree().QueueDelete(this);
    }
}