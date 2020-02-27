using Godot;

public class Shotgun : Weapon
{
    public Shotgun() {
        GD.Print("Shotgun");
        _damage = 25;
        _minAmmoRequired = 1;
        _ammoType = Ammunition.Shells;
        _weaponResource = "res://Scenes/Weapons/Shotgun.tscn";
        _clipSize = 8;
        _clipLeft = _clipSize == -1 ? 999 : _clipSize;
        _coolDown = 1.0f;
        _reloadTime = 4.0f;
        _weaponType = WeaponType.Spread;
        _shootRange = 100f;
        _pelletCount = 6;
        _spread = new Vector3(.04f, .04f, 0f);
    }
}