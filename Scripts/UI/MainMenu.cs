using Godot;
using System;

public class MainMenu : Control
{
    Button _options;
    Button _quit;
    public static bool IsOpen = false;
    Game _game;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _game = GetTree().Root.GetNode("Game") as Game;
        _options = GetNode("VBoxContainer/Options") as Button;
        _quit = GetNode("VBoxContainer/Quit") as Button;

        _options.Connect("pressed", this, "Options_Click");
        _quit.Connect("pressed", this, "Quit_Click");

        this.Close();
    }

    public void Open()
    {
        this.Show();
        UIManager.Open(nameof(MainMenu));
    }

    public void Close()
    {
        this.Hide();
        UIManager.Close(nameof(MainMenu));
    }

    [InputWithoutArg(typeof(MainMenu), nameof(ToggleMenu))]
    public static void ToggleMenu()
    {
        IsOpen = !IsOpen;
        if (IsOpen)
        {
            UIManager.MainMenu.Show();
            UIManager.Open(nameof(MainMenu));
        }
        else
        {
            UIManager.MainMenu.Hide();
            UIManager.Close(nameof(MainMenu));
        }
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
