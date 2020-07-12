using Godot;
public class Axe : Weapon
{
    public Axe() {
        Damage = 25;
        _minAmmoRequired = 0;
        _ammoType = AMMUNITION.NONE;
        _weaponResource = "res://Scenes/Weapons/Axe.tscn";
        _coolDown = 0.5f;
        _weaponType = WEAPONTYPE.MELEE;
        _weaponRange = 10;
    }
}