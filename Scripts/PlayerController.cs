using Godot;
using System;


public class PlayerController : Camera
{
    Game _game;
    Player _player;
    public Player Player { get { return _player; }}

    // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)
    private float move_forward = 0;
    private float move_right = 0;
    private float move_up = 0;
    private float attack = 0;
    private float _cameraAngle = 0f;
    private Vector3 shootTo = new Vector3();
    private float _shootRange = 100000f;

    private Sprite _crosshair;
    public Sprite Crosshair {
        get {
            if (_crosshair == null)
            {
                _crosshair = _game.HUD.Crosshair;
            }
            return _crosshair;
        }
    }

    public override void _Ready()
    {
        _game = GetTree().Root.GetNode("Game") as Game;
    }

    public void Init(Player p)
    {
        _player = p;
        p.Mesh.Visible = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        shootTo = new Vector3();
        if (attack == 1)
        {
            float ypos = Crosshair.Position.y - (Crosshair.Texture.GetSize().y * Crosshair.Scale.y/2);
            Vector2 pos = new Vector2(Crosshair.Position.x, ypos);
            Vector3 origin = ProjectRayOrigin(pos);
            Vector3 to = ProjectRayNormal(pos) * _shootRange;
            shootTo = to + origin;
        }
        
        PlayerCmd pCmd = new PlayerCmd();
        pCmd.playerID = _player.ID;
        pCmd.snapshot = _game.World.LocalSnapNum;
        pCmd.move_forward = move_forward;
        pCmd.move_right = move_right;
        pCmd.move_up = move_up;
        pCmd.aim = this.GlobalTransform.basis;
        pCmd.cam_angle = _cameraAngle;
        pCmd.rotation = _player.Mesh.Rotation;
        pCmd.attack = attack;
        pCmd.attackDir = shootTo;
        pCmd._projName = "";
        _player.pCmdQueue.Add(pCmd);
    }

    [InputWithArg(typeof(PlayerController), nameof(MoveForward))]
    public static void MoveForward(float val)
    {
        Game.Client.move_forward += val;
    }

    [InputWithArg(typeof(PlayerController), nameof(MoveBack))]
    public static void MoveBack(float val)
    {
        Game.Client.move_forward -= val;
    }

    [InputWithArg(typeof(PlayerController), nameof(MoveRight))]
    public static void MoveRight(float val)
    {
        Game.Client.move_right += val;
    }

    [InputWithArg(typeof(PlayerController), nameof(MoveLeft))]
    public static void MoveLeft(float val)
    {
        Game.Client.move_right -= val;
    }

    [InputWithArg(typeof(PlayerController), nameof(Jump))]
    public static void Jump(float val)
    {
        Game.Client.move_up = val;
    }

    [InputWithArg(typeof(PlayerController), nameof(Attack))]
    public static void Attack(float val)
    {
        if (Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            Game.Client.attack = val;
        }
    }

    [InputWithoutArg(typeof(PlayerController), nameof(MouseModeToggle))]
    public static void MouseModeToggle()
    {
        Settings.MouseCursorVisible = !Settings.MouseCursorVisible;
        if (Settings.MouseCursorVisible)
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
        }
        else
        {
            Input.SetMouseMode(Input.MouseMode.Captured);
        }
    }

    [InputWithArg(typeof(PlayerController), nameof(LookUp))]
	public static void LookUp(float val)
	{
        if (val > 0)
        {
            float change = val * Settings.Sensitivity * Settings.InvertMouseValue;
            if (Game.Client._cameraAngle + change < 90f && Game.Client._cameraAngle + change > -90f)
            {
                Game.Client._cameraAngle += change;
                Game.Client.RotateX(Mathf.Deg2Rad(change));
            }
        }
	}


	[InputWithArg(typeof(PlayerController), nameof(LookDown))]
	public static void LookDown(float val)
	{
        if (val > 0)
        {
            float change = -val * Settings.Sensitivity * Settings.InvertMouseValue;
            if (Game.Client._cameraAngle + change < 90f && Game.Client._cameraAngle + change > -90f)
            {
                Game.Client._cameraAngle += change;
                Game.Client.RotateX(Mathf.Deg2Rad(change));
            }
        }
	}


	[InputWithArg(typeof(PlayerController), nameof(LookRight))]
	public static void LookRight(float val)
	{
		if (val > 0)
        {
            float change = Mathf.Deg2Rad(-val * Settings.Sensitivity);
            Game.Client.Player.RotateHead(change);
        }
	}


	[InputWithArg(typeof(PlayerController), nameof(LookLeft))]
	public static void LookLeft(float val)
	{
		if (val > 0)
        {
            float change = Mathf.Deg2Rad(val * Settings.Sensitivity);
            Game.Client.Player.RotateHead(change);
        }
	}
}
