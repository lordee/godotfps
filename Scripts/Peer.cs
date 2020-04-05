using Godot;
using System;
using System.Collections.Generic;

public class Peer
{
    public int ID;
    public Player Player;
    public int LastSnapshot = 0;

    public Peer(int id, Player p)
    {
        ID = id;
        Player = p;
    }
}