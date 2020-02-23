using Godot;
using System;
using System.Collections.Generic;

public class Peer
{
    public int ID;
    public int Ping;
    public Player Player;

    public Peer(int id, int ping, Player p)
    {
        ID = id;
        Ping = ping;
        Player = p;
    }
}