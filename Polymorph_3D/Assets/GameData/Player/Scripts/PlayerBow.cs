using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBow : WeaponScript
{
    public enum BowState_e
    {
        IDLE,
        CHARGING,
        FIRING,
        RELOADING,
        CANCELFIRE,
        ARROWSWP
    }




    [Header("Settings / Preferences")]
    [SerializeField] private Transform _projectileSpawnPoint;
    
    [Header("UI")]
    [SerializeField] private GameObject _bowUI;
    [SerializeField] private Slider _bowChargeSlider;

    [Header("Animation")]
    [SerializeField] private GameObject _viewmodel;
    [SerializeField] private Animator _animator; 


    [Header("Input Info")]
    [SerializeField] private bool _fireButtonPressed;
    [SerializeField] private bool _fireButtonReleased;
    [SerializeField] private bool _swapButtonPressed;
    [SerializeField] private bool _cancelButtonPressed;

    [Header("Arrow Info")]
    [SerializeField] private Arrow _currentArrow;
    [SerializeField] private List<Arrow> _unlockedArrowTypes;
    [SerializeField] private float _arrowChargeTime;
    [SerializeField] private int _cArrowIndex;

    [Header("Bow Info")]
    [SerializeField] private BowState_e _cBowState;
    [SerializeField] private float _cBowChargeTime;
    [SerializeField] private float _bowReloadTimeMax;
    [SerializeField] private float _bowReloadTimer;
    [SerializeField] private bool _mustReload;
    [SerializeField] private bool _isReloadState;
    [SerializeField] private bool _isFullyCharged;









    //TODO: Impliment Projectile shooting via prefabs (and possibly Scriptable Objects)



    private void Update()
    {
        QueuePlayerInput();
        UpdateBowState();
        BowLogic();
        _bowChargeSlider.value = _cBowChargeTime / _arrowChargeTime;

    }
    private void QueuePlayerInput()
    {
        _fireButtonPressed = Input.GetButton("Fire");
        _fireButtonReleased = Input.GetButtonUp("Fire");
        _cancelButtonPressed = Input.GetButton("Alt Fire");
    }

    private void UpdateBowState()
    {
        if (_mustReload || _isReloadState) _cBowState = BowState_e.RELOADING;                       
        else if (_cancelButtonPressed && _fireButtonPressed) _cBowState = BowState_e.CANCELFIRE;   
        else if (_fireButtonPressed) _cBowState = BowState_e.CHARGING;
        else if (!_cancelButtonPressed && _fireButtonReleased) _cBowState = BowState_e.FIRING;      /// if the player is not trying to cancel firing and has released the fire button
        else if (_swapButtonPressed) _cBowState = BowState_e.ARROWSWP;
        else _cBowState = BowState_e.IDLE;
    }

    private void BowLogic()
    {
        switch (_cBowState)
        {
            default:
            case BowState_e.IDLE:
                return;
            case BowState_e.CHARGING:
                ChargeBow();
                break;
            case BowState_e.FIRING:
                FireBow();
                break;
            case BowState_e.RELOADING:
                ReloadBow();
                break;
            case BowState_e.ARROWSWP:
                SwapArrow();
                break;
            case BowState_e.CANCELFIRE:
                CancelFire();
                break;

        }
    }


    private void ChargeBow()
    {

        _cBowChargeTime += Time.fixedDeltaTime;
        _cBowChargeTime = Mathf.Clamp(_cBowChargeTime, 0f, _arrowChargeTime);
        if (_cBowChargeTime >= _arrowChargeTime) _isFullyCharged = true;
    }

    private void FireBow()
    {
        //if (_isFullyCharged) Debug.Log("fired bow while fully charged");
        _currentArrow.Invoke(_projectileSpawnPoint, _isFullyCharged, Player.Singleton.PlayerTargets, Player.Singleton.PlayerIgnorables);
        _mustReload = true;
    }
    private void ReloadBow()
    {
        if (!_isReloadState)
        {
            StartCoroutine(BowReloadRoutine());
        }
    }

    private IEnumerator BowReloadRoutine()
    {
        _isReloadState = true;
        _bowReloadTimer = 0f; 

        while (_bowReloadTimer < _bowReloadTimeMax)
        {
            yield return new WaitForEndOfFrame();
            _bowReloadTimer += Time.deltaTime;
        }
        _mustReload = false;
        _isReloadState = false;
        ResetBowState(false);
    }
    private void SwapArrow()
    {
        int newIndex = _cArrowIndex + 1;
        if (newIndex >= _unlockedArrowTypes.Count) newIndex = 0;
        if(_cArrowIndex != newIndex)
        {
            _cArrowIndex = newIndex;
            _currentArrow = _unlockedArrowTypes[_cArrowIndex];
            ResetBowState(true);
        }
    }

    private void CancelFire()
    {
        ResetBowState(false);
    }

    private void ResetBowState(bool doReload)
    {
        _isReloadState = false;
        _isFullyCharged = false;
        _bowReloadTimer = 0f;
        _cBowChargeTime = 0f;
        _arrowChargeTime = _currentArrow.ChargeTime; 
        _mustReload = doReload; 
    }
    //resets the bow's current state and disables the model
    public override void DisableWeapon()
    {
        ResetBowState(false);
        if(_bowUI != null) _bowUI.SetActive(false);
        if (_viewmodel != null) _viewmodel.SetActive(false);

    }
    //re-enables the weapon and model and plays equip anim
    public override void EnableWeapon()
    {
        _currentArrow = _unlockedArrowTypes[0];
        _cArrowIndex = 0;
        ResetBowState(true);
        if (_bowUI != null) _bowUI.SetActive(true);
        if (_viewmodel != null) _viewmodel.SetActive(true);
    }

    private void OnDisable()
    {
        DisableWeapon(); 
    }
    private void OnEnable()
    {
        EnableWeapon(); 
    }


}
