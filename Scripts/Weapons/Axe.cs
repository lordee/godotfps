using Godot;

public class Axe : Weapon
{
    public Axe() {
        GD.Print("Axe");
        _damage = 25;
        _minAmmoRequired = 0;
        _ammoType = Ammunition.Axe;
        _weaponResource = "res://Scenes/Weapons/Axe.tscn";
        _coolDown = 0.5f;
        _weaponType = WeaponType.Melee;
        _shootRange = 10f;
    }
}