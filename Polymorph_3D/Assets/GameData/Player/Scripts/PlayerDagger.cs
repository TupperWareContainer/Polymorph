using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDagger : WeaponScript
{
    public enum KnifeState_e
    {
          IDLE,
          ATTACKING,
          FUNC1
    }

    [Header("Settings / References")]
    [SerializeField] private Transform _attackInstPoint;
    [SerializeField] private GameObject _daggerModel;
    [SerializeField] private Animator _daggerAnimator;

    [Header("UI")]
    [SerializeField] private GameObject _daggerUI;


    [Header("Weapon Info")]
    [SerializeField] private KnifeState_e _cKnifeState;
    [SerializeField] private float _daggerRange;
    [SerializeField] private float _daggerDamage;
    [SerializeField] private List<DaggerBehavior> _daggerBehaviors;  

    [Header("Input Info")]
    [SerializeField] private bool _attackButtonPressed;
    [SerializeField] private bool _altButtonPressed;




    private void Update()
    {
        QueuePlayerInput();
        UpdateDaggerState();
        DaggerLogic();
        Debug.DrawRay(_attackInstPoint.position, _attackInstPoint.forward * _daggerRange, Color.blue);
    }

    private void QueuePlayerInput()
    {
        _attackButtonPressed = Input.GetButtonDown("Fire");
        _altButtonPressed = Input.GetButtonDown("Alt Fire");
    }

    private void UpdateDaggerState()
    {
        if (_attackButtonPressed) _cKnifeState = KnifeState_e.ATTACKING;
        else if (_altButtonPressed) _cKnifeState = KnifeState_e.FUNC1; 
        else _cKnifeState = KnifeState_e.IDLE; 
    }

    private void DaggerLogic()
    {
        switch (_cKnifeState)
        {
            default:
            case KnifeState_e.FUNC1:

                break;
            case KnifeState_e.IDLE:
                if (CheckIfBackstab()) _daggerAnimator.SetBool("CanBackstab", true);
                else _daggerAnimator.SetBool("CanBackstab", false);
                return;
            case KnifeState_e.ATTACKING:
                Attack();
                break;
        }
    }


    private void Attack()
    {
        RaycastHit hit; 

        if(Physics.Raycast(_attackInstPoint.position,_attackInstPoint.forward,out hit, _daggerRange, Player.Singleton.PlayerTargets))
        {
            EnemyLimb limb = hit.collider.GetComponent<EnemyLimb>(); 

            if(limb == null)
            {
                Debug.LogWarning("Hit enemy limb without a corresponding limb script!");
                return;
            }
            else
            {
                switch (limb.LimbType)
                {
                    case EnemyLimb.LimbType_e.BACK:
                    case EnemyLimb.LimbType_e.BACKOFHEAD:
                        limb.DamageLimb(limb.Owner.Health);
                        _daggerAnimator.SetTrigger("Backstab");
                        return;
                    default:
                        limb.DamageLimb(_daggerDamage);
                        break;
                }
            }
            
        }
        _daggerAnimator.SetTrigger("Attack");


    }

    private void InvokeDaggerBehavior()
    {

    }





    public override void DisableWeapon()
    {
        if (_daggerUI != null) _daggerUI.SetActive(false);
        if (_daggerModel != null) _daggerModel.SetActive(false);
    }
    public override void EnableWeapon()
    {
        if (_daggerUI != null) _daggerUI.SetActive(true);
        if (_daggerModel != null) _daggerModel.SetActive(true);

    }

    private void OnEnable()
    {
        EnableWeapon();
    }
    private void OnDisable()
    {
        DisableWeapon();
    }

    private bool CheckIfBackstab()
    {
        RaycastHit hit;
        Physics.Raycast(_attackInstPoint.position, _attackInstPoint.forward, out hit, _daggerRange, Player.Singleton.PlayerTargets);
        if (hit.collider == null) return false; 
        EnemyLimb limb = hit.collider.GetComponent<EnemyLimb>();

        return limb != null && (limb.LimbType == EnemyLimb.LimbType_e.BACK || limb.LimbType == EnemyLimb.LimbType_e.BACKOFHEAD); 
    }
}
