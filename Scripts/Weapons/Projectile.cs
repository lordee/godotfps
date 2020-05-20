using Godot;
using System;
using System.Collections.Generic;

public class Projectile : KinematicBody
{
    protected Vector3 _direction = new Vector3();
    protected Vector3 _up = new Vector3(0,1,0);
    protected float _speed;
    public float Speed { get { return _speed; }}
    protected float _damage;
    public float Damage { get { return _damage; }}

    protected string _particleResource;
    protected PackedScene _particleScene;
    protected bool _areaOfEffect;
    protected float _areaOfEffectRadius;
    protected Player _playerOwner;
    public Player PlayerOwner { get { return _playerOwner; }}

    private Game _game;
    private State _serverState;
    public State ServerState { get { return _serverState; }}
    public State PredictedState;
    public Vector3 Velocity = new Vector3();

    public WEAPON Weapon;


    public override void _Ready()
    {
        _game = GetTree().Root.GetNode("Game") as Game;
    }

    public Projectile()
    {
    }

    public void Init(Player shooter, Vector3 vel, WEAPON weapon, Game game)
    {   
        // FIXME - ready is not being called before init in addprojectile, so game var not set
        _game = game;
        this.AddCollisionExceptionWith(shooter);
        this.GlobalTransform = shooter.Head.GlobalTransform;
        Velocity = vel;
        this.LookAt(vel * 1000, _game.World.Up);
        _playerOwner = shooter;
        switch (weapon)
        {
            case WEAPON.NAILGUN:
                _damage = NailGun.Damage;
                _speed = NailGun.Speed;
                break;
        }

        _particleScene = (PackedScene)ResourceLoader.Load(_particleResource);
    }

    public void Explode(Player ignore, float damage)
    {
        if (_areaOfEffect)
        {
            this.FindRadius(ignore, damage);
        }
        
        Particles p = (Particles)_particleScene.Instance();
        p.Transform = this.Transform;
        _game.World.ProjectileManager.AddChild(p);
        p.Emitting = true;
        
        // remove projectile
        GetTree().QueueDelete(this);
    }

    protected void FindRadius(Player ignore, float damage)
    {
        // test for radius damage
        SphereShape s = new SphereShape();
        s.Radius = _areaOfEffectRadius;

        // Get space and state of the subject body
        RID space = PhysicsServer.BodyGetSpace(this.GetRid());
        PhysicsDirectSpaceState state = PhysicsServer.SpaceGetDirectState(space);

        // Setup shape query parameters
        PhysicsShapeQueryParameters par = new PhysicsShapeQueryParameters();
        par.ShapeRid = s.GetRid();
        par.Transform = this.Transform;
        
        Godot.Collections.Array result = state.IntersectShape(par);
        foreach (Godot.Collections.Dictionary r in result) {
            if (r["collider"] is Player pl)
            {
                if (pl != ignore || ignore == null)
                {
                    // find how far from explosion as a percentage, apply to damage
                    float dist = this.Transform.origin.DistanceTo(pl.Transform.origin);
                    dist = dist > this._areaOfEffectRadius ? (this._areaOfEffectRadius*.99f) : dist;
                    float pc = ((this._areaOfEffectRadius - dist) / this._areaOfEffectRadius);
                    float d = damage * pc;

                    // inflict damage
                    pl.TakeDamage(_playerOwner, this.GlobalTransform.origin, d);
                }
            }
        }
    }

    public void SetServerState(Vector3 org, Vector3 velo, Vector3 rot)
    {
        //_serverState.StateNum = stateNum;
        _serverState.Origin = org;
        _serverState.Velocity = velo;
        this.Rotation = rot;
    }
}