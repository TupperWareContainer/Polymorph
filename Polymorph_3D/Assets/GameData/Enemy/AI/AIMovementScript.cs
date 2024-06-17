using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIMovementScript : MonoBehaviour
{
    public abstract bool Patrolling { get; }
    public abstract bool HasPatrolRoute { get; }

    public abstract bool MovingToTarget { get; }

    public abstract bool Stopped { get; }



    public abstract void UpdateTarget(Vector3 pt);

    public abstract void Patrol();

    public abstract void MoveToDefaultPosition();

    public abstract void Stop();

    public abstract bool AtTarget();

    public abstract void Idle();

    public abstract bool AtDefaultPosition();


}
public enum EnemyMovementState_e
{
    IDLE,
    MOVING,
    RETURNING_HOME
}
