using Godot;
using static Godot.Mathf;
using static FMath;
using System;
using System.Collections.Generic;
using System.Linq;

public class Player : KinematicBody
{
    // Nodes
    Game _game;
    Spatial _head;
    RayCast _stairCatcher;
    RayCast _feet;
    MeshInstance _mesh;
    AudioStreamPlayer3D _dieSound;
    AudioStreamPlayer3D _jumpSound;
    public MeshInstance Mesh {get { return _mesh; }}
    public Peer Peer;

    public Spatial Head { get { return _head; }}
    public PlayerController PlayerController = null;
    public bool PlayerControlled = false;
    AudioStreamPlayer _grenTimer = null;
    

    // fields
    public int Team;
    public int ID;
    public PLAYERCLASS PlayerClass;
    private Weapon _activeWeapon;
    private Weapon _weapon1;
    private Weapon _weapon2;
    private Weapon _weapon3;
    private Weapon _weapon4;
    public Weapon Weapon1 { get { return _weapon1; }}
    public Weapon Weapon2 { get { return _weapon2; }}
    public Weapon Weapon3 { get { return _weapon3; }}
    public Weapon Weapon4 { get { return _weapon4; }}
    public Weapon ActiveWeapon { 
        get { return _activeWeapon; }
        set { 
                if (_activeWeapon != null)
                {
                    if (_activeWeapon == value)
                    {
                        return;
                    }
                    _activeWeapon.WeaponMesh.Visible = false;
                }
                
                _activeWeapon = value; 
                _activeWeapon.WeaponMesh.Visible = true;
            }
    }
    private WEAPONTYPE _gren1Type;
    private WEAPONTYPE _gren2Type;
    private int _gren1Count = 0;
    private int _gren2Count = 0;
    public HandGrenade PrimingGren = null;
    public List<Debuff> Debuffs = new List<Debuff>();
    public List<Projectile> ActivePipebombs = new List<Projectile>();

    private int _maxArmour = 200;
    private float _currentArmour;
    public float CurrentArmour { get { return _currentArmour; }}
    private int _maxHealth = 100;
    private float _currentHealth;
    public float CurrentHealth { get { return _currentHealth; }}
    
    // movement
    private bool _wishJump;
    private bool _touchingGround = false;
    private Vector3 _playerVelocity = new Vector3();
    public Vector3 PlayerVelocity { 
                                    get { return _playerVelocity; }
                                    set { _playerVelocity = value; }
                                    }
    private Vector3 _moveDirectionNorm = new Vector3();
    private float _jumpSpeed = 27.0f;                // The speed at which the character's up axis gains when hitting jump
    private float _moveSpeed = 15.0f;               // Ground move speed
    private float _runAcceleration = 14.0f;         // Ground accel
    private float _runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
    private float _friction = 6;
    private float _moveScale = 1.0f;
    private bool _climbLadder = false;
    private float _maxStairAngle = 20f;
    private float _stairJumpHeight = 9F;
    public float _airAcceleration = 2.0f;          // Air accel
    public float _airDecceleration = 2.0f;         // Deacceleration experienced when opposite strafing
    public float _sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed
    public float _sideStrafeSpeed = 3.0f;          // What the max speed to generate when side strafing
    public float _airControl = 0.3f;               // How precise air control is
    
    public List<PlayerCmd> pCmdQueue = new List<PlayerCmd>();
    private State _serverState;
    public State ServerState { get { return _serverState; }}
    private State _predictedState;
    public State PredictedState { 
        get { return _predictedState; }
        set { _predictedState = value; }
        }
    
    public MOVETYPE MoveType = MOVETYPE.SPECTATOR;
    private float _timeDead = 0;

    public override void _Ready()
    {
        _game = GetTree().Root.GetNode("Game") as Game;
        _head = (Spatial)GetNode("Head");
        _stairCatcher = (RayCast)GetNode("StairCatcher");
        _mesh = GetNode("MeshInstance") as MeshInstance;
        _feet = GetNode("Feet") as RayCast;
        _dieSound = GetNode("DieSound") as AudioStreamPlayer3D;
        _grenTimer = GetNode("GrenTimer") as AudioStreamPlayer;
        _jumpSound = GetNode("JumpSound") as AudioStreamPlayer3D;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_weapon1 != null)
        {
            _weapon1.PhysicsProcess(delta);
        }
        if (_weapon2 != null)
        {
            _weapon2.PhysicsProcess(delta);
        }
        if (_weapon3 != null)
        {
            _weapon3.PhysicsProcess(delta);
        }
        if (_weapon4 != null)
        {
            _weapon4.PhysicsProcess(delta);
        }

        _predictedState = _serverState;
        if (pCmdQueue.Count == 0)
        {
            pCmdQueue.Add(
                new PlayerCmd{
                    //snapshot = this.Peer.LastSnapshot + 1,
                    snapshot = 0,
                    playerID = ID,
                    move_forward = 0,
                    move_right = 0,
                    move_up = 0,
                    aim = new Basis(),
                    cam_angle = 0,
                    rotation = _predictedState.Rotation,
                    attack = 0
                    }
                );
        }
        else
        {
            pCmdQueue.Sort((x,y) => x.snapshot.CompareTo(y.snapshot));
        }

        Transform t = GlobalTransform;
        t.origin = _predictedState.Origin; // by this point it's a new serverstate
        GlobalTransform = t;

        foreach(PlayerCmd pCmd in pCmdQueue)
        {
            if (!PlayerControlled && IsNetworkMaster())
            {
                _mesh.Rotation = pCmd.rotation;
            }
            
            if (pCmd.snapshot <= Peer.LastSnapshot)
            {
                continue;
            }

            Peer.LastSnapshot = pCmd.snapshot;

            switch (MoveType)
            {
                case MOVETYPE.DEAD:
                    DeadProcess(pCmd, delta);
                    break;
                case MOVETYPE.SPECTATOR:
                    SpectatorProcess(pCmd, delta);
                    break;
                case MOVETYPE.NORMAL:
                    NormalProcess(pCmd, delta);
                    break;
                default:
                    Console.ThrowPrint("No movement type set");
                    break;
            }
        }

        this.ProcessMovement(delta);

        if (IsNetworkMaster())
        {
            WEAPONTYPE wt = (ActiveWeapon == null) ? WEAPONTYPE.NONE : ActiveWeapon.WeaponType;
            SetServerState(_predictedState.Origin, _predictedState.Velocity, _predictedState.Rotation, _currentHealth, _currentArmour, wt);
        }
        else
        {
            // FIXME - stop resending commands after trying 3 times
            _game.Network.SendPMovement(1, ID, pCmdQueue);
        }
        TrimCmdQueue();
    }

    private void SpectatorProcess(PlayerCmd pCmd, float delta)
    {

    }

    private void NormalProcess(PlayerCmd pCmd, float delta)
    {
        if (IsNetworkMaster())
        {
            int diff = _game.World.LocalSnapNum - pCmd.snapshot;
            if (diff < 0)
            {
                return;
            }
            _game.World.RewindPlayers(diff, delta);
        }

        this.ProcessDebuffs(delta);

        this.ProcessImpulses(pCmd);
        this.ProcessAttack(pCmd, delta);

        if (IsNetworkMaster())
        {
            _game.World.FastForwardPlayers();
        }

        this.ProcessMovementCmd(_predictedState, pCmd, delta);
    }

    private void ProcessDebuffs(float delta)
    {
        List<Debuff> agedBuffs = new List<Debuff>();
        foreach (Debuff d in Debuffs)
        {
            if (d.TimeLeft <= 0)
            {
                agedBuffs.Add(d);
                continue;
            }

            if (d.NextThink <= 0)
            {
                d.ApplyDebuff();
            }
            
            d.TimeLeft -= delta;
            d.NextThink -= delta;
        }

        Debuffs.RemoveAll(d => agedBuffs.Contains(d));
    }

    private void ProcessImpulses(PlayerCmd pCmd)
    {
        foreach (float i in pCmd.impulses)
        {
            IMPULSE imp = (IMPULSE)i;
            switch (imp)
            {
                case IMPULSE.WEAPONONE:
                    if (_weapon1 != null)
                    {
                        ActiveWeapon = Weapon1;
                    }
                    break;
                case IMPULSE.WEAPONTWO:
                    if (Weapon2 != null)
                    {
                        ActiveWeapon = Weapon2;
                    }
                    break;
                case IMPULSE.WEAPONTHREE:
                    if (Weapon3 != null)
                    {
                        ActiveWeapon = Weapon3;
                    }
                    break;
                case IMPULSE.WEAPONFOUR:
                    if (Weapon4 != null)
                    {
                        ActiveWeapon = Weapon4;
                    }
                    break;
                case IMPULSE.GRENONE:
                case IMPULSE.GRENTWO:
                    UseHandGrenade(imp, pCmd);
                    break;
                case IMPULSE.DETPIPE:
                    Detpipe();
                    break;
                case IMPULSE.SPECIAL:
                    UseSpecialSkill();
                    break;
            }
        }
    }

    private void UseSpecialSkill()
    {
        switch ((PLAYERCLASS)this.PlayerClass)
        {
            case PLAYERCLASS.DEMOMAN:
                Detpipe();
                break;
        }
    }

    private void Detpipe()
    {
        if (this.PlayerClass == PLAYERCLASS.DEMOMAN)
        {
            if (this.Weapon2.TimeSinceLastShot >= GAMESETTINGS.DETPIPE_DELAY)
            {
                foreach (Pipebomb p in ActivePipebombs)
                {
                    p.Explode(null, p.Damage);
                }
                ActivePipebombs.Clear();
            }
        }
        else
        {
            Console.Log("You are not a demoman");
        }
    }

    private void UseHandGrenade(IMPULSE imp, PlayerCmd pCmd)
    {
        if (PrimingGren != null)
        {
            ThrowGren(pCmd.attackDir);
        }
        else
        {
            int grenCount = 0;
            switch (imp)
            {
                case IMPULSE.GRENONE:
                    grenCount = _gren1Count;
                    break;
                case IMPULSE.GRENTWO:
                    grenCount = _gren2Count;
                    break;
            }

            if (grenCount > 0)
            {
                // play timer
                if (this.ID == _game.Network.ID && _grenTimer != null)
                {
                    _grenTimer.Play();
                }

                WEAPONTYPE grenType = WEAPONTYPE.NONE;

                switch (imp)
                {
                    case IMPULSE.GRENONE:
                        _gren1Count -= 1;
                        grenType = _gren1Type;
                        break;
                    case IMPULSE.GRENTWO:
                        _gren2Count -= 1;
                        grenType = _gren2Type;
                        break;
                }

                string name = _game.World.ProjectileManager.AddProjectile(this, pCmd.attackDir, pCmd.projName, grenType);
                // FIXME - need projnames to be an array, shooting + gren on same frame
                pCmd.projName = name;
            }
            else
            {
                PrintGrenCount(imp);
            }
        }
    }

    private void PrintGrenCount(IMPULSE imp)
    {
        int grenCount = 0;
        WEAPONTYPE grenType = WEAPONTYPE.NONE;

        switch (imp)
        {
            case IMPULSE.GRENONE:
                grenCount = _gren1Count;
                grenType = _gren1Type;
                break;
            case IMPULSE.GRENTWO:
                grenCount = _gren2Count;
                grenType = _gren2Type;
                break;
        }
        Console.Print("You have " + grenCount + " " + grenType + "s left.");
    }

    private void ThrowGren(Vector3 dir)
    {
        PrimingGren.Throw(dir);
        this.PrimingGren = null;
    }

    public void Inflict(WEAPONTYPE inflictorType, float inflictLength, Player attacker)
    {
        Debuffs.Add(
            new Debuff {
                Type = inflictorType,
                TimeLeft = inflictLength,
                Owner = this,
                Attacker = attacker,
        });
    }

    private void DeadProcess(PlayerCmd pCmd, float delta)
    {
        if (_touchingGround)
        {
            _timeDead += delta; // have to wait time after touching ground to respawn
        }

        if (_timeDead > .5)
        {
            if (pCmd.attack == 1 || pCmd.move_up == 1)
            {
                _game.World.Spawn(this);
                _timeDead = 0;
            }
        }
    }

    public void RotateHead(float rad)
    {
        _head.RotateY(rad);
        _mesh.RotateY(rad);
    }

    private void ProcessMovement(float delta)
    {
        ApplyGravity(delta);
        if (!_wishJump)
        {
            ApplyFriction(1.0f, delta);
        }
        else
        {
            ApplyFriction(0, delta);
            _wishJump = false;
        }

        _playerVelocity = this.MoveAndSlide(_playerVelocity, _game.World.Up);
        _touchingGround = IsOnFloor();

        _predictedState = new State {
            StateNum = _predictedState.StateNum + 1,
            Origin = GlobalTransform.origin,
            Velocity = _playerVelocity,
            Rotation = _mesh.Rotation
        };
    }

    public void ProcessAttack(PlayerCmd pCmd, float delta)
    {
        if (pCmd.attack == 1)
        {
            ActiveWeapon.Shoot(pCmd, delta);
        }
    }

    public void ProcessMovementCmd(State predState, PlayerCmd pCmd, float delta)
    {
        _playerVelocity = predState.Velocity;

        // queue jump
        if (pCmd.move_up == 1 && !_wishJump)
        {
            _wishJump = true;
        }
        if (pCmd.move_up <= 0)
        {
            _wishJump = false;
        }

        if (_touchingGround || _climbLadder)
        {
            GroundMove(delta, pCmd);
        }
        else
        {
            AirMove(delta, pCmd);
        }
    }
// TODO - combine/fix via get/set? don't need double checks at the least
    private void SetActiveWeapon(WEAPONTYPE weaponType)
    {
        if (weaponType != WEAPONTYPE.NONE)
        {
            if (Weapon1 != null && weaponType == Weapon1.WeaponType)
            {
                ActiveWeapon = Weapon1;
            }
            else if (Weapon2 != null && weaponType == Weapon2.WeaponType)
            {
                ActiveWeapon = Weapon2;
            }
            else if (Weapon3 != null && weaponType == Weapon3.WeaponType)
            {
                ActiveWeapon = Weapon3;
            }
            else if (Weapon4 != null && weaponType == Weapon4.WeaponType)
            {
                ActiveWeapon = Weapon4;
            }
        }
    }

    public void SetServerState(Vector3 org, Vector3 velo, Vector3 rot, float health, float armour, WEAPONTYPE weaponType)
    {
        _serverState.Origin = org;
        _serverState.Velocity = velo;
        _serverState.Rotation = rot;
        _currentHealth = health;
        _currentArmour = armour;
        SetActiveWeapon(weaponType);

        if (!PlayerControlled)
        {
            this._mesh.Rotation = rot;
        }
    }

    public void TrimCmdQueue()
    {
        if (pCmdQueue.Count > 0)
        {
            int count = (_game.World.ServerSnapNum > _game.World.LocalSnapNum) ? pCmdQueue.Count - 1 : pCmdQueue.Count - (_game.World.LocalSnapNum - _game.World.ServerSnapNum);
             
            for (int i = 0; i < count; i++)
            {
                pCmdQueue.RemoveAt(0);
            }
        }
    }

    public void GroundMove(float delta, PlayerCmd pCmd)
    {
        Vector3 wishDir = new Vector3();

        float scale = CmdScale(pCmd);

        wishDir += pCmd.aim.x * pCmd.move_right;
        wishDir -= pCmd.aim.z * pCmd.move_forward;
        wishDir = wishDir.Normalized();
        _moveDirectionNorm = wishDir;

        float wishSpeed = wishDir.Length();
        wishSpeed *= _moveSpeed;
        Accelerate(wishDir, wishSpeed, _runAcceleration, delta);
       
        if (_climbLadder)
        {
            if (pCmd.move_forward != 0f)
            {
                _playerVelocity.y = _moveSpeed * (pCmd.cam_angle / 90) * pCmd.move_forward;
            }
            else
            {
                _playerVelocity.y = 0;
            }
            if (pCmd.move_right == 0f)
            {
                _playerVelocity.x = 0;
                _playerVelocity.z = 0;
            }
        }

        // walk up stairs
        if (wishSpeed > 0 && _stairCatcher.IsColliding())
        {
            Vector3 col = _stairCatcher.GetCollisionNormal();
            float ang = Mathf.Rad2Deg(Mathf.Acos(col.Dot(_game.World.Up)));
            if (ang < _maxStairAngle)
            {
                _playerVelocity.y = _stairJumpHeight;
            }
        }

        if (_wishJump && IsOnFloor())
        {
            // FIXME - if we add jump speed velocity we enable trimping right?
            _playerVelocity.y = _jumpSpeed;
            _jumpSound.Play();
        }
    }

    private void AirMove(float delta, PlayerCmd pCmd)
    {
        Vector3 wishdir = new Vector3();
        
        float wishvel = _airAcceleration;
        float accel;
        
        float scale = CmdScale(pCmd);

        wishdir += pCmd.aim.x * pCmd.move_right;
        wishdir -= pCmd.aim.z * pCmd.move_forward;

        float wishspeed = wishdir.Length();
        wishspeed *= _moveSpeed;

        wishdir = wishdir.Normalized();
        _moveDirectionNorm = wishdir;

        // CPM: Aircontrol
        float wishspeed2 = wishspeed;
        if (_playerVelocity.Dot(wishdir) < 0)
            accel = _airDecceleration;
        else
            accel = _airAcceleration;
        // If the player is ONLY strafing left or right
        if(pCmd.move_forward == 0 && pCmd.move_right != 0)
        {
            if(wishspeed > _sideStrafeSpeed)
            {
                wishspeed = _sideStrafeSpeed;
            }
                
            accel = _sideStrafeAcceleration;
        }

        Accelerate(wishdir, wishspeed, accel, delta);
        /*if(_airControl > 0)
        {
            AirControl(wishdir, wishspeed2, delta);
        }*/
        // !CPM: Aircontrol       
    }

    // TODO - move this to world
    private void ApplyGravity(float delta)
    {
        if (!_climbLadder && MoveType != MOVETYPE.SPECTATOR)
        {
            _playerVelocity.y -= _game.World.Gravity * delta;
        }
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel, float delta)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;
        
        currentspeed = _playerVelocity.Dot(wishdir);
        addspeed = wishspeed - currentspeed;
        if(addspeed <= 0)
            return;
        accelspeed = accel * delta * wishspeed;
        //if(accelspeed > addspeed)
         //   accelspeed = addspeed;
        _playerVelocity.x += accelspeed * wishdir.x;
        _playerVelocity.z += accelspeed * wishdir.z;
    }

    private void ApplyFriction(float t, float delta)
    {
        Vector3 vec = _playerVelocity;
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.Length();
        drop = 0.0f;

        // Only if the player is on the ground then apply friction
        if (_touchingGround)
        {
            control = speed < _runDeacceleration ? _runDeacceleration : speed;
            drop = control * _friction * delta * t;
        }

        newspeed = speed - drop;
        if(newspeed < 0)
            newspeed = 0;
        if(speed > 0)
            newspeed /= speed;

        _playerVelocity.x *= newspeed;
        _playerVelocity.z *= newspeed;
    }

    /**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
    private void AirControl(Vector3 wishdir, float wishspeed, float delta, PlayerCmd pCmd)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if(Mathf.Abs(pCmd.move_forward) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = _playerVelocity.y;
        _playerVelocity.y = 0;
        // Next two lines are equivalent to idTech's VectorNormalize()
        speed = _playerVelocity.Length();
        _playerVelocity = _playerVelocity.Normalized();

        dot = _playerVelocity.Dot(wishdir);
        k = 32;
        k *= _airControl * dot * dot * delta;

        // Change direction while slowing down
        if (dot > 0)
        {
            _playerVelocity.x = _playerVelocity.x * speed + wishdir.x * k;
            _playerVelocity.y = _playerVelocity.y * speed + wishdir.y * k;
            _playerVelocity.z = _playerVelocity.z * speed + wishdir.z * k;

            _playerVelocity = _playerVelocity.Normalized();
            _moveDirectionNorm = _playerVelocity;
        }

        _playerVelocity.x *= speed;
        _playerVelocity.y = zspeed; // Note this line
        _playerVelocity.z *= speed;
    }

    /*
    ============
    PM_CmdScale
    Returns the scale factor to apply to cmd movements
    This allows the clients to use axial -127 to 127 values for all directions
    without getting a sqrt(2) distortion in speed.
    ============
    */
    
     private float CmdScale(PlayerCmd pCmd)
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(pCmd.move_forward);
        if(Mathf.Abs(pCmd.move_right) > max)
            max = (int)Mathf.Abs(pCmd.move_right);
        if(max <= 0)
            return 0;

        total = Mathf.Sqrt(pCmd.move_forward * pCmd.move_forward + pCmd.move_right * pCmd.move_right);
        scale = _moveSpeed * max / (_moveScale * total);

        return scale;
    }

    public void TakeDamage(Player attacker, Vector3 inflictorOrigin, float damage)
    {       
        if (this.MoveType == MOVETYPE.DEAD)
        {
            return;
        }
        float vel = damage;
        damage = attacker == this ? damage * .5f : damage;

        // take from armour and health
        float a = _currentArmour;
        float h = _currentHealth;
        // calc max a used (4 armour to every 1 health of damage)
        float aUsed = damage / 5 * 4;

        if (aUsed >= a)
        {
            aUsed = a;
        }
        _currentArmour -= Convert.ToInt16(aUsed);

        float hUsed = damage - aUsed;

        if (h > hUsed)
        {
            // they survive
            _currentHealth -= Convert.ToInt16(hUsed);
        }
        else
        {
            this.Die();
            return;
        }

        // add velocity
        AddVelocity(inflictorOrigin, vel);
    }

    public void AddVelocity(Vector3 org, float velocity)
    {
        Vector3 dir = this.GlobalTransform.origin - org;
        dir = dir.Normalized();
        dir = dir * (velocity / 2); // random magic divisor number, i thought it was 10:1...
        _serverState.Velocity += dir;
    }

    public void Spawn(Vector3 spawnPoint)
    {
        if (MoveType == MOVETYPE.DEAD)
        {
            PlayerController pc = _game.Network.PlayerController;
            pc.Translation = new Vector3(pc.Translation.x, 0, pc.Translation.z);
        }

        if (Team == 0)
        {
            MoveType = MOVETYPE.SPECTATOR;
        }
        else
        {
            MoveType = MOVETYPE.NORMAL;
        }

        SetupClass();
       
        this.Translation = spawnPoint;

        this.SetServerState(this.GlobalTransform.origin, this._playerVelocity, this._mesh.Rotation, _maxHealth
                            , _maxArmour, (_activeWeapon == null) ? WEAPONTYPE.NONE : _activeWeapon.WeaponType);
        _predictedState = _serverState;
        _game.Network.SendPlayerInfo(this);
    }

    private void Delete(Weapon weapon)
    {
        if (weapon != null)
        {
            weapon.WeaponMesh.GetParent().RemoveChild(weapon.WeaponMesh);
            weapon.WeaponMesh.Free();
            weapon = null;
        }
    }

    private Weapon SetupWeapon(WEAPONTYPE weapon)
    {
        Weapon weap = null;
        switch (weapon)
        {
            case WEAPONTYPE.AXE:
                weap = new Axe();
                break;
            case WEAPONTYPE.SHOTGUN:
                weap = new Shotgun();
                break;
            case WEAPONTYPE.SUPERSHOTGUN:
                weap = new SuperShotgun();
                break;
            case WEAPONTYPE.NAILGUN:
                weap = new NailGun();
                break;
            case WEAPONTYPE.GRENADELAUNCHER:
                weap = new GrenadeLauncher();
                break;
            case WEAPONTYPE.PIPEBOMBLAUNCHER:
                weap = new PipebombLauncher();
                break;
            case WEAPONTYPE.ROCKETLAUNCHER:
                weap = new RocketLauncher();
                break;
            case WEAPONTYPE.NONE:
            default:

                break;
        }

        if (weap != null)
        {
            weap.Init(_game);
            weap.Spawn(this, nameof(weap));
        }

        return weap;
    }

    public void SetupClass()
    {
        // remove old weapon nodes for readd
        Delete(_weapon1);
        Delete(_weapon2);
        Delete(_weapon3);
        Delete(_weapon4);

        // TODO - maybe one day use reflection to clean this up or do types properly
        switch (PlayerClass)
        {
            case PLAYERCLASS.NONE:

                break;
            case PLAYERCLASS.SCOUT:
                _maxHealth = Scout.Health;
                _maxArmour = Scout.Armour;
                _weapon1 = SetupWeapon(Scout.Weapon1);
                _weapon2 = SetupWeapon(Scout.Weapon2);
                _weapon3 = SetupWeapon(Scout.Weapon3);
                _weapon4 = SetupWeapon(Scout.Weapon4);
                ActiveWeapon = _weapon1;
                _moveSpeed = Scout.MoveSpeed;
                _gren1Type = Scout.Gren1Type;
                _gren2Type = Scout.Gren2Type;
                _gren1Count = Scout.MaxGren1;
                _gren2Count = Scout.MaxGren2;
                break;
            case PLAYERCLASS.SNIPER:

                break;
            case PLAYERCLASS.SOLDIER:
                _maxHealth = Soldier.Health;
                _maxArmour = Soldier.Armour;
                _weapon1 = SetupWeapon(Soldier.Weapon1);
                _weapon2 = SetupWeapon(Soldier.Weapon2);
                _weapon3 = SetupWeapon(Soldier.Weapon3);
                _weapon4 = SetupWeapon(Soldier.Weapon4);
                ActiveWeapon = _weapon1;
                _moveSpeed = Soldier.MoveSpeed;
                _gren1Type = Soldier.Gren1Type;
                _gren2Type = Soldier.Gren2Type;
                _gren1Count = Soldier.MaxGren1;
                _gren2Count = Soldier.MaxGren2;
                break;
            case PLAYERCLASS.DEMOMAN:
                _maxHealth = Demoman.Health;
                _maxArmour = Demoman.Armour;
                _weapon1 = SetupWeapon(Demoman.Weapon1);
                _weapon2 = SetupWeapon(Demoman.Weapon2);
                _weapon3 = SetupWeapon(Demoman.Weapon3);
                _weapon4 = SetupWeapon(Demoman.Weapon4);
                ActiveWeapon = _weapon1;
                _moveSpeed = Demoman.MoveSpeed;
                _gren1Type = Demoman.Gren1Type;
                _gren2Type = Demoman.Gren2Type;
                _gren1Count = Demoman.MaxGren1;
                _gren2Count = Demoman.MaxGren2;
                break;
            case PLAYERCLASS.MEDIC:

                break;
            case PLAYERCLASS.HWGUY:

                break;
            case PLAYERCLASS.PYRO:

                break;
            case PLAYERCLASS.SPY:

                break;
            case PLAYERCLASS.ENGINEER:

                break;
        }
        
        
    }

    public void Die()
    {
        _currentHealth = 0;
        _currentArmour = 0;
        MoveType = MOVETYPE.DEAD;
        if (PlayerControlled)
        {
            // orientation change
            // TODO - pc should be on player if available
            PlayerController pc = _game.Network.PlayerController;
            pc.Translation = new Vector3(pc.Translation.x, _feet.Translation.y, pc.Translation.z);
        }
        pCmdQueue.Clear();
        _dieSound.Play();
    }
}
