public class IncendiaryCannon : Weapon
{
    public IncendiaryCannon() {
        _damage = 40;
        _minAmmoRequired = 3;
        _clipSize = -1;
        _clipLeft = _clipSize;
        _coolDown = 1.0f;
        //_reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.PROJECTILE;
        _ammoType = AMMUNITION.ROCKETS;
        
        _weaponResource = "res://Scenes/Weapons/IncendiaryCannon.tscn";
        _projectileResource = "res://Scenes/Weapons/IncendiaryRocket.tscn";
        _weaponType = WEAPONTYPE.INCENDIARYCANNON;
        _speed = 80;
    }
}