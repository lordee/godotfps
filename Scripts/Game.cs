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
    public static Settings Settings;

    public override void _Ready()
    {
        Network = GetNode("Network") as Network;
        World = GetNode("World") as World;
        Commands = new Commands(this);
        HUD = GetNode("UIManager/HUD") as HUD;
        Map = GetNode("Map");
        UIManager = GetNode("UIManager") as UIManager;
        Settings = new Settings(this);

        Settings.LoadConfig();
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
}
