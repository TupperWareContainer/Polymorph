using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidEnemy : Enemy
{
    private bool _isInvestigativeBehavior;
    private bool _isSeekingBehavior;
    private bool _isIdleBehavior;

    public override bool IsInvestigativeBehavior { get => _isInvestigativeBehavior; }
    public override bool IsSeekingBehavior { get => _isSeekingBehavior; }


    public override bool IsAttackBehavior => base.IsAttackBehavior;

    public override bool IsIdleBehavior { get => _isIdleBehavior; }


    public override IEnumerator InvestigativeBehavior(PlayerDetector detector, AIMovementScript movementScript)
    {
        yield break;
    }
    public override IEnumerator SeekingBehavior()
    {
        yield break;
    }
    public override IEnumerator AttackBehavior()
    {
        yield break;
    }

    public override IEnumerator IdleBehavior()
    {
        yield break;
    }

    public override void ResetBehavior()
    {
        StopAllCoroutines();
        _isInvestigativeBehavior = false;
        _isSeekingBehavior = false;
        _isIdleBehavior = false;
    }
}
