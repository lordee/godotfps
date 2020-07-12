using Godot;

public class Shotgun : Weapon
{
    public Shotgun()
    {
        Damage = 25;
        _minAmmoRequired = 1;
        _clipSize = 8;
        _clipLeft = _clipSize;
        _coolDown = 0.5f;
        _reloadTime = 2.0f;
        _weaponType = WEAPONTYPE.SPREAD;
        _weaponRange = 2048;
        _pelletCount = 6;
        _spread = new Vector3(.04f, .04f, 0f);
        _ammoType = AMMUNITION.SHELLS;
        _weaponResource = "res://Scenes/Weapons/Shotgun.tscn";
        _weapon = WEAPON.SHOTGUN;
    }
}