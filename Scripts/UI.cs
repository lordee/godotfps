using Godot;
using System;

public class UI : CanvasLayer
{
    Player _player;
    Label _health;
    Label _armour;
    public override void _Ready()
    {
        _health = GetNode("HealthLabel") as Label;
        _armour = GetNode("ArmourLabel") as Label;
    }

    public void Init(Player p)
    {
        _player = p;
    }

    public override void _Process(float delta)
    {
        _health.Text = Mathf.CeilToInt(_player.CurrentHealth).ToString();
        _armour.Text = Mathf.CeilToInt(_player.CurrentArmour).ToString();
    }
}
