using Godot;
using System;

public class Rocket : KinematicBody
{
    string _particleResource = "res://Scenes/Weapons/RocketExplosion.tscn";

    public float Speed = 90;
    public float Damage = 100;
    private float _areaOfEffectRadius = 0;
    PackedScene _particleScene;
    private World _world;

    public Vector3 Velocity = new Vector3();

    private Player _playerOwner;
    public Player PlayerOwner { get { return _playerOwner; }}
    private State _serverState;
    public State ServerState { get { return _serverState; }}

    public State PredictedState;

    public override void _Ready()
    {
        _world = GetNode("/root/Initial/World") as World;
        _particleScene = ResourceLoader.Load(_particleResource) as PackedScene;
        _areaOfEffectRadius = Damage / 10;
    }

    public void Init(Player shooter, Vector3 vel)
    {
        this.AddCollisionExceptionWith(shooter);
        this.GlobalTransform = shooter.GlobalTransform;
        Velocity = vel;
        this.LookAt(vel * 1000, _world.Up);
        _playerOwner = shooter;
    }

    public void Explode(Player ignore, float damage)
    {
        this.FindRadius(ignore, damage);
        
        Particles p = (Particles)_particleScene.Instance();
        p.Transform = this.Transform;
        _world.AddChild(p);
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
                    pl.TakeDamage(this, d);
                }
            }
        }
    }
/*
    public void SetServerState(int stateNum, Vector3 org, Vector3 velo, Vector3 rot)
    {
        _serverState.StateNum = stateNum;
        _serverState.Origin = org;
        _serverState.Velocity = velo;
        this.Rotation = rot;
    }
    */
}
