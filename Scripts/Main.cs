using Godot;
using System;

public class Main : Node
{
    public override void _Ready()
    {
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
