using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Lure", menuName = "Scriptable Objects/Dagger Behaviors/Lure")]
public class DBLure : DaggerBehavior
{
    [SerializeField] private GameObject _lure; 
    [SerializeField] private float _cooldownTime;
    [SerializeField] private float _maxLureDistance = 100;
    [SerializeField] private LayerMask _targetable;

    public override float Cooldown { get => _cooldownTime; }

  
    public override void Invoke(Vector3 forward, Vector3 origin)
    {
        /// 1. draw a raycast forward for a certain distance
        /// 2. take the first collider hit and get the nearest point to the ray
        /// 3. spawn a lure at the nearest point and play its sound
        /// 4. delete the lure after its sound is finished playing
        RaycastHit hit;
        if (Physics.Raycast(origin, forward, out hit, _maxLureDistance, _targetable)) /// first check if the player hit something
        {
            if (Physics.Raycast(origin, forward, hit.distance, ~(_targetable | Player.Singleton.PlayerIgnorables)))  /// then check if there is anything in the way
            {
                return; 
            }
            else
            {
                GameObject src = Instantiate(_lure, hit.point, Quaternion.identity);
                if(src == null)
                {
                    Debug.LogError($"{name}::DaggerBehaviorRoutine() : ERROR, FAILED TO INSTANTIATE LURE");
                    return;
                }
                SoundSource audio = src.GetComponent<SoundSource>();

                if(audio == null)
                {
                    Debug.LogError($"{name}::DaggerBehaviorRoutine() : ERROR, LURE LACKS AUDIO SOURCE SCRIPT");
                    return;
                }

                if (!audio.Playing)
                {
                    audio.Play();
                }

            }
        }
    }
}
