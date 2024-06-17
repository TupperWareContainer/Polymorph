using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Singleton;

    public LayerMask PlayerTargets;

    public LayerMask PlayerIgnorables;

    public Transform PlayerDetectReigon;

    public CapsuleCollider PlayerCollider;

    public Transform Camera;

    public bool Crouched;



    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Debug.Log("Attempted to assign value to pre-exsisting singleton, disposing...");
            Destroy(gameObject);
        }
    }






}
