using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMachine : MonoBehaviour
{
    public enum AIState_e
    {
        IDLE,                   /// default state when AI does not have patrol nodes
        PATROLLING,             /// default state when the AI has patrol nodes 
        SEEKING,                /// if the AI is looking for the player that they know exists 
        INVESTIGATING,          /// if the AI is looking for the source of player activity  
        INVESTIGATION_FINISH,   /// if the AI has finished looking for a source of player activity
        ATTACKING,              /// if the AI is attacking the player
    }

    public enum AIType_e
    {
        MOBILE,         
        STATIONARY      
    }

    /// These are placeholder values that should be replaced
    
    const float ATTACK_DISTANCE = 0f; //TODO: FIX ME WITH A REFERENCE TO THE ATTACK SCRIPT
    const float ATTACK_COOLDOWN = 1f; //TODO: FIX ME WITH A REFERENCE TO THE ATTACK SCRIPT


    [Header("AI Info")]
    [SerializeField] private AIState_e _currentState;
    [SerializeField] private AIType_e _aiType = AIType_e.MOBILE;
    [SerializeField] private bool _investigating;
    [SerializeField] private bool _hasAttack = true;

    private bool _doFinishInvestigation;
    private bool _finishingInvestigation;
    private bool _canAttack = true;
    private bool _isSeeking = false;
    private bool _isIdle = false;
    private bool _isPatrolBehavior = false;

    [Header("AI Modules")]
    [SerializeField] private Enemy _enemyScript; 
    [SerializeField] private PlayerDetector _playerDetector;
    [SerializeField] private AIMovementScript _movementScript;

    private void Update()
    {
        _playerDetector.UpdateSuspicionState();
        
        UpdateState();
        AILogic();
    }

    private void UpdateState()
    {
       
        switch (_playerDetector.SuspicionState)
        {
            default:
            case PlayerDetector.SuspicionState_e.UNAWARE:
            case PlayerDetector.SuspicionState_e.AWARE:
                if (_movementScript.HasPatrolRoute) _currentState = AIState_e.PATROLLING;
                else _currentState = AIState_e.IDLE;
                break;
            case PlayerDetector.SuspicionState_e.SUSPICIOUS:
                if (_isIdle) ExitIdleState();
                _currentState = AIState_e.INVESTIGATING;
                if (_doFinishInvestigation || _finishingInvestigation) _currentState = AIState_e.INVESTIGATION_FINISH;
                break;
            case PlayerDetector.SuspicionState_e.DETECTED: /// once the player is detected, the enemy will exit their investigation state, effectively freezing them and giving the player time to react 
                if (_investigating || _finishingInvestigation) ExitInvestigationState();
                _playerDetector.TriggerGracePeriod();
                Debug.Log("Player has been detected! Triggering Grace Period!");
                break;
            case PlayerDetector.SuspicionState_e.ALERT:
                Debug.Log("AI is alert");
                _playerDetector.CanDecaySuspicion = false;
                if (_playerDetector.CanSeePlayer()) _currentState = AIState_e.ATTACKING;                                    /// if alert and can see the player, run attack logic
                else if (_playerDetector.CurrentAlertTimeSeconds > 0f) _currentState = AIState_e.SEEKING;                   /// otherwise run seeking logic
                else _playerDetector.ResetAlertLevel();                                                                     /// if the player has lost LOS for a certain amount of time, reset the alert level
                break;
        }
       
    }

    private void AILogic()
    {
        DoDefaultBehaviors();
        switch (_currentState)
        {
            case AIState_e.PATROLLING:
                Patrol();
                break;
            case AIState_e.IDLE:
                Idle();
                break;
            case AIState_e.INVESTIGATING:
                Investigate();
                break;
            case AIState_e.INVESTIGATION_FINISH:
                FinishInvestigation();
                break;
            case AIState_e.SEEKING:
                Seek();
                break;
            case AIState_e.ATTACKING:
                Attack();
                break;
            default:
                Debug.LogWarning($"Warning: AI script on \"{name}\" tried to invoke unimplimented state: {_currentState}");
                break;
        }
    }


    private void DoDefaultBehaviors()
    {
        _playerDetector.PercieveSurroundings(); 
    }

    private void Patrol()
    {
        if (!_movementScript.Patrolling) _movementScript.Patrol();
        if(_movementScript.AtTarget() && !_isPatrolBehavior)
        {
            _isPatrolBehavior = true;
            StartCoroutine(PatrolRoutine());
        }
    }

    private void Idle()
    {
        if (!_isIdle)
        {
            Debug.Log($"{name} is running idle state");
            _isIdle = true;
            StartCoroutine(IdleRoutine());
        }
    }

    private void ExitIdleState()
    {
        _enemyScript.ResetBehavior();
        StopCoroutine(IdleRoutine());
        _isIdle = false;
    }

    private void Investigate()
    {
        if (!_investigating)
        {
            _investigating = true;
            _playerDetector.CanDecaySuspicion = false;
            StartCoroutine(InvestigationRoutine());
        }
    }
    private void FinishInvestigation()
    {
        if (!_finishingInvestigation)
        {
            StartCoroutine(InvestigationFinishRoutine());
        }
    }

    private void ExitInvestigationState()
    {
        _enemyScript.ResetBehavior();
        StopCoroutine(InvestigationRoutine());
        StopCoroutine(InvestigationFinishRoutine());
        _investigating = false;
        _doFinishInvestigation = false;
        _finishingInvestigation = false;
        _playerDetector.CanDecaySuspicion = true;
    }

    private void Seek()
    {
        /// 1. If attempting an attack, reset the attack status
        /// 2. reduce alert time by frame delta time
        /// 3. set agent target to last known player activity position
        /// 4. move towards last known player activity position

        if(!_canAttack) ResetAttackStatus(); 
        _playerDetector.CurrentAlertTimeSeconds -= Time.deltaTime;

        if (!_isSeeking)
        {
            StartCoroutine(SeekRoutine());
        }

    }

    private void Attack()
    {
        /// 1. Reset alert timer
        /// 2. if not within range of the player, move within range (script will call Seek method instead if not within LOS)
        /// 3. if within range, attack the player (use seperate attack script)
        /// 4. wait for attack cooldown
        /// 5. repeat

        if (_isSeeking) StopCoroutine(SeekRoutine());

        _playerDetector.CurrentAlertTimeSeconds = _playerDetector.AlertDecayTimeSeconds;

        if (_hasAttack && _playerDetector.GetDistanceFromPlayer() <= ATTACK_DISTANCE && _canAttack) StartCoroutine(AttackRoutine());
        else if (_aiType == AIType_e.MOBILE && _playerDetector.GetDistanceFromPlayer() > ATTACK_DISTANCE) _movementScript.UpdateTarget(Player.Singleton.transform.position);

    }

    private IEnumerator PatrolRoutine()
    {
        PatrolNodeBehavior_t behavior = _movementScript.GetCurrentPatrolNode().GetBehavior();
        Debug.Log("invoking patrol routine");

        Debug.Log("Behavior String: " + behavior.ToString());
       
        if (behavior.LookAtDirection)
        {

            Debug.Log("Looking at direction");
            _movementScript.SetDesiredRotation(behavior.Yaw);
            yield return new WaitUntil(() => !_movementScript.Rotating);
          
        }

        if (behavior.WaitAtNode)
        {
            _movementScript.Stop();
            yield return new WaitForSeconds(behavior.WaitTime);
        }

        _movementScript.UpdatePatrolNode();
        _isPatrolBehavior = false;
    }

    private IEnumerator IdleRoutine()
    {
        Debug.Log($"{name} is running idle routine");

        _movementScript.Idle();

        yield return new WaitUntil(()=> _movementScript.Stopped);

        StartCoroutine(_enemyScript.IdleBehavior());

        yield return new WaitUntil(()=> !_enemyScript.IsIdleBehavior);

        _isIdle = false;
    }

    private IEnumerator InvestigationRoutine()
    {
        /// 1. set target: _playerDetector.LastKnownPActivityPos 
        /// 2. Move towards target
        /// 3. if within stopping distance of last known pActivity, and can see the last known position of player activity, search for the player
        /// 4. player detection is handled outside of script
        _movementScript.Stop();

        yield return new WaitUntil(() => _movementScript.Stopped);

        _movementScript.UpdateTarget(_playerDetector.LastKnownPActivityPos);

        while (!_movementScript.AtTarget())
        {
            _movementScript.UpdateTarget(_playerDetector.LastKnownPActivityPos);
            yield return new WaitForFixedUpdate();
        }


        /// perform custom investigation behavior

        if (_enemyScript != null)
        {
            StartCoroutine(_enemyScript.InvestigativeBehavior(_playerDetector,_movementScript));
            yield return new WaitUntil(() => !_enemyScript.IsInvestigativeBehavior); 
        }

        _playerDetector.CanDecaySuspicion = true;
        _investigating = false;
        _doFinishInvestigation = true;
        
        _movementScript.Stop();
        yield return new WaitUntil(() => _movementScript.Stopped);
    }

    private IEnumerator InvestigationFinishRoutine()
    {
        _finishingInvestigation = true;

        float baseSuspicion = _playerDetector.SuspicionLevel;
        _movementScript.MoveToDefaultPosition();

        while (!_movementScript.AtTarget())
        {
            if (_playerDetector.SuspicionPerSecond > 0)       /// if the enemy gains any suspicion while moving back towards the default point, enable investigation mode
            {
                Debug.Log($"{name} gained suspicion while it was moving back towards default position, {name} is allowed to investigate");
                _movementScript.Stop();
                yield return new WaitUntil(() => _movementScript.Stopped);
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        _finishingInvestigation = false;
        _doFinishInvestigation = false;
    }

    /// <summary>
    /// Moves the player towards the last known position of player activity
    /// </summary>
    /// <returns></returns>
    private IEnumerator SeekRoutine()
    {
        _isSeeking = true;
        _movementScript.Stop();

        yield return new WaitUntil(() => _movementScript.Stopped);
        _movementScript.UpdateTarget(_playerDetector.LastKnownPActivityPos);

        while (!_movementScript.AtTarget())
        {
            _movementScript.UpdateTarget(_playerDetector.LastKnownPActivityPos);
            yield return new WaitForFixedUpdate();
        }
        ///perform custom seeking behavior

        if (_enemyScript != null)
        {
            StartCoroutine(_enemyScript.SeekingBehavior());
            yield return new WaitUntil(() => !_enemyScript.IsSeekingBehavior);
        }

        _isSeeking = false;
    }

    /// TODO: FIX ME BY CALLING METHOD ON ATTACK SCRIPT AND PLAYING RELEVANT ANIMATION
    private IEnumerator AttackRoutine()
    {
        _canAttack = false;
        _movementScript.Stop();
        yield return new WaitUntil(() => _movementScript.Stopped);
        ///Attack logic goes here

        if (_enemyScript != null)
        {
            StartCoroutine(_enemyScript.AttackBehavior());
            yield return new WaitUntil(() => !_enemyScript.IsAttackBehavior); 
        }


        Debug.LogWarning($"Warning: Unimplimented attack routine has been executed by {name}");
        Invoke(nameof(ResetAttackStatus), ATTACK_COOLDOWN);
    }

    private void ResetAttackStatus()
    {
        _canAttack = true;
        StopCoroutine(AttackRoutine());
    }


}
