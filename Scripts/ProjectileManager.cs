using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProjectileManager : Node
{
    private List<Rocket> _projectiles = new List<Rocket>();
    HashSet<Rocket> _remove = new HashSet<Rocket>();
    private World _world;

    public override void _Ready()
    {
        _world = GetNode("/root/Initial/World") as World;
    }

    public void AddProjectile(Rocket proj, Player shooter, Vector3 dest)
    {
        _projectiles.Add(proj);
        this.AddChild(proj);
        proj.Init(shooter, dest);
    }

    public void ProcessProjectiles(float delta)
    {
        foreach (Rocket proj in _projectiles)
        {
            Vector3 vel = proj.Direction * proj.Speed;
            Vector3 motion = vel * delta;
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
        }
        _projectiles.RemoveAll(p => _remove.Contains(p));
        _remove.Clear();
    }
}
