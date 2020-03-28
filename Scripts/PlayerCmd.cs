using Godot;

public struct PlayerCmd
{
    public int snapshot;
    public int playerID;
    public float move_forward;
    public float move_right;
    public float move_up;
    public Basis aim;
    public float cam_angle;
    public Vector3 rotation;
    public float attack;
    public Vector3 attackDir;
}