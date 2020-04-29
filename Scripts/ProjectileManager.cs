using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProjectileManager : Node
{
    private List<Rocket> _projectiles = new List<Rocket>();
    public List<Rocket> Projectiles { get { return _projectiles; }}
    HashSet<Rocket> _remove = new HashSet<Rocket>();
    private Game _game;

    PackedScene _rocketScene;

    public override void _Ready()
    {
        _game = GetTree().Root.GetNode("Game") as Game;
        _rocketScene = ResourceLoader.Load("res://Scenes/Weapons/Rocket.tscn") as PackedScene;
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
    public string AddProjectile(Player shooter, Vector3 dest, string projName)
    {
        Rocket proj = _rocketScene.Instance() as Rocket;
        _projectiles.Add(proj);
        this.AddChild(proj);
        Vector3 vel = dest.Normalized() * proj.Speed;
        Peer p = _game.Network.PeerList.Where(e => e.ID == shooter.ID).SingleOrDefault();
        float ping = 0f;
        if (p != null)
        {
            ping = IsNetworkMaster() ? p.Ping : 0f;
        }
        
        proj.Init(shooter, vel);
        proj.Name = projName.Length > 0 ? projName : shooter.ID + "!" + proj.Name;
        
        // godot inserts @ signs and numbers to non unique names that can happen due to sync issues
        // it also currently doesn't allow you to manually insert them, so server sets wrong name! and sends back wrong name!
        // great architecture!

        // if this is too slow we need to track unique projectile count on client and set that as name, but i worry about uniqueness still
        proj.Name = proj.Name.Replace("@", "");

        //ProcessProjectile(proj, ping);
        return proj.Name;
    }

    public void AddNetworkedProjectile(string name, string pID, Vector3 org, Vector3 vel, Vector3 rot)
    {
        Rocket proj = _projectiles.Where(p => p.Name.Replace("@", "") == name).SingleOrDefault();
        
        if (proj == null)
        {
            Peer p = _game.Network.PeerList.Where(e => e.ID == Convert.ToInt64(pID)).SingleOrDefault();
            if (p != null)
            {
                proj = _rocketScene.Instance() as Rocket;
                proj.Name = name;
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
