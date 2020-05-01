using Godot;
using System;

public class MainMenu : Control, IUIItem
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
    }

    public void Close()
    {
        this.Hide();
    }

    [InputWithoutArg(typeof(MainMenu), nameof(MainMenuToggle))]
    public static void MainMenuToggle()
    {
        UIManager.Open(UIManager.MainMenu);
    }

    private void Options_Click()
    {
        UIManager.Open(UIManager.OptionsMenu);
    }

    private void Quit_Click()
    {
        _game.Quit();
    }

    public void UI_Up()
    {

    }

    public void UI_Down()
    {

    }

    public void UI_Cancel()
    {
        UIManager.Close();
    }

    public void UI_Accept()
    {
        
    }
}
