using Godot;
using static Godot.Mathf;
using static FMath;
using System;

public class Player : KinematicBody
{
    // Nodes
    Spatial _head;
    World _world;
    RayCast _stairCatcher;
    MeshInstance _mesh;
    public MeshInstance Mesh {get { return _mesh; }}

    // fields
    public int Team;
    public int ID;
    

    // movement
    PlayerCmd _pCmd = new PlayerCmd();
    private bool _wishJump;
    private bool _touchingGround = false;
    private Vector3 _playerVelocity = new Vector3();
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
    
    public override void _Ready()
    {
        _head = (Spatial)GetNode("Head");
        _world = GetNode("/root/Initial/World") as World;
        _stairCatcher = (RayCast)GetNode("StairCatcher");
        _mesh = GetNode("MeshInstance") as MeshInstance;
    }

    public void RotateHead(float rad)
    {
        _head.RotateY(rad);
        _mesh.RotateY(rad);
    }

    public void SetMovement(PlayerCmd pCmd)
    {
        _pCmd = pCmd;
    }

    public void ProcessMovement(float delta)
    {
        // FIXME store these calls once...
        if (this.ID != GetTree().GetNetworkUniqueId())
        {
            _mesh.Rotation = _pCmd.rotation;
        }
        QueueJump();

        // do air move which does gravity
        if (_touchingGround || _climbLadder)
        {
            GroundMove(delta);
        }
        else
        {
            AirMove(delta);
        }

        _playerVelocity = this.MoveAndSlide(_playerVelocity, _world.Up);
        _touchingGround = IsOnFloor();

        _pCmd.move_forward = 0f;
        _pCmd.move_right = 0f;
        _pCmd.move_up = 0f;
        _pCmd.rotation = new Vector3();
        _pCmd.aim = new Basis();
    }

    public void GroundMove(float delta)
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

        float scale = CmdScale();

        wishDir += _pCmd.aim.x * _pCmd.move_right;
        wishDir -= _pCmd.aim.z * _pCmd.move_forward;
        wishDir = wishDir.Normalized();
        _moveDirectionNorm = wishDir;

        float wishSpeed = wishDir.Length();
        wishSpeed *= _moveSpeed;
        Accelerate(wishDir, wishSpeed, _runAcceleration, delta);
       
        if (_climbLadder)
        {
            if (_pCmd.move_forward != 0f)
            {
                _playerVelocity.y = _moveSpeed * (_pCmd.cam_angle / 90) * _pCmd.move_forward;
            }
            else
            {
                _playerVelocity.y = 0;
            }
            if (_pCmd.move_right == 0f)
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

    private void AirMove(float delta)
    {
        Vector3 wishdir = new Vector3();
        
        float wishvel = _airAcceleration;
        float accel;
        
        float scale = CmdScale();

        wishdir += _pCmd.aim.x * _pCmd.move_right;
        wishdir -= _pCmd.aim.z * _pCmd.move_forward;

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
        if(_pCmd.move_forward == 0 && _pCmd.move_right != 0)
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

    private void QueueJump()
    {
        if (_pCmd.move_up == 1 && !_wishJump)
        {
            _wishJump = true;
        }
        if (_pCmd.move_up == -1)
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
    private void AirControl(Vector3 wishdir, float wishspeed, float delta)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if(Mathf.Abs(_pCmd.move_forward) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
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
    
     private float CmdScale()
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(_pCmd.move_forward);
        if(Mathf.Abs(_pCmd.move_right) > max)
            max = (int)Mathf.Abs(_pCmd.move_right);
        if(max <= 0)
            return 0;

        total = Mathf.Sqrt(_pCmd.move_forward * _pCmd.move_forward + _pCmd.move_right * _pCmd.move_right);
        scale = _moveSpeed * max / (_moveScale * total);

        return scale;
    }
    
}

public struct PlayerCmd
{
    public float move_forward;
    public float move_right;
    public float move_up;
    public Basis aim;
    public float cam_angle;
    public Vector3 rotation;
}