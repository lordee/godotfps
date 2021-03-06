using Godot;
using System;

public class Scout
{
    static public int Health = 100;
    static public int Armour = 25;
    static public int Shells = 50;
    static public int Nails = 200;
    static public int Rockets = 50;
    static public int Cells = 100;
    static public int MaxGren1 = 2;
    static public int MaxGren2 = 3;
    static public int MoveSpeed = 45;
    static public WEAPONTYPE Gren1Type = WEAPONTYPE.FLASH;
    static public WEAPONTYPE Gren2Type = WEAPONTYPE.CONCUSSION;
    static public WEAPONTYPE Weapon1 = WEAPONTYPE.NAILGUN;
    static public WEAPONTYPE Weapon2 = WEAPONTYPE.SHOTGUN;
    static public WEAPONTYPE Weapon3 = WEAPONTYPE.AXE;
    static public WEAPONTYPE Weapon4 = WEAPONTYPE.NONE;
}

public class Soldier
{
    static public int Health = 100;
    static public int Armour = 200;
    static public int Shells = 100;
    static public int Nails = 100;
    static public int Rockets = 50;
    static public int Cells = 50;
    static public int MaxGren1 = 4;
    static public int MaxGren2 = 1;
    static public int MoveSpeed = 24;
    static public WEAPONTYPE Gren1Type = WEAPONTYPE.FRAG;
    static public WEAPONTYPE Gren2Type = WEAPONTYPE.SHOCK;
    static public WEAPONTYPE Weapon1 = WEAPONTYPE.ROCKETLAUNCHER;
    static public WEAPONTYPE Weapon2 = WEAPONTYPE.SUPERSHOTGUN;
    static public WEAPONTYPE Weapon3 = WEAPONTYPE.SHOTGUN;
    static public WEAPONTYPE Weapon4 = WEAPONTYPE.AXE;
}

public class Medic
{
    static public int Health = 100;
    static public int Armour = 80;
    static public int Shells = 50;
    static public int Nails = 150;
    static public int Rockets = 50;
    static public int Cells = 50;
    static public int MaxGren1 = 4;
    static public int MaxGren2 = 2;
    static public int MoveSpeed = 32;
    static public WEAPONTYPE Gren1Type = WEAPONTYPE.FRAG;
    static public WEAPONTYPE Gren2Type = WEAPONTYPE.CONCUSSION;
    static public WEAPONTYPE Weapon1 = WEAPONTYPE.SUPERNAILGUN;
    static public WEAPONTYPE Weapon2 = WEAPONTYPE.SUPERSHOTGUN;
    static public WEAPONTYPE Weapon3 = WEAPONTYPE.SHOTGUN;
    static public WEAPONTYPE Weapon4 = WEAPONTYPE.SYRINGE;
}

/*
public class Observer : TFClass
{
    public Observer()
    {
        _weapon1 = null;
        _weapon2 = null;
        _weapon3 = null;
        _weapon4 = null;
        _gren1 = Ammunition.None;
        _gren2 = Ammunition.None;
        _health = 0;
        _armour = 0;
        _maxShells = 0;
        _maxNails = 0;
        _maxRockets = 0;
        _maxCells = 0;
        _maxGren1 = 0;
        _maxGren2 = 0;
    }
}

public class Sniper : TFClass
{
    public Sniper()
    {
        _weapon1 = new SniperRifle();
        _weapon2 = new NailGun();
        _weapon3 = new Axe();
        _weapon4 = null;
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.Flare;
        _health = 85;
        _armour = 50;
        _maxShells = 75;
        _maxNails = 50;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 4;
    }
}

public class Pyro : TFClass
{
    public Pyro()
    {
        _weapon1 = new FlameThrower();
        _weapon2 = new PyroLauncher();
        _weapon3 = new Shotgun();
        _weapon4 = new Axe();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.NapalmGrenade;
        _health = 100;
        _armour = 150;
        _maxShells = 50;
        _maxNails = 50;
        _maxRockets = 60;
        _maxCells = 200;
        _maxGren1 = 4;
        _maxGren2 = 4;
    }
}

public class Spy : TFClass
{
    public Spy()
    {
        _weapon1 = new Tranquiliser();
        _weapon2 = new SuperShotgun();
        _weapon3 = new NailGun();
        _weapon4 = new Knife();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.GasGrenade;
        _health = 90;
        _armour = 50;
        _maxShells = 50;
        _maxNails = 100;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 4;
    }
}

public class Engineer : TFClass
{
    public Engineer()
    {
        _weapon1 = new RailGun();
        _weapon2 = new SuperShotgun();
        _weapon3 = new Spanner();
        _weapon4 = null;
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.EMPGrenade;
        _health = 80;
        _armour = 50;
        _maxShells = 50;
        _maxNails = 50;
        _maxRockets = 50;
        _maxCells = 200;
        _maxGren1 = 4;
        _maxGren2 = 4;
    }
}
*/