using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLimb : MonoBehaviour
{
    public enum LimbType_e
    {
        CRITPOINT,
        HEAD,
        BACK,
        BACKOFHEAD,
        OTHER
    }
    [Header("Limb Info")]
    [SerializeField] private LimbType_e _limbType;
    [SerializeField] private Enemy _owner;
    public LimbType_e LimbType { get => _limbType;}
    public Enemy Owner { get => _owner; set => _owner = value; }


    private void Awake()
    {
        if(_owner == null)
        {
            Debug.LogWarning($"{name}::EnemyLimb : WARNING: OWNER IS NULL");
        }
    }
    public void DamageLimb(float amt)
    {
        if(_owner == null)
        {
            Debug.LogWarning($"{name}::EnemyLimb::DamageLimb() : WARNING: TRIED TO DAMAGE OWNER BUT OWNER IS NULL");
        }
        else
        {
            Owner.DamageEnemy(amt);
        }
    }
}
