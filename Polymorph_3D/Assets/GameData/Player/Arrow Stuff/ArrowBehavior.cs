using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ArrowBehavior : ScriptableObject
{

    public abstract void Invoke(Arrow arrow);
}
