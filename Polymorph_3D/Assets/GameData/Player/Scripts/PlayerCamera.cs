using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _cameraTransform;

    [Header("Preferences")]
    [SerializeField] private float _mouseSensitivityMult = 1.0f;


    private float _xRot;
    private float _yRot;

    private void Start()
    {
        _xRot = _cameraTransform.rotation.x;
        _yRot = _playerTransform.rotation.y; 
    }

    private void Update()
    {
        /// This might cause issues with varying framerates later on, possible fix is to multiply by Time.deltaTime
        _xRot -= Input.GetAxisRaw("Mouse Y") * _mouseSensitivityMult;
        _yRot += Input.GetAxisRaw("Mouse X") * _mouseSensitivityMult;

        _xRot = Mathf.Min(_xRot, 90);
        _xRot = Mathf.Max(_xRot, -90);


        _playerTransform.rotation = Quaternion.Euler(0f, _yRot, 0f);
        _cameraTransform.localRotation = Quaternion.Euler(_xRot, 0f, 0f); 
    }




    

}                    
