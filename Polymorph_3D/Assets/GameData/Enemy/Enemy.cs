using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] private string _name;
    [SerializeField] private float _health;
    [SerializeField] private Transform _head;

    public float Health { get => _health;}

    public virtual bool IsInvestigativeBehavior { get => false; }

    public virtual bool IsSeekingBehavior { get => false; }

    public virtual bool IsAttackBehavior { get => false; }

    public virtual bool IsIdleBehavior { get => false; }

    public virtual Transform Head { get=> _head; set=> _head = value; }


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
