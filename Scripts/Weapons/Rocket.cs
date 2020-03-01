using Godot;
using System;

public class Rocket : KinematicBody
{
    string _particleResource = "res://Scenes/Weapons/RocketExplosion.tscn";

    public float Speed = 90;
    public float Damage = 100;
    private float _areaOfEffectRadius = 0;
    public Vector3 Direction = new Vector3();
    PackedScene _particleScene;

    private Player _playerOwner;

    public override void _Ready()
    {
        _particleScene = ResourceLoader.Load(_particleResource) as PackedScene;
        _areaOfEffectRadius = Damage / 10;
    }

    public void Init(Player shooter, Vector3 dest)
    {
        this.AddCollisionExceptionWith(shooter);
        this.GlobalTransform = shooter.GlobalTransform;
        this.Direction = dest.Normalized();
        _playerOwner = shooter;
    }

    public void Explode(Player ignore, float damage)
    {
        this.FindRadius(ignore, damage);
        
        Particles p = (Particles)_particleScene.Instance();
        p.Transform = this.Transform;
        GetNode("/root/Initial/World").AddChild(p);
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
                GD.Print("found player");
                if (pl != ignore || ignore == null)
                {
                    // find how far from explosion as a percentage, apply to damage
                    float dist = this.Transform.origin.DistanceTo(pl.Transform.origin);
                    dist = dist > this._areaOfEffectRadius ? (this._areaOfEffectRadius*.99f) : dist;
                    float pc = ((this._areaOfEffectRadius - dist) / this._areaOfEffectRadius);
                    float d = damage * pc;
                    GD.Print("dam: " + damage);
                    GD.Print("pc: " + pc);
                    GD.Print("dist: " + dist);
                    GD.Print("aoe: " + this._areaOfEffectRadius);
                    GD.Print(this._playerOwner.Name);
                    GD.Print(pl.Name);
                    if (pl == this._playerOwner)
                    {
                        d = d * 0.5f;
                    }
                    GD.Print("inflicted dam: " + d);
                    // inflict damage
                    //pl.TakeDamage(this.Transform, _weaponOwnerString, _weaponOwnerInflictLength, this._playerOwner, d);
                }
            }
        }
    }
}
