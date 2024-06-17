using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementOld : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private LayerMask _ignore;
    [SerializeField] private CapsuleCollider _collider; 

    [Header("Preferences")]
    [SerializeField] private float _baseMovementSpeed;
    [SerializeField] private float _crouchSpeedModifier = 0.75f;
    [SerializeField] private float _sprintSpeedModifier = 1.25f;
    [SerializeField] private float _slideDuration = 1f; 
    [SerializeField] private float _jumpHeight;
    [SerializeField] private bool _toggleCrouch = false;
    [SerializeField] private float _gravityScale = 1f;

    [Header("WallTech Parameters")]
    [SerializeField] private LayerMask _wallLayers;
    [SerializeField] private float _wallCheckDistance;



    [Header("Debug")]

    [SerializeField] private float _jumpVelocity;

    [SerializeField] private bool _isForward, _isBackward, _isLeft, _isRight;

    [SerializeField] private bool _isJumpPressed;

    [SerializeField] private bool _isJumpAllowed;

    [SerializeField] private bool _isAirborne;
    [SerializeField] private bool _isCrouchPressed;
    [SerializeField] private bool _isCrouched;
    [SerializeField] private bool _isSliding; 
    [SerializeField] private bool _isSprinting;
    [SerializeField] private bool _isWallJump;
    [SerializeField] private bool _arrestHorizontalMovement;
    //[SerializeField] private bool _isWallRunning; 


    private bool _forceExitCrouch; 

    private float _playerScale = 1f; 





    private void Awake()
    {
        _jumpVelocity = (-2f * Physics.gravity.y * _gravityScale) * _jumpHeight;
        _jumpVelocity = Mathf.Sqrt(_jumpVelocity);
        StartCoroutine(CheckJumpValidity());
        StartCoroutine(CrouchLogic());
    }

   
    private void CheckForPlayerInput()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");
        if (hAxis > 0)
        {
            _isRight = true;
            _isLeft = false;

        }
        else if (hAxis < 0)
        {
            _isLeft = true;
            _isRight = false;
        }
        else
        {
            _isLeft = false;
            _isRight = false;
        }

        if (vAxis > 0)
        {
            _isForward = true;
            _isBackward = false;
        }
        else if (vAxis < 0)
        {
            _isBackward = true;
            _isForward = false;
        }
        else
        {
            _isForward = false;
            _isBackward = false;
        }

        if ( Input.GetButton("Jump"))
        {
            _isJumpPressed = true;
        }

        _isSprinting = Input.GetButton("Sprint"); 
        


        if (_toggleCrouch && !_isAirborne)
        {
            if (Input.GetButtonDown("Crouch"))
            {
                _isCrouchPressed = !_isCrouchPressed;
            }
        }
        else if(!_isAirborne)
        {
            _isCrouchPressed = Input.GetButton("Crouch");
        }

        if (_forceExitCrouch && (!_isCrouchPressed || _toggleCrouch))
        {
            _forceExitCrouch = false;
            _isCrouchPressed = false;
        }


    }

    private void Update()
    {
        CheckForPlayerInput();
        ExtDebug.DrawBoxCastBox(transform.position, Vector3.one * 0.5f, Quaternion.identity, Vector3.down, 0.5f * _playerScale, Color.green);
        Debug.DrawRay(transform.position, transform.right * _wallCheckDistance, Color.red);
        Debug.DrawRay(transform.position, transform.right * -_wallCheckDistance, Color.blue);

    }


    private void FixedUpdate()
    {
        MovePlayer(_isForward, _isBackward, _isLeft, _isRight,_isSprinting);
    }


    private void MovePlayer(bool f, bool b, bool l, bool r, bool highProfile)
    {
        //if (_isWallRunning) return;
        if (_isSliding) return;
        Vector3 right = Vector3.zero;
        Vector3 forward = Vector3.zero;
        Vector3 jumpVector = Vector3.zero;

        float yVelocity = _rb.velocity.y;

        if (f)
        {
            forward = transform.forward;
        }
        else if (b)
        {
            forward = -transform.forward;
        }

        
        if (l)
        {
            right = -transform.right;
        }
        else if (r)
        {
            right = transform.right;
        }

       

        Vector3 movementVector = Vector3.Normalize(forward + right) * _baseMovementSpeed;

        if (highProfile)
        {
            if (_isSprinting)
            {
                movementVector *= _sprintSpeedModifier;
            }
            if (_isCrouched && !_forceExitCrouch && !_isAirborne)
            {
                Slide(movementVector.normalized);
            }
            else if (_isAirborne)
            {
                /// wall running, deprecated
                /// if (CheckWallRun()) DoWallRun();
                RaycastHit hit;
                if(CheckWalls(out hit))
                {
                    jumpVector += hit.normal * 2 * _baseMovementSpeed * _sprintSpeedModifier;
                    _isWallJump = true;
                    
                }
                else
                {
                    _isWallJump = false;
                }


            }


        }
        else
        {
            if (!_isAirborne && _isCrouched) movementVector *= _crouchSpeedModifier;
        }

        if (_isJumpPressed && (_isJumpAllowed || _isWallJump))
        {
            jumpVector += Vector3.up * _jumpVelocity;
            _isWallJump = false;
        }
        else if (_isAirborne)
        {
            jumpVector = Vector3.up * (_rb.velocity.y + (Physics.gravity.y * _gravityScale) * Time.fixedDeltaTime);
            //yVelocity = _rb.velocity.y + (Physics.gravity.y * _gravityScale) * Time.fixedDeltaTime;
            _isJumpPressed = false;
        }
        else
        {
            jumpVector = Vector3.zero;
        }



        _rb.velocity = movementVector + jumpVector; 

    }
    private bool CheckWalls(out RaycastHit hitInfo)
    {
        RaycastHit leftHit, rightHit;

        bool right = Physics.Raycast(transform.position, transform.right, out rightHit, _wallCheckDistance, _wallLayers);

        if (right)
        {
            hitInfo = rightHit;
            return true;
        }

        bool left = Physics.Raycast(transform.position, -transform.right, out leftHit, _wallCheckDistance, _wallLayers);

        if (left)
        {
            hitInfo = leftHit;
            return true;
        }

        hitInfo = new RaycastHit();

        return false;
    }

    private IEnumerator CheckJumpValidity()
    {

        while (true)
        {
            yield return new WaitForEndOfFrame();
            ///Check if player is grounded

            bool grounded = Physics.BoxCast(transform.position, Vector3.one * 0.5f * _playerScale, Vector3.down, Quaternion.identity, 0.5f * _playerScale, ~_ignore);

            ExtDebug.DrawBoxCastBox(transform.position, Vector3.one * 0.5f * _playerScale, Quaternion.identity, Vector3.down, 0.5f * _playerScale, Color.green);

            _isAirborne = !grounded;


            _isJumpAllowed = grounded; 
        }

    }

    private IEnumerator CrouchLogic()
    {
        while (true)
        {
            yield return new WaitUntil(() => _isCrouchPressed && !_forceExitCrouch);

            ApplyCrouchModifiers();

            yield return new WaitUntil(() => (!_isCrouchPressed || _forceExitCrouch) && !_isSliding);

            RemoveCrouchModifiers();



        }
    }

    private void ApplyCrouchModifiers()
    {
        _isCrouched = true;
        _collider.height = 1f;
        _playerScale = 0.5f;
    }
    private void RemoveCrouchModifiers()
    {
        _isCrouched = false;
        _collider.height = 2f;
        _playerScale = 1f;
    }

    private void Slide(Vector3 directionVector)
    {
        _isSliding = true;
        StartCoroutine(SlideRoutine(directionVector)); 
    } 

    private IEnumerator SlideRoutine(Vector3 direction)
    {
        float timer = _slideDuration; 

        while((timer > 0 && !_forceExitCrouch)  && !_isAirborne)
        {
            timer -= Time.deltaTime;

            _rb.velocity = direction * _baseMovementSpeed * _sprintSpeedModifier; 

            yield return new WaitForEndOfFrame(); 
        }
        _forceExitCrouch = true; 
        _isSliding = false; 
    }


   

   


    /*
     *  Deprecated Wall run code, might use in the future if I want to impliment wall running
     */

    /* private bool CheckWallRun(out RaycastHit hitInfo)
    {
        RaycastHit leftHit, rightHit;

        bool right = Physics.Raycast(transform.position, transform.right, out rightHit, _wallCheckDistance, _wallRunLayers);

        if (right)
        {
            hitInfo = rightHit;
            return true; 
        }
        
        bool left = Physics.Raycast(transform.position, -transform.right, out leftHit, _wallCheckDistance, _wallRunLayers);

        if (left)
        {
            hitInfo = leftHit;
            return true;
        }

        hitInfo = new RaycastHit(); 

        return false; 
    }

    private bool CheckWallRun()
    {

        bool right = Physics.Raycast(transform.position, transform.right, _wallCheckDistance, _wallRunLayers);

        if (right)
        {
            return true;
        }

        bool left = Physics.Raycast(transform.position, -transform.right, _wallCheckDistance, _wallRunLayers);

        if (left)
        {
            return true;
        }


        return false;
    }

    private Vector3 GetWallRunDirection(RaycastHit hitinfo)
    {
        Vector3 directionVector = Vector3.Cross(hitinfo.normal, transform.up).normalized;

        if ((transform.forward - directionVector).magnitude > (transform.forward - -directionVector).magnitude) directionVector = -directionVector;


        return directionVector;


    }

    private void DoWallRun()
    {
        StartCoroutine(WallrunRoutine());

    }
    
    private IEnumerator WallrunRoutine()
    {
        RaycastHit hit;
        _isWallRunning = true;

        while (CheckWallRun(out hit) && _isSprinting && Input.GetButton("Jump"))
        {
            yield return new WaitForFixedUpdate();
            Vector3 wallForward = GetWallRunDirection(hit);

            _rb.velocity = wallForward * _baseMovementSpeed * _sprintSpeedModifier;
            
        }
        _isWallRunning = false; 
    }
*/
}
