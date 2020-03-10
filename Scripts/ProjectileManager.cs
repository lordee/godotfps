using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProjectileManager : Node
{
    private List<Rocket> _projectiles = new List<Rocket>();
    public List<Rocket> Projectiles { get { return _projectiles; }}
    HashSet<Rocket> _remove = new HashSet<Rocket>();
    private World _world;

    PackedScene _rocketScene;

    public override void _Ready()
    {
        _world = GetNode("/root/Initial/World") as World;
        _rocketScene = ResourceLoader.Load("res://Scenes/Weapons/Rocket.tscn") as PackedScene;
    }

    public void AddProjectile(Player shooter, Vector3 dest)
    {
        Rocket proj = _rocketScene.Instance() as Rocket;
        _projectiles.Add(proj);
        this.AddChild(proj);
        Vector3 vel = dest.Normalized() * proj.Speed;
        proj.Init(shooter, vel);
    }

    public void AddNetworkedProjectile(int stateNum, string name, string pID, Vector3 org, Vector3 vel, Vector3 rot)
    {
        Rocket proj = _projectiles.Where(p => p.Name == name).SingleOrDefault();
        
        if (proj == null)
        {
            Player shooter = GetNode("/root/Initial/World/" + pID) as Player;
            if (shooter != null)
            {
                proj = _rocketScene.Instance() as Rocket;
                _projectiles.Add(proj);
                this.AddChild(proj);
                proj.Init(shooter, vel);
            }
            else
            {
                return;
            }
        }
        else
        {
            proj.SetServerState(stateNum, org, vel, rot);
            Transform t = proj.GlobalTransform;
                
            t.origin = org;
            proj.GlobalTransform = t;
            proj.Rotation = rot;
            proj.Velocity = vel;
        }
    }

    public void ProcessProjectiles(float delta)
    {
        foreach (Rocket proj in _projectiles)
        {
            State predictedState = proj.ServerState;
            predictedState = ProcessMovement(predictedState, delta, proj);

            if (IsNetworkMaster())
            {
                proj.SetServerState(predictedState.StateNum, predictedState.Origin, predictedState.Velocity, proj.Rotation);
            }
        }
        _projectiles.RemoveAll(p => _remove.Contains(p));
        _remove.Clear();
    }

    private State ProcessMovement(State newState, float delta, Rocket proj)
    {
        Vector3 motion = proj.Velocity * delta;
        KinematicCollision c = proj.MoveAndCollide(motion);
        if (c != null)
        {
            Random ran = new Random();
            float damage = proj.Damage + ran.Next(0,20);
            // if c collider is kinematic body (direct hit)
            if (c.Collider is Player pl)
            {
                //pl.TakeDamage(this.Transform, _weaponOwnerString, _weaponOwnerInflictLength, _playerOwner, damage);
                proj.Explode(pl, damage);
            }
            else {
                proj.Explode(null, damage);
            }
            _remove.Add(proj);
        }

        proj.PredictedState = new State {
            StateNum = proj.PredictedState.StateNum + 1,
            Origin = proj.GlobalTransform.origin,
            Velocity = proj.Velocity
        };

        return proj.PredictedState;
    }
}
