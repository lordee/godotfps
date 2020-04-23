using Godot;
using System;

public class MainMenu : Control
{
    Button _options;
    Button _quit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _options = GetNode("VBoxContainer/Options") as Button;
        _quit = GetNode("VBoxContainer/Quit") as Button;

        _options.Connect("pressed", this, "Options_Click");
        _quit.Connect("pressed", this, "Quit_Click");
    }

    private void Options_Click()
    {
        GD.Print("options click!");
    }

    private void Quit_Click()
    {
        GetTree().Quit();
    }
}
