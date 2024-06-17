using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{


    [Header("Weapon Info")]
    [SerializeField] private List<WeaponScript> _weaponScripts;
    [SerializeField] private WeaponScript _activeWeapon;
    [SerializeField] private int _activeWeaponIndex;

    [Header("Input Info")]
    [SerializeField] private bool _swapButtonPressed; 


    private void Awake()
    {
        if (_weaponScripts.Count <= 0) Debug.LogWarning("PlayerWeaponManager::Awake() : WARNING: NO WEAPON SCRIPTS EQUIPPED");
        else
        {
            _activeWeapon = _weaponScripts[0];
            _activeWeaponIndex = 0;
            _activeWeapon.enabled = true; 

        }
    }


    private void Update()
    {
        QueuePlayerInput();
        WeaponManagerLogic(); 
    }

    private void QueuePlayerInput()
    {
        _swapButtonPressed = Input.GetButtonDown("WepSwp");
    }

    private void WeaponManagerLogic()
    {
        if (_swapButtonPressed)
        {
            SwapActiveWeapon();
        }
    }

    private void SwapActiveWeapon()
    {
        int nextIndex = _activeWeaponIndex + 1;

        if (nextIndex >= _weaponScripts.Count) nextIndex = 0;

        if (nextIndex != _activeWeaponIndex)
        {
            _activeWeapon.enabled = false;
            _activeWeapon = _weaponScripts[nextIndex];
            _activeWeapon.enabled = true;
            _activeWeaponIndex = nextIndex;
        }
    }
}
