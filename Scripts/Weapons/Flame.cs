using Godot;
using System;

public class Flame : Particles
{
    // particles don't emit unless you're looking at them....?
    private float _lifeTime = 0f;
    private float _maxLifeTime = Flamethrower.FlameLifeTime;
    public float Damage = 0;
    public WEAPONTYPE WeaponType;

    Area _area;
    public Player PlayerOwner = null;

    public override void _Ready()
    {
        _area = GetNode("Area") as Area;
        _area.Connect("body_entered", this, "on_Flame_Entered");
        _area.Connect("body_exited", this, "on_Flame_Exited");
    }

    public override void _Process(float delta)
    {
        _lifeTime += delta;
        if (!this.Emitting || _lifeTime >= _maxLifeTime)
        {
            GetTree().QueueDelete(this);
        }
    }

    private void on_Flame_Entered(Player p)
    {
        p.TakeDamage(PlayerOwner, this.GlobalTransform.origin, Damage);

        p.AddDebuff(PlayerOwner, WeaponType, Flamethrower.BurnLength);
    }

    private void on_Flame_Exited(Player p)
    {
        
    }
}
