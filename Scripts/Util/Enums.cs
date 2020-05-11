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
    PROJECTILE = 2,
}

public class ButtonInfo
{
	public enum TYPE {UNSET, SCANCODE, MOUSEBUTTON, MOUSEWHEEL, MOUSEAXIS, CONTROLLERBUTTON, CONTROLLERAXIS}
	public enum DIRECTION {UP, DOWN, RIGHT, LEFT};
}

public enum CT
{
    PlayerController,
    Command
}