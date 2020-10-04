using Godot;
using System;
using System.Collections.Generic;

abstract public class Weapon : MeshInstance
{
    protected float _damage;
    public float Damage { get { return _damage; }}
    protected float _speed;
    public float Speed { get { return _speed; }}
    protected int _minAmmoRequired;
    protected int _clipSize;
    protected float _coolDown;
    protected WEAPONTYPE _weapon;
    public WEAPONTYPE WeaponType { get { return _weapon; }}
    protected WEAPONSHOTTYPE _weaponShotType;
    protected AMMUNITION _ammoType;
    protected int _weaponRange;
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

    private void SpreadShoot(int shots, bool up, bool left, Vector3 shotLoc, Vector3 norm)
    {
        Random ran = new Random();
        Vector3 dir = (shotLoc - this._playerOwner.Head.GlobalTransform.origin).Normalized();
        float range = (shotLoc - this._playerOwner.Head.GlobalTransform.origin).Length();
        Vector3 newDir;
        // FIXME - in certain directions x seems to always be "0" units away from crosshair, so just a vertical line of puffs
        while (shots > 0)
        {
            float spreadX = (float)ran.Next(0,100) * _spread.x * (left ? -1 : 1) / 100;
            float spreadY = (float)ran.Next(0,100) * _spread.y * (up ? -1 : 1) / 100;
            newDir = new Vector3(
                dir.x + (spreadX * norm.x)
                , dir.y + spreadY
                , dir.z + (spreadX * norm.z)
                );           
            
            newDir *= range;

            // FIXME - work out damage based on percentage of max weapon range travelled??
            this.DoHit(newDir);

            shots -= 1;
        }
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

                // TODO - should we move to weapon classes? Maybe for pipebomb launcher?
                switch (_weaponShotType)
                {
                    case WEAPONSHOTTYPE.HITSCAN:
                    case WEAPONSHOTTYPE.MELEE:
                        this.DoHit(pCmd.attackDir.Normalized() * _weaponRange);
                        break;
                    case WEAPONSHOTTYPE.SPREAD:
                        // raycast to extremely far away
                        Vector3 shootTo = _playerOwner.Head.GlobalTransform.origin + pCmd.attackDir; 
                        PhysicsDirectSpaceState spaceState = _playerOwner.GetWorld().DirectSpaceState;
                        Godot.Collections.Dictionary res = spaceState.IntersectRay(_playerOwner.Head.GlobalTransform.origin, shootTo, new Godot.Collections.Array { this, _playerOwner }, 1);
                        // if hits, use that as our far point
                        if (res.Count > 0)
                        {
                            Vector3 shotLoc = ((Vector3)res["position"]);
                            Vector3 norm = (Vector3)res["normal"];

                            _game.World.ParticleManager.MakePuff(PUFFTYPE.PUFF, shotLoc, null);
                            Particles p = _game.World.ParticleManager.MakePuff(PUFFTYPE.PUFF, shotLoc, null);
                            Transform t = p.GlobalTransform;

                            GD.Print(norm);
                            t.basis.y = norm;
                            t.basis.x = -t.basis.z.Cross(norm);
                            t.basis = t.basis.Orthonormalized();
                            
                            p.GlobalTransform = t;
                            //Vector3 tr = p.Translation;
                            
                            //p.Translation = new Vector3(p.Translation.x + 1, p.Translation.y, p.Translation.z);

                            Vector3 trans = new Vector3(0, 1, 1);
                            p.Translate(p.Transform.basis.Xform(trans));

                            //GD.Print(norm);

                            /*SpreadShoot(_pelletCount / 4, true, true, shotLoc, norm);
                            SpreadShoot(_pelletCount / 4, true, false, shotLoc, norm);
                            SpreadShoot(_pelletCount / 4, false, true, shotLoc, norm);
                            SpreadShoot(_pelletCount / 4, false, false, shotLoc, norm);*/
                        }

                        // trace to plane
                        // get plane normal
                        // ???
                        // move along plane normal
                        break;
                    case WEAPONSHOTTYPE.PROJECTILE:
                    case WEAPONSHOTTYPE.GRENADE:
                        // FIXME - i think attack button is set still and due to sync differences the client attack time is still useable?  projname is empty because client never used it.
                        // this will be an issue for hitscan weapons?
                        if (_game.Network.ID == 1 && pCmd.playerID != 1 && pCmd.projName.Length <= 2)
                        {
                            return false;
                        }
                        string name = _game.World.ProjectileManager.AddProjectile(_playerOwner, pCmd.attackDir, pCmd.projName, _weapon);
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

    private void DoHit(Vector3 dir)
    {
        Vector3 shootTo = _playerOwner.Head.GlobalTransform.origin + dir; 
        PhysicsDirectSpaceState spaceState = _playerOwner.GetWorld().DirectSpaceState;
        Godot.Collections.Dictionary res = spaceState.IntersectRay(_playerOwner.Head.GlobalTransform.origin, shootTo, new Godot.Collections.Array { this, _playerOwner }, 1);
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

                _game.World.ParticleManager.MakePuff(PUFFTYPE.PUFF, pos, (Node)rb);
            } 
            else if (res["collider"] is KinematicBody kb)
            {
                Player pl = (Player)kb;
                dam = Damage / _pelletCount;

                pl.TakeDamage(_playerOwner, _playerOwner.GlobalTransform.origin, dam);
                _game.World.ParticleManager.MakePuff(PUFFTYPE.BLOOD, pos, (Node)kb);
            }
            else
            {
                _game.World.ParticleManager.MakePuff(PUFFTYPE.PUFF, pos, null);
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
        if (_weaponShotType == WEAPONSHOTTYPE.PROJECTILE || _weaponShotType == WEAPONSHOTTYPE.GRENADE)
        {
            _projectileScene = (PackedScene)ResourceLoader.Load(_projectileResource);
        }
    }
}