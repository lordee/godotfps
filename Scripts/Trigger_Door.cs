using Godot;
using System;
using System.Collections.Generic;
using System.Collections;

public class Trigger_Door : Area
{
    private Godot.Collections.Dictionary _fields;
    private float _speed = 0; // move speed, need to convert
    private float _wait = 3; // delay before closing again
    private float _waitCount;
    private Vector3 _closeLocation;
    private Vector3 _openLocation;
    private Vector3 _destination;
    private float _damage = 2;
    private float _angle = 0;
    private float _lip = .8f;
    private World _world;
    // lip
    // sounds
    AudioStreamPlayer3D _sndOpen = null;
    AudioStreamPlayer3D _sndClose = null;
    // team
    // allowteams
    // FIXME should use bitmask
    List<int> _allowTeams = new List<int>();
    private float _maxHealth = 0;
    private float _health = 0;
    MeshInstance _mesh;
    List<CollisionShape> _collisions = new List<CollisionShape>();
    List<Trigger_Door> _linkedDoors = new List<Trigger_Door>();
    private STATE _open = STATE.CLOSE;
    Vector3 _lastPos;


    public override void _Ready()
    {
        _world = GetNode("/root/Initial/World") as World;
    }

    public void Init(Godot.Collections.Dictionary fields)
    {
        _fields = fields;
        Connect("area_entered", this, "_on_area_entered");
        Connect("area_exited", this, "_on_area_exited");

        string nodeName = this.Name;
        // example: entity_entity_316_brush_brush_0_trigger
        string entityName = nodeName.Substring(("entity_").Length);
        entityName = entityName.Substring(("entity_").Length);
        entityName = "entity_" + entityName.Substring(0, entityName.Find("_", 0));

        // link doors to mesh
        _mesh = GetNode("/root/Initial/Map/QodotMap/Meshes/" + entityName) as MeshInstance;
        // link doors to collision
        Godot.Collections.Array collisions = (GetNode("/root/Initial/Map/QodotMap/Collision/Static Collision") as StaticBody).GetChildren();
        foreach(CollisionShape cs in collisions)
        {
            if (cs.Name.Contains(entityName))
            {
                _collisions.Add(cs);
            }
        }

        foreach (DictionaryEntry kvp in fields)
        {
            string val = kvp.Value.ToString();
            switch (kvp.Key.ToString().ToLower())
            {
                case "allowteams":
                    string teamVal = val;
                    if (teamVal.Contains("blue"))
                    {
                        _allowTeams.Add(1);
                    }
                    if (teamVal.Contains("red"))
                    {
                        _allowTeams.Add(2);
                    }
                    if (teamVal.Contains("yellow"))
                    {
                        _allowTeams.Add(3);
                    }
                    if (teamVal.Contains("green"))
                    {
                        _allowTeams.Add(4);
                    }
                    break;
                case "team_no":
                        _allowTeams.Add(Convert.ToInt16(val));
                    break;
                case "lip":
                    _lip = (float)Convert.ToDouble(val);
                    _lip = _lip / 10;
                    break;
                case "angle":
                    _angle = (float)Convert.ToDouble(val);
                    break;
                case "dmg":
                    _damage = (float)Convert.ToDouble(val);
                    break;
                case "speed":
                    _speed = (float)Convert.ToDouble(val);
                    _speed = _speed / 10;
                    break;
                case "sounds":
                    int type = Convert.ToInt16(val);
                    SetupSound(type);
                    break;
                case "wait":
                    _wait = (float)Convert.ToDouble(val);
                    break;
            }
        }
        AABB boundingBox = _mesh.GetAabb();
        _closeLocation = GlobalTransform.origin;
        _lastPos = _closeLocation;
        _destination = _closeLocation;
        _openLocation = GlobalTransform.origin;

        // -1 is up
        // -2 is down
        // 0 and above are horizontal
        if (_angle == -1)
        {
            _openLocation.y -= (boundingBox.Size.y - _lip);
        }
        else if (_angle == -2)
        {
            _openLocation.y += (boundingBox.Size.y - _lip);
        }
        else
        {
            // FIXME until we run into something that uses it, we just assume full x or z bounding box
            float moveDist = 0f;
            if (_angle == 90f || _angle == 270f)
            {
                moveDist = boundingBox.Size.x - _lip;
            }
            else
            {
                moveDist = boundingBox.Size.z - _lip;
            }
            
            Spatial moveNode = new Spatial();
            AddChild(moveNode);
            moveNode.RotateY(Mathf.Deg2Rad(_angle));
            moveNode.Translate(new Vector3(0,0,moveDist));
            _openLocation = moveNode.GlobalTransform.origin;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (_open)
        {
            case STATE.OPEN:
                _waitCount += delta;
                if (_waitCount >= _wait)
                {
                    ToggleOpenNoLinked();
                }
                break;
            case STATE.OPENING:
            case STATE.CLOSING:
                PlaySound();
                _lastPos = GlobalTransform.origin;
                Move(delta);               
                break;
        }
    }
    // FIXME - change body entered to generic node to process pipes, flags etc
    private void ProcessBlockers(Area body)
    {
        // TODO
        // if object inside doors new position
            // if the object can be moved
                // move the object
            // can't be moved
                // do damage
                // move back to last position

        // physics handles this already and automoves, need to test large square blocks that would squish
    }

    private void Move(float delta)
    {
        float dist = (_destination - GlobalTransform.origin).Length();
        Vector3 dest = new Vector3();
        float snapLimit = 0.2f;//1f/64f;
        if (dist < snapLimit)
        {
            // snap
            Transform glob = GlobalTransform;
            glob.origin = _destination;
            this.GlobalTransform = glob;

            if (_open == STATE.OPENING)
            {
                _open = STATE.OPEN;
            }
            else if (_open == STATE.CLOSING)
            {
                _open = STATE.CLOSE;
            }
        }
        else
        {
            // move
            dest = (_destination - GlobalTransform.origin).Normalized();
            dest *= _speed * delta;
            GlobalTranslate(dest);
            _mesh.GlobalTranslate(dest);
            foreach(CollisionShape cs in _collisions)
            {
                cs.GlobalTranslate(dest);
            }
        }
    }

    private void PlaySound()
    {
        if (_open == STATE.OPENING)
        {
            if (_sndOpen != null && !_sndOpen.Playing)
            {
                _sndOpen.Play();
            }
        }
        else if (_open == STATE.CLOSING)
        {
            if (_sndClose != null && !_sndClose.Playing)
            {
                _sndClose.Play();
            }
        }   
    }

    private void SetupSound(int type)
    {
        AudioStream open = null;
        AudioStream close = null;
        switch (type)
        {
            case 1:
                open = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/doormv1.wav");
                close = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/drclos4.wav");
                break;
            case 2:
                open = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/hydro1.wav");
                close = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/hydro2.wav");
                break;
            case 3:
                open = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/stndr1.wav");
                close = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/stndr2.wav");
                break;
            case 4:
                open = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/ddoor1.wav");
                close = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/ddoor2.wav");
                break;
            default:
                // FIXME test
                open = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/doormv1.wav");
                close = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/doors/drclos4.wav");
                break;
        }

        if (open != null && close != null)
        {
            _sndOpen = new AudioStreamPlayer3D();
            _sndClose = new AudioStreamPlayer3D();
            _sndOpen.Stream = open;
            _sndClose.Stream = close;
            _sndOpen.Name = "sndOpen";
            _sndClose.Name = "sndClose";
            AddChild(_sndOpen);
            AddChild(_sndClose);
        }
    }

    public void AddLinkedDoor(Trigger_Door door)
    {
        _linkedDoors.Add(door);
    }

    public AABB GetAABB()
    {
        return _mesh.GetAabb();
    }

    public void ToggleOpen()
    {
        foreach (Trigger_Door door in _linkedDoors)
        {
            door.ToggleOpenNoLinked();
        }
        ToggleOpenNoLinked();
    }
    
    public void ToggleOpenNoLinked()
    {
        if (_open == STATE.OPEN)
        {
            Close();
        }
        else if (_open == STATE.CLOSE)
        {
            Open();
        }
    }

    private void Close()
    {
        _open = STATE.CLOSING;
        if (_maxHealth > 0)
        {
            _health = _maxHealth;
        }
        _destination = _closeLocation;
        if (_speed <= 0)
        {
            GD.PrintErr("No speed set on door entity");
        }
    }

    private void Open()
    {
        _waitCount = 0;
        _destination = _openLocation;
        _open = STATE.OPENING;

        if (_speed <= 0)
        {
            GD.PrintErr("No speed set on door entity");
        }
    }

    private bool CanOpen(Player p)
    {
        bool canOpen = false;
        if (_allowTeams.Count > 0)
        {
            if (_allowTeams.Contains(p.Team))
            {
                canOpen = true;
            }
        }
        else
        {
            canOpen = true;
        }

        return canOpen;
    }

    private void _on_area_entered(Area body)
    {
        if (_open == STATE.OPENING || _open == STATE.CLOSING)
        {
            ProcessBlockers(body);
        }

        if (body.GetParent() is Player p)
        {
            if (_open == STATE.CLOSE)
            {
                if (CanOpen(p))
                {
                    ToggleOpen();
                }
            }
        }
    }
    
    private void _on_area_exited(Area body)
    {
    }
}