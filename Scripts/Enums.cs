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

public enum MT // movetype
{
    FLY,
    NORMAL,
    SPECTATOR,
    DEAD,
    NONE,
    LOCK,
}

public enum ET // entity type
{
    PLAYER = 1,
}

public enum Ammunition 
{
    Shells,
    Nails,
    Rockets,
    Cells,
    FragGrenade,
    MFTGrenade,
    ConcussionGrenade,
    MIRVGrenade,
    NapalmGrenade,
    NailGrenade,
    GasGrenade,
    EMPGrenade,
    Flare,
    Axe,
    None
}

public enum WeaponType
{
    Melee,
    Spread,
    Hitscan,
    Projectile,
    Grenade
}

public enum PuffType
{
    Blood,
    Puff
}