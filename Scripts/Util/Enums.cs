using System.Collections.Generic;

public class PACKET
{
    public const string IMPULSE = @"\p";
    public const string HEADER = @"\h";
    public const string END = @"\e";
}

public class GAMESETTINGS
{
    public const float DETPIPE_DELAY = 0.5f;
}

public enum PACKETSTATE
{
    UNINITIALISED,
    HEADER,
    IMPULSE,
    END,
}

public enum STATE
{
    TOP = 0,
    BOTTOM = 1,
    UP = 2,
    DOWN = 3,
    OPEN = 4,
    CLOSE = 5,
    OPENING = 6,
    CLOSING = 7
}

public enum MOVETYPE // movetype
{
    FLY,
    NORMAL,
    SPECTATOR,
    DEAD,
    NONE,
    LOCK,
    TOSS,
    BOUNCE,
}

public enum ENTITYTYPE // entity type
{
    PLAYER = 1,
    PROJECTILE = 2,
}

public class ButtonInfo
{
	public enum TYPE {UNSET, SCANCODE, MOUSEBUTTON, MOUSEWHEEL, MOUSEAXIS, CONTROLLERBUTTON, CONTROLLERAXIS}
	public enum DIRECTION {UP, DOWN, RIGHT, LEFT};
}

public enum CT
{
    PLAYERCONTROLLER,
    COMMAND
}

public enum IMPULSE
{
    NONE = 0,
    WEAPONONE,
    WEAPONTWO,
    WEAPONTHREE,
    WEAPONFOUR,
    GRENONE,
    GRENTWO,
    DETPIPE,
    SPECIAL,
    DETPACK5,
    DETPACK20,
    DETPACK50,
    DETPACK255,
    DETPACKUNSET,
}

public enum PLAYERCLASS
{
    NONE = 0,
    SCOUT = 1,
    SNIPER = 2,
    SOLDIER = 3,
    DEMOMAN = 4,
    MEDIC = 5,
    HWGUY = 6,
    PYRO = 7,
    SPY = 8,
    ENGINEER = 9

}

public enum PARTICLE
{
    BLOOD,
    PUFF,
    FLAMETHROWER
}

public enum WEAPONSHOTTYPE
{
    MELEE,
    SPREAD,
    HITSCAN,
    PROJECTILE,
    GRENADE,
}

public enum WEAPONTYPE
{
    NONE,
    SYRINGE,
    AXE,
    SHOTGUN,
    SUPERSHOTGUN,
    NAILGUN,
    SUPERNAILGUN,
    GRENADELAUNCHER,
    PIPEBOMBLAUNCHER,
    ROCKETLAUNCHER,
    FLAMETHROWER,
    INCENDIARYCANNON,
    MINIGUN,
    FRAG,
    FLASH,
    CONCUSSION,
    SHOCK,
    MIRV,
    MIRVCHILD,
    NAPALM,
    DETPACK,
}

public enum AMMUNITION 
{
    NONE,
    SHELLS,
    NAILS,
    ROCKETS,
    CELLS,
}

public class ProjectileInfo
{
    public enum PROJECTILE
    {
        NAIL = 1,
        ROCKET = 2,
        GRENADE = 3,
        FRAG = 4,
        FLASH = 5,
        CONCUSSION = 6,
        SHOCK = 7,
        MIRV = 8,
        MIRVCHILD = 9,
        PIPEBOMB = 10,
        DETPACK = 11,
        BULLET,
        NAPALM,
        INCENDIARYROCKET
    }

    static public Dictionary<WEAPONTYPE, PROJECTILE> Weapons = new Dictionary<WEAPONTYPE, PROJECTILE> {
        {WEAPONTYPE.NAILGUN, ProjectileInfo.PROJECTILE.NAIL},
        {WEAPONTYPE.SUPERNAILGUN, ProjectileInfo.PROJECTILE.NAIL},
        {WEAPONTYPE.GRENADELAUNCHER, ProjectileInfo.PROJECTILE.GRENADE},
        {WEAPONTYPE.PIPEBOMBLAUNCHER, ProjectileInfo.PROJECTILE.PIPEBOMB},
        {WEAPONTYPE.ROCKETLAUNCHER, ProjectileInfo.PROJECTILE.ROCKET},
        {WEAPONTYPE.INCENDIARYCANNON, ProjectileInfo.PROJECTILE.INCENDIARYROCKET},
        {WEAPONTYPE.FRAG, ProjectileInfo.PROJECTILE.FRAG},
        {WEAPONTYPE.FLASH, ProjectileInfo.PROJECTILE.FLASH},
        {WEAPONTYPE.CONCUSSION, ProjectileInfo.PROJECTILE.CONCUSSION},
        {WEAPONTYPE.SHOCK, ProjectileInfo.PROJECTILE.SHOCK},
        {WEAPONTYPE.MIRV, ProjectileInfo.PROJECTILE.MIRV},
        {WEAPONTYPE.MIRVCHILD, ProjectileInfo.PROJECTILE.MIRVCHILD},
        {WEAPONTYPE.NAPALM, ProjectileInfo.PROJECTILE.NAPALM},
        {WEAPONTYPE.DETPACK, ProjectileInfo.PROJECTILE.DETPACK},
    };

    static public Dictionary<PROJECTILE, string> Scenes = new Dictionary<PROJECTILE, string> {
        {ProjectileInfo.PROJECTILE.NAIL, "res://Scenes/Weapons/Nail.tscn"},
        {ProjectileInfo.PROJECTILE.BULLET, "res://Scenes/Weapons/Bullet.tscn"},
        {ProjectileInfo.PROJECTILE.GRENADE, "res://Scenes/Weapons/Grenade.tscn"},
        {ProjectileInfo.PROJECTILE.PIPEBOMB, "res://Scenes/Weapons/Pipebomb.tscn"},
        {ProjectileInfo.PROJECTILE.ROCKET, "res://Scenes/Weapons/Rocket.tscn"},
        {ProjectileInfo.PROJECTILE.INCENDIARYROCKET, "res://Scenes/Weapons/IncendiaryRocket.tscn"},
        {ProjectileInfo.PROJECTILE.CONCUSSION, "res://Scenes/Weapons/HandGrenades/ConcussionGrenade.tscn"},
        {ProjectileInfo.PROJECTILE.FLASH, "res://Scenes/Weapons/HandGrenades/FlashGrenade.tscn"},
        {ProjectileInfo.PROJECTILE.FRAG, "res://Scenes/Weapons/HandGrenades/FragGrenade.tscn"},
        {ProjectileInfo.PROJECTILE.SHOCK, "res://Scenes/Weapons/HandGrenades/ShockGrenade.tscn"},
        {ProjectileInfo.PROJECTILE.MIRV, "res://Scenes/Weapons/HandGrenades/MIRVGrenade.tscn"},
        {ProjectileInfo.PROJECTILE.NAPALM, "res://Scenes/Weapons/HandGrenades/Napalm.tscn"},
        {ProjectileInfo.PROJECTILE.MIRVCHILD, "res://Scenes/Weapons/Grenade.tscn"},
        {ProjectileInfo.PROJECTILE.DETPACK, "res://Scenes/Weapons/Detpack.tscn"},
    };
}