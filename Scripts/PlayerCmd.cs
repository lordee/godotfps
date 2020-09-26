using Godot;
using System.Collections.Generic;

public class PlayerCmd
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
    public string _projName;
    public string projName { 
        get { return _projName; }
        set { _projName = value; }
        }
    public List<float> impulses = new List<float>();
}