using Godot;
using System;
using System.Collections.Generic;

public class World : Node
{
    private Godot.Collections.Array spawnsTeam1 = new Godot.Collections.Array();
    private Godot.Collections.Array spawnsTeam2 = new Godot.Collections.Array();
    private int currentSpawnTeam1 = 0;
    private int currentSpawnTeam2 = 0;

    private Vector3 _up = new Vector3(0,1,0);
    public Vector3 Up { get { return _up; }}

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
    

    private Network _network;

    public override void _Ready()
    {
        _network = GetNode("/root/Initial/Network") as Network;
    }

    public override void _PhysicsProcess(float delta)
    {
        foreach (int peer in _network.PeerList)
        {
            Player p = GetNodeOrNull(peer.ToString()) as Player;
            if (p != null)
            {
                p.ProcessMovement(delta);
            }
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
            object classname;
            try
            {
                if (fields != null)
                {
                    if (fields.ContainsKey("classname"))
                    {
                        fields.TryGetValue("classname", out classname);
                        switch (classname.ToString())
                        {
                            case "info_player_start":
                                object team;
                                fields.TryGetValue("allowteams", out team);
                                if (team.ToString().Contains("blue"))
                                {
                                    spawnsTeam1.Add(ent);
                                }
                                if (team.ToString().Contains("red"))
                                {
                                    spawnsTeam2.Add(ent);
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GD.Print("Error: " + ex.ToString());
            }
        }

        Spatial triggers = GetNode("/root/Initial/Map/QodotMap/Triggers") as Spatial;
        Godot.Collections.Array triggerents = triggers.GetChildren();
        List<Trigger_Door> doors = new List<Trigger_Door>();

        foreach (Area ent in triggerents)
        {
            Godot.Collections.Dictionary fields = ent.Get("properties") as Godot.Collections.Dictionary;
            object classname = null;
            if (fields != null)
            {
                if (fields.ContainsKey("classname"))
                {
                    fields.TryGetValue("classname", out classname);
                }
                if (classname != null && classname.ToString() == "trigger_door")
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

        this.LinkDoors(doors);
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

    public Player AddPlayer(int networkID, bool playerControlled)
    {
        PackedScene playerScene = (PackedScene)ResourceLoader.Load("res://Scenes/Player.tscn");
        Player player = (Player)playerScene.Instance();
        this.AddChild(player);
        player.SetName(networkID.ToString());
        player.ID = networkID;

        if (playerControlled)
        {
            PackedScene controller = ResourceLoader.Load("res://Scenes/PlayerController.tscn") as PackedScene;
            PlayerController pc = controller.Instance() as PlayerController;
            player.GetNode("Head").AddChild(pc);
            pc.Init(player);
        }
        
        return player;
    }

    public void Spawn(Player p)
    {
        p.SetTranslation(GetNextSpawn(p.Team));
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
            return teamID == 9 ? new Vector3(0,10,0) : spawn.GetTranslation();
    }
}
