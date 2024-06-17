using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDetectionVolume : MonoBehaviour
{

    public bool PlayerWithinVolume { get; private set; }


    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == Player.Singleton.gameObject.layer)
        {
            PlayerWithinVolume = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == Player.Singleton.gameObject.layer)
        {
            PlayerWithinVolume = false;
        }
    }

}
