using Godot;
using System;
using System.Collections.Generic;

public class Trigger_Door : Area
{
    private Godot.Collections.Dictionary _fields;
    private float _speed = 0; // move speed, need to convert
    private float _wait = 3; // delay before closing again
    private float _waitCount;
    private Vector3 _closeLocation;
    private Vector3 _openLocation;
    private Vector3 _destination;
    private bool _open = false;
    private float _damage = 0;
    private float _angle = 0;
    private World _world;
    // lip
    // sounds
    AudioStreamPlayer3D _sndOpen = null;
    AudioStreamPlayer3D _sndClose = null;
    // team
    // allowteams
    private float _maxHealth = 0;
    private float _health = 0;
    MeshInstance _mesh;
    List<CollisionShape> _collisions = new List<CollisionShape>();
    List<Trigger_Door> _linkedDoors = new List<Trigger_Door>();

    public override void _Ready()
    {
        _world = GetNode("/root/Initial/World") as World;
    }

    public void Init(Godot.Collections.Dictionary fields)
    {
        _fields = fields;
        Connect("area_entered", this, "_on_area_entered");
        Connect("area_exited", this, "_on_area_exited");

        string nodeName = this.GetName();
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

        // todo fields, open location
        foreach(KeyValuePair<object, object> kvp in fields)
        {
            switch (kvp.Key.ToString().ToLower())
            {
                case "allowteams":

                    break;
                case "lip":

                    break;
                case "angle":
                    _angle = (float)Convert.ToDouble(kvp.Value);
                    break;
                case "team":

                    break;
                case "dmg":
                    _damage = (float)Convert.ToDouble(kvp.Value);
                    break;
                case "speed":
                    _speed = (float)Convert.ToDouble(kvp.Value);
                    break;
                case "sounds":
                    int type = Convert.ToInt16(kvp.Value);
                    SetupSound(type);
                    break;
                case "wait":
                    _wait = (float)Convert.ToDouble(kvp.Value);
                    break;
            }
        }
        _speed = 10;
        AABB boundingBox = _mesh.GetAabb();
        _closeLocation = GlobalTransform.origin;
        _destination = _closeLocation;
        _openLocation = GlobalTransform.origin;

        // -1 is up
        // -2 is down
        // 0 and above are horizontal
        if (_angle == -1)
        {
            _openLocation.y -= boundingBox.Size.y;
        }
        else if (_angle == -2)
        {
            _openLocation.y += boundingBox.Size.y;
        }
        else
        {
            // FIXME until we run into something that uses it, we just assume full x or z bounding box
            float moveDist = 0f;
            if (_angle == 90f || _angle == 270f)
            {
                moveDist = boundingBox.Size.x;
            }
            else
            {
                moveDist = boundingBox.Size.z;
            }
            
            Spatial moveNode = new Spatial();
            AddChild(moveNode);
            moveNode.RotateY(Mathf.Deg2Rad(_angle));
            moveNode.Translate(new Vector3(0,0,moveDist));
            _openLocation = moveNode.GetGlobalTransform().origin;
        }
        
    }

    public override void _PhysicsProcess(float delta)
    {
        if ((_open && GetGlobalTransform().origin != _openLocation)
            || (!_open && GlobalTransform.origin != _closeLocation))
        {
            if (_open && _sndOpen != null && !_sndOpen.Playing)
            {
                _sndOpen.Play();
            }
            else if (!_open && _sndClose != null && !_sndClose.Playing)
            {
                _sndClose.Play();
            }

            float dist = (_destination - GlobalTransform.origin).Length();
            Vector3 dest = new Vector3();
            float snapLimit = 0.1f;//1f/64f;
            if (dist < snapLimit)
            {
                // snap
                Transform glob = GlobalTransform;
                glob.origin = _destination;
                SetGlobalTransform(glob);
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
        else if (_open)
        {
            // we're done moving
            _waitCount += delta;
            if (_waitCount >= _wait)
            {
                ToggleOpenNoLinked();
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
            _sndOpen.SetStream(open);
            _sndClose.SetStream(close);
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
        if (_open)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Close()
    {
        _open = !_open;
        // TODO play sound
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
        // TODO play sound
        //sound(self, CHAN_VOICE, self.noise2, 1, ATTN_NORM);
        _destination = _openLocation;
        // toggle open state
        _open = !_open;

        if (_speed <= 0)
        {
            GD.PrintErr("No speed set on door entity");
        }
    }

    private void _on_area_entered(Area body)
    {
        if (body.GetParent() is Player p)
        {
            if (!_open)
            {
                ToggleOpen();
            }
        }
    }
    
    private void _on_area_exited(Area body)
    {
    }
}