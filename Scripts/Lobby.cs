using Godot;
using System;

public class Lobby : Control
{
    int DEFAULT_PORT = 8910; // some random number, pick your port properly

    Button _joinBtn;
    Button _hostBtn;
    LineEdit _address;
    Game _game;
    public override void _Ready()
    {
        _joinBtn = (Button)GetNode("panel/join");
        _hostBtn  = (Button)GetNode("panel/host");
        _hostBtn.Connect("pressed", this, "_On_Host_Pressed");
        _joinBtn.Connect("pressed", this, "_On_Join_Pressed");
        _address = GetNode("panel/address") as LineEdit;
        _game = GetTree().Root.GetNode("Game") as Game;
        UIManager.Open(nameof(Lobby));
    }

    private void _Set_Status(string text, bool isok)
    {
        // simple way to show status		
        Label ok = (Label)GetNode("panel/status_ok");
        Label fail = (Label)GetNode("panel/status_fail");
        if (isok)
        {
            ok.Text = text;
            fail.Text = "";
        }
        else
        {
            ok.Text = "";
            fail.Text = text;
        }      
    }

    private void _On_Host_Pressed()
    {
        _game.Network.Host(DEFAULT_PORT);
        Close();
    }

    private void _On_Join_Pressed()
    {   
        _game.Network.ConnectTo(_address.Text, DEFAULT_PORT);
        
        Close();
    }

    private void Close()
    {
        this.Hide();
        UIManager.Close(nameof(Lobby));
    }
}