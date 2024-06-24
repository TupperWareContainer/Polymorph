using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Lure", menuName = "Scriptable Objects/Dagger Behaviors/Lure")]
public class DBLure : DaggerBehavior
{
    [Header("Settings")]
    [SerializeField] private float _cooldownTime;
    public override float Cooldown { get => _cooldownTime; }
    public override void Invoke(Vector3 forward)
    {

    }
}
