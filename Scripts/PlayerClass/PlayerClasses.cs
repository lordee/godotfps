using Godot;
using System;

public class Scout
{
    static public int Health = 100;
    static public int Armour = 25;
    //static public IHandGrenade Gren1;
    //static public IHandGrenade Gren2;
    static public int Shells = 50;
    static public int Nails = 200;
    static public int Rockets = 50;
    static public int Cells = 100;
    static public int MaxGren1 = 4;
    static public int MaxGren2 = 4;
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


public class Scout : TFClass
{
    public Scout()
    {
        _weapon1 = new NailGun();
        _weapon2 = new Shotgun();
        _weapon3 = new Axe();
        _weapon4 = null;
        _gren1 = Ammunition.MFTGrenade;
        _gren2 = Ammunition.ConcussionGrenade;
        _health = 75;
        _armour = 50;
        _maxShells = 50;
        _maxNails = 200;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 4;
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

public class Soldier : TFClass
{
    public Soldier()
    {
        _weapon1 = new RocketLauncher();
        _weapon2 = new SuperShotgun();
        _weapon3 = new Shotgun();
        _weapon4 = new Axe();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.NailGrenade;
        _health = 100;
        _armour = 200;
        _maxShells = 100;
        _maxNails = 50;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 200;
    }
}

public class Demoman : TFClass
{
    public Demoman()
    {
        _weapon1 = new GrenadeLauncher();
        _weapon2 = new PipebombLauncher();
        _weapon3 = new Shotgun();
        _weapon4 = new Axe();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.MIRVGrenade;
        _health = 90;
        _armour = 120;
        _maxShells = 50;
        _maxNails = 50;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 4;
    }
}

public class Medic : TFClass
{
    public Medic()
    {
        _weapon1 = new SuperNailGun();
        _weapon2 = new SuperShotgun();
        _weapon3 = new Shotgun();
        _weapon4 = new Syringe();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.ConcussionGrenade;
        _health = 100;
        _armour = 80;
        _maxShells = 50;
        _maxNails = 150;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 100;
    }
}

public class HWGuy : TFClass
{
    public HWGuy()
    {
        _weapon1 = new MiniGun();
        _weapon2 = new SuperShotgun();
        _weapon3 = new Shotgun();
        _weapon4 = new Axe();
        _gren1 = Ammunition.FragGrenade;
        _gren2 = Ammunition.MIRVGrenade;
        _health = 100;
        _armour = 300;
        _maxShells = 200;
        _maxNails = 50;
        _maxRockets = 50;
        _maxCells = 50;
        _maxGren1 = 4;
        _maxGren2 = 2;
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