using Godot;
using System;
using System.Collections.Generic;

public class HUD : CanvasLayer
{
    Player _player;
    Label _health;
    Label _armour;
    public Sprite Crosshair;
    Control _manager;
    public Node2D AimAt;
    List<uint> times = new List<uint>();
    Label _fps;

    // Canvas layer currently has no visibility controls, parent a control node and use that for all other children
    private bool _visible = true;
    public bool Visible { 
        get { return _visible; }
        set { 
                _visible = value; 
                _manager.Visible = value;
            }
        }

    public override void _Ready()
    {
        _manager = GetNode("Manager") as Control;
        _health = _manager.GetNode("HealthLabel") as Label;
        _armour = _manager.GetNode("ArmourLabel") as Label;
        Crosshair = _manager.GetNode("Crosshair") as Sprite;
        AimAt = _manager.GetNode("AimAt") as Node2D;
        _fps = _manager.GetNode("FPS") as Label;
        this.Visible = false;
    }

    public void Init(Player p)
    {
        _player = p;
    }

    public override void _Process(float delta)
    {
        if (this.Visible)
        {
            _health.Text = Mathf.CeilToInt(_player.CurrentHealth).ToString();
            _armour.Text = Mathf.CeilToInt(_player.CurrentArmour).ToString();
            
            uint now = OS.GetTicksMsec();
            while (times.Count > 0 && times[0] <= now - 1000)
            {
                times.RemoveAt(0);
            }

            times.Add(now);
            _fps.Text = times.Count.ToString() + " FPS";

        }
    }   
}
