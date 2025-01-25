using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    [SerializeField]
    bool isTriggered = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (Player.Instance.IsSliding && isTriggered)
        {
            transform.parent.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        }
        if (!Player.Instance.IsSliding)
        {
            transform.parent.gameObject.GetComponent<BoxCollider>().isTrigger = false;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            isTriggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            isTriggered = false;
        }
    }
}
