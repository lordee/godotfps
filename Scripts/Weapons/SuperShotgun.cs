using Godot;
public class SuperShotgun : Weapon
{
    public SuperShotgun() {
        GD.Print("SuperShotgun");
        _damage = 50;
        _minAmmoRequired = 2;
        _ammoType = Ammunition.Shells;
        _weaponResource = "res://Scenes/Weapons/SuperShotgun.tscn";
        _clipSize = 16;
        _clipLeft = _clipSize == -1 ? 999 : _clipSize;
        _coolDown = 1.0f;
        _reloadTime = 4.0f;
        _weaponType = WeaponType.Spread;
        _shootRange = 100f;
        _pelletCount = 14;
        _spread = new Vector3(.14f, .08f, 0f);
    }
}