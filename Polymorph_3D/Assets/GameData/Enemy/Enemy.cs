using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] private string _name;
    [SerializeField] private float _health;

    public float Health { get => _health;}

    public virtual bool IsInvestigativeBehavior { get => false; }

    public virtual bool IsSeekingBehavior { get => false; }

    public virtual bool IsAttackBehavior { get => false; }

    public virtual bool IsIdleBehavior { get => false; }


    public abstract void ResetBehavior();

    public void DamageEnemy(float amt)
    {
        _health -= amt;
        if (_health <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        Destroy(gameObject);
    }

    public abstract IEnumerator InvestigativeBehavior(PlayerDetector detector, AIMovementScript movementScript);

    public abstract IEnumerator SeekingBehavior();

    public abstract IEnumerator AttackBehavior();

    public abstract IEnumerator IdleBehavior();



}
