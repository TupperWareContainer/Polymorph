using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Arrow Variant",menuName = "Scriptable Objects/Projectiles/Arrow Variant")]
public class Arrow : ScriptableObject
{
    [Header("References")]
    public GameObject Projectile;
    public List<ArrowBehavior> Behaviors; 
    [Header("Projectile Parameters")]
    public float BaseProjectileSpeed;
    public float ChargedProjectileSpeed;
    public float ChargeTime;
    public float BaseDamage;
    public float ChargeDamage;
    public float HeadshotDamageMultiplier;
    




    public void Invoke(Transform instPointTransform, bool isCharged, LayerMask targetable, LayerMask ignorable)
    {
        ArrowProjectile projectile = Instantiate(Projectile, instPointTransform.position, instPointTransform.rotation).GetComponent<ArrowProjectile>();
                         
        foreach(ArrowBehavior behavior in Behaviors)
        {
            projectile.OnHitDelegate += ()=> behavior.Invoke(this); 
        } 
        
        if (isCharged) projectile.Initiate(ChargedProjectileSpeed, ChargeDamage, HeadshotDamageMultiplier, targetable, ignorable);
        else projectile.Initiate(BaseProjectileSpeed, BaseDamage, HeadshotDamageMultiplier, targetable, ignorable);
    }

}
