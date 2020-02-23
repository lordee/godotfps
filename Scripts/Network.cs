using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Network : Node
{
    private int maxPlayers = 8;
    Initial initial;
    World _world;
    PlayerController _pc;
    private int _id;
    public List<Peer> PeerList = new List<Peer>();

    private bool _active = false;

    private float _gameTime = 0f;
    private float _lastPingSent = 0f;
    private float _lastPingGT = 0f;

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
            _gameTime += delta;
            _lastPingSent += delta;
            if (IsNetworkMaster())
            {
                string pingString = "";
                // send updates to each peer
                foreach (Peer p in PeerList)
                {
                    Vector3 org = p.Player.GlobalTransform.origin;
                    Vector3 velo = p.Player.PlayerVelocity;
                    Vector3 rot = p.Player.Mesh.Rotation;
                    RpcUnreliable(nameof(ReceivePMovementClient), p.ID, org.x, org.y, org.z, velo.x, velo.y, velo.z, rot.x, rot.y, rot.z);

                    if (p.ID != 1)
                    {
                        pingString += "," + p.ID.ToString() + "," + p.Ping.ToString();
                    }
                }
                
                if (_lastPingSent > 3f)
                {
                    _lastPingSent = 0f;
                    _lastPingGT = _gameTime;
                    if (pingString.Length > 0)
                    {
                        pingString = pingString.Substr(1, pingString.Length);
                    }
                    byte[] pingBytes = Encoding.UTF8.GetBytes(pingString);
                    RpcUnreliable(nameof(Ping), pingBytes);
                }
            }
            // send update to network master
            SendPMovement(1, _id, _pc.pCmd);
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
        PlayerController c = _world.AddPlayer(id, playerControlled);

        if (c != null)
        {
            _pc = c;
        }

        Player p = GetNode("/root/Initial/World/" + id) as Player;
        PeerList.Add(new Peer(id, 0, p));
    }
    
    private void SyncWorld(int id)
    {
        // TODO - send over all ents to new player?
        foreach(Peer p in PeerList)
        {
            RpcId(id, nameof(SyncWorldReceive), ET.PLAYER, p.ID.ToString());
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
    public void ReceivePMovementClient(int id, float orgX, float orgY, float orgZ, float veloX, float veloY, float veloZ
        , float rotX, float rotY, float rotZ)
    {
        Vector3 org = new Vector3(orgX, orgY, orgZ);
        Player p = GetNode("/root/Initial/World/" + id.ToString()) as Player;
        Transform t = p.GlobalTransform;
        t.origin = org;

        if (id == _id)
        {
            if ((p.GlobalTransform.origin - org).Length() > 35) // randomnumber()
            {
                GD.Print("correcting origin");
                
                p.GlobalTransform = t;
            }
        }
        else
        {
            p.GlobalTransform = t;
            p.PlayerVelocity = new Vector3(veloX, veloY, veloZ);
            p.Rotation = new Vector3(rotX, rotY, rotZ);
        }
    }

    // clients
    [Slave]
    public void Ping(byte[] pingBytes)
    {
        string pingString = Encoding.UTF8.GetString(pingBytes);
        string[] split = pingString.Split(",");
        for(int i = 0; i < split.Length; i++)
        {
            int id = 0;
            int ping = 0;
            if (i % 2 == 0)
            {
                id = Convert.ToInt32(split[i++]);
                ping = Convert.ToInt32(split[i]);
                if (id == _id)
                {
                    // it's actually ping to server
                    id = 1;
                }
                Peer p = PeerList.Where(p2 => p2.ID == id).First();
                p.Ping = ping;
            }
        }

        RpcUnreliable(nameof(PingServer), _id);
    }

    // server receives and notes
    [Remote]
    public void PingServer(int id)
    {
        // note response time against client
        Peer p = PeerList.Where(p2 => p2.ID == id).First();
        p.Ping = Convert.ToInt16(_gameTime - _lastPingGT);
    }

    public void SendPMovement(int RecID, int id, PlayerCmd pCmd)
    {
        // FIXME this is obviously bad - I can't remember why this was bad now... more detail next time
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
