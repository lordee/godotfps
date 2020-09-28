using Godot;

public class SuperShotgun : Weapon
{
    public SuperShotgun() {
        _damage = 50;
        _minAmmoRequired = 2;
        _clipSize = 16;
        _clipLeft = _clipSize;
        _coolDown = 1.0f;
        _reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.SPREAD;
        _weaponRange = 2048;
        _pelletCount = 16;
        _spread = new Vector3(.14f, .08f, 0f);
        _ammoType = AMMUNITION.SHELLS;
        _weaponResource = "res://Scenes/Weapons/SuperShotgun.tscn";
        _weapon = WEAPONTYPE.SUPERSHOTGUN;
    }
}