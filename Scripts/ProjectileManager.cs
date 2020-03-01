using Godot;
using System;
using System.Collections.Generic;

public class ProjectileManager : Node
{
    private List<Rocket> projectiles = new List<Rocket>();

    public override void _Ready()
    {
        
    }

    public void AddProjectile(Rocket proj, Player shooter, Vector3 dest)
    {
        proj.Init(shooter, dest);

        projectiles.Add(proj);
        this.AddChild(proj);

    }

    public void ProcessProjectiles(float delta)
    {
        foreach (Rocket proj in projectiles)
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
                projectiles.Remove(proj);
            }
        }
    }


}
