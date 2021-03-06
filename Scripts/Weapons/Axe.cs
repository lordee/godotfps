using Godot;
public class Axe : Weapon
{
    public Axe() {
        _damage = 25;
        _minAmmoRequired = 0;
        _ammoType = AMMUNITION.NONE;
        _weaponResource = "res://Scenes/Weapons/Axe.tscn";
        _coolDown = 0.5f;
        _weaponShotType = WEAPONSHOTTYPE.MELEE;
        _weaponRange = 10;
        _weaponType = WEAPONTYPE.AXE;
    }
}