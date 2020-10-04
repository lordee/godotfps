public class GrenadeLauncher : Weapon
{
    public GrenadeLauncher() {
        _damage = 120;
        _minAmmoRequired = 1;
        _clipSize = 6;
        _clipLeft = _clipSize;
        _coolDown = 0.6f;
        _reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.PROJECTILE;
        _ammoType = AMMUNITION.ROCKETS;
        
        _weaponResource = "res://Scenes/Weapons/GrenadeLauncher.tscn";
        _projectileResource = "res://Scenes/Weapons/Grenade.tscn";
        _weapon = WEAPONTYPE.GRENADELAUNCHER;
        _speed = 60;
    }
}