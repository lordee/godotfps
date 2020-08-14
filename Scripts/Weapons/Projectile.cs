using Godot;
using System;
using System.Collections.Generic;

public class Projectile : KinematicBody
{
    protected Vector3 _direction = new Vector3();
    protected Vector3 _up = new Vector3(0,1,0);
    protected float _speed;
    public float Speed { 
        get { return _speed; }
        set { _speed = value; }
    }
    protected float _verticalSpeed;
    public float VerticalSpeed { 
        get { return _verticalSpeed; }
        set { _verticalSpeed = value; }
    }
    protected float _damage = 0;
    public float Damage { get { return _damage; }}

    protected string _particleResource;
    protected PackedScene _particleScene;
    protected bool _areaOfEffect;
    protected float _areaOfEffectRadius;
    protected Player _playerOwner;
    public Player PlayerOwner { get { return _playerOwner; }}

    protected Game _game;
    private State _serverState;
    public State ServerState { get { return _serverState; }}
    public State PredictedState;
    public Vector3 Velocity = new Vector3();

    public WEAPONTYPE Weapon;
    protected MOVETYPE _moveType;
    public MOVETYPE MoveType { get { return _moveType; }}

    protected Dictionary<Player, float> _explodedPlayers = new Dictionary<Player, float>();

    public override void _Ready()
    {
        //_game = GetTree().Root.GetNode("Game") as Game;
    }

    public Projectile()
    {
    }

    virtual public void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {   
        _game = game;
        this.AddCollisionExceptionWith(shooter);
        this.GlobalTransform = shooter.Head.GlobalTransform;
        Velocity = vel;
        this.LookAt(vel * 1000, _game.World.Up);
        _playerOwner = shooter;
        _moveType = MOVETYPE.FLY;
        Weapon = weapon;
        switch (weapon)
        {
            case WEAPONTYPE.NAILGUN:
                _damage = NailGun.Damage;
                _speed = NailGun.Speed;
                break;
        }

        _particleScene = (PackedScene)ResourceLoader.Load(_particleResource);
    }

    virtual public void Explode(Player ignore, float damage)
    {
        if (_areaOfEffect)
        {
            Godot.Collections.Array result = _game.World.FindRadius(this, _areaOfEffectRadius);
            foreach (Godot.Collections.Dictionary r in result) 
            {
                if (r["collider"] is Player pl)
                {
                    if (pl != ignore || ignore == null)
                    {
                        // find how far from explosion as a percentage, apply to damage
                        float dist = this.Transform.origin.DistanceTo(pl.Transform.origin);
                        dist = dist > _areaOfEffectRadius ? (_areaOfEffectRadius*.99f) : dist;
                        float pc = ((_areaOfEffectRadius - dist) / _areaOfEffectRadius);
                        float d = damage * pc;

                        // inflict damage
                        pl.TakeDamage(_playerOwner, this.GlobalTransform.origin, d);
                        _explodedPlayers.Add(pl, pc);
                    }
                }
            }
        }
        
        Particles p = (Particles)_particleScene.Instance();
        p.Transform = this.Transform;
        _game.World.ProjectileManager.AddChild(p);
        p.Emitting = true;
        
        // remove projectile
        GetTree().QueueDelete(this);
    }

    public void SetServerState(Vector3 org, Vector3 velo, Vector3 rot)
    {
        //_serverState.StateNum = stateNum;
        _serverState.Origin = org;
        _serverState.Velocity = velo;
        this.Rotation = rot;
    }
}