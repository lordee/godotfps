using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class World : Node
{
    private Godot.Collections.Array spawnsTeam1 = new Godot.Collections.Array();
    private Godot.Collections.Array spawnsTeam2 = new Godot.Collections.Array();
    private int currentSpawnTeam1 = 0;
    private int currentSpawnTeam2 = 0;

    private Vector3 _up = new Vector3(0,1,0);
    public Vector3 Up { get { return _up; }}

    private float _backRecTime = 80f;
    public float BackRecTime { get { return _backRecTime; }}

    private float _gravity = 80f;
    public float Gravity { get { return _gravity; }}

    private float _waterFriction = 0.4f;
    public float WaterFriction { get { return _waterFriction; }}

    private float _flyFriction = 0.4f;
    public float FlyFriction { get { return _flyFriction; }}

    private float _groundFriction = 0.6f;
    public float GroundFriction { get { return _groundFriction; }}

    private float _stopSpeed = 10f;
    public float StopSpeed { get { return _stopSpeed; }}

    private float _accelerate = 1f; // qw 10f
    public float Accelerate { get { return _accelerate; }}

    private List<string> _playerNodeNames = new List<string>();
    
    // nodes
    private Network _network;
    private ProjectileManager _projectileManager;
    public ProjectileManager ProjectileManager {
        get { return _projectileManager; }
    }
    private Player _worldOwner;


    private float _gameTime = 0f;
    public float GameTime { get { return _gameTime; }}

    public float FrameDelta = 0f;

    
    private int _serverSnapNum = 0;
    public int ServerSnapNum { 
        get { return _serverSnapNum; }
        set { _serverSnapNum = value; }
    }

    private int _localSnapNum = 0;
    public int LocalSnapNum { 
        get { return _localSnapNum; }
        set { _localSnapNum = value; }
        }


    // FIXME - just add to scene tree manually instead of evaluating this constantly, same for network
    private bool _active = false;

    public override void _Ready()
    {
        _network = GetNode("/root/Initial/Network") as Network;
        _projectileManager = GetNode("/root/Initial/World/ProjectileManager") as ProjectileManager;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_active)
        {
            FrameDelta = delta;
            _gameTime += delta;
            _localSnapNum++;
            if (IsNetworkMaster())
            {
                _serverSnapNum = _localSnapNum;
            }
        }
    }

    public bool RewindPlayers(int ticks, float delta)
    {
        bool rewound = false;

        ticks = ticks > _network.Snapshots.Count ? _network.Snapshots.Count : ticks; // we only hold backrectime worth of ticks
        if (ticks > 0)
        {
            int pos = _network.Snapshots.Count - ticks;
            SnapShot sn = _network.Snapshots[pos];
            foreach(PlayerSnap psn in sn.PlayerSnap)
            {
                Player brp = GetNode(psn.NodeName) as Player;
                Transform t = brp.GlobalTransform;
                t.origin = psn.Origin;
                brp.GlobalTransform = t;
            }
            rewound = true;
        }

        return rewound;
    }

    public void FastForwardPlayers()
    {
        SnapShot sn = _network.Snapshots[_network.Snapshots.Count - 1];
        foreach(PlayerSnap psn in sn.PlayerSnap)
        {
            Player brp = GetNode(psn.NodeName) as Player;
            Transform t = brp.GlobalTransform;
            t.origin = psn.Origin;
            brp.GlobalTransform = t;
        }
    }

    public void StartWorld()
    {
        PackedScene main = (PackedScene)ResourceLoader.Load("res://Maps/lastresort_b5.tscn");
        Spatial inst = (Spatial)main.Instance();
        Initial of = GetNode("/root/Initial") as Initial;

        of.AddChild(inst);

        Spatial entitySpawns = GetNode("/root/Initial/Map/QodotMap/Entity Spawns") as Spatial;
        Godot.Collections.Array ents = entitySpawns.GetChildren();

        foreach(Spatial ent in ents)
        {
            Godot.Collections.Dictionary fields = ent.Get("properties") as Godot.Collections.Dictionary;

            if (fields != null)
            {
                foreach (DictionaryEntry kvp in fields)
                {
                    if (kvp.Key.ToString().ToLower().Contains("classname"))
                    {
                        switch (kvp.Value.ToString().ToLower())
                        {
                            case "info_player_start":
                                foreach(DictionaryEntry kvp2 in fields)
                                {
                                    switch(kvp2.Key.ToString().ToLower())
                                    {
                                        case "allowteams":
                                            string team = kvp2.Value.ToString().ToLower();
                                            if (team.Contains("blue"))
                                            {
                                                spawnsTeam1.Add(ent);
                                            }
                                            if (team.Contains("red"))
                                            {
                                                spawnsTeam2.Add(ent);
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        Spatial triggers = GetNode("/root/Initial/Map/QodotMap/Triggers") as Spatial;
        Godot.Collections.Array triggerents = triggers.GetChildren();
        List<Trigger_Door> doors = new List<Trigger_Door>();

        foreach (Area ent in triggerents)
        {
            Godot.Collections.Dictionary fields = ent.Get("properties") as Godot.Collections.Dictionary;

            foreach(DictionaryEntry kvp in fields)
            {
                if (kvp.Key.ToString().ToLower() == "classname")
                {
                    if (kvp.Value.ToString().ToLower() == "trigger_door")
                    {
                        // https://github.com/godotengine/godot/issues/31994#issuecomment-570073343
                        ulong objId = Convert.ToUInt64(ent.GetInstanceId());
                        ent.SetScript(ResourceLoader.Load("Scripts/Trigger_Door.cs"));
                        
                        Trigger_Door newEnt = GD.InstanceFromId(objId) as Trigger_Door;
                        newEnt.SetProcess(true);
                        newEnt.Notification(NotificationReady);
                        newEnt.Init(fields);

                        doors.Add(newEnt);
                    }
                }
            }
        }

        this.LinkDoors(doors);
        _active = true;
    }

    private void LinkDoors(List<Trigger_Door> doors)
    {
        foreach (Trigger_Door door in doors)
        {
            AABB bbox = door.GetAABB();
            bbox = bbox.Grow(0.2f);

            foreach (Trigger_Door dtest in doors)
            {
                if (dtest != door)
                {
                    AABB testBbox = dtest.GetAABB();
                    if (bbox.Intersects(testBbox))
                    {
                        door.AddLinkedDoor(dtest);
                    }
                }
            }
        }
    }

    public PlayerController AddPlayer(int networkID, bool playerControlled)
    {
        PlayerController pc = null;
        PackedScene playerScene = (PackedScene)ResourceLoader.Load("res://Scenes/Player.tscn");
        Player player = (Player)playerScene.Instance();
        this.AddChild(player);
        player.Name = networkID.ToString();
        player.ID = networkID;
        _playerNodeNames.Add(player.Name);

        if (playerControlled)
        {
            PackedScene controller = ResourceLoader.Load("res://Scenes/PlayerController.tscn") as PackedScene;
            pc = controller.Instance() as PlayerController;
            player.GetNode("Head").AddChild(pc);
            pc.Init(player);
            pc.SetProcess(true);
            pc.Notification(NotificationReady);
            Input.SetMouseMode(Input.MouseMode.Visible);
            _worldOwner = player;
        }
        player.Team = 1;

        if (IsNetworkMaster())
        {
            Spawn(player);
        }

        return pc;
    }

    public void Spawn(Player p)
    {
        p.Spawn(this.GetNextSpawn(p.Team));        
    }

    public Vector3 GetNextSpawn(int teamID)
    {
        Spatial spawn = null;
        switch (teamID)
            {
                case 1:
                    if (spawnsTeam1.Count <= currentSpawnTeam1)
                    {
                        currentSpawnTeam1 = 0;
                    }
                    spawn = (Spatial)spawnsTeam1[currentSpawnTeam1];
                    currentSpawnTeam1++;
                break;
                case 2:
                    if (spawnsTeam2.Count <= currentSpawnTeam2)
                    {
                        currentSpawnTeam2 = 0;
                    }
                    spawn = (Spatial)spawnsTeam2[currentSpawnTeam2];
                    currentSpawnTeam2++;
                break;
                case 9:
                    // nothing for now, just break
                    
                break;
            }
            return teamID == 9 ? new Vector3(0,10,0) : spawn.Translation;
    }
}
