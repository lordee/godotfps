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
    // team
    // allowteams
    private float _maxHealth = 0;
    private float _health = 0;
    MeshInstance _mesh;
    List<CollisionShape> _collisions = new List<CollisionShape>();

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

            float moveDist = boundingBox.Size.x;
            _openLocation.x -= moveDist;
            this.LookAt(_openLocation, _world.Up);
            this.Rotate(_world.Up, Mathf.Deg2Rad(_angle));
            Vector3 forward = GetGlobalTransform().basis.z;
            _openLocation = forward * moveDist;
            
            
            
            
            /*float rads = Mathf.Deg2Rad(_angle);
            float x = Mathf.Cos(rads) * moveDist;
            float z = Mathf.Sin(rads) * moveDist;
            _openLocation.x = x;*/
            //_openLocation.z = z;


            //_angle = 90;


            // fix for quake -> godot
            //_angle = 90;
            //_openLocation.x -= boundingBox.Size.x;
            //float dist = (_openLocation - GlobalTransform.origin).Length();
            //this.Rotate(_world.Up, Mathf.Deg2Rad(_angle));
            //this.GlobalTransform.basis.Row0.Rotated(_world.Up, Mathf.Deg2Rad(_angle));
            //_angle = 90;
            /*float rads = Mathf.Deg2Rad(_angle);
            float x = Mathf.Cos(rads);
            float y = _openLocation.y;
            float z = Mathf.Sin(rads);
            _openLocation = new Vector3(x,0,z) * dist;
            _openLocation.y = y;*/

            /*_angle = 90;
            Vector3 dest = (_openLocation - _closeLocation);
            dest = dest.Rotated(_world.Up, Mathf.Deg2Rad(_angle));
            _openLocation = dest;*/

        }
        
    }

    public override void _PhysicsProcess(float delta)
    {
        if ((_open && GetGlobalTransform().origin != _openLocation)
            || (!_open && GlobalTransform.origin != _closeLocation))
        {
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
        _waitCount = 0;
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