using Godot;
using System;

public class Initial : Node
{
    MainMenu _mainMenu;
    bool _menuToggled = false;
    public override void _Ready()
    {
        PackedScene lobby = (PackedScene)ResourceLoader.Load("res://Scenes/Lobby.tscn");
        Lobby inst = (Lobby)lobby.Instance();
        this.AddChild(inst);

        PackedScene mainMenu = (PackedScene)ResourceLoader.Load("res://Scenes/UI/MainMenu.tscn");
        _mainMenu = mainMenu.Instance() as MainMenu;
        Node init = GetNode("/root/Initial");

        init.AddChild(_mainMenu);
        _mainMenu.Hide();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("menu_toggle"))
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
            _menuToggled = !_menuToggled;
            if (_menuToggled)
            {
                _mainMenu.Show();
            }
            else
            {
                _mainMenu.Hide();
            }
        }
    }
}
