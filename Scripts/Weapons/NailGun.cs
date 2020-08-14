using Godot;

public class NailGun : Weapon
{
    public NailGun()
    {
        Damage = 9;
        Speed = 50;
        _minAmmoRequired = 1;
        _clipSize = -1;
        _coolDown = 0.2f;
        _weaponShotType = WEAPONSHOTTYPE.PROJECTILE;
        _ammoType = AMMUNITION.NAILS;
        _weaponResource = "res://Scenes/Weapons/NailGun.tscn";
        _projectileResource = "res://Scenes/Weapons/Nail.tscn";
        _weapon = WEAPONTYPE.NAILGUN;
    }
}