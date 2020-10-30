using Godot;

public class Minigun : Weapon
{
    enum SPINSOUND
    {
        NONE,
        IDLE,
        UP,
        DOWN,
    }
    private AudioStreamPlayer3D _spinDownSound;
    private AudioStreamPlayer3D _spinUpSound;
    private AudioStreamPlayer3D _idleSound;
    private SPINSOUND _spinSound = SPINSOUND.NONE;
    public bool Idle = false;

    private float _spinUpTimeRequired = 1f;
    private float _spinUpTime = 0f;
    public float SpinUpTime { get { return _spinUpTime; }}

    public Minigun() {
        _damage = 8;
        _minAmmoRequired = 1;
        _clipSize = 100;
        _clipLeft = _clipSize;
        _coolDown = 0.2f;
        _reloadTime = 4.0f;
        _weaponShotType = WEAPONSHOTTYPE.SPREAD;
        _ammoType = AMMUNITION.SHELLS;
        _spread = new Vector3(.04f, .04f, 0f);
        
        _weaponResource = "res://Scenes/Weapons/Minigun.tscn";
        _projectileResource = "res://Scenes/Weapons/Bullet.tscn";
        _weaponType = WEAPONTYPE.MINIGUN;
        _speed = 300;
    }

    override public void Spawn(Player p, string nodeName)
    {
        base.Spawn(p, nodeName);
        _spinDownSound = (AudioStreamPlayer3D)_weaponMesh.GetNode("SpinDownSound");
        _spinUpSound = (AudioStreamPlayer3D)_weaponMesh.GetNode("SpinUpSound");
        _idleSound = (AudioStreamPlayer3D)_weaponMesh.GetNode("IdleSound");
    }

    override public void PhysicsProcess(float delta)
    {
        this.TimeSinceLastShot += delta;
        this.TimeSinceReloaded += delta;
        
        if (_spinUpTime != 0 && !ShootPressed)
        {
            if (_spinUpTime <= 0)
            {
                _spinUpTime = 0;
            }
            else
            {
                _spinUpTime = (_spinUpTime - delta) < 0 ? 0 : _spinUpTime - delta;

                if (_spinSound != SPINSOUND.DOWN)
                {
                    _spinDownSound.Play();
                    _spinSound = SPINSOUND.DOWN;
                }
            }
        }
    }

    public override bool Shoot(PlayerCmd pCmd, float delta)
    {
        // track spin up
        if (_spinUpTime >= _spinUpTimeRequired)
        {
            if (Idle)
            {
                _idleSound.Play();
            }
            else
            {
                // if spun up enough, shoot
                _shootSound.Play();
                Vector3 shootTo = _playerOwner.Head.GlobalTransform.origin + pCmd.attackDir; 
                PhysicsDirectSpaceState spaceState = _playerOwner.GetWorld().DirectSpaceState;
                Godot.Collections.Dictionary res = spaceState.IntersectRay(_playerOwner.Head.GlobalTransform.origin, shootTo, new Godot.Collections.Array { this, _playerOwner }, 1);
                // if hits, use that as our far point
                if (res.Count > 0)
                {
                    Vector3 shotLoc = ((Vector3)res["position"]);
                    Vector3 norm = (Vector3)res["normal"];
                    SpreadShoot(5, true, true, shotLoc, norm);
                }
            }
        }
        else
        {
            if (_spinSound != SPINSOUND.UP)
            {
                _spinUpSound.Play();
                _spinSound = SPINSOUND.UP;
            }
            _spinUpTime += delta;
        }

        return true;
    }
}