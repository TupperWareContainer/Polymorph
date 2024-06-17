using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private bool _instantiated;
    [SerializeField] private Rigidbody _rb;


    public delegate void OnHit();
    public OnHit OnHitDelegate;

    private bool _destroy; 

    private float _speed;
    private float _damage;
    private float _headshotMultiplier;
    private float _lifeTime = 10f;
    private LayerMask _targetLayers;
    private LayerMask _ignorableLayers;


    public void Awake()
    {
        _instantiated = false;
    }


    public void Initiate(float speed, float damage, float headshotMultiplier,LayerMask targetLayers, LayerMask ignorableLayers)
    {
        _speed = speed;
        _damage = damage;
        _targetLayers = targetLayers;
        _ignorableLayers = ignorableLayers;
        _headshotMultiplier = headshotMultiplier;
        _instantiated = true;
        StartCoroutine(ArrowLogic());
        //Debug.Log("Arrow has been instantiated with damage " + _damage);

    }


    private IEnumerator ArrowLogic()
    {
        _rb.constraints = RigidbodyConstraints.FreezeAll;

        yield return new WaitWhile(() => !_instantiated);

        _rb.constraints = RigidbodyConstraints.None;
        _rb.velocity = _speed * transform.forward;
        Invoke(nameof(TriggerHitBehavior), _lifeTime);

        while (!_destroy)
        {
            _rb.velocity += Physics.gravity * Time.fixedDeltaTime;
            transform.LookAt(transform.position + _rb.velocity); // allows for the arrow to realistically point in the direction it is traveling
            yield return new WaitForFixedUpdate(); 
        }
        TriggerHitBehavior();
    }



    private void TriggerHitBehavior()
    {
        if(OnHitDelegate != null) OnHitDelegate.Invoke();
        Destroy(gameObject);
        
    }

    private float CalculateDamage(bool headshot)
    {
        float ret = _damage;
        if (headshot) ret *= _headshotMultiplier;
        return ret; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_instantiated) return;
        else if((_targetLayers & (1<< other.gameObject.layer)) != 0)
        {
            EnemyLimb limb = other.GetComponent<EnemyLimb>();
            if(limb != null)
            {
                Debug.Log($"Arrow has hit enemy in limb {limb.LimbType}");
                switch (limb.LimbType)
                {
                    case EnemyLimb.LimbType_e.CRITPOINT:
                    case EnemyLimb.LimbType_e.HEAD:
                    case EnemyLimb.LimbType_e.BACKOFHEAD:
                        limb.DamageLimb(CalculateDamage(true));
                        Debug.Log("Arrow has crit!");
                        break;
                    default:
                        limb.DamageLimb(CalculateDamage(false));
                        break;
                }
                _destroy = true; 
            }
        }
        
        if( (_ignorableLayers & (1<< other.gameObject.layer)) == 0 && (_targetLayers & (1<<other.gameObject.layer)) == 0) _destroy = true;

    }

}
