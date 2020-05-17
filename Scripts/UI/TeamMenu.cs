using Godot;
using System;

public class TeamMenu : VBoxContainer, IUIItem
{
    enum ButtonType
    {
        TeamOne = 1,
        TeamTwo = 2,
        Specator = 3,
        Cancel = 4
    }
    public override void _Ready()
    {
        
    }

    private void _on_btn_pressed(int buttonID)
    {
        ButtonType bt = (ButtonType)buttonID;
        switch (bt)
        {
            case ButtonType.TeamOne:
            case ButtonType.TeamTwo:
                JoinTeam(buttonID);
                break;
            case ButtonType.Specator:
                JoinSpectator();
                break;
        }
    }

    public void JoinTeam(int teamID)
    {
        // TODO - ask server to join team
        UIManager.Game.Commands.JoinTeam(teamID);

        // we assume client side that it's fine, server can re prompt us if not
        Game.Client.Player.Team = teamID;

        UIManager.Close();
        UIManager.Open(UIManager.ClassMenu);
    }

    private void JoinSpectator()
    {
        //TODO - implement join spectator
    }

    public void Open()
    {
        this.Show();
    }

    public void Close()
    {
        this.Hide();
    }

    public void UI_Up()
    {

    }

    public void UI_Down()
    {

    }

    public void UI_Cancel()
    {
        if (Game.Client.Player.Team != 0)
        {
            UIManager.Close();
        }
    }

    public void UI_Accept()
    {
        
    }
}
