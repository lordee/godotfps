using Godot;
public class Syringe : Weapon
{
    public static int BioDamage = 10;

    public Syringe() {
        _damage = 10;
        _minAmmoRequired = 0;
        _ammoType = AMMUNITION.NONE;
        _weaponResource = "res://Scenes/Weapons/Syringe.tscn";
        _coolDown = 0.5f;
        _weaponShotType = WEAPONSHOTTYPE.MELEE;
        _weaponRange = 10;
        _debuffLength = 999;
        _weaponType = WEAPONTYPE.SYRINGE;
    }
}