using Godot;
using System;

public class Lobby : Control
{
    int DEFAULT_PORT = 8910; // some random number, pick your port properly

    Button _joinBtn;
    Button _hostBtn;
    LineEdit _address;
    Network _network;
    public override void _Ready()
    {
        _joinBtn = (Button)GetNode("panel/join");
        _hostBtn  = (Button)GetNode("panel/host");
        _hostBtn.Connect("pressed", this, "_On_Host_Pressed");
        _joinBtn.Connect("pressed", this, "_On_Join_Pressed");
        _address = GetNode("panel/address") as LineEdit;
        _network = GetNode("/root/Initial/Network") as Network;
    }

    // Network callbacks from SceneTree

    // callback from SceneTree
    private void _Player_Connected(int id)
    {
        GD.Print("player connected");
        // someone connected, start the game!
	    PackedScene main = (PackedScene)ResourceLoader.Load("res://Scenes/Main.tscn");
        Main inst = (Main)main.Instance();
        Node of = GetNode("/root/Initial");

        of.AddChild(inst);
	
        //_network = (Network)GetNode("/root/OpenFortress/Network");
        //_network.Active = true;
        this.Hide();
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
        GD.Print("on host pressed");
        _network.Host(DEFAULT_PORT);
        this.Hide();
    }

    private void _On_Join_Pressed()
    {   
        _network.ConnectTo(_address.Text, DEFAULT_PORT);
        
        this.Hide();
    }
	
    /*private void _On_Join_Pressed()
    {
        LineEdit add = (LineEdit)GetNode("panel/address");
        string ip = add.Text;
        System.Net.IPAddress IPadd;

        if (!System.Net.IPAddress.TryParse(ip, out IPadd))
        {
            _Set_Status("IP address is invalid", false);
            return;
        }

        _network = (Network)GetNode("/root/OpenFortress/Network");
        _network.OFClientConnect(ip, DEFAULT_PORT);
        
        _Set_Status("Connecting..", true);
    }*/
}