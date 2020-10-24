using Godot;

public class SuperNailGun : Weapon
{
    public SuperNailGun()
    {
        _damage = 13;
        _speed = 150;
        _minAmmoRequired = 2;
        _clipSize = -1;
        _coolDown = 0.2f;
        _weaponShotType = WEAPONSHOTTYPE.PROJECTILE;
        _ammoType = AMMUNITION.NAILS;
        _weaponResource = "res://Scenes/Weapons/SuperNailGun.tscn";
        _projectileResource = "res://Scenes/Weapons/Nail.tscn";
        _weapon = WEAPONTYPE.SUPERNAILGUN;
    }
}