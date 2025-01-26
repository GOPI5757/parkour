using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
    [SerializeField]
    public int checkPoint = 0;


    public void GotCheckpoint()
    {
        GetComponent<AudioSource>().Play();
        GetComponent<BoxCollider>().enabled = false;
    }
}
