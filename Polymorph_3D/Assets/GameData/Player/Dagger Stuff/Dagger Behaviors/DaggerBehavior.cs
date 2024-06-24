using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class DaggerBehavior : ScriptableObject
{
    public abstract float Cooldown { get;  }
    public abstract void Invoke(Vector3 forward, Vector3 origin);

}
