using Godot;
using System;

public class Game : Node
{
    public Network Network;
    public Commands Commands;
    public World World;
    public HUD HUD;
    public Node Map;
    public static PlayerController Client;
    public static UIManager UIManager;

    public override void _Ready()
    {
        Network = GetNode("Network") as Network;
        World = GetNode("World") as World;
        Commands = new Commands(this);
        HUD = GetNode("UIManager/HUD") as HUD;
        Map = GetNode("Map");
        UIManager = GetNode("UIManager") as UIManager;

        LoadConfig();
    }

    public void Quit()
    {
        Network.Disconnect();
        GetTree().Quit();
    }

    public void LoadUI(PlayerController p)
    {
        HUD.Init(p.Player);
        HUD.Visible = true;
        Client = p;
    }

    private void LoadConfig()
    {
        // TODO - load/save of config

        // if not exist, load defaults
        LoadDefaultConfig();
    }

    private void LoadDefaultConfig()
    {
        // kb move
        Bindings.Bind("W", nameof(PlayerController.MoveForward), false);
        Bindings.Bind("A", nameof(PlayerController.MoveLeft), false);
        Bindings.Bind("S", nameof(PlayerController.MoveBack), false);
        Bindings.Bind("D", nameof(PlayerController.MoveRight), false);
        Bindings.Bind("Space", nameof(PlayerController.Jump), false);

        // mouse
        Bindings.Bind("MouseOne", nameof(PlayerController.Attack), false);
        Bindings.Bind("MouseUp",  nameof(PlayerController.LookUp), false);
        Bindings.Bind("MouseDown", nameof(PlayerController.LookDown), false);
        Bindings.Bind("MouseRight",nameof(PlayerController.LookRight), false);
        Bindings.Bind("MouseLeft", nameof(PlayerController.LookLeft), false);

        // other
        Bindings.Bind("M", nameof(PlayerController.MouseModeToggle), false);
        Bindings.Bind("Escape", nameof(MainMenu.ToggleMenu), true);
        Bindings.Bind("`", nameof(Console.ConsoleToggle), true);
    }
}
