using Godot;
using System;
using System.Collections.Generic;



public class MIRVGrenade : HandGrenade
{
    public static string ProjectileResource = "res://Scenes/Weapons/HandGrenades/MIRVGrenade.tscn";
    private static string MIRVResource = "res://Scenes/Weapons/Grenade.tscn";
    private static int _MIRVCount = 4;
    List<Projectile> mirvs = new List<Projectile>();
    

    public MIRVGrenade()
    {
    }

    public override void Init(Player shooter, Vector3 vel, WEAPONTYPE weapon, Game game)
    {
        base.Init(shooter, vel, weapon, game);
        _damage = 30;
        _grenadeType = WEAPONTYPE.MIRV;
    }

    override protected void PrimeTimeFinished()
    {
        // spawn child grenades
        Random ran = new Random();
        Vector3 dir = new Vector3(ran.Next(150), ran.Next(150), ran.Next(150));
        for (int i = 0; i < _MIRVCount; i++)
        {
            string projName = _game.World.ProjectileManager.AddProjectile(_playerOwner, dir, "", WEAPONTYPE.MIRVCHILD);
            Projectile p = _game.World.ProjectileManager.Projectiles.Find(e => e.Name == projName);
            mirvs.Add(p);
            p.GlobalTransform = this.GlobalTransform;
        }
        
        // make sure they don't just bounce on each other and float in air
        foreach (Projectile m in mirvs)
        {
            foreach (Projectile m2 in mirvs)
            {
                if (m != m2)
                {
                    m.AddCollisionExceptionWith(m2);
                }
            }
        }

        // now have original mirv explode
        this.Explode(null, _damage);
        this.FinishExplode();
    }
}