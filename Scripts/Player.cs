using Godot;
using static Godot.Mathf;
using static FMath;
using System;
using System.Collections.Generic;

public class Player : KinematicBody
{
    // Nodes
    Spatial _head;
    World _world;
    RayCast _stairCatcher;
    MeshInstance _mesh;
    public MeshInstance Mesh {get { return _mesh; }}
    ProjectileManager _projectileManager;

    public bool PlayerControlled = false;

    // fields
    public int Team;
    public int ID;
    
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
    
    public Queue<PlayerCmd> pCmdQueue = new Queue<PlayerCmd>();
    private State _serverState;
    public State ServerState { get { return _serverState; }}
    private State _predictedState;
    public State PredictedState { get { return _predictedState; }}

    private int _maxArmour = 200;
    private float _currentArmour;
    public float CurrentArmour { get { return _currentArmour; }}
    private int _maxHealth = 100;
    private float _currentHealth;
    public float CurrentHealth { get { return _currentHealth; }}

    // test rocket stuff
    float _lastRocketShot = 0f;
    float _rocketCD = .5f;
    //float _shootRange = 1000f;

    public override void _Ready()
    {
        _head = (Spatial)GetNode("Head");
        _world = GetNode("/root/Initial/World") as World;
        _stairCatcher = (RayCast)GetNode("StairCatcher");
        _mesh = GetNode("MeshInstance") as MeshInstance;
        _projectileManager = GetNode("/root/Initial/World/ProjectileManager") as ProjectileManager;
    }

    public void RotateHead(float rad)
    {
        _head.RotateY(rad);
        _mesh.RotateY(rad);
    }

    public void ProcessCommands(float delta)
    {
        State predictedState = _serverState;
        bool addedCmd = false;
        if (pCmdQueue.Count == 0)
        {
            pCmdQueue.Enqueue(new PlayerCmd{
                move_forward = 0,
                move_right = 0,
                move_up = 0,
                aim = new Basis(),
                cam_angle = 0,
                rotation = _mesh.Rotation,
                attack = 0
            });
            addedCmd = true;
        }

        foreach(PlayerCmd pCmd in pCmdQueue)
        {
            ProcessAttack(pCmd, delta);
            predictedState = ProcessMovement(predictedState, pCmd, delta);
        }

        if (addedCmd)
        {
            pCmdQueue.Dequeue();
        }

        if (IsNetworkMaster())
        {
            SetServerState(predictedState.StateNum, predictedState.Origin, predictedState.Velocity, _mesh.Rotation, _currentHealth, _currentArmour);
        }
    }

    public void ProcessAttack(PlayerCmd pCmd, float delta)
    {
        _lastRocketShot += delta;

        if (pCmd.attack == 1 && _lastRocketShot >= _rocketCD)
        {           
            _projectileManager.AddProjectile(this, pCmd.attackDir);

            _lastRocketShot = 0f;
        }
    }

    public State ProcessMovement(State predState, PlayerCmd pCmd, float delta)
    {
        _playerVelocity = predState.Velocity;
        Transform t = new Transform();
        t = GlobalTransform;
        t.origin = predState.Origin;
        GlobalTransform = t;

        if (!PlayerControlled)
        {
            _mesh.Rotation = pCmd.rotation;
        }
        QueueJump(pCmd);

        // do air move which does gravity
        if (_touchingGround || _climbLadder)
        {
            GroundMove(delta, pCmd);
        }
        else
        {
            AirMove(delta, pCmd);
        }

        _playerVelocity = this.MoveAndSlide(_playerVelocity, _world.Up);
        _touchingGround = IsOnFloor();

        _predictedState = new State {
            StateNum = _predictedState.StateNum + 1,
            Origin = GlobalTransform.origin,
            Velocity = _playerVelocity
        };
        return _predictedState;
    }

    public void SetServerState(int stateNum, Vector3 org, Vector3 velo, Vector3 rot, float health, float armour)
    {
        _serverState.StateNum = stateNum;
        _serverState.Origin = org;
        _serverState.Velocity = velo;
        _currentHealth = health;
        _currentArmour = armour;

        if (!PlayerControlled)
        {
            this._mesh.Rotation = rot;
        }

        if (pCmdQueue.Count > 0)
        {
            int count = (stateNum > _predictedState.StateNum) ? pCmdQueue.Count - 1 : pCmdQueue.Count - (_predictedState.StateNum - stateNum);
            
            for (int i = 0; i < count; i++)
            {
                pCmdQueue.Dequeue();
            }
        }
    }

    public void GroundMove(float delta, PlayerCmd pCmd)
    {
        Vector3 wishDir = new Vector3();

        if (!_wishJump)
        {
            ApplyFriction(1.0f, delta);
        }
        else
        {
            ApplyFriction(0, delta);
        }

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
        /*else
        {
            _playerVelocity.y = 0;
        }*/

        // walk up stairs
        if (wishSpeed > 0 && _stairCatcher.IsColliding())
        {
            Vector3 col = _stairCatcher.GetCollisionNormal();
            float ang = Mathf.Rad2Deg(Mathf.Acos(col.Dot(_world.Up)));
            if (ang < _maxStairAngle)
            {
                _playerVelocity.y = _stairJumpHeight;
            }
        }

        if (_wishJump && IsOnFloor())
        {
            _playerVelocity.y = _jumpSpeed;
            _wishJump = false;
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

        // Apply gravity
        if (!_climbLadder)
        {
            _playerVelocity.y -= _world.Gravity * delta;
        }
    }

    private void QueueJump(PlayerCmd pCmd)
    {
        if (pCmd.move_up == 1 && !_wishJump)
        {
            _wishJump = true;
        }
        if (pCmd.move_up == -1)
        {
            _wishJump = false;
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

    public void TakeDamage(Rocket inflictor, float damage)
    {       
        float vel = damage;
        damage = inflictor.PlayerOwner == this ? damage * .5f : damage;

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
        AddVelocity(inflictor.GlobalTransform.origin, vel);
    }

    private void AddVelocity(Vector3 org, float velocity)
    {
        Vector3 dir = this.Transform.origin - org;
        dir = dir.Normalized();
        dir = dir * (velocity / 2); // random magic divisor number, i thought it was 10:1...
        _serverState.Velocity += dir;
    }

    public void Spawn(Vector3 spawnPoint)
    {
        this.Translation = spawnPoint;
        // FIXME - this is ugly, it should just be sent on next network update?
        this.SetServerState(this.ServerState.StateNum + 1, this.GlobalTransform.origin, this._playerVelocity, this._mesh.Rotation, _maxHealth, _maxArmour);
        _currentHealth = _maxHealth;
        _currentArmour = _maxArmour;
    }

    private void Die()
    {
        // FIXME
        // death sound
        // orientation change
        // respawn on input
        // log the death

        _world.Spawn(this);
    }
}
