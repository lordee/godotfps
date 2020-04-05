using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Network : Node
{
    Initial _initial;
    World _world;
    ProjectileManager _projectileManager;
    
    private int maxPlayers = 8;
    
    PlayerController _pc;
    private int _id;
    public List<Peer> PeerList = new List<Peer>();

    private bool _active = false;
    private float _lastPingSent = 0f;
    private float _lastPingGT = 0f;

    StringBuilder sb = new StringBuilder();

    public List<SnapShot> Snapshots = new List<SnapShot>();
    

    public override void _Ready()
    {
        GetTree().Connect("network_peer_connected", this, "ClientConnected");
        GetTree().Connect("network_peer_disconnected", this, "ClientDisconnected");
        GetTree().Connect("connected_to_server", this, "ConnectionSuccess");
        GetTree().Connect("connection_failed", this, "ConnectionFailed");
        GetTree().Connect("server_disconnected", this, "ConnectionRemoved");

        _initial = GetNode("/root/Initial") as Initial;
        _world = GetNode("/root/Initial/World") as World;
        _projectileManager = GetNode("/root/Initial/World/ProjectileManager") as ProjectileManager;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_active)
        {
            _lastPingSent += delta;
            if (IsNetworkMaster())
            {
                while(Snapshots.Count > _world.BackRecTime / delta)
                {
                    Snapshots.RemoveAt(0);
                }
                SnapShot sn = new SnapShot();
                sn.SnapNum = _world.LocalSnapNum;

                Snapshots.Add(sn);

                string packetString = BuildPacketString(sn);
                byte[] packetBytes = Encoding.UTF8.GetBytes(packetString);
                RpcUnreliable(nameof(ReceivePacket), packetBytes);
            }
            else
            {
                // FIXME - stop resending commands after trying 3 times, also send pcmds in 1 packet...
                foreach(PlayerCmd pcmd in _pc.Player.pCmdQueue)
                {
                    SendPMovement(1, _id, pcmd);
                }
            }
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

    // FIXME - move to a different node
    private void LoadUI(Player p)
    {
        PackedScene uips = ResourceLoader.Load("res://Scenes/UI.tscn") as PackedScene;
        UI ui = uips.Instance() as UI;
        _initial.AddChild(ui);
        ui.Init(p);
    }

    private void AddPlayer(int id, bool playerControlled)
    {
        PlayerController c = _world.AddPlayer(id, playerControlled);

        if (c != null)
        {
            _pc = c;
            LoadUI(_pc.Player);
        }

        Player p = GetNode("/root/Initial/World/" + id) as Player;
        p.PlayerControlled = playerControlled;
        PeerList.Add(new Peer(id, p));
    }
    
    private void SyncWorld(int id)
    {
        // TODO - send over all ents to new player?
        foreach(Peer p in PeerList)
        {
            RpcId(id, nameof(SyncWorldReceive), _world.LocalSnapNum, ET.PLAYER, p.ID);
        }
    }

    // only clients receive this, only on first connect?
    [Remote]
    public void SyncWorldReceive(int serverSnapNum, ET entType, int id)
    {
        _world.ServerSnapNum = serverSnapNum;
        switch (entType)
        {
            case ET.PLAYER:
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
    public void ReceivePMovementServer(int snapNum, int id, float move_forward, float move_right, float move_up, Vector3 aimx
    , Vector3 aimy, Vector3 aimz, float camAngle, float rotX, float rotY, float rotZ, float att, float attDirX
    , float attDirY, float attDirZ)
    {
        Peer p = PeerList.Where(x => x.ID == id).FirstOrDefault();
        if (p == null)
        {
            return;
        }
        if (snapNum <= p.LastSnapshot)
        {
            return;
        }

        Player pl = p.Player;
        
        Basis aim = new Basis(aimx, aimy, aimz);
        
        PlayerCmd pCmd;
        pCmd.playerID = id;
        pCmd.snapshot = snapNum;
        pCmd.aim = aim;
        pCmd.move_forward = move_forward;
        pCmd.move_right = move_right;
        pCmd.move_up = move_up;
        pCmd.cam_angle = camAngle;
        pCmd.rotation = new Vector3(rotX, rotY, rotZ);
        pCmd.attack = att;
        pCmd.attackDir = new Vector3(attDirX, attDirY, attDirZ);
        //p.pCmdQueue.Enqueue(pCmd);
        pl.pCmdQueue.Add(pCmd);
    }

    // FIXME - only h/a of owning player
    public void UpdatePlayer(int id, float health, float armour, Vector3 org, Vector3 velo, Vector3 rot)
    {
        //Player p = PeerList.Where(p2 => p2.ID == id).First().Player;
        Player p = GetNode("/root/Initial/World/" + id.ToString()) as Player;
        p.SetServerState(org, velo, rot, health, armour);
    }

    public void SendPMovement(int RecID, int id, PlayerCmd pCmd)
    {       
        RpcUnreliableId(RecID, nameof(ReceivePMovementServer), pCmd.snapshot, id, pCmd.move_forward, pCmd.move_right
        , pCmd.move_up, pCmd.aim.x, pCmd.aim.y, pCmd.aim.z, pCmd.cam_angle, pCmd.rotation.x, pCmd.rotation.y
        , pCmd.rotation.z, pCmd.attack, pCmd.attackDir.x, pCmd.attackDir.y, pCmd.attackDir.z);
    }

    [Slave]
    public void ReceivePacket(byte[] packet)
    {
        string pkStr = Encoding.UTF8.GetString(packet);
        string[] split = pkStr.Split(",");
        int snapNum = Convert.ToInt32(split[0]);
        _world.ServerSnapNum = snapNum;

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
        _world.LocalSnapNum = _world.LocalSnapNum < _world.ServerSnapNum ? _world.ServerSnapNum : _world.LocalSnapNum;
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
        _projectileManager.AddNetworkedProjectile(pName, pID, porg, pvel, prot);
    }

    private void ProcessPlayerPacket(string[] split, ref int i)
    {
        int id = Convert.ToInt32(split[i++]);
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
        UpdatePlayer(id, health, armour, org, vel, rot);
    }

    private string BuildPacketString(SnapShot sn)
    {
        sb.Clear();
        sb.Append(_world.LocalSnapNum);
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
        foreach(Rocket p in _projectileManager.Projectiles)
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

        if (sb.Length > (_world.LocalSnapNum.ToString().Length + 1))
        {
            sb.Remove(sb.Length - 1, 1);
        }
        return sb.ToString();
    }
}
