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
    private Network _network;

    PackedScene _rocketScene;

    public override void _Ready()
    {
        _world = GetNode("/root/Initial/World") as World;
        _rocketScene = ResourceLoader.Load("res://Scenes/Weapons/Rocket.tscn") as PackedScene;
        _network = GetNode("/root/Initial/Network") as Network;
    }

    public override void _PhysicsProcess(float delta)
    {
        foreach (Rocket proj in _projectiles)
        {
            ProcessProjectile(proj, delta);
        }
        _projectiles.RemoveAll(p => _remove.Contains(p));
        _remove.Clear();
    }

    // when a player attacks
    public void AddProjectile(Player shooter, Vector3 dest)
    {
        Rocket proj = _rocketScene.Instance() as Rocket;
        _projectiles.Add(proj);
        this.AddChild(proj);
        Vector3 vel = dest.Normalized() * proj.Speed;
        Peer p = _network.PeerList.Where(e => e.ID == shooter.ID).SingleOrDefault();
        float ping = 0f;
        if (p != null)
        {
            ping = IsNetworkMaster() ? p.Ping : 0f;
        }
        
        proj.Init(shooter, vel);
        //ProcessProjectile(proj, ping);
    }

    public void AddNetworkedProjectile(string name, string pID, Vector3 org, Vector3 vel, Vector3 rot)
    {
        Rocket proj = _projectiles.Where(p => p.Name == name).SingleOrDefault();
        
        if (proj == null)
        {
            Peer p = _network.PeerList.Where(e => e.ID == Convert.ToInt64(pID)).SingleOrDefault();
            if (p != null)
            {
                proj = _rocketScene.Instance() as Rocket;
                _projectiles.Add(proj);
                this.AddChild(proj);
                float ping = IsNetworkMaster() ? 0 : p.Ping;
                proj.Init(p.Player, vel);
                //ProcessProjectile(proj, ping);
            }
            else
            {
                return;
            }
        }
        else // apply update to existing projectile
        {
            proj.SetServerState(org, vel, rot);
            Transform t = proj.GlobalTransform;
                
            t.origin = org;
            proj.GlobalTransform = t;
            proj.Rotation = rot;
            proj.Velocity = vel;
        }
    }

    public void ProcessProjectile(Rocket proj, float delta)
    {
        State predictedState = proj.ServerState;
        predictedState = ProcessMovement(predictedState, delta, proj);

        /*if (IsNetworkMaster())
        {
            proj.SetServerState(predictedState.StateNum, predictedState.Origin, predictedState.Velocity, proj.Rotation);
        }*/
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
                pl.TakeDamage(proj, damage);
                proj.Explode(pl, damage);
            }
            else {
                proj.Explode(null, damage);
            }
            _remove.Add(proj);
        }

        proj.PredictedState = new State {
            StateNum = newState.StateNum + 1,
            Origin = proj.GlobalTransform.origin,
            Velocity = proj.Velocity
        };

        return proj.PredictedState;
    }
}
