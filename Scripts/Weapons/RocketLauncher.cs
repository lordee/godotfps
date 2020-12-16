public class RocketLauncher : Weapon
{
    public RocketLauncher() {
        _damage = 100;
        _minAmmoRequired = 1;
        _clipSize = 4;
        _clipLeft = _clipSize;
        _coolDown = 0.1f;
        _reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.PROJECTILE;
        _ammoType = AMMUNITION.ROCKETS;
        
        _weaponResource = "res://Scenes/Weapons/RocketLauncher.tscn";
        _projectileResource = "res://Scenes/Weapons/Rocket.tscn";
        _weaponType = WEAPONTYPE.ROCKETLAUNCHER;
        _speed = 100;
    }
}