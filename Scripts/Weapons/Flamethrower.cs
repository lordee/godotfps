using Godot;

public class Flamethrower : Weapon
{
    static public float FlameLifeTime = .1f;
    static public float BurnLength = 5f;
    static public float BurnDamage = 15;

    PackedScene particles;
    public Flamethrower() {
        _damage = 15;
        _minAmmoRequired = 1;
        _clipSize = -1;
        _clipLeft = _clipSize;
        _coolDown = 0.1f;
        //_reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.HITSCAN;
        _ammoType = AMMUNITION.CELLS;
        
        _weaponResource = "res://Scenes/Weapons/Flamethrower.tscn";
        _projectileResource = "res://Scenes/Weapons/Flame.tscn";

        particles = (PackedScene)ResourceLoader.Load(_projectileResource);

        _weaponType = WEAPONTYPE.FLAMETHROWER;
        //_weaponRange = 20;
    }

    public override bool Shoot(PlayerCmd pCmd, float delta)
    {
        // spawn particles
        Flame f = (Flame)_game.World.ParticleManager.SpawnParticle(PARTICLE.FLAMETHROWER, _playerOwner.Head.GlobalTransform, _playerOwner);
        f.Damage = _damage;
        return true;
    }
}