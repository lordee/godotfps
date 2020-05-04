using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Network : Node
{
    Game _game;
    
    private int maxPlayers = 8;
    
    PlayerController _pc;
    private int _id;
    public List<Peer> PeerList = new List<Peer>();

    private bool _active = false;

    StringBuilder sb = new StringBuilder();

    public List<SnapShot> Snapshots = new List<SnapShot>();
    
    // TODO - only send entity updates if entity state has changed.  Compare it to last acknowledged packet from client

    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, "ClientConnected");
        GetTree().Connect("network_peer_disconnected", this, "ClientDisconnected");
        GetTree().Connect("connected_to_server", this, "ConnectionSuccess");
        GetTree().Connect("connection_failed", this, "ConnectionFailed");
        GetTree().Connect("server_disconnected", this, "ConnectionRemoved");

        _game = GetNode("/root/Game") as Game;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_active)
        {
            if (IsNetworkMaster())
            {
                while(Snapshots.Count > _game.World.BackRecTime / delta)
                {
                    Snapshots.RemoveAt(0);
                }
                SnapShot sn = new SnapShot();
                sn.SnapNum = _game.World.LocalSnapNum;

                Snapshots.Add(sn);

                string packetString = BuildPacketString(sn);
                byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
                RpcUnreliable(nameof(ReceivePacket), packetBytes);
            }
        }
    }

    public void Disconnect()
    {
        // TODO - implement
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
        _game.Quit();
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
        _game.World.StartWorld();

        AddPlayer(_id, true);
        
        _active = true;
	}

    private void AddPlayer(int id, bool playerControlled)
    {
        PlayerController c = _game.World.AddPlayer(id, playerControlled);

        if (c != null)
        {
            _pc = c;
            _game.LoadUI(_pc);
        }

        Player p = _game.World.GetNode(id.ToString()) as Player;
        p.PlayerControlled = playerControlled;
        Peer pe = new Peer(id, p);
        PeerList.Add(pe);
        p.Peer = pe;
    }
    
    private void SyncWorld(int id)
    {
        // TODO - send over all ents to new player?
        foreach(Peer p in PeerList)
        {
            RpcId(id, nameof(SyncWorldReceive), _game.World.LocalSnapNum, ET.PLAYER, p.ID);
        }
    }

    // only clients receive this, only on first connect?
    [Remote]
    public void SyncWorldReceive(int serverSnapNum, ET entType, int id)
    {
        _game.World.ServerSnapNum = serverSnapNum;
        switch (entType)
        {
            case ET.PLAYER:
                if (id == GetTree().GetNetworkUniqueId())
                {
                    _game.World.StartWorld();
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
    public void ReceivePMovementServer(byte[] packet)
    {
        string pkStr = Encoding.UTF8.GetString(packet);
        string[] split = pkStr.Split(",");

        int serverSnapNumAck = Convert.ToInt32(split[0]);
        int id = Convert.ToInt32(split[1]);

        Peer p = PeerList.Where(x => x.ID == id).FirstOrDefault();
        if (p == null)
        {
            return;
        }
        p.Ping = (_game.World.LocalSnapNum - serverSnapNumAck) * _game.World.FrameDelta;

        for (int i = 2; i < split.Length; i++)
        {
            PlayerCmd pCmd = new PlayerCmd();
            pCmd.playerID = id;
            pCmd.snapshot = Convert.ToInt32(split[i++]);
            pCmd.move_forward = float.Parse(split[i++]);
            pCmd.move_right = float.Parse(split[i++]);
            pCmd.move_up = float.Parse(split[i++]);
            pCmd.aim = new Basis(
                                new Vector3(float.Parse(split[i++]), float.Parse(split[i++]), float.Parse(split[i++])),
                                new Vector3(float.Parse(split[i++]), float.Parse(split[i++]), float.Parse(split[i++])),
                                new Vector3(float.Parse(split[i++]), float.Parse(split[i++]), float.Parse(split[i++]))
                                );
            pCmd.cam_angle = float.Parse(split[i++]);
            pCmd.rotation = new Vector3(float.Parse(split[i++]), float.Parse(split[i++]), float.Parse(split[i++]));
            pCmd.attack = float.Parse(split[i++]);
            pCmd._projName = split[i++];
            pCmd.attackDir = new Vector3(float.Parse(split[i++]), float.Parse(split[i++]), float.Parse(split[i]));
            if (pCmd.snapshot > p.LastSnapshot)
            {
                p.Player.pCmdQueue.Add(pCmd);
            }
        }
    }

    // FIXME - only h/a of owning player
    public void UpdatePlayer(int id, float ping, float health, float armour, Vector3 org, Vector3 velo, Vector3 rot)
    {
        Peer p = PeerList.Where(p2 => p2.ID == id).First();
        p.Ping = ping;
        p.Player.SetServerState(org, velo, rot, health, armour);
    }

    public void SendPMovement(int RecID, int id, List<PlayerCmd> pCmdQueue)
    {       
        string packetString = BuildClientCmdPacket(id, pCmdQueue);
        byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);

        RpcUnreliableId(RecID, nameof(ReceivePMovementServer), packetBytes);
    }

    [Slave]
    public void ReceivePacket(byte[] packet)
    {
        string pkStr = Encoding.UTF8.GetString(packet);
        string[] split = pkStr.Split(",");
        int snapNum = Convert.ToInt32(split[0]);
        _game.World.ServerSnapNum = snapNum;

        for (int i = 1; i < split.Length; i++)
        {
            ET type = (ET)Convert.ToInt32(split[i++]);
            switch(type)
            {
                case ET.PLAYER:
                    ProcessPlayerPacket(split, ref i);
                    break;
                case ET.PROJECTILE:
                    ProcessProjectilePacket(split, ref i);
                    break;
            }
        }
        _game.World.LocalSnapNum = _game.World.LocalSnapNum < _game.World.ServerSnapNum ? _game.World.ServerSnapNum : _game.World.LocalSnapNum;
    }

    private void ProcessProjectilePacket(string[] split, ref int i)
    {
        string pName = split[i++];
        string pID = split[i++];
        Vector3 porg = new Vector3(
            float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
        );
        Vector3 pvel = new Vector3(
            float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
        );

        Vector3 prot = new Vector3(
            float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i],  System.Globalization.CultureInfo.InvariantCulture)
        );
        _game.World.ProjectileManager.AddNetworkedProjectile(pName, pID, porg, pvel, prot);
    }

    private void ProcessPlayerPacket(string[] split, ref int i)
    {
        int id = Convert.ToInt32(split[i++]);
        float ping = float.Parse(split[i++]);
        int health = Convert.ToInt32(split[i++]);
        int armour = Convert.ToInt32(split[i++]);
        Vector3 org = new Vector3(
            float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
        );
        Vector3 vel = new Vector3(
            float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
        );

        Vector3 rot = new Vector3(
            float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i++],  System.Globalization.CultureInfo.InvariantCulture)
            , float.Parse(split[i],  System.Globalization.CultureInfo.InvariantCulture)
        );
        UpdatePlayer(id, ping, health, armour, org, vel, rot);
    }

    private string BuildPacketString(SnapShot sn)
    {
        sb.Clear();
        sb.Append(_game.World.LocalSnapNum);
        sb.Append(",");
        // players
        foreach(Peer p in PeerList)
        {
            Vector3 org = p.Player.ServerState.Origin;
            Vector3 velo = p.Player.ServerState.Velocity;
            Vector3 rot = p.Player.Mesh.Rotation;

            PlayerSnap ps = new PlayerSnap();
            ps.Origin = org;
            ps.Velocity = velo;
            ps.NodeName = p.Player.Name;
            ps.Rotation = rot;
            ps.CmdQueue = p.Player.pCmdQueue;
            sn.PlayerSnap.Add(ps);

            sb.Append((int)ET.PLAYER);
            sb.Append(",");
            sb.Append(p.ID);
            sb.Append(",");
            sb.Append(p.Ping);
            sb.Append(",");
            sb.Append(p.Player.CurrentHealth);
            sb.Append(",");
            sb.Append(p.Player.CurrentArmour);
            sb.Append(",");
            sb.Append(org.x);
            sb.Append(",");
            sb.Append(org.y);
            sb.Append(",");
            sb.Append(org.z);
            sb.Append(",");
            sb.Append(velo.x);
            sb.Append(",");
            sb.Append(velo.y);
            sb.Append(",");
            sb.Append(velo.z);
            sb.Append(",");
            sb.Append(rot.x);
            sb.Append(",");
            sb.Append(rot.y);
            sb.Append(",");
            sb.Append(rot.z);
            sb.Append(",");
        }
        // projectiles
        foreach(Rocket p in _game.World.ProjectileManager.Projectiles)
        {
            sb.Append((int)ET.PROJECTILE);
            sb.Append(",");
            sb.Append(p.Name);
            sb.Append(",");
            sb.Append(p.PlayerOwner.ID);
            sb.Append(",");
            sb.Append(p.GlobalTransform.origin.x);
            sb.Append(",");
            sb.Append(p.GlobalTransform.origin.y);
            sb.Append(",");
            sb.Append(p.GlobalTransform.origin.z);
            sb.Append(",");
            sb.Append(p.Velocity.x);
            sb.Append(",");
            sb.Append(p.Velocity.y);
            sb.Append(",");
            sb.Append(p.Velocity.z);
            sb.Append(",");
            sb.Append(p.Rotation.x);
            sb.Append(",");
            sb.Append(p.Rotation.y);
            sb.Append(",");
            sb.Append(p.Rotation.z);
            sb.Append(",");
        }

        if (sb.Length > (_game.World.LocalSnapNum.ToString().Length + 1))
        {
            sb.Remove(sb.Length - 1, 1);
        }
        return sb.ToString();
    }

    private string BuildClientCmdPacket(int id, List<PlayerCmd> pCmdQueue)
    {
        sb.Clear();
        sb.Append(_game.World.ServerSnapNum);
        sb.Append(",");
        sb.Append(id.ToString());
        sb.Append(",");
        foreach(PlayerCmd pCmd in pCmdQueue)
        {
            sb.Append(pCmd.snapshot);
            sb.Append(",");
            sb.Append(pCmd.move_forward);
            sb.Append(",");
            sb.Append(pCmd.move_right);
            sb.Append(",");
            sb.Append(pCmd.move_up);
            sb.Append(",");
            sb.Append(pCmd.aim.x.x);
            sb.Append(",");
            sb.Append(pCmd.aim.x.y);
            sb.Append(",");
            sb.Append(pCmd.aim.x.z);
            sb.Append(",");
            sb.Append(pCmd.aim.y.x);
            sb.Append(",");
            sb.Append(pCmd.aim.y.y);
            sb.Append(",");
            sb.Append(pCmd.aim.y.z);
            sb.Append(",");
            sb.Append(pCmd.aim.z.x);
            sb.Append(",");
            sb.Append(pCmd.aim.z.y);
            sb.Append(",");
            sb.Append(pCmd.aim.z.z);
            sb.Append(",");
            sb.Append(pCmd.cam_angle);
            sb.Append(",");
            sb.Append(pCmd.rotation.x);
            sb.Append(",");
            sb.Append(pCmd.rotation.y);
            sb.Append(",");
            sb.Append(pCmd.rotation.z);
            sb.Append(",");
            sb.Append(pCmd.attack);
            sb.Append(",");
            sb.Append("\"" + pCmd.projName + "\"");
            sb.Append(",");
            sb.Append(pCmd.attackDir.x);
            sb.Append(",");
            sb.Append(pCmd.attackDir.y);
            sb.Append(",");
            sb.Append(pCmd.attackDir.z);
            sb.Append(",");
        }
        if (pCmdQueue.Count > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }
        return sb.ToString();
    }
}
