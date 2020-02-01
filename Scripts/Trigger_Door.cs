using Godot;
using System;
using System.Collections.Generic;

public class Trigger_Door : Area
{
    private Godot.Collections.Dictionary _fields;
    private float _speed = 0; // move speed, need to convert
    private float _wait = 0; // delay before closing again
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
        
    }

    public void Init(Godot.Collections.Dictionary fields)
    {
        _fields = fields;
        Connect("area_entered", this, "_on_area_entered");
        Connect("area_exited", this, "_on_area_exited");

        // link doors to each other
        // link doors to mesh
        _mesh = GetNode("/root/Initial/Map/QodotMap/Meshes/entity_316") as MeshInstance;
        // link doors to collision
        _collisions.Add(GetNode("/root/Initial/Map/QodotMap/Collision/Static Collision/entity_316_brush_0_collision") as CollisionShape);
        _collisions.Add(GetNode("/root/Initial/Map/QodotMap/Collision/Static Collision/entity_316_brush_1_collision") as CollisionShape);



        _closeLocation = this.GetTranslation();
        // todo fields, open location
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_destination != GetTranslation())
        {
            // move
        }
        else
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
        // TODO play sound
        if (_maxHealth > 0)
        {
            _health = _maxHealth;
        }
        _destination = _closeLocation;
        if (_speed >= 0)
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
        _open = true;
        _mesh.Visible = false;
        foreach(CollisionShape cs in _collisions)
        {
            cs.SetDisabled(true);
        }

        if (_speed >= 0)
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