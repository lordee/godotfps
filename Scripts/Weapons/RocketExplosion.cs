using Godot;
using System;

public class RocketExplosion : Particles
{
    // particles don't emit unless you're looking at them....?
    private float _lifeTime = 0f;
    private float _maxLifeTime = 1f;
    
    public override void _Ready()
    {
    }

    public override void _Process(float delta)
    {
        _lifeTime += delta;
        if (!this.Emitting || _lifeTime >= _maxLifeTime)
        {
            GetTree().QueueDelete(this);
        }
    }
}
