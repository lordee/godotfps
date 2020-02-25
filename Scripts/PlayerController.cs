using Godot;
using System;


public class PlayerController : Camera
{
    Player _player;
    public Player Player { get { return _player; }}
    Network _network;

    // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)
    private float move_forward = 0;
    private float move_right = 0;
    private float move_up = 0;
    private float _cameraAngle = 0f;

    // settings
    private float mouseSensitivity = 0.2f;

    public override void _Ready()
    {
        _network = GetNode("/root/Initial/Network") as Network;
    }

    public void Init(Player p)
    {
        _player = p;
        p.Mesh.Visible = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionJustPressed("jump"))
        {
            move_up = 1;
        }
        if (Input.IsActionJustReleased("jump"))
        {
            move_up = -1;
        }
        move_forward = 0;
        if (Input.IsActionPressed("move_forward"))
        {
            move_forward += 1;
        }
        if (Input.IsActionPressed("move_back"))
        {
            move_forward += -1;
        }
        move_right = 0;
        if (Input.IsActionPressed("move_right"))
        {
            move_right += 1;
        }
        if (Input.IsActionPressed("move_left"))
        {
            move_right += -1;
        }

        PlayerCmd pCmd;
        pCmd.move_forward = move_forward;
        pCmd.move_right = move_right;
        pCmd.move_up = move_up;
        pCmd.aim = this.GlobalTransform.basis;
        pCmd.cam_angle = _cameraAngle;
        pCmd.rotation = _player.Mesh.Rotation;
        _player.pCmdQueue.Enqueue(pCmd);
    }

    public override void _Input(InputEvent e)
    {
        // moving mouse
        if (Input.GetMouseMode() == Input.MouseMode.Captured)
        {
            if (e is InputEventMouseMotion em)
            {
                if (em.Relative.Length() > 0)
                {          
                    float look_right = em.Relative.x;
                    float look_up = em.Relative.y;

                    // limit how far up/down we look
                    // invert mouse
                    float rotate = Mathf.Deg2Rad(-look_right * mouseSensitivity);
                    float change = look_up * mouseSensitivity;

                    // inverted, fix later
                    _player.RotateHead(rotate);
                    if (_cameraAngle + change < 90F && _cameraAngle + change > -90F)
                    {
                        this.RotateX(Mathf.Deg2Rad(change));
                        _cameraAngle += change;
                    }
                }
            }
        }
    }    
}
