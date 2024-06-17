using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : AIMovementScript
{
    

    [Header("Movement Info")]
    [SerializeField] private EnemyMovementState_e _movementState;
    [SerializeField] private bool _patrolling;
    [SerializeField] private int _currentPatrolNode;
    [SerializeField] private bool _movingToDefaultPos;
    [SerializeField] private bool _hasPathToPatrolNode;
    [SerializeField] private bool _atTarget;
    [SerializeField] private bool _stopped;
    [SerializeField] private Vector3 _target;

    [Header("Preferences")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private EnemyPatrolNode[] _patrolNodes;
    [SerializeField] private float _speed;             /// fallback speed if agent is null; 
    [SerializeField] private Vector3 _defaultPosition; /// the position the AI should return to after being moved;
    [SerializeField] private bool _canMove; 

    public override bool Patrolling { get => _patrolling;}
    public override bool HasPatrolRoute { get => _patrolNodes.Length > 0; }

    public override bool MovingToTarget { get => _agent.velocity.magnitude > 0; } 

    public override bool Stopped { get => !MovingToTarget; }

   
    private void Awake()
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();

        if (!HasPatrolRoute && _canMove)
        {
            _currentPatrolNode = -1;
            MoveToDefaultPosition();
        }
        
    }

    private void Update()
    {
        UpdateMovementState();
        _atTarget = AtTarget();
        _stopped = Stopped;
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


    public override void UpdateTarget(Vector3 pt)
    {
        _agent.SetDestination(pt);
        
        _target = pt; 
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

        if(AtTarget())
        {
            _hasPathToPatrolNode = false;
            _currentPatrolNode++;
            if (_currentPatrolNode >= _patrolNodes.Length) _currentPatrolNode = 0; 
        }

        
    }

    

    public override void MoveToDefaultPosition()
    {
        UpdateTarget(_defaultPosition);
        _movingToDefaultPos = true;
    }

    public override void Stop()
    {
        UpdateTarget(transform.position);
        _patrolling = false; 
    }

    public override bool AtTarget()
    {
        if(_target.y - transform.position.y <= _agent.height * 0.5f) /// if the target is within the height range of the agent, calculate distance based on x & z position
        {
            float distX = transform.position.x - _target.x;
            float distZ = transform.position.z - _target.z;

            float distSquared = distX * distX + distZ * distZ;

            return distSquared <= _agent.stoppingDistance * _agent.stoppingDistance;
        }
        else return Vector3.SqrMagnitude(transform.position - _target) <= _agent.stoppingDistance * _agent.stoppingDistance; /// otherwise calculate distance across all axii

    }

    public override void Idle()
    {
        if(!AtDefaultPosition() && !MovingToTarget)
        {
            MoveToDefaultPosition();
        }
        else if (AtDefaultPosition())
        {
            _movingToDefaultPos = false;
        }


    }

    public override bool AtDefaultPosition()
    {
        return Vector3.Distance(transform.position, _defaultPosition) <= _agent.stoppingDistance;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(_target, 0.5f);
    }
}
