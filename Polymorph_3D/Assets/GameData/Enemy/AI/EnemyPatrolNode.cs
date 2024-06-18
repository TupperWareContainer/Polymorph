using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolNode : MonoBehaviour
{
    /// Currently empty, I have plans to impliment custom behavior in-between patrol nodes via this script

    [Header("Custom Behavior")]
    [SerializeField] private bool _waitAtNode;
    [SerializeField] private bool _lookAtDirection;

    [Header("Behavior Settings")]
    [SerializeField] private float _waitTime;
    [SerializeField] private float _yawDirection;


    public PatrolNodeBehavior_t GetBehavior()
    {
        return new PatrolNodeBehavior_t(_waitAtNode,_lookAtDirection,_waitTime,_yawDirection);
    }


    private void OnDrawGizmos()
    {
        if (_lookAtDirection)
        {
            transform.rotation = Quaternion.Euler(0f, _yawDirection,0f);
            Gizmos.matrix = transform.localToWorldMatrix;


            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 2f);

            Gizmos.matrix = Matrix4x4.identity;
        }
    }

}
[System.Serializable]
public struct PatrolNodeBehavior_t
{
    public bool WaitAtNode;
    public bool LookAtDirection;

    
    public float WaitTime;
    public float Yaw;

    public PatrolNodeBehavior_t(bool waitAtNode, bool lookAtDirection, float waitTime, float yaw)
    {
        WaitAtNode = waitAtNode;
        LookAtDirection = lookAtDirection;
        WaitTime = waitTime;
        Yaw = yaw;
    }

    public override string ToString()
    {
        return $"WaitAtNode: {WaitAtNode}, WaitTime: {WaitTime}, LookAtDirection : {LookAtDirection}, Yaw: {Yaw}";
    }
}
