using Godot;

public class RocketLauncher : Weapon
{
    public RocketLauncher() {
        GD.Print("RocketLauncher");
        _minAmmoRequired = 1;
        _ammoType = Ammunition.Rockets;
        _weaponResource = "res://Scenes/Weapons/RocketLauncher.tscn";
        _projectileResource = "res://Scenes/Weapons/Rocket.tscn";
        _projectileSpeed = 40;
        _damage = 100;
        _clipSize = 4;
        _clipLeft = _clipSize == -1 ? 999 : _clipSize;
        _coolDown = 1.0f;
        _reloadTime = 4.0f;
        _weaponType = WeaponType.Projectile;
    }
}