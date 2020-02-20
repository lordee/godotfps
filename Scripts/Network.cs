using Godot;
using System;
using System.Collections.Generic;

public class Network : Node
{
    private int maxPlayers = 8;
    Initial initial;
    World _world;
    PlayerController _pc;
    private int _id;
    public List<int> PeerList = new List<int>();
    public List<Player> PlayerList = new List<Player>();

    private bool _active = false;

    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, "ClientConnected");
        GetTree().Connect("network_peer_disconnected", this, "ClientDisconnected");
        GetTree().Connect("connected_to_server", this, "ConnectionSuccess");
        GetTree().Connect("connection_failed", this, "ConnectionFailed");
        GetTree().Connect("server_disconnected", this, "ConnectionRemoved");

        initial = GetNode("/root/Initial") as Initial;
        _world = GetNode("/root/Initial/World") as World;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_active)
        {
            if (IsNetworkMaster())
            {
                // send updates to each peer
                foreach (Player p in PlayerList)
                {
                    RpcUnreliable(nameof(ReceivePMovementClient), p.ID, p.GlobalTransform);
                }
            }
            // send update to network master
            SendPMovementServer(1, _id, _pc.pCmd);
        }
    }

    public void ClientConnected(string ids)
    {
        GD.Print("Client connected - ID: " +  ids);
        int id = Convert.ToInt32(ids);

        // client connects, event on client and server
        if (IsNetworkMaster())
        {
            AddPlayer(id, false);

            SyncWorld(id);
        }
    }

    public void ClientDisconnected(string id)
    {
        GD.Print("Client disconnected - ID: " +  id);
    }

    public void ConnectionSuccess()
    {
        GD.Print("ConnectionSuccess");    
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
        GetTree().NetworkPeer = Peer;
	}

    public void Host(int port)
	{
		var Peer = new NetworkedMultiplayerENet();
		Peer.CreateServer(port, maxPlayers);

		GD.Print($"Started hosting on port '{port}'");
        GetTree().NetworkPeer = Peer;
        _id = GetTree().GetNetworkUniqueId();
        _world.StartWorld();

        AddPlayer(_id, true);
        _active = true;
	}

    private void AddPlayer(int id, bool playerControlled)
    {
        PeerList.Add(id);
        
        PlayerController c = _world.AddPlayer(id, playerControlled);

        if (c != null)
        {
            _pc = c;
        }

        Player p = GetNode("/root/Initial/World/" + id) as Player;
        PlayerList.Add(p);
    }
    
    private void SyncWorld(int id)
    {
        // TODO - send over all ents to new player?
        foreach(int pid in PeerList)
        {
            RpcId(id, nameof(SyncWorldReceive), ET.PLAYER, pid.ToString());
        }
    }

    // only clients receive this, only on first connect?
    [Remote]
    public void SyncWorldReceive(ET entType, string nodeName)
    {
        switch (entType)
        {
            case ET.PLAYER:
                int id = Convert.ToInt32(nodeName);
                if (id == GetTree().GetNetworkUniqueId())
                {
                    _world.StartWorld();
                    _id = id;
                    AddPlayer(id, true);
                    _active = true;
                }
                else
                {
                    AddPlayer(id, false);
                }
                break;
        }
    }

    [Remote]
    public void ReceivePMovementServer(int id, float move_forward, float move_right, float move_up, Vector3 aimx
    , Vector3 aimy, Vector3 aimz, float camAngle, float rotX, float rotY, float rotZ)
    {
        Basis aim = new Basis(aimx, aimy, aimz);
        Player p = GetNode("/root/Initial/World/" + id.ToString()) as Player;
        PlayerCmd pCmd;
        pCmd.aim = aim;
        pCmd.move_forward = move_forward;
        pCmd.move_right = move_right;
        pCmd.move_up = move_up;
        pCmd.cam_angle = camAngle;
        pCmd.rotation = new Vector3(rotX, rotY, rotZ);
        p.SetMovement(pCmd);
    }

    [Slave]
    public void ReceivePMovementClient(int id, Transform t)
    {
        Player p = GetNode("/root/Initial/World/" + id.ToString()) as Player;
        
        if (id == _id)
        {
            if ((p.GlobalTransform.origin - t.origin).Length() > 35) // randomnumber()
            {
                GD.Print("correcting origin");
                p.GlobalTransform = t;
            }
        }
        else
        {
            p.GlobalTransform = t;
        }
    }

    public void SendPMovementServer(int RecID, int id, PlayerCmd pCmd)
    {
        // FIXME this is obviously bad
        if (_id == id)
        {
            ReceivePMovementServer(id, pCmd.move_forward, pCmd.move_right, pCmd.move_up, pCmd.aim.x, pCmd.aim.y, pCmd.aim.z, pCmd.cam_angle, pCmd.rotation.x, pCmd.rotation.y, pCmd.rotation.z);
        }
        
        if (!IsNetworkMaster())
        {
            RpcUnreliableId(RecID, nameof(ReceivePMovementServer), id, pCmd.move_forward, pCmd.move_right, pCmd.move_up, pCmd.aim.x, pCmd.aim.y, pCmd.aim.z, pCmd.cam_angle, pCmd.rotation.x, pCmd.rotation.y, pCmd.rotation.z);
        }
    }
}
