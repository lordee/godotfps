using Godot;
using System;

public class Bullet : Projectile
{
    public override void _Ready()
    {
        _particleResource = "res://Scenes/Weapons/NailTink.tscn";
        _areaOfEffect = false;
    }
}