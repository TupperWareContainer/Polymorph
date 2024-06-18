using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemyMovement : AIMovementScript
{
    [Header("Movement Info")]
    [SerializeField] private EnemyMovementState_e _movementState;
    [SerializeField] private bool _patrolling;
    [SerializeField] private int _currentPatrolNode;
    [SerializeField] private bool _movingToDefaultPos;
    [SerializeField] private bool _hasPathToPatrolNode;
    [SerializeField] private Vector3 _startingDirection;
    [SerializeField] private Vector3 _currentDirection;
    [SerializeField] private Vector3 _target;
    [SerializeField] private Vector3 _targetDirection;
    [SerializeField] private float _lerpTime;
    [SerializeField] private float _cLerpTime;
    [SerializeField] private bool _atTarget;



    [Header("Preferences")]
    [SerializeField] private EnemyPatrolNode[] _patrolNodes;
    [SerializeField] private float _angularSpeedDegrees;
    [SerializeField] private Vector3 _defaultRotationEuler; /// the position the AI should return to after being moved;
    [SerializeField] private bool _canMove;
    [SerializeField] private float _precicion = 0.001f;



    public override bool Patrolling { get => _patrolling; }

    public override bool HasPatrolRoute { get => _patrolNodes.Length > 0; }

    public override bool MovingToTarget { get => Vector3.Distance(transform.forward,_target) <= _precicion; }

    public override bool Stopped { get => !MovingToTarget; }

    public override bool Rotating { get => MovingToTarget; }


    private void Awake()
    {
        _startingDirection = transform.forward;
        if (!HasPatrolRoute && _canMove)
        {
            _currentPatrolNode = -1;
            MoveToDefaultPosition();
        }
    }

    private void Update()
    {
        UpdateMovementState(); 
    }

    private void FixedUpdate()
    {
        _currentDirection = transform.forward;
        _atTarget = AtTarget();
        Move();
    }

    private void UpdateMovementState()
    {
        if (MovingToTarget)
        {
            _movementState = EnemyMovementState_e.MOVING;
        }
        else if (Stopped) _movementState = EnemyMovementState_e.IDLE;
        else if (_movingToDefaultPos) _movementState = EnemyMovementState_e.RETURNING_HOME;
    }

    private void Move()
    {
        if(_cLerpTime < _lerpTime)
        {
            _cLerpTime += Time.fixedDeltaTime;
            Vector3 direction = Vector3.Lerp(_startingDirection, _targetDirection, Mathf.Clamp(_cLerpTime / _lerpTime,0f,1f));
            transform.forward = direction;
        }

    } 



    public override bool AtDefaultPosition()
    {
        return transform.rotation.Equals(Quaternion.Euler(_defaultRotationEuler));
    }

    public override bool AtTarget()
    {
        return Vector3.Distance(_currentDirection, _targetDirection) <= _precicion;
    }

    public override void Idle()
    {
        return; 
    }

    public override void MoveToDefaultPosition()
    {
        Vector3 dp = (Quaternion.Euler(_defaultRotationEuler) * transform.forward) * 100f;
        UpdateTarget(transform.position + dp); 
    }

    public override void Patrol()
    {
        if (!HasPatrolRoute)
        {
            Debug.LogWarning($"Warning : Movement Script on {name} is attempting to run patrol behavior despite having no patrol nodes!");
            _patrolling = false;
            return;
        }
        _patrolling = true;

        if (!_hasPathToPatrolNode)
        {
            UpdateTarget(_patrolNodes[_currentPatrolNode].transform.position);
            _hasPathToPatrolNode = true;
        }

        
    }
    public override void UpdatePatrolNode()
    {
        _hasPathToPatrolNode = false;
        _currentPatrolNode++;
        if (_currentPatrolNode >= _patrolNodes.Length) _currentPatrolNode = 0;
    }

    public override void Stop()
    {
        UpdateTarget(transform.forward * 10f);
    }

    public override void UpdateTarget(Vector3 pt)
    {
        _target = pt;

        _startingDirection = transform.forward;
        _targetDirection = (_target - transform.position).normalized;
        _cLerpTime = 0f;
        _lerpTime = (_targetDirection - _startingDirection).magnitude / _angularSpeedDegrees; 
    }
    public override EnemyPatrolNode GetCurrentPatrolNode()
    {
        if (!HasPatrolRoute) return null;
        return _patrolNodes[_currentPatrolNode];
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(_target, 0.5f);
    }
}
