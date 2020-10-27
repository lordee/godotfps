using Godot;
using System;

public class Demoman
{
    static public int Health = 100;
    static public int Armour = 100;
    static public int Shells = 50;
    static public int Nails = 50;
    static public int Rockets = 50;
    static public int Cells = 50;
    static public int MaxGren1 = 4;
    static public int MaxGren2 = 1;
    static public int MoveSpeed = 28;
    static public WEAPONTYPE Gren1Type = WEAPONTYPE.FRAG;
    static public WEAPONTYPE Gren2Type = WEAPONTYPE.MIRV;
    static public WEAPONTYPE Weapon1 = WEAPONTYPE.GRENADELAUNCHER;
    static public WEAPONTYPE Weapon2 = WEAPONTYPE.PIPEBOMBLAUNCHER;
    static public WEAPONTYPE Weapon3 = WEAPONTYPE.SHOTGUN;
    static public WEAPONTYPE Weapon4 = WEAPONTYPE.AXE;

    static public void Detpipe(Player p)
    {
        if (p.PlayerClass == PLAYERCLASS.DEMOMAN)
        {
            if (p.Weapon2.TimeSinceLastShot >= GAMESETTINGS.DETPIPE_DELAY)
            {
                foreach (Pipebomb pi in p.ActivePipebombs)
                {
                    pi.Explode(null, pi.Damage);
                }
                p.ActivePipebombs.Clear();
            }
        }
        else
        {
            Console.Log("You are not a demoman");
        }
    }

    static public void Detpack(Player p, int seconds, bool set)
    {
        if (set)
        {
            if (p.IsOnFloor())
            {
                Console.Log("Setting detpack");
                p.SettingDetpack = true;
                p.MoveType = MOVETYPE.NONE;
                // TODO - detpack setting anim (for now hide viewmodel)
                p.DetpackTimeSetting = seconds;
                p.SetDetpackTime = 0f;
                p.ActiveWeapon.WeaponMesh.Visible = false;
            }
            else
            {
                Console.Log("You must be on the ground to set a detpack!");
            }
        }
        else
        {
            Console.Log("Retrieving detpack");
            p.MoveType = MOVETYPE.NORMAL;
            p.SettingDetpack = false;
        }
    }
}