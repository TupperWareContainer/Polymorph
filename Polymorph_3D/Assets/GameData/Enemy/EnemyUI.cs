using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider _detectionSlider;

    [Header("Enemy Scripts")]
    [SerializeField] private PlayerDetector _detectorScript;


    private void Update()
    {
        _detectionSlider.value = _detectorScript.SuspicionLevel / _detectorScript.MaxSuspicion;
        _detectionSlider.transform.LookAt(Player.Singleton.transform);
    }
}
