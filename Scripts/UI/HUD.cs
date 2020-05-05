using Godot;
using System;

public class HUD : CanvasLayer
{
    Player _player;
    Label _health;
    Label _armour;
    public Sprite Crosshair;
    Control _manager;
    public Node2D AimAt;

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
        }
    }   
}
