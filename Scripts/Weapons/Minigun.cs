using Godot;

public class Minigun : Weapon
{
    public Minigun() {
        _damage = 8;
        _minAmmoRequired = 1;
        _clipSize = 100;
        _clipLeft = _clipSize;
        _coolDown = 0.2f;
        _reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.SPREAD;
        _ammoType = AMMUNITION.SHELLS;
        _spread = new Vector3(.04f, .04f, 0f);
        
        _weaponResource = "res://Scenes/Weapons/Minigun.tscn";
        _projectileResource = "res://Scenes/Weapons/Bullet.tscn";
        _weaponType = WEAPONTYPE.MINIGUN;
        _speed = 300;
    }
}