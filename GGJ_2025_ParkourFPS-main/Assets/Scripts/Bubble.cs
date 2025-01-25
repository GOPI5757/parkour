using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public float timeToWaitBeforePop;


    private void OnCollisionEnter(Collision other) 
    {
        if(other.transform.CompareTag("Player"))
        {
            Destroy(gameObject, timeToWaitBeforePop);
        }
    }
}
