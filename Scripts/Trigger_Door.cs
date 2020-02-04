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
    // lip
    // sounds
    // team
    // allowteams
    private float _maxHealth = 0;
    private float _health = 0;
    MeshInstance _mesh;
    List<CollisionShape> _collisions = new List<CollisionShape>();

    public override void _Ready()
    {
        GD.Print("door ready");
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

        // TODO link doors to each other
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
        float angle = 0;
        foreach(KeyValuePair<object, object> kvp in fields)
        {
            switch (kvp.Key.ToString().ToLower())
            {
                case "allowteams":

                    break;
                case "lip":

                    break;
                case "angle":
                    angle = (float)Convert.ToDouble(kvp.Value);
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

                    break;
                case "wait":
                    _wait = (float)Convert.ToDouble(kvp.Value);
                    break;
            }
        }
        _speed = 10;
        AABB boundingBox = _mesh.GetAabb();
        //float moveSize = 0;
        _closeLocation = GlobalTransform.origin;
        _destination = _closeLocation;

        _openLocation = _closeLocation;
        _openLocation.x -= boundingBox.Size.x;
    }

    public override void _PhysicsProcess(float delta)
    {
        if ((_open && GlobalTransform.origin != _openLocation)
            || (!_open && GlobalTransform.origin != _closeLocation))
        {
            // move
            Vector3 dir = (_destination - GlobalTransform.origin).Normalized();
            dir *= _speed * delta;
            SetTranslation(dir);
            _mesh.SetTranslation(dir);
            foreach(CollisionShape cs in _collisions)
            {
                cs.SetTranslation(dir);
            }
        }
        else if (_open)
        {
            // we're done moving
            _waitCount += delta;
            if (_open && _waitCount >= _wait)
            {
                ToggleOpen();
            }
        }
    }

    private void ToggleOpen()
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
        // TODO play sound
        //sound(self, CHAN_VOICE, self.noise2, 1, ATTN_NORM);
        _destination = _openLocation;
        // toggle open state
        _open = !_open;
        /*_mesh.Visible = false;
        foreach(CollisionShape cs in _collisions)
        {
            cs.SetDisabled(true);
        }*/

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
        GD.Print("door area exited");
    }
}