using Godot;
using System;

public class Main : Node
{
    //Network _network;

    public override void _Ready()
    {
        //_network = (Network)GetNode("/root/OpenFortress/Network");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
            GetTree().Quit();
        }
    }
   
}
