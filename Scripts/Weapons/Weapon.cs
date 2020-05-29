using Godot;
using System;
using System.Collections.Generic;

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
    public int AmmoLeft = 100;
    public bool Reloading = false;
    protected int _pelletCount = 1;
    protected Vector3 _spread = new Vector3();

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
    public MeshInstance WeaponMesh { get { return _weaponMesh; }}
    protected string _projectileResource;
    protected PackedScene _projectileScene;
    private Vector3 _weaponPosition = new Vector3(.5f, -.3f, -1f);
    private Sprite3D _muzzleFlash;
    private AudioStreamPlayer3D _shootSound;
    private AudioStreamPlayer3D _reloadSound;
    string puffResource = "res://Scenes/Weapons/Puff.tscn";
    string bloodResource = "res://Scenes/Weapons/BloodPuff.tscn";
    PackedScene puffScene;
    PackedScene bloodScene;

    // Nodes
    Game _game;
    Player _playerOwner;

    // FIXME - there has to be a better way than this? values aren't being stored if i use godot nodes
    public void Init(Game game)
    {
        _game = game;
        // TODO - manage these in projectile manager or some other manager instead of constantly loading the scene
        puffScene = (PackedScene)ResourceLoader.Load(puffResource);
        bloodScene = (PackedScene)ResourceLoader.Load(bloodResource);
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
                        Random ran = new Random();
                        
                        float pc = _pelletCount;
                        while (pc > 0)
                        {
                            Vector3 newTo = pCmd.attackDir.Normalized();
                            if (_weaponType == WEAPONTYPE.SPREAD)
                            {
                                float ranX = (float)ran.Next(0,1) == 0 ? -1 : 1;
                                float ranY = (float)ran.Next(0,1) == 0 ? -1 : 1;
                                newTo = new Vector3(
                                    newTo.x + (float)ran.Next(0,100) * _spread.x * ranX / 100
                                    , newTo.y + (float)ran.Next(0,100) * _spread.y * ranY / 100
                                    , newTo.z);
                                
                                newTo *= 2048;
                            }

                            this.DoHit(newTo);

                            pc -= 1;
                        }
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

    private void DoHit(Vector3 shootTo)
    {
        PhysicsDirectSpaceState spaceState = _playerOwner.GetWorld().DirectSpaceState;
        Godot.Collections.Dictionary res = spaceState.IntersectRay(_playerOwner.GlobalTransform.origin, shootTo, new Godot.Collections.Array { this, _playerOwner }, 1);
        if (res.Count > 0)
        {
            Vector3 pos = (Vector3)res["position"];
            float dam = 0;
            // track if collides, track puff counts
            // leave this for fun
            if (res["collider"] is RigidBody rb)
            {           
                Vector3 impulse = (pos - _playerOwner.GlobalTransform.origin).Normalized();
                Vector3 position = pos - rb.GlobalTransform.origin;
                rb.ApplyImpulse(position, impulse * 10);

                MakePuff(PUFFTYPE.PUFF, pos, (Node)rb);
            } 
            else if (res["collider"] is KinematicBody kb)
            {
                Player pl = (Player)kb;
                dam = Damage / _pelletCount;

                pl.TakeDamage(_playerOwner, _playerOwner.GlobalTransform.origin, dam);
                MakePuff(PUFFTYPE.BLOOD, pos, (Node)kb);
            }
            else
            {
                MakePuff(PUFFTYPE.PUFF, pos, null);
            }
        }
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
        p.Head.AddChild(_weaponMesh);
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
    }

        private void MakePuff(PUFFTYPE puff, Vector3 pos, Node puffOwner)
    {
        Particles puffPart = null;
        switch (puff)
        {
            case PUFFTYPE.BLOOD:
                puffPart = (Particles)bloodScene.Instance();
            break;
            case PUFFTYPE.PUFF:
                puffPart = (Particles)puffScene.Instance();
            break;
        }

        puffPart.Translation = pos;
        if (puffOwner != null)
        {
            puffOwner.AddChild(puffPart);
        }
        else
        {
            _game.World.AddChild(puffPart);
        }
        
        puffPart.Emitting = true;
    }
}