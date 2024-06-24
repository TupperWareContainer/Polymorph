using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerDetector : MonoBehaviour
{
    public enum SuspicionState_e
    {
        UNAWARE,        /// AI has not yet seen the player and has no reason to be on the lookout 
        AWARE,          /// AI has seen glimpses of player or minimal player activity
        SUSPICIOUS,     /// AI has seen the player for a prolonged period of time before they dissapeared, or has perceived egregious amounts of player activity
        DETECTED,       /// AI has seen the player without breaking LOS for a prolonged period of time
        ALERT,          /// AI has detected the player 
    }

    [Header("Detection Cone Settings")]
    [SerializeField] private float _maxDetectionDistance;
    [SerializeField] private float _detectionFOV;
    [SerializeField] private Transform _head;
    [SerializeField] private float _detectionDecayRate;
    [SerializeField] private bool _useTriggerVolume; /// if true, overrides the procedurally generated fov cone with a specified volume
    [SerializeField] private AIDetectionVolume _detectionVolume; 

    [Header("Sight Detection Settings")]
    [SerializeField] private float _slowDetectionDistance;
    [SerializeField] private float _normalDetectionDistance;
    [SerializeField] private float _instantDetectionDistance;
    [SerializeField] private float _slowDetectionRate;
    [SerializeField] private float _normalDetectionRate;
    [SerializeField] private float _detectionGracePeriod; /// time in which the player can break LOS without triggering the alert state

    [Header("Sound Detection Settings")]
    [SerializeField] private bool _deaf = true;
    [SerializeField] private LayerMask _soundSourceLayer;
    [SerializeField] private int _maxSoundSources;
    [SerializeField] private float _hearingRadius;

    

    [Header("Detector Info")]
    [SerializeField] private float _suspicionLevel;
    [SerializeField] private float _suspicionThresholdPercentage; 
    [SerializeField] private float _maxSuspicion;
    [SerializeField] private float _minSuspicion;
    [SerializeField] private bool _hasDetectedPlayer;
    [SerializeField] private float _alertDecayTimeSeconds;
    [SerializeField] private float _cAlertTimeSeconds;
    [SerializeField] private SuspicionState_e _suspicionState = SuspicionState_e.UNAWARE;
    [SerializeField] private Vector3 _lastKnownPActivityPos;


    public float SuspicionPerSecond = 0f;

    [Header("Preferences")]
    [SerializeField] private bool _drawGizmosNotSelected = false;

    public float SuspicionLevel { get => _suspicionLevel;}
    public float MaxSuspicion { get => _maxSuspicion;}

    public bool IsAwareOfPlayer { get => _suspicionLevel > _minSuspicion;}
    public bool IsSuspicious { get => _suspicionLevel >= _maxSuspicion * _suspicionThresholdPercentage; }
    public bool HasDetectedPlayer { get => _hasDetectedPlayer; set => _hasDetectedPlayer = value;  }

    public bool CanDecaySuspicion { get; set; }

    public bool IsGraceState { get; private set; }

    public SuspicionState_e SuspicionState { get => _suspicionState;}
    public Vector3 LastKnownPActivityPos { get => _lastKnownPActivityPos; set => _lastKnownPActivityPos = value; }
    public float AlertDecayTimeSeconds { get => _alertDecayTimeSeconds; set => _alertDecayTimeSeconds = value; }
    public float CurrentAlertTimeSeconds { get => _cAlertTimeSeconds; set => _cAlertTimeSeconds = value; }

    private void Awake()
    {
        CanDecaySuspicion = true;
        ResetAlertLevel();
    }
    private void Update()
    {
        Visualize();

    }

    public void RaiseSuspicionLevel()
    {
        float distance = Vector3.Distance(_head.position, Player.Singleton.PlayerDetectReigon.position);

        float oldSuspicionLevel = _suspicionLevel;

        float detectionModifier = 1f;

        if (Player.Singleton.Concealed) detectionModifier -= Player.Singleton.ConcealedDetectionModifier;
        if (Player.Singleton.Crouched) detectionModifier -= Player.Singleton.CrouchedDetectionModifier;


        if (_instantDetectionDistance >= 0 && distance < _instantDetectionDistance)
        {
            _suspicionLevel = _maxSuspicion;
        }
        else if (_normalDetectionDistance >= 0 && distance < _normalDetectionDistance)
        {
            _suspicionLevel += _normalDetectionRate * Time.deltaTime * detectionModifier;
        }
        else if (_slowDetectionDistance >= 0 && distance < _slowDetectionDistance)
        {
            _suspicionLevel += _slowDetectionRate * Time.deltaTime * detectionModifier; 
        }

        _suspicionLevel = Mathf.Clamp(_suspicionLevel, _minSuspicion, _maxSuspicion);
        SuspicionPerSecond = (_suspicionLevel - oldSuspicionLevel) * (1 / Time.deltaTime);

    }


    public void UpdateSuspicionState()
    {
        if (_hasDetectedPlayer) _suspicionState = SuspicionState_e.ALERT;
        else if (_suspicionLevel >= _maxSuspicion && CanSeePlayer()) _suspicionState = SuspicionState_e.DETECTED;
        else if (_suspicionLevel >= _maxSuspicion * _suspicionThresholdPercentage) _suspicionState = SuspicionState_e.SUSPICIOUS;
        else if (_suspicionLevel > 0f) _suspicionState = SuspicionState_e.AWARE;
        else _suspicionState = SuspicionState_e.UNAWARE;
    }

    public void PercieveSurroundings()
    {
        if (CanSeePlayer())
        {
            RaiseSuspicionLevel();
            _lastKnownPActivityPos = Player.Singleton.transform.position;
        }
        else if (!_deaf && CanHearPlayerActivity(out _lastKnownPActivityPos))
        {
            //Debug.Log($"{name} heard suspicious sound, setting suspicion level to suspicious");
            _suspicionLevel = Mathf.Max(_maxSuspicion * _suspicionThresholdPercentage, _suspicionLevel);
        }
        else if(CanDecaySuspicion && _suspicionLevel > _minSuspicion)
        {
            //Debug.Log("Decaying suspicion");
            DecaySuspicion(); 
        }
    }

    public void TriggerGracePeriod()
    {
        if (!IsGraceState)
        {
            IsGraceState = true;
            StartCoroutine(DetectionGraceRoutine());
        }
    }
    /// <summary>
    /// Simulates a grace period between the AI detecting the player and the AI entering its alert state.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DetectionGraceRoutine()
    {
        float timer = _detectionGracePeriod;


        while (timer > 0f)
        {
            Debug.Log($"Grace Period ends in: {timer}");
            if (!CanSeePlayer()) /// if the enemy loses LOS of the player during the grace period, revert back to the suspicious state
            {
                _suspicionLevel = _maxSuspicion * _suspicionThresholdPercentage;
                _hasDetectedPlayer = false;
                IsGraceState = false;
                Debug.Log("Enemy has lost sight of player, reverting back to suspicious state");
                yield break;
            }

            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame(); 
        }
        Debug.Log("Enemy has not lost sight of player in time, moving to alert state");
        _hasDetectedPlayer = true;
        IsGraceState = false;
        yield break;

        
    }

    public void ResetAlertLevel()
    {
        if (_suspicionState != SuspicionState_e.ALERT) Debug.LogWarning($"WARNING : {name} tried to reset alert level despite not being in alert status");
        _suspicionLevel = 0f;
        _hasDetectedPlayer = false;
        CanDecaySuspicion = true; 
        _cAlertTimeSeconds = _alertDecayTimeSeconds;
    }

    public void DecaySuspicion()
    {
        float oldSuspicionLevel = _suspicionLevel;

        _suspicionLevel -= _detectionDecayRate * Time.deltaTime;
        _suspicionLevel = Mathf.Clamp(_suspicionLevel, _minSuspicion, _maxSuspicion);

        SuspicionPerSecond = (_suspicionLevel - oldSuspicionLevel) * (1 / Time.deltaTime);

    }
    public bool CanHearPlayerActivity(out Vector3 soundPosition)
    {
        /// Check for sound sources in hearing range
        Collider[] colliders = new Collider[_maxSoundSources];

        if (Physics.OverlapSphereNonAlloc(transform.position, _hearingRadius, colliders, _soundSourceLayer) <= 0)
        {
            soundPosition = _lastKnownPActivityPos;
            return false;
        }

        float smallestSqrDist = float.MaxValue;
        int smallestSqrDistIndex = -1;
        
        /// run through list of sound sources found within range and determine if they are valid sound sources
        /// valid sound sources are sources that are currently playing sound and have a sound source object attatched to them

        for(int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null) continue;
            float distance = (transform.position - colliders[i].transform.position).sqrMagnitude;
            SoundSource src = colliders[i].GetComponent<SoundSource>();

            if(src != null && src.Playing && distance < smallestSqrDist)
            {
                smallestSqrDist = distance;
                smallestSqrDistIndex = i; 
            }
        }
        if(smallestSqrDistIndex == -1)
        {
            soundPosition = _lastKnownPActivityPos;
            return false; 
        }
        soundPosition = colliders[smallestSqrDistIndex].transform.position;
        return true;
    }
    public bool CanHearPlayerActivity()
    {
        /// Check for sound sources in hearing range
        Collider[] colliders = new Collider[_maxSoundSources];

        if (Physics.OverlapSphereNonAlloc(transform.position, _hearingRadius, colliders, _soundSourceLayer) < 0)
        {
            return false;
        }

        float smallestSqrDist = float.MaxValue;
        int smallestSqrDistIndex = -1;

        /// run through list of sound sources found within range and determine if they are valid sound sources
        /// valid sound sources are sources that are currently playing sound and have a sound source object attatched to them

        for (int i = 0; i < colliders.Length; i++)
        {
            float distance = (transform.position - colliders[i].transform.position).sqrMagnitude;
            SoundSource src = colliders[i].GetComponent<SoundSource>();

            if (src != null && src.Playing && distance < smallestSqrDist)
            {
                smallestSqrDist = distance;
                smallestSqrDistIndex = i;
            }
        }
        if (smallestSqrDistIndex == -1)
        {
            return false;
        }
        return true;
    }

    public bool CanSeePlayer()
    {
        /// is the player within range?
        
        Vector3 playerPos = Player.Singleton.PlayerDetectReigon.position;

        float distance = GetDistanceFromPlayer();

        bool isWithinRange = distance <= _maxDetectionDistance;

        if (!isWithinRange) return false;


        /// is the player within FOV or the detection volume? (only applies if not detected)
        Vector3 playerRelativePos = playerPos - _head.position;

        if (_useTriggerVolume && _detectionVolume != null)
        {
            if (!_detectionVolume.PlayerWithinVolume && !_hasDetectedPlayer) return false;
        }
        else
        {
            float theta = Mathf.Acos(Vector3.Dot(playerRelativePos.normalized, _head.forward)) * Mathf.Rad2Deg;

            if (theta > _detectionFOV * 0.5f && !_hasDetectedPlayer) return false;
        }

        /// is there a LOS from the enemy to the player?

        int ignoreBitmask = ~((1 << gameObject.layer) | Player.Singleton.PlayerIgnorables);

        bool canSeePlayer = !Physics.Raycast(_head.position, playerRelativePos.normalized, distance, ignoreBitmask);

        if (!canSeePlayer) return false;


        return true;
    }

    public bool CanPerceivePlayer()
    {
        if (_deaf) return CanSeePlayer(); 
        else return CanSeePlayer() || CanHearPlayerActivity();
    }

    public float GetDistanceFromPlayer()
    {
        return Vector3.Distance(_head.position, Player.Singleton.PlayerDetectReigon.position);
    }

    private void Visualize()
    {
        Color c = CanSeePlayer() ? Color.green : Color.red;


        Debug.DrawLine(_head.position, Player.Singleton.PlayerDetectReigon.position, c);

    }
    private void DrawFOVCone()
    {
        Quaternion a1 = Quaternion.Euler(0f, _detectionFOV * 0.5f, 0f);
        Quaternion a2 = Quaternion.Euler(0f, -_detectionFOV * 0.5f, 0f);

        Quaternion a3 = Quaternion.Euler(_detectionFOV * 0.5f, 0f, 0f);
        Quaternion a4 = Quaternion.Euler(-_detectionFOV * 0.5f, 0f, 0f);


        Gizmos.matrix = _head.localToWorldMatrix;
        Handles.matrix = _head.localToWorldMatrix;

        Vector3 a = getPointProjected(Vector3.zero, a1, Vector3.forward, _maxDetectionDistance);
        Vector3 b = getPointProjected(Vector3.zero, a2, Vector3.forward, _maxDetectionDistance);

        Vector3 c = getPointProjected(Vector3.zero, a3, Vector3.forward, _maxDetectionDistance);
        Vector3 d = getPointProjected(Vector3.zero, a4, Vector3.forward, _maxDetectionDistance);


        Gizmos.color = Color.blue;

        Gizmos.DrawLine(Vector3.zero, a);
        Gizmos.DrawLine(Vector3.zero, b);
        Gizmos.DrawLine(Vector3.zero, c);
        Gizmos.DrawLine(Vector3.zero, d);

        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(c, d);

        if (_slowDetectionDistance > 0f)
        {
            Vector3 a_SDR = getPointProjected(Vector3.zero, a1, Vector3.forward, _slowDetectionDistance);
            Vector3 b_SDR = getPointProjected(Vector3.zero, a2, Vector3.forward, _slowDetectionDistance);
            Vector3 c_SDR = getPointProjected(Vector3.zero, a3, Vector3.forward, _slowDetectionDistance);
            Vector3 d_SDR = getPointProjected(Vector3.zero, a4, Vector3.forward, _slowDetectionDistance);

            Gizmos.color = Color.green;
            Handles.color = Color.green;

            Gizmos.DrawLine(a_SDR, b_SDR);
            Gizmos.DrawLine(c_SDR, d_SDR);

            float radius = (a_SDR - b_SDR).magnitude * 0.5f;
            Vector3 centerpoint = b_SDR + (a_SDR - b_SDR) * 0.5f;
            Handles.DrawWireDisc(centerpoint, Vector3.forward, radius);
        }

        if (_normalDetectionDistance > 0f)
        {

            Vector3 a_NDR = getPointProjected(Vector3.zero, a1, Vector3.forward, _normalDetectionDistance);
            Vector3 b_NDR = getPointProjected(Vector3.zero, a2, Vector3.forward, _normalDetectionDistance);
            Vector3 c_NDR = getPointProjected(Vector3.zero, a3, Vector3.forward, _normalDetectionDistance);
            Vector3 d_NDR = getPointProjected(Vector3.zero, a4, Vector3.forward, _normalDetectionDistance);

            Gizmos.color = Color.yellow;
            Handles.color = Color.yellow;

            Gizmos.DrawLine(a_NDR, b_NDR);
            Gizmos.DrawLine(c_NDR, d_NDR);

            float radius = (a_NDR - b_NDR).magnitude * 0.5f;
            Vector3 centerpoint = b_NDR + (a_NDR - b_NDR) * 0.5f;
            Handles.DrawWireDisc(centerpoint, Vector3.forward, radius);

        }

        if (_instantDetectionDistance > 0f)
        {
            Vector3 a_IDR = getPointProjected(Vector3.zero, a1, Vector3.forward, _instantDetectionDistance);
            Vector3 b_IDR = getPointProjected(Vector3.zero, a2, Vector3.forward, _instantDetectionDistance);
            Vector3 c_IDR = getPointProjected(Vector3.zero, a3, Vector3.forward, _instantDetectionDistance);
            Vector3 d_IDR = getPointProjected(Vector3.zero, a4, Vector3.forward, _instantDetectionDistance);

            Gizmos.color = Color.red;
            Handles.color = Color.red;


            Gizmos.DrawLine(a_IDR, b_IDR);
            Gizmos.DrawLine(c_IDR, d_IDR);

            float radius = (a_IDR - b_IDR).magnitude * 0.5f;
            Vector3 centerpoint = b_IDR + (a_IDR - b_IDR) * 0.5f;
            Handles.DrawWireDisc(centerpoint, Vector3.forward, radius);
        }

        Gizmos.matrix = Matrix4x4.identity;
        Handles.matrix = Matrix4x4.identity;
    }

    private void DrawDetectionDistances()
    {
        Handles.matrix = _head.localToWorldMatrix;
        if (_slowDetectionDistance > 0f)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(Vector3.forward * _slowDetectionDistance, Vector3.forward, 2f); 
        }
        if(_normalDetectionDistance > 0f)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(Vector3.forward * _normalDetectionDistance, Vector3.forward, 2f);
        }
        if (_instantDetectionDistance > 0f)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(Vector3.forward * _instantDetectionDistance, Vector3.forward, 2f);
        }

        Handles.matrix = Matrix4x4.identity;
        
    }

    private void DrawHearingRadius()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(Vector3.zero, _hearingRadius);

        Gizmos.matrix = Matrix4x4.identity;
    }
    /// <summary>
    /// returns a point projected across a line with a initial direction. This line is then rotated with a given rotation;
    /// </summary>
    /// <param name="point"></param> the initial point
    /// <param name="rotation"></param> the rotation of the output relative to the initial point and the point at line * distance
    /// <param name="direction"></param> the initial direction that the point is projected in before the rotation is applied  
    /// <param name="distance"></param> the distance to projected point
    /// <returns></returns>
    private Vector3 getPointProjected(Vector3 point, Quaternion rotation, Vector3 direction, float distance)
    {
        if (direction.magnitude > 1f) direction = direction.normalized;
        return point + rotation * (direction * distance);
    }


    

    private void OnDrawGizmosSelected()
    {
        if (_drawGizmosNotSelected) return;

        if (!_useTriggerVolume) DrawFOVCone();
        else DrawDetectionDistances();
        DrawHearingRadius();
    }
    private void OnDrawGizmos()
    {
        if (!_drawGizmosNotSelected) return;

        if (!_useTriggerVolume) DrawFOVCone();
        else DrawDetectionDistances();
        DrawHearingRadius();
    }
}
