using Godot;

abstract public class Weapon : MeshInstance
{
    static public float Damage;
    static public float Speed;
    protected int _minAmmoRequired;
    protected int _clipSize;
    protected float _coolDown;
    protected WEAPON _weapon;
    protected WEAPONTYPE _weaponType;
    protected AMMUNITION _ammoType;
    protected int _shootRange;
    protected float _reloadTime;
    private float _timeSinceLastShot;
    public float TimeSinceLastShot
    {
        get {
            return _timeSinceLastShot;
        }
        set {
            if (value > 0.1f && _muzzleFlash != null)
            {
                _muzzleFlash.Hide();
            }
            _timeSinceLastShot = value;
        }
    }
    public int AmmoLeft;
    public bool Reloading = false;

    protected int _clipLeft = 0;
    public int ClipLeft
    {
        // TODO - pick one
        get {
            return _clipSize == -1 ? 999 : _clipLeft;
        }
        set {
            _clipLeft = _clipSize == -1 ? 999 : value;
        }
    }

    protected float _timeSinceReloaded;
    public float TimeSinceReloaded
    {
        get {
            return _timeSinceReloaded;
        }
        set {
            _timeSinceReloaded = value;
            if (_timeSinceReloaded > _reloadTime && this.Reloading)
            {
                this.Reload(true);
            }
        }
    }


    protected string _weaponResource;
    protected MeshInstance _weaponMesh;
    protected string _projectileResource;
    //protected Projectile _projectileMesh;
    protected PackedScene _projectileScene;
    private Vector3 _weaponPosition = new Vector3(.5f, 1f, -1f);
    private Sprite3D _muzzleFlash;
    private AudioStreamPlayer3D _shootSound;
    private AudioStreamPlayer3D _reloadSound;

    // Nodes
    Game _game;
    Player _playerOwner;

    // FIXME - there has to be a better way than this? values aren't being stored if i use godot nodes
    public void Init(Game game)
    {
        _game = game;
    }

    virtual public void PhysicsProcess(float delta)
    {
        this.TimeSinceLastShot += delta;
        this.TimeSinceReloaded += delta;
    }

    virtual public bool Shoot(PlayerCmd pCmd, float delta)
    {
        bool shot = false;
        if (ClipLeft >= _minAmmoRequired)
        {
            if (_timeSinceLastShot >= _coolDown)
            {
                shot = true;
                ClipLeft -= _minAmmoRequired;

                if (_muzzleFlash != null)
                {
                    _muzzleFlash.Show();
                }
                _shootSound.Play();
                TimeSinceLastShot = 0;

                // TODO - should we move to weapon classes? Maybe for pipebomb launcher at least
                switch (_weaponType)
                {
                    case WEAPONTYPE.HITSCAN:
                    case WEAPONTYPE.MELEE:
                    case WEAPONTYPE.SPREAD:

                        break;
                    case WEAPONTYPE.PROJECTILE:
                    case WEAPONTYPE.GRENADE:
                        // FIXME - i think attack button is set still and due to sync differences the client attack time is still useable?  projname is empty because client never used it.
                        // this will be an issue for hitscan weapons?
                        if (_game.Network.ID == 1 && pCmd.playerID != 1 && pCmd.projName.Length <= 2)
                        {
                            return false;
                        }
                        string name =_game.World.ProjectileManager.AddProjectile(_playerOwner, pCmd.attackDir, pCmd.projName, _weapon);
                        pCmd.projName = name;
                        break;
                }
            }
        }
        else
        {
            // force a reload
            this.Reload(false);
        }
        return shot;
    }

    public void Reload(bool reloadFinished)
    {
        if (reloadFinished)
        {
            GD.Print("Reloaded");
            _weaponMesh.Visible = true;
            _clipLeft = this.AmmoLeft < _clipSize ? this.AmmoLeft : _clipSize;
            this.Reloading = false;
        } 
        else if (!this.Reloading)
        {
            GD.Print("Reloading...");
            if (_reloadSound != null)
            {
                _reloadSound.Play();
            }
            _weaponMesh.Visible = false;
            this.TimeSinceReloaded = 0f;
            this.Reloading = true;
        }
    }

    public void Spawn(Player p, string nodeName)
    {
        PackedScene PackedScene = (PackedScene)ResourceLoader.Load(_weaponResource);
        _weaponMesh = (MeshInstance)PackedScene.Instance();
        p.AddChild(_weaponMesh);
        _playerOwner = p;
        _weaponMesh.Translation = _weaponPosition;
        _weaponMesh.Name = nodeName;
        _weaponMesh.Visible = false;
        _shootSound = (AudioStreamPlayer3D)_weaponMesh.GetNode("ShootSound");
        if (_weaponMesh.HasNode("MuzzleFlash"))
        {
            _muzzleFlash = (Sprite3D)_weaponMesh.GetNode("MuzzleFlash");
        }
        if (_weaponMesh.HasNode("ReloadSound"))
        {
            _reloadSound = (AudioStreamPlayer3D)_weaponMesh.GetNode("ReloadSound");
        }
        
        // projectile mesh
        if (_weaponType == WEAPONTYPE.PROJECTILE || _weaponType == WEAPONTYPE.GRENADE)
        {
            _projectileScene = (PackedScene)ResourceLoader.Load(_projectileResource);
        }
        
        GD.Print("Loaded " + nodeName);
    }
}