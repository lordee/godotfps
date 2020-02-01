using Godot;
using System;
using System.Collections.Generic;

public class Network : Node
{
    private int maxPlayers = 8;
    Initial initial;
    World world;
    private int _id;
    public List<int> PeerList = new List<int>();

    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, "ClientConnected");
        GetTree().Connect("network_peer_disconnected", this, "ClientDisconnected");
        GetTree().Connect("connected_to_server", this, "ConnectionSuccess");
        GetTree().Connect("connection_failed", this, "ConnectionFailed");
        GetTree().Connect("server_disconnected", this, "ConnectionRemoved");

        initial = GetNode("/root/Initial") as Initial;
        world = GetNode("/root/Initial/World") as World;
    }

    public void ClientConnected(string id)
    {
        GD.Print("Client connected - ID: " +  id);
    }

    public void ClientDisconnected(string id)
    {
        GD.Print("Client disconnected - ID: " +  id);
    }

    public void ConnectionSuccess()
    {
        GD.Print("ConnectionSuccess");
        
        //_game.InstantiatePlayer(GetTree().GetNetworkUniqueId().ToString(), true);        
    }

    public void ConnectionFailed()
    {
        GD.Print("ConnectionFailed");
    }

    public void ConnectionRemoved()
    {
        GD.Print("ConnectionRemoved");
    }

    public void ConnectTo(string InIp, int port)
	{
		//Set static string Ip
		NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
		Peer.CreateClient(InIp, port);
        GetTree().SetNetworkPeer(Peer);
	}

    public void Host(int port)
	{
		var Peer = new NetworkedMultiplayerENet();
		Peer.CreateServer(port, maxPlayers);

		GD.Print($"Started hosting on port '{port}'");
        GetTree().SetNetworkPeer(Peer);
        _id = GetTree().GetNetworkUniqueId();

		PeerList.Add(_id);
		//Nicknames[ServerId] = Game.Nickname;
        world.StartWorld();
        Player p = world.AddPlayer(_id, true);
        p.Team = 1;
        world.Spawn(p);
        Input.SetMouseMode(Input.MouseMode.Captured);
	}

    [Remote]
    public void ReceivePMovement(int playerID, float move_forward, float move_right, float move_up, float look_right, float look_up, Vector3 aimx, Vector3 aimy, Vector3 aimz, float camAngle)
    {
        Basis aim = new Basis(aimx, aimy, aimz);
        Player p = GetNode("/root/Initial/World/" + playerID.ToString()) as Player;
        p.SetMovement(move_forward, move_right, move_up, look_right, look_up, aim, camAngle);
    }

    public void SendPMovement(int RecID, int id, float move_forward, float move_right, float move_up, float look_right, float look_up, Basis aim, float camAngle)
    {
        if (IsNetworkMaster())
        {
            ReceivePMovement(id, move_forward, move_right, move_up, look_right, look_up, aim.x, aim.y, aim.z, camAngle);
        }
        else
        {
            RpcUnreliableId(RecID, nameof(ReceivePMovement), id, move_forward, move_right, move_up, look_right, look_up, aim.x, aim.y, aim.z, camAngle);
        }
    }
}
