using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// The state of the player determines how the velocity is modified
    /// </summary>
    public enum MovementType_e   
    {
        IDLE,       // Player is not attempting to move
        AIRBORNE,   // Player is attempting to move while in air
        WALKING,    // Player is attempting to move while walking
        SPRINTING,   // Player is attempting to move while sprinting
        SLIDING,    // Player is attempting to move while sliding
        CROUCHED,   // Player is attempting to move while crouched
        DISABLED    // Player is attempting to move while disabled
    }

    [Header("Player Info")]
    [SerializeField] private MovementType_e _currentMovementType;
    [SerializeField] private CapsuleCollider _collider;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private bool _grounded;
    [SerializeField] private bool _crouching; 

    [Header("Input Info")]
    [SerializeField] private float _horizontalInputAxis;
    [SerializeField] private float _verticalInputAxis;
    [SerializeField] private bool _jumpButtonPressed;
    [SerializeField] private bool _crouchButtonPressed;
    [SerializeField] private bool _sprintButtonPressed;
    [SerializeField] private bool _mLock = true;

    [Header("Preferences")]
    [SerializeField] private LayerMask _ignore;
    [SerializeField] private float _sprintingMovementSpeed; 
    [SerializeField] private float _walkingMovementSpeed;
    [SerializeField] private float _crouchSpeed;
    [SerializeField] private float _airborneAccelerationRate;
    [SerializeField] private float _gravityScale;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _groundDetectionDistance;
    
    private float _jumpVelocity;

    private float _playerScale = 1f;
   


    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        _jumpVelocity = (-2f * Physics.gravity.y * _gravityScale) * _jumpHeight;
        _jumpVelocity = Mathf.Sqrt(_jumpVelocity);
    }


    private void QueryPlayerInput()
    {
        _horizontalInputAxis = Input.GetAxisRaw("Horizontal");
        _verticalInputAxis = Input.GetAxisRaw("Vertical");
        _jumpButtonPressed = Input.GetButton("Jump");
        _crouchButtonPressed = Input.GetButton("Crouch");
        _sprintButtonPressed = Input.GetButton("Sprint");

        if (Input.GetButtonDown("ToggleMLock")) _mLock = !_mLock;
    }

    private void StateMachine()
    {
        if (!IsPlayerInput()) _currentMovementType = MovementType_e.IDLE;
        else if (IsTouchingGround() && _crouchButtonPressed) _currentMovementType = MovementType_e.CROUCHED;
        else if (IsTouchingGround() && _sprintButtonPressed) _currentMovementType = MovementType_e.SPRINTING;
        else if (IsTouchingGround()) _currentMovementType = MovementType_e.WALKING;
        else _currentMovementType = MovementType_e.AIRBORNE; 

    }

    private void UpdatePlayer()
    {
        if (_mLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
       
        if(!_crouchButtonPressed && _crouching)
        {
            RemoveCrouchModifiers();
        }
        switch (_currentMovementType)
        {
            default:
            case MovementType_e.IDLE:
                if (IsTouchingGround()) _rb.velocity = Vector3.zero;
                break;
            case MovementType_e.DISABLED:
                _rb.velocity = Vector3.zero;
                break;
            case MovementType_e.SPRINTING:
                MovePlayer(_sprintingMovementSpeed);
                break;
            case MovementType_e.CROUCHED:
                if (!_crouching) ApplyCrouchModifiers();
                MovePlayer(_crouchSpeed);
                break;
            case MovementType_e.WALKING:
                MovePlayer(_walkingMovementSpeed);
                break;
            case MovementType_e.AIRBORNE:
                MovePlayerAirborne();
                break;

        }
        if (!IsTouchingGround())
        {
            _rb.velocity += Physics.gravity * _gravityScale * Time.fixedDeltaTime;
            _rb.useGravity = true;   
        }
        else _rb.useGravity = false;

    }

    private void ApplyCrouchModifiers()
    {
        _crouching = true;
        _collider.height = 1;
        _rb.MovePosition(transform.position + Vector3.down * 0.5f);
        Player.Singleton.PlayerDetectReigon = transform;
        Player.Singleton.Crouched = true; 
        _playerScale = 0.5f;
    }
    private void RemoveCrouchModifiers()
    {
        _crouching = false;
        _collider.height = 2;
        _rb.MovePosition(transform.position + Vector3.up * 0.5f);
        _playerScale = 1f;
        Player.Singleton.Crouched = false;
        Player.Singleton.PlayerDetectReigon = Player.Singleton.Camera;
    }

    private void MovePlayer(float movementSpeed)
    {
        RaycastHit hit; 


        Vector3 forwardVector = Vector3.zero;
        Vector3 rightVector = Vector3.zero;
        Vector3 upVector = Vector3.zero;

        if (_verticalInputAxis != 0) forwardVector = transform.forward * Mathf.Sign(_verticalInputAxis);
        if (_horizontalInputAxis != 0) rightVector = transform.right * Mathf.Sign(_horizontalInputAxis);
        if (_jumpButtonPressed) upVector = Vector3.up * _jumpVelocity;

        Vector3 movementVector = forwardVector + rightVector;

        movementVector = movementVector.normalized;
        movementVector *= movementSpeed;

        movementVector = new Vector3(movementVector.x, 0f, movementVector.z);


        if (!Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, ~_ignore))
        {
            Debug.LogWarning("Ground Normal raycast could not find geometry beneath player, reverting to non-projected velocity");
        }
        else if (!hit.normal.Equals(Vector3.up)) /// if the player is on a slope, project the velocity onto the slope's plane
        {
            movementVector = Vector3.ProjectOnPlane(movementVector, hit.normal);
            
        }
        _rb.velocity = movementVector + upVector;


    }


    private void MovePlayerAirborne()
    {
        Vector3 forwardVector = Vector3.zero;
        Vector3 rightVector = Vector3.zero;

        if (_verticalInputAxis != 0) forwardVector = transform.forward * Mathf.Sign(_verticalInputAxis);
        if (_horizontalInputAxis != 0) rightVector = transform.right * Mathf.Sign(_horizontalInputAxis);

        Vector3 movementVector = forwardVector + rightVector;
        movementVector = movementVector.normalized;

        _rb.velocity += movementVector * _airborneAccelerationRate * Time.fixedDeltaTime; // add the acceleration vector

        float ySpeed = _rb.velocity.y;

        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); 

        _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _sprintingMovementSpeed); // clamp the horizontal velocity to be within the maximum player speed

        _rb.velocity = new Vector3(_rb.velocity.x, ySpeed, _rb.velocity.z); // re-add the y velocity after clamp so player will continue to fall at a constant rate
        

    }
  
    private void Update()
    {
        QueryPlayerInput();
        StateMachine();
        if (_crouching) ExtDebug.DrawBoxCastBox(transform.position, new Vector3(0.5f, 0.25f, 0.5f), Quaternion.identity, Vector3.down, (_collider.height * 0.5f - 0.25f) + _groundDetectionDistance, Color.green);
        else ExtDebug.DrawBoxCastBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, Vector3.down, (_collider.height * 0.5f - 0.5f) + _groundDetectionDistance, Color.green);
        Debug.DrawRay(transform.position, Vector3.down * (_collider.height * 0.5f + 0.01f),Color.green);
    }


    private void FixedUpdate()
    {
        UpdatePlayer();
        _grounded = IsTouchingGround();
    }


    private bool IsTouchingGround()
    {
        if (_crouching) return Physics.BoxCast(transform.position, new Vector3(0.5f, 0.25f, 0.5f), Vector3.down, Quaternion.identity, (_collider.height * 0.5f - 0.25f) + _groundDetectionDistance,~_ignore) || Physics.Raycast(transform.position,Vector3.down,(_collider.height * 0.5f) + 0.01f,~_ignore);
        else return Physics.BoxCast(transform.position, Vector3.one * 0.5f, Vector3.down, Quaternion.identity, (_collider.height * 0.5f - 0.5f) + _groundDetectionDistance,~_ignore) || Physics.Raycast(transform.position, Vector3.down, (_collider.height * 0.5f) + 0.01f, ~_ignore);
    }

    private bool IsPlayerInput()
    {
        return _horizontalInputAxis != 0 || _verticalInputAxis != 0 || _jumpButtonPressed || _crouchButtonPressed || _sprintButtonPressed;
    }  

   

}